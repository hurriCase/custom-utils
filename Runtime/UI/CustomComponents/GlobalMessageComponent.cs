using CustomUtils.Runtime.Attributes;
using JetBrains.Annotations;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents
{
    [PublicAPI]
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class GlobalMessageComponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _button;

        [SerializeField] private float _showingDuration = 2f;
        [SerializeField] private float _fadingDuration = 0.5f;
        [SerializeField] private bool _shouldAutoClose = true;

        [SerializeField, Self] private CanvasGroup _canvasGroup;

        private Tween _tween;

        private void Awake()
        {
            if (_button)
                _button.onClick.AddListener(() =>
                {
                    _tween.Stop();
                    HideMessage();
                });

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void ShowMessage(string message)
        {
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            _messageText.SetText(message);

            _tween.Stop();

            _tween = Tween.Alpha(
                _canvasGroup,
                endValue: 1f,
                _fadingDuration,
                useUnscaledTime: true,
                endDelay: _showingDuration
            ).OnComplete(this, static globalMessage =>
            {
                globalMessage._canvasGroup.alpha = 1f;

                if (globalMessage._shouldAutoClose)
                    globalMessage.HideMessage();
            });
        }

        public void HideMessage()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;

            _tween.Stop();

            _tween = Tween.Alpha(
                _canvasGroup,
                endValue: 0f,
                _fadingDuration,
                useUnscaledTime: true
            ).OnComplete(this, static globalMessage => globalMessage._canvasGroup.alpha = 0f);
        }

        private void OnDisable()
        {
            _tween.Stop();
        }
    }
}