using System;
using CustomUtils.Unmanaged;
using UnityEngine;

namespace CustomUtils.Runtime.Audio.Data
{
    internal readonly struct AliveAudioData<TEnum> : IEquatable<AliveAudioData<TEnum>> where TEnum : unmanaged, Enum
    {
        internal TEnum SoundType { get; }
        internal AudioSource AudioSource { get; }

        internal AliveAudioData(TEnum soundType, AudioSource audioSource)
        {
            SoundType = soundType;
            AudioSource = audioSource;
        }

        public bool Equals(AliveAudioData<TEnum> other)
        {
            var isTypeEqual = UnsafeEnumConverter<TEnum>.ToInt32(SoundType)
                .Equals(UnsafeEnumConverter<TEnum>.ToInt32(other.SoundType));

            var isSourceEqual = AudioSource == other.AudioSource;

            return isTypeEqual && isSourceEqual;
        }

        public override bool Equals(object obj) => obj is AliveAudioData<TEnum> other && Equals(other);

        public override int GetHashCode() => AudioSource.GetHashCode();
    }
}