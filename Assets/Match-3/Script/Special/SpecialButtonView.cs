﻿using DG.Tweening;
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

        public void SetUp(string name, Sprite icon)
        {
            _text.text = name;
            if (icon != null) _image.sprite = icon;
            _defaultColor = _background.color;
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
            _background.color = _activatedColor;
            _durationDelayedCall = AnimateButtonFillAmount(1, 0, durationSeconds);
            _durationDelayedCall.onComplete = () =>
            {
                _background.color = _warmingUpColor;
                onEffectFinishedCallback?.Invoke();

                // Warmup Animation
                _warmupDelayedCall = AnimateButtonFillAmount(0, 1, warmupSeconds);
                _warmupDelayedCall.onComplete = () =>
                {
                    _background.color = _defaultColor;
                    _button.interactable = true;
                    onWarmupFinishedCallback?.Invoke();
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
