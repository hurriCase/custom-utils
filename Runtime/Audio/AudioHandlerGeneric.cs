using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Pools.Objects;
using CustomUtils.Runtime.Storage;
using CustomUtils.Unsafe;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;

namespace CustomUtils.Runtime.Audio
{
    public abstract class AudioHandlerGeneric<TMusicType, TSoundType> : MonoBehaviour,
        IAudioHandlerGeneric<TMusicType, TSoundType>
        where TMusicType : unmanaged, Enum
        where TSoundType : unmanaged, Enum
    {
        [SerializeField] private AudioDatabaseGeneric<TMusicType, TSoundType> _audioDatabaseGeneric;

        [SerializeField] private AudioConfig _audioConfig;
        [SerializeField] private PoolConfig<AudioSource> _soundPoolConfig;
        [SerializeField] private AudioSource _clipSource;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _oneShotSource;

        public PersistentReactiveProperty<float> MusicVolume { get; } = new();
        public PersistentReactiveProperty<float> SfxVolume { get; } = new();

        public bool SfxEnabled => SfxVolume.Value > 0;
        public bool MusicEnabled => MusicVolume.Value > 0;

        private const string MusicVolumeKey = "music_volume";
        private const string SfxVolumeKey = "sfx_volume";

        private const float SilenceDb = -80f;
        private const float DbConversionFactor = 20f;

        private readonly Dictionary<int, float> _lastPlayedTimes = new();
        private readonly List<AliveAudioData<TSoundType>> _aliveAudios = new();
        private readonly List<AliveAudioData<TSoundType>> _audiosToRemove = new();

        private Pool<AudioSource> _soundPool;

        private AudioMixer _audioMixer;

        public virtual async UniTask InitAsync(IAddressablesLoader addressablesLoader, CancellationToken token)
        {
            _audioMixer = await addressablesLoader.LoadAsync<AudioMixer>(_audioConfig.AudioMixerReference, token);

            await MusicVolume.InitializeAsync(MusicVolumeKey, token, _audioConfig.DefaultMusicVolume);
            await SfxVolume.InitializeAsync(SfxVolumeKey, token, _audioConfig.DefaultSFXVolume);

            _soundPool = new ComponentPool<AudioSource>(_soundPoolConfig);

            SfxVolume.SubscribeUntilDestroy(this, static (volume, self) => self.OnSoundVolumeChanged(volume));
            MusicVolume.SubscribeUntilDestroy(this, static (volume, self) => self.OnMusicVolumeChanged(volume));
        }

        public virtual AudioSource PlaySound(TSoundType soundType, float volumeModifier = 1, float pitchModifier = 1)
        {
            var soundData = _audioDatabaseGeneric.SoundContainers[soundType];
            if (!ShouldPlaySound(soundType, soundData))
                return null;

            var soundTypeValue = UnsafeEnumConverter<TSoundType>.ToInt32(soundType);
            _lastPlayedTimes[soundTypeValue] = Time.unscaledTime;

            var soundSource = _soundPool.Get();
            soundSource.clip = soundData.AudioData.AudioClip;
            soundSource.pitch = pitchModifier * soundData.AudioData.RandomPitch.Value;
            soundSource.volume = volumeModifier * soundData.AudioData.RandomVolume.Value;

            soundSource.Play();

            var aliveData = new AliveAudioData<TSoundType>(soundType, soundSource);
            _aliveAudios.Add(aliveData);

            PlaySoundInternal(aliveData).Forget();

            return soundSource;
        }

        public virtual AudioSource PlayClip(AudioClip soundType, float volumeModifier = 1, float pitchModifier = 1)
        {
            _clipSource.clip = soundType;
            _clipSource.pitch = pitchModifier;
            _clipSource.volume = volumeModifier;

            _clipSource.Play();

            return _clipSource;
        }

        public virtual void StopClip()
        {
            _clipSource.Stop();
        }

        public virtual void PlayOneShotSound(TSoundType soundType, float volumeModifier = 1, float pitchModifier = 1)
        {
            var soundData = _audioDatabaseGeneric.SoundContainers[soundType];

            if (soundData?.AudioData == null || !soundData.AudioData?.AudioClip)
                return;

            _oneShotSource.pitch = pitchModifier * soundData.AudioData.RandomPitch.Value;
            _oneShotSource.volume = volumeModifier * soundData.AudioData.RandomVolume.Value;
            _oneShotSource.PlayOneShot(soundData.AudioData.AudioClip);
        }

        public virtual AudioSource PlayMusic(TMusicType musicType)
        {
            var musicData = _audioDatabaseGeneric.MusicContainers[musicType];
            return musicData == null ? null : PlayMusic(musicData);
        }

        public virtual AudioSource PlayMusic(AudioData data)
        {
            if (data == null || !data.AudioClip)
                return null;

            _musicSource.clip = data.AudioClip;
            _musicSource.pitch = data.RandomPitch.Value;
            _musicSource.volume = data.RandomVolume.Value;
            _musicSource.Play();

            return _musicSource;
        }

        public virtual void StopMusic()
        {
            _musicSource.Stop();
        }

        public virtual void StopSound(TSoundType soundType)
        {
            var soundTypeValueToRemove = UnsafeEnumConverter<TSoundType>.ToInt32(soundType);

            _audiosToRemove.Clear();
            foreach (var audioData in _aliveAudios)
            {
                var soundTypeValue = UnsafeEnumConverter<TSoundType>.ToInt32(audioData.SoundType);

                if (soundTypeValue != soundTypeValueToRemove)
                    continue;

                audioData.AudioSource.Stop();
                _soundPool.Release(audioData.AudioSource);
                _audiosToRemove.Add(audioData);
            }

            foreach (var audioData in _audiosToRemove)
                _aliveAudios.Remove(audioData);
        }

        /// <summary>
        /// Called when sound volume changes to update all active sound sources
        /// </summary>
        /// <param name="normalizedVolume">New sound volume level</param>
        protected virtual void OnSoundVolumeChanged(float normalizedVolume)
        {
            _audioMixer.SetFloat(_audioConfig.SFXMixerKey, NormalizedToDb(normalizedVolume));
        }

        /// <summary>
        /// Called when music volume changes to update the music source
        /// </summary>
        /// <param name="normalizedVolume">New music volume level</param>
        protected virtual void OnMusicVolumeChanged(float normalizedVolume)
        {
            _audioMixer.SetFloat(_audioConfig.MusicMixerKey, NormalizedToDb(normalizedVolume));
        }

        private bool ShouldPlaySound(TSoundType soundType, SoundContainer soundData)
        {
            if (soundData?.AudioData == null || !soundData.AudioData?.AudioClip)
                return false;

            var soundValue = UnsafeEnumConverter<TSoundType>.ToInt32(soundType);
            return soundData.Cooldown == 0 ||
                   !_lastPlayedTimes.TryGetValue(soundValue, out var lastTime) ||
                   !(Time.unscaledTime < lastTime + soundData.Cooldown);
        }

        private async UniTask PlaySoundInternal(AliveAudioData<TSoundType> aliveData)
        {
            await UniTask.WaitForSeconds(aliveData.AudioSource.clip.length);

            _soundPool.Release(aliveData.AudioSource);
            _aliveAudios.Remove(aliveData);
        }

        private static float NormalizedToDb(float normalizedVolume)
            => Mathf.Approximately(normalizedVolume, 0f)
                ? SilenceDb
                : Mathf.Log10(normalizedVolume) * DbConversionFactor;

        protected virtual void OnDestroy()
        {
            MusicVolume.Dispose();
            SfxVolume.Dispose();
        }
    }
}