using System;
using System.Threading;
using CustomUtils.Runtime.Storage;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.Audio
{
    /// <summary>
    /// Base class for handling audio playback including music and sound effects
    /// </summary>
    /// <typeparam name="TMusicType">Enum type for music categories</typeparam>
    /// <typeparam name="TSoundType">Enum type for sound effect categories</typeparam>
    [PublicAPI]
    public interface IAudioHandlerGeneric<in TMusicType, in TSoundType>
        where TMusicType : unmanaged, Enum
        where TSoundType : unmanaged, Enum
    {
        /// <summary>
        /// Gets the reactive property for music volume control.
        /// Value range is typically 0.0 to 1.0, where 0 is muted and 1 is full volume.
        /// Changes to this property will automatically persist to storage and update all active music sources.
        /// </summary>
        PersistentReactiveProperty<float> MusicVolume { get; }

        /// <summary>
        /// Gets the reactive property for sound effects volume control.
        /// Value range is typically 0.0 to 1.0, where 0 is muted and 1 is full volume.
        /// Changes to this property will automatically persist to storage and update all active sound sources.
        /// </summary>
        PersistentReactiveProperty<float> SoundVolume { get; }

        /// <summary>
        /// Initializes the audio handler with pooled audio sources and volume subscriptions
        /// </summary>
        UniTask InitAsync(
            float defaultMusicVolume = 1f,
            float defaultSoundVolume = 1f,
            CancellationToken token = default);

        /// <summary>
        /// Plays a sound with the specified parameters
        /// </summary>
        /// <param name="soundType">Type of sound to play</param>
        /// <param name="volumeModifier">Volume multiplier</param>
        /// <param name="pitchModifier">Pitch multiplier</param>
        /// <returns>AudioSource playing the sound, or null if failed</returns>
        AudioSource PlaySound(TSoundType soundType, float volumeModifier = 1, float pitchModifier = 1);

        /// <summary>
        /// Plays an AudioClip directly using a dedicated clip source.
        /// This method bypasses the sound database and plays the provided clip immediately.
        /// Only one clip can be played at a time - calling this method will replace any currently playing clip.
        /// </summary>
        /// <param name="soundType">The AudioClip to play directly</param>
        /// <param name="volumeModifier">Volume multiplier applied to the base sound volume</param>
        /// <param name="pitchModifier">Pitch multiplier for the playback speed</param>
        /// <returns>AudioSource playing the clip, or null if the clip is null</returns>
        AudioSource PlayClip(AudioClip soundType, float volumeModifier = 1, float pitchModifier = 1);

        /// <summary>
        /// Stops the currently playing clip that was started with PlayClip.
        /// Has no effect if no clip is currently playing.
        /// </summary>
        void StopClip();

        /// <summary>
        /// Stops all instances of the specified sound type
        /// </summary>
        /// <param name="soundType">Type of sound to stop</param>
        void StopSound(TSoundType soundType);

        /// <summary>
        /// Stop music
        /// </summary>
        void StopMusic();

        /// <summary>
        /// Plays a one-shot sound that doesn't need to be tracked or stopped
        /// </summary>
        /// <param name="soundType">Type of sound to play</param>
        /// <param name="volumeModifier">Volume multiplier</param>
        /// <param name="pitchModifier">Pitch multiplier</param>
        void PlayOneShotSound(TSoundType soundType, float volumeModifier = 1, float pitchModifier = 1);

        /// <summary>
        /// Plays music of the specified type
        /// </summary>
        /// <param name="musicType">Type of music to play</param>
        /// <returns>AudioSource playing the music, or null if failed</returns>
        AudioSource PlayMusic(TMusicType musicType);

        /// <summary>
        /// Plays music from the specified audio data
        /// </summary>
        /// <param name="data">Audio data to play</param>
        /// <returns>AudioSource playing the music, or null if failed</returns>
        AudioSource PlayMusic(AudioData data);
    }
}