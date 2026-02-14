using System;
using CustomUtils.Runtime.CustomTypes.Randoms;
using UnityEngine;

namespace CustomUtils.Runtime.Audio
{
    /// <summary>
    /// Contains audio clip and randomization settings for sound playback
    /// </summary>
    [Serializable]
    public class AudioData
    {
        [field: SerializeField] public AudioClip AudioClip { get; private set; }
        [field: SerializeField] public RandomFloat RandomVolume { get; private set; }
        [field: SerializeField] public RandomFloat RandomPitch { get; private set; }
    }
}