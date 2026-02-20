using System;
using System.Collections.Generic;
using CustomUtils.Runtime.Extensions;
using JetBrains.Annotations;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CustomUtils.Runtime.UI.CustomComponents
{
    [PublicAPI]
    public sealed class AdaptiveTextMeshProUGUI : TextMeshProUGUI, ILayoutSelfController
    {
        [field: SerializeField] public DimensionType StaticDimensionType { get; private set; }
        [field: SerializeField] public float BaseFontSize { get; private set; }
        [field: SerializeField] public float StaticReferenceSize { get; private set; }
        [field: SerializeField] public bool ExpandToFitText { get; private set; }

        private readonly Subject<string> _textChangedSubject = new();
        private readonly HashSet<ITextSizeNotifiable> _sizeNotifiable = new();

        private DrivenRectTransformTracker _tracker;
        private Vector2 _lastSize;
        private float _lastPreferredHeight;
        private float _lastPreferredWidth;

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            _tracker.Clear();
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            _textChangedSubject?.Dispose();
            _sizeNotifiable.Clear();
            base.OnDestroy();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetTextFont();
        }
#endif

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            SetTextFont();
        }

        public override void SetVerticesDirty()
        {
            ExpandContainerIfNeeded();

            base.SetVerticesDirty();

            HandleSizeChange();
        }

        /// <summary>
        /// Registers an object to be notified when the text size changes.
        /// </summary>
        /// <param name="notifiable">An object implementing the <see cref="ITextSizeNotifiable"/> interface,
        /// which will receive notifications when the text size changes.</param>
        public void RegisterSizeNotifiable(ITextSizeNotifiable notifiable)
        {
            _sizeNotifiable.Add(notifiable);
        }

        /// <summary>
        /// Unregisters an object so that it no longer receives notifications when the text size changes.
        /// </summary>
        /// <param name="notifiable">An object implementing the <see cref="ITextSizeNotifiable"/> interface,
        /// which will be removed from receiving text size change notifications.</param>
        public void UnregisterSizeNotifiable(ITextSizeNotifiable notifiable)
        {
            _sizeNotifiable.Remove(notifiable);
        }

        public void SetLayoutHorizontal() { }

        public void SetLayoutVertical() { }

        private void SetDirty()
        {
            if (IsActive() is false)
                return;

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }

        private void SetTextFont()
        {
            if (StaticReferenceSize == 0 || BaseFontSize == 0 || StaticDimensionType == DimensionType.None)
                return;

            var scaleFactor = StaticDimensionType switch
            {
                DimensionType.Width => rectTransform.rect.width / StaticReferenceSize,
                DimensionType.Height => rectTransform.rect.height / StaticReferenceSize,
                _ => 0
            };

            if (scaleFactor.IsReasonable() is false)
                return;

            fontSize = BaseFontSize * scaleFactor;
        }

        private void ExpandContainerIfNeeded()
        {
            if (ExpandToFitText is false || StaticDimensionType == DimensionType.None)
            {
                _tracker.Clear();
                return;
            }

            ForceMeshUpdate();

            switch (StaticDimensionType)
            {
                case DimensionType.Width:
                    if (Mathf.Approximately(_lastPreferredHeight, preferredHeight)
                        || preferredHeight.IsReasonable() is false)
                        return;

                    _tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaY);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);
                    _lastPreferredHeight = preferredHeight;
                    break;

                case DimensionType.Height:
                    if (Mathf.Approximately(_lastPreferredWidth, preferredWidth)
                        || preferredWidth.IsReasonable() is false)
                        return;

                    _tracker.Add(this, rectTransform, DrivenTransformProperties.SizeDeltaX);
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth);
                    _lastPreferredWidth = preferredWidth;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HandleSizeChange()
        {
            var currentSize = rectTransform.rect.size;
            if (_lastSize == currentSize)
                return;

            _lastSize = currentSize;
            NotifySizeChange();
        }

        private void NotifySizeChange()
        {
            foreach (var notifiable in _sizeNotifiable)
                notifiable?.OnTextSizeChanged();
        }
    }
}