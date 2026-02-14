using System;
using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace CustomUtils.Runtime.UI.Windows
{
    [PublicAPI]
    [Serializable]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class WindowBase : MonoBehaviour
    {
        [SerializeField, Self] protected CanvasGroup canvasGroup;

        internal virtual void BaseInitialize() { }

        public virtual void Initialize() { }

        public abstract UniTask ShowAsync();
        public abstract UniTask HideAsync();

        public virtual void HideImmediately() => canvasGroup.Hide();
    }
}