using System;
using UnityEngine;

namespace match3.special
{
    public class SpecialView : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private SpecialButtonView _clearLinesButton;
        [SerializeField] private SpecialButtonView _explosionButton;
        [SerializeField] private SpecialButtonView _colorClearButton;

        private Action _onClearLinesCallback;
        private Action _onExplosionCallback;
        private Action _onColorClearCallback;

        private void Start()
        {
            _clearLinesButton.AddOnClickListener(OnClearLinesTriggered);
            _explosionButton.AddOnClickListener(OnExplosionTriggered);
            _colorClearButton.AddOnClickListener(OnColorClearTriggered);
        }

        public void SetOnClickEvents(
            Action OnClearLinesCallback,
            Action OnExplosionCallback,
            Action OnColorClearCallback)
        {
            _onClearLinesCallback = OnClearLinesCallback;
            _onExplosionCallback = OnExplosionCallback;
            _onColorClearCallback = OnColorClearCallback;
        }

        private void OnClearLinesTriggered()
        {
            _onClearLinesCallback?.Invoke();
        }

        private void OnExplosionTriggered()
        {
            _onExplosionCallback?.Invoke();
        }

        private void OnColorClearTriggered()
        {
            _onColorClearCallback?.Invoke();
        }


        // ========================== Animations ============================

        private void AnimateButton(SpecialButtonView button, Special special,
            Action onEffectFinishedCallback = null,
            Action onWarmupFinishedCallback = null)
        {
            button.AnimateButton(special.durationSeconds, special.warmupSeconds,
                onEffectFinishedCallback,
                onWarmupFinishedCallback
                );
        }

        public void AnimateClearLinesButton(Special special,
            Action onEffectFinishedCallback = null,
            Action onWarmupFinishedCallback = null)
            => AnimateButton(_clearLinesButton, special, onEffectFinishedCallback, onWarmupFinishedCallback);

        public void AnimateExplosionButton(Special special,
            Action onEffectFinishedCallback = null,
            Action onWarmupFinishedCallback = null)
            => AnimateButton(_explosionButton, special, onEffectFinishedCallback, onWarmupFinishedCallback);

        public void AnimateColorClearButton(Special special,
            Action onEffectFinishedCallback = null,
            Action onWarmupFinishedCallback = null)
            => AnimateButton(_colorClearButton, special, onEffectFinishedCallback, onWarmupFinishedCallback);

        // ========================== Setters ============================

        private void SetupButton(SpecialButtonView button, string name, Sprite icon)
        {
            button.SetUp(name, icon);
        }

        public void SetupClearLines(string name, Sprite icon) => SetupButton(_clearLinesButton, name, icon);
        public void SetupExplosion(string name, Sprite icon) => SetupButton(_explosionButton, name, icon);
        public void SetupColorClear(string name, Sprite icon) => SetupButton(_colorClearButton, name, icon);


        // ========================== Dispose ============================

        private void OnDestroy()
        {
            _onClearLinesCallback = null;
            _onExplosionCallback = null;
            _onColorClearCallback = null;
        }
    }
}
