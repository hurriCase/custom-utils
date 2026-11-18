using System;
using System.Collections.Generic;
using CustomUtils.Runtime.UI.Windows.Windows.Base;
using R3;

namespace CustomUtils.Runtime.UI.Windows
{
    internal sealed class WindowStack : IDisposable
    {
        internal ReadOnlyReactiveProperty<WindowBase> TopWindow => _topWindow;
        internal ReadOnlyReactiveProperty<bool> IsOverlaid => _isOverlaid;
        internal ReadOnlyReactiveProperty<WindowBase> CoveringWindow => _coveringWindow;
        internal Observable<WindowBase> OnWindowOpened => _windowOpened;
        internal IReadOnlyList<WindowBase> Windows => _windows;
        internal WindowBase BottomWindow => _windows.Count > 0 ? _windows[0] : null;
        internal bool IsBottomShowHeld { get; private set; }

        private readonly ReactiveProperty<WindowBase> _topWindow = new();
        private readonly ReactiveProperty<bool> _isOverlaid = new();
        private readonly ReactiveProperty<WindowBase> _coveringWindow = new();
        private readonly Subject<WindowBase> _windowOpened = new();
        private readonly List<WindowBase> _windows = new();

        internal void HoldInitial(WindowBase window)
        {
            IsBottomShowHeld = true;
            _windows.Insert(0, window);
            UpdateState();
        }

        internal WindowBase ReleaseBottom()
        {
            IsBottomShowHeld = false;

            var bottomWindow = BottomWindow;
            if (bottomWindow)
                _windowOpened.OnNext(bottomWindow);

            return bottomWindow;
        }

        internal List<WindowBase> Replace(WindowBase window)
        {
            var closingWindows = new List<WindowBase>(_windows);
            closingWindows.Remove(window);

            _windows.Clear();
            _windows.Add(window);
            UpdateState();

            if (!IsBottomShowHeld)
                _windowOpened.OnNext(window);

            return closingWindows;
        }

        internal WindowBase Push(WindowBase window)
        {
            _windows.Remove(window);

            var coveredWindow = _windows.Count > 0 ? _windows[^1] : null;
            _windows.Add(window);
            UpdateState();
            _windowOpened.OnNext(window);

            return coveredWindow;
        }

        internal List<WindowBase> Clear()
        {
            var closingWindows = new List<WindowBase>(_windows);

            _windows.Clear();
            UpdateState();

            return closingWindows;
        }

        internal bool HasCoveringAbove(int index)
        {
            var aboveIndex = index + 1;
            return _windows.FindIndex(aboveIndex, static window => window.IsCovering) >= 0;
        }

        internal bool Remove(WindowBase window)
        {
            if (!_windows.Remove(window))
                return false;

            UpdateState();
            return true;
        }

        public void Dispose()
        {
            _topWindow.Dispose();
            _isOverlaid.Dispose();
            _coveringWindow.Dispose();
            _windowOpened.Dispose();
        }

        private void UpdateState()
        {
            _coveringWindow.Value = _windows.FindLast(static window => window.IsCovering);
            _topWindow.Value = _windows.Count > 0 ? _windows[^1] : null;
            _isOverlaid.Value = _windows.Count > 1;
        }
    }
}