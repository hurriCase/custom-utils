using System;
using CustomUtils.Runtime.CustomTypes.Collections;
using UnityEngine;

namespace CustomUtils.Runtime.Audio
{
    /// <inheritdoc />
    /// <summary>
    /// Audio database that stores and retrieves sound and music containers
    /// </summary>
    /// <typeparam name="TMusicType">Music enum type</typeparam>
    /// <typeparam name="TSoundType">Sound enum type</typeparam>
    public abstract class AudioDatabaseGeneric<TMusicType, TSoundType> : ScriptableObject
        where TMusicType : unmanaged, Enum
        where TSoundType : unmanaged, Enum
    {
        /// <summary>
        /// Collection of sound containers configured for this database
        /// </summary>
        [field: SerializeField] public EnumArray<TSoundType, SoundContainer> SoundContainers { get; private set; }

        /// <summary>
        /// Collection of music containers configured for this database
        /// </summary>
        [field: SerializeField] public EnumArray<TMusicType, AudioData> MusicContainers { get; private set; }
    }
}