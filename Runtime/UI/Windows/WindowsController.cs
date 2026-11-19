using System;
using System.Collections.Generic;
using System.Threading;
using CustomUtils.Runtime.AddressableSystem;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Extensions.Observables;
using CustomUtils.Runtime.UI.Windows.Windows.Base;
using CustomUtils.Runtime.UI.Windows.Windows.Parameterized;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using VContainer;

namespace CustomUtils.Runtime.UI.Windows
{
    /// <summary>
    /// Keeps one stack of open windows. A window with <see cref="WindowBase.ClearsStack"/> replaces the whole stack;
    /// any other window opens on top. A window is covered while any window above it has a covering
    /// <see cref="WindowMode"/>.
    /// </summary>
    [PublicAPI]
    public class WindowsController : MonoBehaviour
    {
        [SerializeField] private List<AssetReferenceT<GameObject>> _windowReferences;
        [SerializeField] private List<LazyWindowReference> _lazyWindowReferences;

        /// <summary>
        /// The window on top of the stack, or null when the stack is empty.
        /// </summary>
        public ReadOnlyReactiveProperty<WindowBase> TopWindow => _stack.TopWindow;

        /// <summary>True while any window is open above the bottom window of the stack.</summary>
        public ReadOnlyReactiveProperty<bool> IsOverlaid => _stack.IsOverlaid;

        /// <summary>The topmost open window with a covering <see cref="WindowMode"/>, or null.</summary>
        public ReadOnlyReactiveProperty<WindowBase> CoveringWindow => _stack.CoveringWindow;

        /// <summary>
        /// Fires when a window is opened, before its show plays; a held bottom window fires once
        /// <see cref="ShowBottomWindowAsync"/> releases it.
        /// </summary>
        public Observable<WindowBase> OnWindowOpened => _stack.OnWindowOpened;

        private readonly WindowStack _stack = new();

        private StackTransition _transition;
        private WindowLoader _windowLoader;

        [Inject]
        /// <summary>
        /// Receives the dependencies from VContainer.
        /// </summary>
        public void Inject(IObjectResolver objectResolver, IAddressablesLoader addressablesLoader)
        {
            _transition = new StackTransition(_stack, destroyCancellationToken);
            _windowLoader = new WindowLoader(addressablesLoader, objectResolver, destroyCancellationToken, transform);
            _windowLoader.OnCreated.SubscribeUntilDestroy(this, static (window, self) => self.SetUpWindow(window));
        }

        /// <summary>
        /// Loads and creates every non-lazy window. Call it once before opening windows.
        /// </summary>
        public virtual async UniTask InitializeAsync(CancellationToken token = default)
        {
            var sourceWithDestroy = token.CreateLinkedTokenSourceWithDestroy(this);
            await _windowLoader.LoadAsync(_windowReferences, _lazyWindowReferences, sourceWithDestroy.Token);
        }

        /// <summary>
        /// Plays the bottom window's show. Until this call, the bottom window of a controller with an initial window
        /// shows only its background, so windows can open behind a loading screen.
        /// </summary>
        public async UniTask ShowBottomWindowAsync()
        {
            var bottomWindow = _stack.ReleaseBottom();
            if (!bottomWindow)
                return;

            await _transition.ShowBottomAsync(bottomWindow);
        }

        /// <summary>
        /// Covers every window of this controller while another controller, drawn above it, has a covering window open.
        /// </summary>
        public void BindExternalCover(ReadOnlyReactiveProperty<WindowBase> coveringWindow)
        {
            _transition.BindExternalCover(coveringWindow);
        }

        /// <summary>
        /// Plays every open window's show backwards at blend speed, together with its hide pose. Their backgrounds
        /// stay on screen until the next window clears the stack.
        /// </summary>
        public async UniTask HideAllAsync(CancellationToken token)
        {
            await _transition.HideAllAsync(token);
        }

        /// <summary>
        /// Closes every window at once, without animations. Call it after <see cref="HideAllAsync"/> once the windows
        /// are off screen.
        /// </summary>
        public void CloseAll()
        {
            _transition.ClearFocus();

            var closingWindows = _stack.Clear();
            foreach (var window in closingWindows)
                window.HideImmediately();
        }

        /// <summary>
        /// Opens the window of type <typeparamref name="TWindow"/>, loading it first when it is lazy. A window with
        /// <see cref="WindowBase.ClearsStack"/> replaces the stack; any other opens on top. Completes when the
        /// transition has finished.
        /// </summary>
        public async UniTask<WindowBase> Open<TWindow>(CancellationToken token = default)
            where TWindow : WindowBase
        {
            var window = await _windowLoader.GetAsync<TWindow>();
            return await OpenAsync(window, token);
        }

        /// <summary>
        /// Passes <paramref name="parameters"/> to the window, then opens it like <see cref="Open{TWindow}"/>.
        /// </summary>
        public async UniTask<WindowBase> Open<TParameterizedWindow, TParameters>(
            TParameters parameters,
            CancellationToken token = default)
            where TParameterizedWindow : ParameterizedWindowBase<TParameters>
        {
            var window = await _windowLoader.GetAsync<TParameterizedWindow>();
            window.SetParameters(parameters);
            return await OpenAsync(window, token);
        }

        /// <summary>
        /// Closes the window of type <typeparamref name="TWindow"/> when it is loaded. Completes once it has closed.
        /// </summary>
        public async UniTask CloseAsync<TWindow>(CancellationToken token = default) where TWindow : WindowBase
        {
            if (!_windowLoader.TryGetLoad(typeof(TWindow), out var load))
                return;

            var window = await load;
            await window.HideAsync(token);
        }

        /// <summary>
        /// Opens <typeparamref name="TWindow"/> whenever <paramref name="component"/> is clicked. Clicks while it
        /// opens are ignored.
        /// </summary>
        public void BindOpen<TWindow>(UIBehaviour component) where TWindow : WindowBase
        {
            component.OnPointerClickAsObservable()
                .SubscribeAwait(
                    this,
                    static async (_, self, token) => await self.Open<TWindow>(token),
                    AwaitOperation.Drop)
                .RegisterTo(component.destroyCancellationToken);
        }

        private void SetUpWindow(WindowBase window)
        {
            window.OnHiding
                .SubscribeSelf(this, static (hidingWindow, self) => self.HandleHiding(hidingWindow))
                .RegisterTo(destroyCancellationToken);

            window.HideImmediately(true);

            if (!window.InitialWindow)
                return;

            _stack.HoldInitial(window);
            window.transform.SetAsFirstSibling();
            window.ShowBackground();
        }

        private async UniTask<WindowBase> OpenAsync(WindowBase window, CancellationToken token)
        {
            if (window.ClearsStack)
                return await ReplaceStackAsync(window, token);

            return await PushAsync(window, token);
        }

        private async UniTask<WindowBase> ReplaceStackAsync(WindowBase window, CancellationToken token)
        {
            var isReopened = window == _stack.BottomWindow;
            var closingWindows = _stack.Replace(window);

            if (!isReopened)
                window.transform.SetAsLastSibling();

            await _transition.PlayAsync(window, closingWindows, token);

            window.transform.SetAsFirstSibling();
            return window;
        }

        private async UniTask<WindowBase> PushAsync(WindowBase window, CancellationToken token)
        {
            var coveredWindow = _stack.Push(window);
            if (coveredWindow)
                ArrangeOver(coveredWindow, window);

            await _transition.PlayAsync(window, Array.Empty<WindowBase>(), token);
            return window;
        }

        private void ArrangeOver(WindowBase coveredWindow, WindowBase window)
        {
            var isRevealedBySlide = coveredWindow.IsSlideOver && !window.IsSlideOver;
            if (!isRevealedBySlide)
            {
                if (!window.IsInFrontOf(coveredWindow))
                    window.transform.SetAsLastSibling();

                return;
            }

            if (window.IsInFrontOf(coveredWindow))
                window.transform.SetSiblingIndex(coveredWindow.transform.GetSiblingIndex());

            window.ShowBackground();
        }

        private void HandleHiding(WindowBase window)
        {
            if (!_stack.Remove(window))
                return;

            var closingWindows = new[] { window };
            _transition.PlayAsync(window, closingWindows, destroyCancellationToken).Forget();
        }

        private void OnDestroy()
        {
            _transition.Dispose();
            _windowLoader.Dispose();
            _stack.Dispose();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var reference in _lazyWindowReferences)
                reference.UpdateWindowType();
        }
#endif
    }
}