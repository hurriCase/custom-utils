using System.Threading;
using Cysharp.Threading.Tasks;

namespace CustomUtils.Runtime.LocalizationProviders
{
    internal interface ILocalization
    {
        UniTask<string> GetLocalizationAsync(CancellationToken token);
        string GetLocalization();
    }
}