using CustomUtils.Runtime.UI.CustomComponents.Locks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents.Selectables.Buttons
{
    /// <summary>
    /// Button that shows its locked look while it is not interactable.
    /// </summary>
    [PublicAPI]
    public class LockableButton : Button
    {
        /// <summary>
        /// Gets the locked look.
        /// </summary>
        [field: SerializeField] public LockView Lock { get; private set; }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            Lock.SetLocked(!interactable);
        }
    }
}
