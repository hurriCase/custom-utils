using System;
using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.UI.Windows.Windows.Base;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CustomUtils.Runtime.UI.Windows
{
    /// <summary>
    /// A window that is loaded when it is first opened and unloaded once it is closed.
    /// </summary>
    [PublicAPI]
    [Serializable]
    public sealed class LazyWindowReference
    {
        [field: SerializeField] public AssetReferenceT<GameObject> Prefab { get; private set; }

        // we can't know the window type before load, so we store it
        [SerializeField, InspectorReadOnly] private string _windowTypeName;

        internal Type WindowType => Type.GetType(_windowTypeName);

#if UNITY_EDITOR
        internal void UpdateWindowType()
        {
            var prefab = Prefab?.editorAsset;
            var window = prefab ? prefab.GetComponent<WindowBase>() : null;
            _windowTypeName = window ? window.GetType().AssemblyQualifiedName : string.Empty;
        }
#endif
    }
}
