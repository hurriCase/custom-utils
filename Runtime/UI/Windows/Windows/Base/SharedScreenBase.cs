using System.ComponentModel;
using System.Threading;
using CustomUtils.Runtime.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using R3;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Windows.Windows.Base
{
    [PublicAPI]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class SharedScreenBase : WindowBase
    {
        [field: SerializeField] internal bool InitialWindow { get; private set; }

        public Observable<Unit> OnShown => _shown;
        private readonly Subject<Unit> _shown = new();

        public Observable<Unit> OnHidden => _hidden;
        private readonly Subject<Unit> _hidden = new();

        public override UniTask ShowAsync(CancellationToken token)
        {
            canvasGroup.Show();
            _shown.OnNext(Unit.Default);
            return UniTask.CompletedTask;
        }

        public override UniTask HideAsync(CancellationToken token)
        {
            canvasGroup.Hide();
            _hidden.OnNext(Unit.Default);
            return UniTask.CompletedTask;
        }

        private void OnDestroy()
        {
            _shown.Dispose();
            _hidden.Dispose();
        }
    }
}