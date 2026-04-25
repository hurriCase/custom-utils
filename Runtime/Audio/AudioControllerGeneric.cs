using System;
using System.Collections.Generic;
using CustomUtils.Runtime.Audio.Data;
using CustomUtils.Runtime.Pools.Objects;
using CustomUtils.Unmanaged;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Audio
{
    [PublicAPI]
    public abstract class AudioControllerGeneric<TMusicType, TSfxType> : MonoBehaviour
        where TMusicType : unmanaged, Enum
        where TSfxType : unmanaged, Enum
    {
        [SerializeField] private AudioDatabaseGeneric<TMusicType, TSfxType> _audioDatabaseGeneric;

        [SerializeField] private PoolConfig<AudioSource> _sfxPoolConfig;
        [SerializeField] private AudioSource _clipSource;
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _oneShotSource;

        private readonly Dictionary<int, float> _lastPlayedTimes = new();
        private readonly List<AliveAudioData<TSfxType>> _aliveAudios = new();
        private readonly List<AliveAudioData<TSfxType>> _audiosToRemove = new();

        private Pool<AudioSource> _sfxPool;

        public virtual void Initialize()
        {
            _sfxPool = new ComponentPool<AudioSource>(_sfxPoolConfig);
        }

        public virtual AudioSource PlaySfx(TSfxType sfxType, float volumeModifier = 1, float pitchModifier = 1)
        {
            var soundData = _audioDatabaseGeneric.SoundContainers[sfxType];
            if (!ShouldPlaySfx(sfxType, soundData))
                return null;

            var soundTypeValue = UnsafeEnumConverter<TSfxType>.ToInt32(sfxType);
            _lastPlayedTimes[soundTypeValue] = Time.unscaledTime;

            var soundSource = _sfxPool.Get();
            soundSource.clip = soundData.AudioData.AudioClip;
            soundSource.pitch = pitchModifier * soundData.AudioData.RandomPitch.Value;
            soundSource.volume = volumeModifier * soundData.AudioData.RandomVolume.Value;

            soundSource.Play();

            var aliveData = new AliveAudioData<TSfxType>(sfxType, soundSource);
            _aliveAudios.Add(aliveData);

            PlaySfxInternal(aliveData).Forget();

            return soundSource;
        }

        public virtual AudioSource PlayClip(AudioClip sfxType, float volumeModifier = 1, float pitchModifier = 1)
        {
            _clipSource.clip = sfxType;
            _clipSource.pitch = pitchModifier;
            _clipSource.volume = volumeModifier;

            _clipSource.Play();

            return _clipSource;
        }

        public virtual void StopClip()
        {
            _clipSource.Stop();
        }

        public virtual void PlayOneShotSfx(TSfxType sfxType, float volumeModifier = 1, float pitchModifier = 1)
        {
            var soundData = _audioDatabaseGeneric.SoundContainers[sfxType];

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

        public virtual void StopSfx(TSfxType soundType)
        {
            var soundTypeValueToRemove = UnsafeEnumConverter<TSfxType>.ToInt32(soundType);

            _audiosToRemove.Clear();
            foreach (var audioData in _aliveAudios)
            {
                var soundTypeValue = UnsafeEnumConverter<TSfxType>.ToInt32(audioData.SoundType);

                if (soundTypeValue != soundTypeValueToRemove)
                    continue;

                audioData.AudioSource.Stop();
                _sfxPool.Release(audioData.AudioSource);
                _audiosToRemove.Add(audioData);
            }

            foreach (var audioData in _audiosToRemove)
                _aliveAudios.Remove(audioData);
        }

        private bool ShouldPlaySfx(TSfxType sfxType, SoundContainer soundData)
        {
            if (soundData?.AudioData == null || !soundData.AudioData?.AudioClip)
                return false;

            var sfxValue = UnsafeEnumConverter<TSfxType>.ToInt32(sfxType);
            return soundData.Cooldown == 0 ||
                   !_lastPlayedTimes.TryGetValue(sfxValue, out var lastTime) ||
                   !(Time.unscaledTime < lastTime + soundData.Cooldown);
        }

        private async UniTask PlaySfxInternal(AliveAudioData<TSfxType> aliveData)
        {
            await UniTask.WaitForSeconds(aliveData.AudioSource.clip.length);

            _sfxPool.Release(aliveData.AudioSource);
            _aliveAudios.Remove(aliveData);
        }
    }
}