using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CustomUtils.Runtime.Audio
{
    [PublicAPI]
    [CreateAssetMenu(menuName = nameof(AudioConfig), fileName = nameof(AudioConfig))]
    public class AudioConfig : ScriptableObject
    {
        [field: SerializeField] public AssetReference AudioMixerReference { get; private set; }
        [field: SerializeField] public string SFXMixerKey { get; private set; }
        [field: SerializeField] public string MusicMixerKey { get; private set; }
        [field: SerializeField] public float DefaultSFXVolume { get; private set; }
        [field: SerializeField] public float DefaultMusicVolume { get; private set; }
    }
}