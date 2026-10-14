using CustomUtils.Editor.Scripts.CustomEditorUtilities.Scopes;
using UnityEditor;
using Object = UnityEngine.Object;

namespace CustomUtils.Editor.Scripts.FigmaBridge.Clipboard
{
    internal abstract class FigmaClipboardMenuBase<TTarget, TData>
        where TTarget : Object
        where TData : FigmaClipboardData
    {
        protected abstract string ExpectedSource { get; }
        protected abstract string UndoName { get; }

        protected void Paste(MenuCommand menuCommand)
        {
            var target = GetTarget(menuCommand.context);

            if (!FigmaClipboardReader.TryRead<TData>(ExpectedSource, target.name, out var clipboardData))
                return;

            using (new RecordUndoScope(target, UndoName))
                Apply(target, clipboardData);
        }

        protected virtual TTarget GetTarget(Object context) => (TTarget)context;

        protected abstract void Apply(TTarget target, TData clipboardData);
    }
}
