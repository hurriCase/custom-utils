using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Extensions.Observables;
using CustomUtils.Runtime.UI.Windows.Windows.Base;
using Cysharp.Threading.Tasks;
using R3;

namespace CustomUtils.Runtime.UI.Windows
{
    internal sealed class StackTransition : IDisposable
    {
        private readonly WindowStack _stack;
        private readonly CancellationToken _lifetimeToken;

        private WindowBase _focusedWindow;
        private WindowBase _externalCoveringWindow;
        private CancellationTokenSource _externalCoverCancellation;

        internal StackTransition(WindowStack stack, CancellationToken lifetimeToken)
        {
            _stack = stack;
            _lifetimeToken = lifetimeToken;
        }

        internal async UniTask ShowBottomAsync(WindowBase bottomWindow)
        {
            if (bottomWindow.IsVisible)
                return;

            if (IsCovered(0))
            {
                bottomWindow.HideImmediately(true);
                return;
            }

            await bottomWindow.ShowAsync(_lifetimeToken);
            FocusTopWindow();
        }

        internal void BindExternalCover(ReadOnlyReactiveProperty<WindowBase> coveringWindow)
        {
            coveringWindow
                .SubscribeSelf(this, static (window, self) => self.UpdateExternalCover(window))
                .RegisterTo(_lifetimeToken);
        }

        internal async UniTask HideAllAsync(CancellationToken token)
        {
            ClearFocus();

            var hideTasks = new List<UniTask>();
            foreach (var window in _stack.Windows)
            {
                if (!window.IsVisible)
                    continue;

                var hideTask = window.BlendOutAsync(token);
                hideTasks.Add(hideTask);
            }

            await UniTask.WhenAll(hideTasks);
        }

        internal async UniTask PlayAsync(
            WindowBase sourceWindow,
            IReadOnlyList<WindowBase> closingWindows,
            CancellationToken token)
        {
            ClearFocus();

            var isBelowInstant = sourceWindow.IsSlideOver;
            var isUncoveredInFront = closingWindows.Count > 0 && sourceWindow.Mode == WindowMode.Blend;
            var frontTasks = new List<UniTask>();
            var showingWindows = new List<WindowBase>();
            var coveredWindows = new List<WindowBase>();

            if (sourceWindow == _externalCoveringWindow)
            {
                var shownTask = sourceWindow.OnShown.FirstAsync(token).AsUniTask();
                frontTasks.Add(shownTask);
            }

            var windows = _stack.Windows;
            for (var i = 0; i < windows.Count; i++)
            {
                var window = windows[i];
                var isCovered = IsCovered(i);
                if (isCovered && window.IsVisible)
                {
                    coveredWindows.Add(window);
                    continue;
                }

                if (isCovered || window.IsVisible)
                    continue;

                if (i == 0 && _stack.IsBottomShowHeld)
                {
                    window.ShowBackground();
                    continue;
                }

                if (isBelowInstant && window != sourceWindow)
                {
                    window.Reveal();
                    continue;
                }

                if (isUncoveredInFront)
                    window.transform.SetAsLastSibling();

                showingWindows.Add(window);
            }

            HoldSharedElements(showingWindows, coveredWindows, closingWindows);

            foreach (var window in showingWindows)
            {
                var showTask = ShowOnOwnLifetimeAsync(window, token);
                frontTasks.Add(showTask);
            }

            var frontShown = new UniTaskCompletionSource();
            var hideTasks = new List<UniTask>();
            foreach (var window in coveredWindows)
            {
                var hideTask = HideWindowAsync(window, false, isBelowInstant, frontShown.Task, token);
                hideTasks.Add(hideTask);
            }

            foreach (var window in closingWindows)
            {
                var hideTask = HideWindowAsync(window, true, _stack.IsBottomShowHeld, frontShown.Task, token);
                hideTasks.Add(hideTask);
            }

            try
            {
                await UniTask.WhenAll(frontTasks);
            }
            finally
            {
                frontShown.TrySetResult();
            }

            FocusTopWindow();

            await UniTask.WhenAll(hideTasks);
        }

        internal void ClearFocus()
        {
            if (_focusedWindow)
                _focusedWindow.SetFocused(false);

            _focusedWindow = null;
        }

        public void Dispose()
        {
            CancellationExtensions.DisposeToken(ref _externalCoverCancellation);
        }

        private void UpdateExternalCover(WindowBase coveringWindow)
        {
            var uncoveringWindow = _externalCoveringWindow;
            _externalCoveringWindow = coveringWindow;

            if (coveringWindow && !uncoveringWindow)
            {
                var coverToken = CancellationExtensions.GetFreshToken(ref _externalCoverCancellation);
                var coverTask = PlayAsync(coveringWindow, Array.Empty<WindowBase>(), coverToken);
                coverTask.SuppressCancellationThrow().Forget();
                return;
            }

            if (coveringWindow || !uncoveringWindow)
                return;

            CancellationExtensions.DisposeToken(ref _externalCoverCancellation);

            if (!uncoveringWindow.IsSlideOver)
            {
                ShowUncoveredBackgrounds();
                uncoveringWindow.FadeOutBackground();
            }

            PlayAsync(uncoveringWindow, Array.Empty<WindowBase>(), _lifetimeToken).Forget();
        }

        private async UniTask HideWindowAsync(
            WindowBase window,
            bool isClosing,
            bool isInstant,
            UniTask frontShown,
            CancellationToken token)
        {
            await window.StartHideAsync(isClosing, isInstant, token);
            await frontShown;

            if (token.IsCancellationRequested)
                return;

            window.CompleteHide();
        }

        private void HoldSharedElements(
            List<WindowBase> showingWindows,
            List<WindowBase> coveredWindows,
            IReadOnlyList<WindowBase> closingWindows)
        {
            var hidingWindows = new List<WindowBase>(coveredWindows);
            foreach (var window in closingWindows)
            {
                if (window.IsVisible)
                    hidingWindows.Add(window);
            }

            var sharedKeys = CollectSharedElementKeys(showingWindows);
            var hidingKeys = CollectSharedElementKeys(hidingWindows);
            sharedKeys.IntersectWith(hidingKeys);

            if (sharedKeys.Count == 0)
                return;

            foreach (var window in showingWindows)
                window.HoldSharedElements(sharedKeys);

            foreach (var window in hidingWindows)
                window.HoldSharedElements(sharedKeys);
        }

        private HashSet<string> CollectSharedElementKeys(List<WindowBase> windows)
        {
            var keys = new HashSet<string>();
            foreach (var window in windows)
            {
                foreach (var element in window.SharedElements)
                {
                    if (element.gameObject.activeInHierarchy)
                        keys.Add(element.Key);
                }
            }

            return keys;
        }

        private void ShowUncoveredBackgrounds()
        {
            var windows = _stack.Windows;
            for (var i = 0; i < windows.Count; i++)
            {
                var window = windows[i];
                if (!IsCovered(i) && !window.IsVisible)
                    window.ShowBackground();
            }
        }

        private bool IsCovered(int index)
            => _externalCoveringWindow || _stack.HasCoveringAbove(index);

        private UniTask ShowOnOwnLifetimeAsync(WindowBase window, CancellationToken token)
            => window.ShowAsync(window.destroyCancellationToken).AttachExternalCancellation(token);

        private void FocusTopWindow()
        {
            var topWindow = _stack.TopWindow.CurrentValue;
            if (_externalCoveringWindow || topWindow == _focusedWindow)
                return;

            ClearFocus();
            _focusedWindow = topWindow;

            if (topWindow)
                topWindow.SetFocused(true);
        }
    }
}