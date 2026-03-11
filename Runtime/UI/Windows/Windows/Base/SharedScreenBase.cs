using System.ComponentModel;
using CustomUtils.Runtime.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Windows.Windows.Base
{
    [PublicAPI]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class SharedScreenBase : WindowBase
    {
        [field: SerializeField] internal bool InitialWindow { get; private set; }

        public override UniTask ShowAsync()
        {
            canvasGroup.Show();
            return UniTask.CompletedTask;
        }

        public override UniTask HideAsync()
        {
            canvasGroup.Hide();
            return UniTask.CompletedTask;
        }
    }
}