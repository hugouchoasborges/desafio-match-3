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


        // ========================== Dispose ============================

        private void OnDestroy()
        {
            _onClearLinesCallback = null;
            _onExplosionCallback = null;
            _onColorClearCallback = null;
        }
    }
}
