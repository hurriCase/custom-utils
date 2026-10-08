using System.Threading;
using CustomUtils.Runtime.Storage.Persistent;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;

namespace CustomUtils.Runtime.Audio
{
    [PublicAPI]
    public interface IAudioVolumeController
    {
        PersistentReactiveProperty<float> SfxVolume { get; }
        PersistentReactiveProperty<float> MusicVolume { get; }
        bool SfxEnabled { get; }
        bool MusicEnabled { get; }
        float DefaultSfxVolume { get; }
        float DefaultMusicVolume { get; }
        UniTask InitializeAsync(CancellationToken token);
    }
}