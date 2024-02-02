using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace match3.tile
{
    public class TileSpotView : MonoBehaviour
    {
        [SerializeField] private Button _button;

        // Position
        private int _x;
        private int _y;

        // Callback
        private Action<int, int> _onClickCallback;

        public void Initialize(Transform parentTransform, int positionX, int positionY, Action<int, int> OnClick, bool worldPositionStays = false)
        {
            transform.SetParent(parentTransform, worldPositionStays);
            _x = positionX;
            _y = positionY;
            _onClickCallback = OnClick;
        }

        public void SetTile(TileView tile)
        {
            tile.SetParent(transform, false);
            tile.SetPosition(transform.position);
        }

        /// <summary>
        /// Sets the tile to the corresponding one with a movement animation
        /// </summary>
        /// <param name="tile"></param>
        /// <returns>The DG.Tweening.Tween created from this operation</returns>

        public Tween AnimatedSetTile(TileView tile)
        {
            tile.SetParent(transform, true);
            return tile.DOMove(transform.position, 0.3f);
        }

        // ========================== Button Events ============================

        private void OnTileClick()
        {
            _onClickCallback?.Invoke(_x, _y);
        }

        private void OnEnable() => AddButtonsListeners();

        private void OnDisable() => RemoveButtonsListeners();

        private void AddButtonsListeners()
        {
            _button.onClick.AddListener(OnTileClick);
        }

        private void RemoveButtonsListeners()
        {
            _button.onClick.RemoveAllListeners();
        }

        // ========================== Dispose ============================

        private void OnDestroy()
        {
            _onClickCallback = null;
            RemoveButtonsListeners();
        }

    }
}