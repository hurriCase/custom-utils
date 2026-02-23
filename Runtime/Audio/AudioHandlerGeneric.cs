using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Storage;
using CustomUtils.Unsafe;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CustomUtils.Runtime.Audio
{
    public abstract class AudioHandlerGeneric<TMusicType, TSoundType> : MonoBehaviour,
        IAudioHandlerGeneric<TMusicType, TSoundType>
        where TMusicType : unmanaged, Enum
        where TSoundType : unmanaged, Enum
    {
        [SerializeField] private AudioDatabaseGeneric<TMusicType, TSoundType> _audioDatabaseGeneric;

        [SerializeField] private AudioSource _soundSourcePrefab;
        [SerializeField] private AudioSource _clipSource;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _oneShotSource;

        [SerializeField] private int _soundPoolSize;
        [SerializeField] private int _maxPoolSize;

        public PersistentReactiveProperty<float> MusicVolume { get; } = new();
        public PersistentReactiveProperty<float> SoundVolume { get; } = new();

        private readonly Dictionary<int, float> _lastPlayedTimes = new();
        private readonly List<AliveAudioData<TSoundType>> _aliveAudios = new();
        private readonly List<AliveAudioData<TSoundType>> _audiosToRemove = new();

        private PoolHandler<AudioSource> _soundPool;
        private AudioData _currentMusicData;

        private const string MusicVolumeKey = "MusicVolumeKey";
        private const string SoundVolumeKey = "SoundVolumeKey";

        public virtual async UniTask InitAsync(
            float defaultMusicVolume = 1f,
            float defaultSoundVolume = 1f,
            CancellationToken cancellationToken = default)
        {
            await MusicVolume.InitAsync(MusicVolumeKey, destroyCancellationToken, defaultMusicVolume);
            await SoundVolume.InitAsync(SoundVolumeKey, destroyCancellationToken, defaultSoundVolume);

            _soundPool = new PoolHandler<AudioSource>();
            _soundPool.Initialize(_soundSourcePrefab, _soundPoolSize, _maxPoolSize, parent: transform);

            SoundVolume.SubscribeUntilDestroy(this, static (volume, self) => self.OnSoundVolumeChanged(volume));
            MusicVolume.SubscribeUntilDestroy(this, static (volume, self) => self.OnMusicVolumeChanged(volume));
        }

        public virtual AudioSource PlaySound(TSoundType soundType, float volumeModifier = 1, float pitchModifier = 1)
        {
            var soundData = _audioDatabaseGeneric.SoundContainers[soundType];
            if (ShouldPlaySound(soundType, soundData) is false)
                return null;

            var soundTypeValue = UnsafeEnumConverter<TSoundType>.ToInt32(soundType);
            _lastPlayedTimes[soundTypeValue] = Time.unscaledTime;

            var soundSource = _soundPool.Get();
            soundSource.clip = soundData.AudioData.AudioClip;
            soundSource.pitch = pitchModifier * soundData.AudioData.RandomPitch.Value;
            soundSource.volume = SoundVolume.Value * volumeModifier * soundData.AudioData.RandomVolume.Value;

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
            _clipSource.volume = SoundVolume.Value * volumeModifier;

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
            _oneShotSource.volume = SoundVolume.Value * volumeModifier * soundData.AudioData.RandomVolume.Value;
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
            _musicSource.volume = MusicVolume.Value * data.RandomVolume.Value;
            _musicSource.Play();

            _currentMusicData = data;

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
        /// <param name="soundVolume">New sound volume level</param>
        protected virtual void OnSoundVolumeChanged(float soundVolume)
        {
            foreach (var aliveAudioData in _aliveAudios)
                aliveAudioData.AudioSource.volume = soundVolume;
        }

        /// <summary>
        /// Called when music volume changes to update the music source
        /// </summary>
        /// <param name="musicVolume">New music volume level</param>
        protected virtual void OnMusicVolumeChanged(float musicVolume)
        {
            _musicSource.volume = (_currentMusicData?.RandomVolume.Value ?? 0) * musicVolume;
        }

        private bool ShouldPlaySound(TSoundType soundType, SoundContainer soundData)
        {
            if (soundData?.AudioData == null || !soundData.AudioData?.AudioClip)
                return false;

            var soundValue = UnsafeEnumConverter<TSoundType>.ToInt32(soundType);
            return soundData.Cooldown == 0 ||
                   _lastPlayedTimes.TryGetValue(soundValue, out var lastTime) is false ||
                   (Time.unscaledTime < lastTime + soundData.Cooldown) is false;
        }

        private async UniTask PlaySoundInternal(AliveAudioData<TSoundType> aliveData)
        {
            await UniTask.WaitForSeconds(aliveData.AudioSource.clip.length);

            _soundPool.Release(aliveData.AudioSource);
            _aliveAudios.Remove(aliveData);
        }

        protected virtual void OnDestroy()
        {
            MusicVolume.Dispose();
            SoundVolume.Dispose();
        }
    }
}