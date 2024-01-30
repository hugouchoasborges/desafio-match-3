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


        [Header("Size Settigns")]
        [SerializeField] private Vector2 _defaultCellSize = Vector2.one * 50;

        enum Axis { WIDTH, HEIGHT }
        [SerializeField] private Axis _axisToFit = Axis.HEIGHT;

        [Tooltip("Percentage of the container's size to fill with the Grid Layout Group")]
        [SerializeField]
        [Range(0, 1)]
        private float _axisSizePerc = 1f;

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
            float cellSizeX = _defaultCellSize.x;
            float cellSizeY = _defaultCellSize.y;

            float axisSizeMultiplier = _axisToFit == Axis.HEIGHT
                ? _container.rect.height / (cellSizeY + _layout.spacing.y)
                : _container.rect.width / (cellSizeX + _layout.spacing.x);

            float fitMultiplier = axisSizeMultiplier / _layout.constraintCount;

            _layout.cellSize = new Vector2(cellSizeX, cellSizeY) * fitMultiplier * _axisSizePerc;
        }

        [ContextMenu("Reset Fit")]
        private void ResetLayout()
        {
            _layout.cellSize = _defaultCellSize;
        }
    }
}
