#if UNITY_LOCALIZATION_INSTALLED
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization;

namespace CustomUtils.Runtime.LocalizationProviders
{
    [Serializable]
    internal sealed class UnityLocalization : ILocalization
    {
        [SerializeField] private LocalizedString _localizationKey;

        public async UniTask<string> GetLocalizationAsync(CancellationToken token)
            => await _localizationKey.GetLocalizedStringAsync().ToUniTask(cancellationToken: token);

        public string GetLocalization() => _localizationKey.GetLocalizedString();
    }
}
#endif