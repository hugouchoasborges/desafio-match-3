using DG.Tweening;
using UnityEngine;

namespace match3.board
{
    public class BoardOffsetView : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Canvas _canvas;
        private RectTransform _canvasRectTransform;

        private Vector2 _topAnchoredPosition;
        private Vector2 _originalAnchoredPosition;

        private void Awake()
        {
            _rectTransform = (gameObject.transform as RectTransform);
            _originalAnchoredPosition = _rectTransform.anchoredPosition;

            // Get the canvas attached to the RectTransform
            _canvas = _rectTransform.GetComponentInParent<Canvas>();
            _canvasRectTransform = _canvas.transform as RectTransform;

            UpdateScreenValues();

        }

        private void UpdateScreenValues()
        {
            // Calculate the top position in canvas space
            Vector2 topPosition = new Vector2(Screen.width / 2f, 2 * (Screen.height + _rectTransform.sizeDelta.y / 2f));

            // Convert the canvas space position to local position of the RectTransform's parent
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRectTransform,
                topPosition, _canvas.worldCamera,
                out _topAnchoredPosition);
        }

        public void AnimateTransitionDown(float duration = 1f)
        {
            _rectTransform.anchoredPosition = _topAnchoredPosition;

            // Use DOTween to lerp between the start and end positions
            DOTween.To(
                () => _rectTransform.anchoredPosition, 
                position => _rectTransform.anchoredPosition = position, 
                _originalAnchoredPosition, duration)
                .SetEase(Ease.OutQuad)
                .SetDelay(.5f); // Delay before starting it
        }
    }
}
