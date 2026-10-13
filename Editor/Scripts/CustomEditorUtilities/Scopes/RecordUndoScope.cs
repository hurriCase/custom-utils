using System;
using JetBrains.Annotations;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CustomUtils.Editor.Scripts.CustomEditorUtilities.Scopes
{
    /// <inheritdoc />
    /// <summary>
    /// Records an undo step for the target and, when disposed, keeps its changes as prefab instance overrides.
    /// Wrap direct property changes made from editor code in it.
    /// </summary>
    [PublicAPI]
    public sealed class RecordUndoScope : IDisposable
    {
        private readonly Object _target;

        internal RecordUndoScope(Object target, string undoName)
        {
            _target = target;
            Undo.RecordObject(target, undoName);
        }

        /// <inheritdoc />
        /// <summary>
        /// Records the changes made inside the scope as prefab instance overrides.
        /// </summary>
        public void Dispose()
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(_target);
        }
    }
}
