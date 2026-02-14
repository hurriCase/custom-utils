using System;
using UnityEngine;

namespace CustomUtils.Runtime.Audio
{
    [Serializable]
    public sealed class SoundContainer
    {
        [field: SerializeField] public AudioData AudioData { get; private set; }
        [field: SerializeField] public float Cooldown { get; private set; }
    }
}