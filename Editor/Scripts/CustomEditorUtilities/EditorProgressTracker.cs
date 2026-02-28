using System;
using System.Threading;
using CustomUtils.Runtime.Helpers;
using Cysharp.Threading.Tasks;
using PrimeTween;
using UnityEngine;

// ReSharper disable MemberCanBeInternal
namespace CustomUtils.Editor.Scripts.CustomEditorUtilities
{
    /// <inheritdoc />
    /// <summary>
    /// Handles progress tracking and display for editor windows and custom inspectors.
    /// </summary>
    /// <remarks>
    /// Provides functionality for tracking operation progress and displaying visual feedback
    /// through the Unity Editor interface. This class can be used by both custom editor windows
    /// and custom inspectors to provide consistent progress indication for long-running operations.
    /// Implements IDisposable to properly clean up resources when no longer needed.
    /// </remarks>
    public sealed class EditorProgressTracker : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether an operation is currently in progress.
        /// </summary>
        /// <value>
        /// <c>true</c> if an operation is in progress; otherwise, <c>false</c>.
        /// </value>
        public bool HasOperation { get; private set; }

        private readonly float _completedProgressShowingDuration;

        private string _currentOperation;
        private string _operationInfo;
        private float _operationProgress;

        private CancellationTokenSource _cancellationSource;
        private Tween _cancellationTween;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditorProgressTracker"/> class.
        /// </summary>
        /// <param name="completedProgressShowingDuration">The duration in seconds to display the completed progress bar.</param>
        /// <remarks>
        /// The default duration is 10 seconds.
        /// </remarks>
        public EditorProgressTracker(float completedProgressShowingDuration = 10f)
        {
            _completedProgressShowingDuration = completedProgressShowingDuration;
        }

        /// <summary>
        /// Updates the progress information and triggers a repaint of the owner window or inspector.
        /// </summary>
        /// <param name="operation">Name of the operation to display.</param>
        /// <param name="info">Additional information about the operation to display.</param>
        /// <param name="progress">Progress value between 0 and 1 indicating completion percentage.</param>
        /// <remarks>
        /// This method should be called whenever the progress of an operation changes to
        /// update the visual representation in the editor.
        /// </remarks>
        public void UpdateProgress(string operation, string info, float progress)
        {
            _currentOperation = operation;
            _operationInfo = info;
            _operationProgress = progress;
            HasOperation = _operationProgress < 1f;
        }

        /// <summary>
        /// Completes the current operation and resets progress tracking after a delay.
        /// </summary>
        /// <param name="completeInfo">Optional message to display upon completion. Defaults to "Completed" if not specified.</param>
        /// <remarks>
        /// This method sets the progress to 100% and displays a completion message.
        /// After 10 seconds, the progress bar will automatically be hidden.
        /// </remarks>
        public void CompleteOperation(string completeInfo = null)
        {
            UpdateProgress(_currentOperation,
                string.IsNullOrWhiteSpace(completeInfo) ? "Completed" : completeInfo,
                1.0f);

            _cancellationTween.Complete();
            _cancellationTween = Tween.Delay(this, _completedProgressShowingDuration, static tracker =>
            {
                tracker.HasOperation = false;
                tracker._currentOperation = string.Empty;
                tracker._operationInfo = string.Empty;
                tracker._operationProgress = 0f;
            });
        }

        /// <summary>
        /// Draws the progress bar in the editor if an operation is in progress.
        /// </summary>
        /// <remarks>
        /// This method should be called from an editor window's OnGUI method or a custom inspector's
        /// OnInspectorGUI method to display the progress bar. If no operation is in progress,
        /// nothing will be drawn. A "Close" button is provided to dismiss the progress bar manually.
        /// </remarks>
        public void DrawProgressIfNeeded()
        {
            if (!HasOperation && !_cancellationTween.isAlive)
                return;

            EditorVisualControls.DrawProgressBar(_currentOperation, _operationInfo, _operationProgress);

            if (_cancellationTween.isAlive && EditorVisualControls.Button("Ok"))
                _cancellationTween.Complete();

            if (!_cancellationTween.isAlive && EditorVisualControls.Button("Cancel"))
                CancellationSourceHelper.CancelAndDisposeCancellationTokenSource(ref _cancellationSource);
        }

        /// <summary>
        /// Creates and manages a progress-tracked asynchronous operation that returns a result.
        /// </summary>
        /// <typeparam name="T">The type of result returned by the operation.</typeparam>
        /// <param name="progressTitle">The title to display for the operation.</param>
        /// <param name="progressInfo">Additional information to display about the operation.</param>
        /// <param name="progressFunc">A function that performs the operation, accepting a progress reporter and cancellation token.</param>
        /// <returns>A UniTask representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method handles the entire lifecycle of a progress-tracked operation, including setup,
        /// progress reporting, cancellation support, and cleanup. If the operation returns a string,
        /// it will be used as the completion message.
        /// </remarks>
        public async UniTask CreateProgressAsync<T>(string progressTitle, string progressInfo,
            Func<IProgress<float>, CancellationToken, UniTask<T>> progressFunc)
        {
            UpdateProgress(progressTitle, progressInfo, 0f);

            var progress = Progress.CreateOnlyValueChanged<float>(value =>
            {
                UpdateProgress(
                    progressTitle,
                    progressInfo,
                    value);
            });

            try
            {
                _cancellationTween.Complete();
                CancellationSourceHelper.SetNewCancellationTokenSource(ref _cancellationSource);

                var result = await progressFunc(progress, _cancellationSource.Token);

                if (result is string completeMessage)
                    CompleteOperation(completeMessage);
                else
                    CompleteOperation();
            }
            catch (Exception ex)
            {
                UpdateProgress("Operation Error", ex.Message, 1f);
                Debug.LogError($"[PackageWindowBase::CreateProgressAsync] Error during progress action: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates and manages a progress-tracked asynchronous operation that doesn't return a result.
        /// </summary>
        /// <param name="progressTitle">The title to display for the operation.</param>
        /// <param name="progressInfo">Additional information to display about the operation.</param>
        /// <param name="progressFunc">A function that performs the operation, accepting a progress reporter and cancellation token.</param>
        /// <returns>A UniTask representing the asynchronous operation.</returns>
        public async UniTask CreateProgressAsync(string progressTitle, string progressInfo,
            Func<IProgress<float>, CancellationToken, UniTask> progressFunc)
        {
            await CreateProgressAsync<object>(progressTitle, progressInfo, async (progress, token) =>
            {
                await progressFunc(progress, token);
                return null;
            });
        }

        /// <inheritdoc />
        /// <summary>
        /// Disposes of resources used by the EditorProgressTracker.
        /// </summary>
        public void Dispose()
        {
            CancellationSourceHelper.CancelAndDisposeCancellationTokenSource(ref _cancellationSource);
        }
    }
}