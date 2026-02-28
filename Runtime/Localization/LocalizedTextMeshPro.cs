#if CUSTOM_LOCALIZATION
using CustomUtils.Runtime.Attributes;
using CustomUtils.Runtime.Extensions.Observables;
using TMPro;
using UnityEngine;

namespace CustomUtils.Runtime.Localization
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    internal sealed class LocalizedTextMeshPro : MonoBehaviour
    {
        [field: SerializeField] internal LocalizationKey LocalizationKey { get; private set; }
        [field: SerializeField, Self] internal TextMeshProUGUI Text { get; private set; }

        private void Start()
        {
            LocalizationController.Language.SubscribeUntilDestroy(this, static self => self.Localize());
        }

        private void Localize()
        {
            if (!LocalizationKey.IsValid)
            {
                Debug.LogWarning("[LocalizedTextMeshPro::Localize] Localization key is invalid", gameObject);
                return;
            }

            Text.text = LocalizationController.Localize(LocalizationKey);
        }
    }
}
#endif