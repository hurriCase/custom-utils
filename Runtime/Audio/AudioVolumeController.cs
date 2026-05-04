using System;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.Audio.Data;
using CustomUtils.Runtime.Storage;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Scripting;

namespace CustomUtils.Runtime.Audio
{
    [Preserve]
    public sealed class AudioVolumeController : IAudioVolumeController, IDisposable
    {
        public PersistentReactiveProperty<float> SfxVolume { get; } = new();
        public PersistentReactiveProperty<float> MusicVolume { get; } = new();

        public bool SfxEnabled => SfxVolume.Value > 0;
        public bool MusicEnabled => MusicVolume.Value > 0;
        public float DefaultSfxVolume { get; }
        public float DefaultMusicVolume { get; }

        private const string SfxVolumeKey = "sfx_volume";
        private const string MusicVolumeKey = "music_volume";
        private const float SilenceDb = -80f;
        private const float DbConversionFactor = 20f;

        private AudioMixer _audioMixer;

        private IDisposable _volumeSubscriptions;

        private readonly IAddressablesLoader _addressablesLoader;
        private readonly AudioConfig _audioConfig;

        [Preserve]
        public AudioVolumeController(IAddressablesLoader addressablesLoader, AudioConfig audioConfig)
        {
            _addressablesLoader = addressablesLoader;
            _audioConfig = audioConfig;
            DefaultSfxVolume = audioConfig.DefaultSFXVolume;
            DefaultMusicVolume = audioConfig.DefaultMusicVolume;
        }

        public async UniTask InitializeAsync(CancellationToken token)
        {
            _audioMixer = await _addressablesLoader.LoadAsync<AudioMixer>(_audioConfig.AudioMixerReference, token);

            var initTasks = new[]
            {
                SfxVolume.InitializeAsync(SfxVolumeKey, token, DefaultSfxVolume),
                MusicVolume.InitializeAsync(MusicVolumeKey, token, DefaultMusicVolume)
            };

            await UniTask.WhenAll(initTasks);

            var sfxSubscription = SfxVolume.Subscribe(this,
                static (volume, self) => self.SetSfxVolume(volume));

            var musicSubscription = MusicVolume.Subscribe(this,
                static (volume, self) => self.SetMusicVolume(volume));

            _volumeSubscriptions = Disposable.Combine(sfxSubscription, musicSubscription);
        }

        private void SetSfxVolume(float normalizedVolume)
            => _audioMixer.SetFloat(_audioConfig.SFXMixerKey, NormalizedToDb(normalizedVolume));

        private void SetMusicVolume(float normalizedVolume)
            => _audioMixer.SetFloat(_audioConfig.MusicMixerKey, NormalizedToDb(normalizedVolume));

        private static float NormalizedToDb(float normalizedVolume)
            => Mathf.Approximately(normalizedVolume, 0f)
                ? SilenceDb
                : Mathf.Log10(normalizedVolume) * DbConversionFactor;

        public void Dispose()
        {
            _volumeSubscriptions?.Dispose();
            SfxVolume.Dispose();
            MusicVolume.Dispose();
        }
    }
}