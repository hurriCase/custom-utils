#if PRIMETWEEN_PRO_INSTALLED
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CustomUtils.Runtime.UI.SequentialShow
{
    internal readonly struct SequentialShowItem : IComparable<SequentialShowItem>
    {
        internal const float RowTolerance = 1f;

        internal Vector2 Position { get; }
        internal bool IsInViewport { get; }
        internal int Step { get; }

        private readonly TweenSequentialShowItem _item;
        private readonly SequentialShowAnimation _nestedAnimation;

        internal SequentialShowItem(
            TweenSequentialShowItem item,
            SequentialShowAnimation nestedAnimation,
            Vector2 position,
            bool isInViewport,
            int step)
        {
            _item = item;
            _nestedAnimation = nestedAnimation;
            Position = position;
            IsInViewport = isInViewport;
            Step = step;
        }

        internal SequentialShowItem WithStep(int step)
            => new(_item, _nestedAnimation, Position, IsInViewport, step);

        internal void Reset()
        {
            if (_nestedAnimation)
            {
                _nestedAnimation.ResetItems();
                return;
            }

            _item.ResetToStart();
        }

        internal void Show(float timeScale)
        {
            if (_nestedAnimation)
            {
                _nestedAnimation.Play(0f);
                _nestedAnimation.SpeedUp(timeScale);
                return;
            }

            _item.Show(timeScale);
        }

        internal void ShowInstantly()
        {
            if (_nestedAnimation)
            {
                _nestedAnimation.ShowInstantly();
                return;
            }

            _item.ShowInstantly();
        }

        internal void Hide(bool isReversingShow)
        {
            if (_nestedAnimation)
            {
                _nestedAnimation.Hide(false);
                return;
            }

            _item.Hide(isReversingShow);
        }

        internal void Return(bool isReversingShow)
        {
            if (_nestedAnimation)
            {
                _nestedAnimation.Return();
                return;
            }

            _item.Return(isReversingShow);
        }

        internal void SpeedUp(float timeScale)
        {
            if (_nestedAnimation)
            {
                _nestedAnimation.SpeedUp(timeScale);
                return;
            }

            _item.SpeedUp(timeScale);
        }

        internal UniTask WaitUntilShownAsync(CancellationToken token)
            => _nestedAnimation ? UniTask.CompletedTask : _item.WaitUntilShownAsync(token);

        public int CompareTo(SequentialShowItem other)
        {
            var heightDifference = other.Position.y - Position.y;
            if (Mathf.Abs(heightDifference) > RowTolerance)
                return heightDifference > 0f ? 1 : -1;

            return Position.x.CompareTo(other.Position.x);
        }
    }
}
#endif
