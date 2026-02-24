#if CUSTOM_LOCALIZATION
using System;
using System.Threading;
using CustomUtils.Runtime.Extensions;
using CustomUtils.Runtime.Localization;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CustomUtils.Runtime.LocalizationProviders
{
    [Serializable]
    internal sealed class CustomLocalization : ILocalization
    {
        [SerializeField] private LocalizationKey _localizationKey;

        public UniTask<string> GetLocalizationAsync(CancellationToken token)
            => UniTask.FromResult(_localizationKey.GetLocalization());

        public string GetLocalization() => _localizationKey.GetLocalization();
    }
}
#endif