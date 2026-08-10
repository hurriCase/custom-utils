using System;
using System.ComponentModel;
using System.Threading;
using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Windows.Windows.Base
{
    [PublicAPI]
    [Serializable]
    [RequireComponent(typeof(CanvasGroup))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class WindowBase : MonoBehaviour
    {
        [SerializeField, Self] protected CanvasGroup canvasGroup;

        public virtual void BaseInitialize() { }

        public virtual void Initialize() { }

        public abstract UniTask ShowAsync(CancellationToken token);
        public abstract UniTask HideAsync(CancellationToken token);

        public async UniTask ToggleVisibility(bool isVisible, CancellationToken token)
        {
            if (isVisible)
            {
                await ShowAsync(token);
                return;
            }

            await HideAsync(token);
        }

        public virtual void HideImmediately(bool isSilent = false)
        {
            canvasGroup.Hide();
        }
    }
}