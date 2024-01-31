using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace match3.special
{
    public class SpecialButtonView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _image;

        [Header("Special Settings")]
        [SerializeField][Range(0, 30)] private int _warmupSeconds = 10;
        [SerializeField][Range(0, 30)] private int _durationSeconds = 10;

        // Click
        private Action _onClickCallback;

        private Tween _warmupDelayedCall = null;
        private Tween _durationDelayedCall = null;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }


        // ========================== Click Listener ============================

        public void AddOnClickListener(Action onClick)
        {
            _onClickCallback = onClick;
        }

        private void OnClick()
        {
            // Disable button
            _button.interactable = false;

            KillTweens();

            // Activation animation
            _warmupDelayedCall = AnimateButtonFillAmount(1, 0, _durationSeconds);
            _warmupDelayedCall.onComplete = () =>
            {

                // Warmup Animation
                _durationDelayedCall = AnimateButtonFillAmount(0, 1, _warmupSeconds);
                _durationDelayedCall.onComplete = () =>
                {
                    _button.interactable = true;
                };
            };

            // Callback
            _onClickCallback?.Invoke();
        }

        private Tween AnimateButtonFillAmount(float from, float to, float duration)
        {
            float fillAmount = from;

            Tween tween = DOTween.To(() => fillAmount, x => fillAmount = x, to, duration)
                .OnUpdate(() =>
                {
                    _image.fillAmount = fillAmount;
                });

            return tween;
        }

        private void KillTweens()
        {
            _warmupDelayedCall?.Kill();
            _durationDelayedCall?.Kill();
        }

        private void OnDestroy()
        {
            KillTweens();
        }

    }
}
