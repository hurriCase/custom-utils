#if CUSTOM_LOCALIZATION
using System;
using UnityEngine;

namespace CustomUtils.Runtime.Localization
{
    [Serializable]
    public struct LocalizationKey
    {
        [field: SerializeField] public string GUID { get; private set; }

        public bool IsValid => !string.IsNullOrEmpty(GUID);
    }
}
#endif