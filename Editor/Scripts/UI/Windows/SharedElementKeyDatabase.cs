using System.Collections.Generic;
using CustomUtils.Runtime.AssetLoader;
using CustomUtils.Runtime.CustomTypes.Singletons;
using UnityEngine;

namespace CustomUtils.Editor.Scripts.UI.Windows
{
    [Resource(name: nameof(SharedElementKeyDatabase), isEditorResource: true)]
    internal sealed class SharedElementKeyDatabase : SingletonScriptableObject<SharedElementKeyDatabase>
    {
        [field: SerializeField] internal List<string> Keys { get; private set; }
    }
}
