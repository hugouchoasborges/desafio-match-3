using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace match3.board
{
    public class BoardContentFitter : MonoBehaviour
    {

        [Header("Components")]
        [Tooltip("GridLayoutGroup to re-scale")]
        [SerializeField]
        private GridLayoutGroup _layout;

        [Tooltip("RectTransform to use as screen boundaries")]
        [SerializeField]
        private RectTransform _container;

        [SerializeField]
        private RectTransform _backgroundContainer;


        [Header("Size Settigns")]
        [SerializeField] private Vector2 _defaultCellSize = Vector2.one * 50;

        enum Axis { WIDTH, HEIGHT }
        [SerializeField] private Axis _axisToFit = Axis.HEIGHT;

        [Tooltip("Percentage of the container's size to fill with the Grid Layout Group")]
        [SerializeField]
        [Range(0, 1)]
        private float _axisSizePerc = 1f;

        private RectTransform _topLeftChild;

        void Start()
        {
            // Update current cellSize
            _defaultCellSize = _layout.cellSize;

            UpdateLayout();
        }

        void OnRectTransformDimensionsChange()
        {
            UpdateLayout();
        }

        [ContextMenu("Fit to Container")]
        private void UpdateLayout()
        {
            StopCoroutine(UpdateLayout_Coroutine());
            StartCoroutine(UpdateLayout_Coroutine());
        }

        private IEnumerator UpdateLayout_Coroutine()
        {
            float cellSizeX = _defaultCellSize.x;
            float cellSizeY = _defaultCellSize.y;

            float axisSizeMultiplier = _axisToFit == Axis.HEIGHT
                ? _container.rect.height / (cellSizeY + _layout.spacing.y)
                : _container.rect.width / (cellSizeX + _layout.spacing.x);

            float fitMultiplier = axisSizeMultiplier / _layout.constraintCount;

            _layout.cellSize = new Vector2(cellSizeX, cellSizeY) * fitMultiplier * _axisSizePerc;

            // Needed so the Grid Layout can be computed
            yield return new WaitForEndOfFrame();

            // Background adjustments
            if (_topLeftChild == null)
                _topLeftChild = _layout.gameObject.transform.GetChild(1) as RectTransform;

            _backgroundContainer.sizeDelta = new Vector2(_layout.minWidth, _layout.minHeight);
            _backgroundContainer.anchoredPosition = new Vector2(
                _topLeftChild.anchoredPosition.x - (_layout.cellSize.x / 2),
                _topLeftChild.anchoredPosition.y + (_layout.cellSize.y / 2)
                );

            if (Application.isPlaying)
                _backgroundContainer.gameObject.SetActive(true);
        }

#if UNITY_EDITOR
        void OnApplicationQuit()
        {
            _backgroundContainer.gameObject.SetActive(false);
        }
#endif

        [ContextMenu("Reset Fit")]
        private void ResetLayout()
        {
            _layout.cellSize = _defaultCellSize;
        }
    }
}
