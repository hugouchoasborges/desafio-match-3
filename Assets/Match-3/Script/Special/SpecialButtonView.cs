using DG.Tweening;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace match3.special
{
    public class SpecialButtonView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _image;
        [SerializeField] private Image _background;
        [SerializeField] private TMP_Text _text;

        [Header("Special Settings")]
        [SerializeField][Range(0, 30)] private int _warmupSeconds = 10;
        [SerializeField][Range(0, 30)] private int _durationSeconds = 10;
        [SerializeField] private Color _activatedColor = Color.green;
        [SerializeField] private Color _warmingUpColor = Color.red;
        private Color _defaultColor = Color.white;

        // Properties
        public bool interactable
        {
            get => _button.interactable;
            set => _button.interactable = value;
        }

        // Click
        private Action _onClickCallback;

        private Tween _warmupDelayedCall = null;
        private Tween _durationDelayedCall = null;

        private bool _active = false;
        private bool _warmingUp = false;

        public bool isActive => _active;
        public bool isWarmingUp => _warmingUp;
        public bool isAnimating => isActive || isWarmingUp;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }

        public void SetUp(string name, Sprite icon)
        {
            _text.text = name;
            if (icon != null) _image.sprite = icon;
            _text.enabled = icon == null;
            _defaultColor = _background.color;
        }


        // ========================== Click Listener ============================

        public void AddOnClickListener(Action onClick)
        {
            _onClickCallback = onClick;
        }

        private void OnClick()
        {
            // Callback
            _onClickCallback?.Invoke();
        }


        // ========================== Tween Animations ============================

        public void AnimateButton(float durationSeconds, float warmupSeconds,
            Action onEffectFinishedCallback = null,
            Action onWarmupFinishedCallback = null)
        {
            KillTweens();

            // Activation animation
            _active = true;
            _background.color = _activatedColor;
            _durationDelayedCall = AnimateButtonFillAmount(1, 0, durationSeconds);
            _durationDelayedCall.onComplete = () =>
            {
                _active = false;
                _warmingUp = true;
                _background.color = _warmingUpColor;

                Action callback = onEffectFinishedCallback;
                onEffectFinishedCallback = null;
                callback?.Invoke();

                // Warmup Animation
                _warmupDelayedCall = AnimateButtonFillAmount(0, 1, warmupSeconds);
                _warmupDelayedCall.onComplete = () =>
                {
                    _warmingUp = false;
                    _background.color = _defaultColor;

                    Action callback = onWarmupFinishedCallback;
                    onWarmupFinishedCallback = null;
                    callback?.Invoke();
                };
            };
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
