using System;
using System.ComponentModel;
using System.Threading;
using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.Windows.Windows.Base
{
    [PublicAPI]
    [Serializable]
    [RequireComponent(typeof(CanvasGroup), typeof(Canvas), typeof(GraphicRaycaster))]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class WindowBase : MonoBehaviour
    {
        [SerializeField, Self] protected CanvasGroup canvasGroup;
        [SerializeField, Self] private Canvas _canvas;

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
            SetVisible(false);
        }

        protected void SetVisible(bool isVisible)
        {
            _canvas.enabled = isVisible;
            canvasGroup.SetVisible(isVisible);
        }
    }
}