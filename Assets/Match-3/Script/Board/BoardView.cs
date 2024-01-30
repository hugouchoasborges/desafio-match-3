using DG.Tweening;
using match3.settings;
using match3.tile;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace match3.board
{
    public class BoardView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TilePrefabRepository _tilePrefabRepository;
        [SerializeField] private GridLayoutGroup _boardContainer;

        private TileSpotView[][] _tileSpots;
        private TileView[][] _tiles;

        private Action<int, int> _onTileClickCallback;

        private void OnTileSpotClick(int x, int y)
        {
            _onTileClickCallback?.Invoke(x, y);
        }

        // ----------------------------------------------------------------------------------
        // ========================== Board Operations ============================
        // ----------------------------------------------------------------------------------

        // ========================== Create ============================

        public void CreateBoard(Board board, Action<int, int> onClick)
        {
            _onTileClickCallback = onClick;

            int lines = board.lines;
            int columns = board.columns;

            _boardContainer.constraintCount = columns;

            _tileSpots = new TileSpotView[lines][];
            _tiles = new TileView[lines][];

            for (int y = 0; y < lines; y++)
            {
                _tileSpots[y] = new TileSpotView[columns];
                _tiles[y] = new TileView[columns];

                for (int x = 0; x < columns; x++)
                {

                    TileSpotView tileSpot = Instantiate(_tilePrefabRepository.TileSpotPrefab);

                    tileSpot.Initialize(_boardContainer.transform, x, y, OnTileSpotClick);
                    _tileSpots[y][x] = tileSpot;

                    TileView tilePrefab = _tilePrefabRepository.GetTileFromType(board[y][x].type);
                    if (tilePrefab != null)
                    {
                        TileView tile = Instantiate(tilePrefab);
                        tileSpot.SetTile(tile);

                        _tiles[y][x] = tile;
                    }
                }
            }
        }


        // ========================== Destroy ============================

        public void DestroyBoard()
        {
            for (int y = 0; y < _tiles.Length; y++)
            {
                for (int x = 0; x < _tiles[y].Length; x++)
                {
                    // TODO: Queue this to a pool
                    Destroy(_tiles[y][x].gameObject);
                    Destroy(_tileSpots[y][x].gameObject);
                }
            }

            _tileSpots = null;
            _tiles = null;
        }


        // ----------------------------------------------------------------------------------
        // ========================== Tiles Operations ============================
        // ----------------------------------------------------------------------------------


        // ========================== Swap ============================

        public Tween SwapTiles(int fromX, int fromY, int toX, int toY)
        {
            Sequence swapSequence = DOTween.Sequence();
            swapSequence.Append(_tileSpots[fromY][fromX].AnimatedSetTile(_tiles[toY][toX]));
            swapSequence.Join(_tileSpots[toY][toX].AnimatedSetTile(_tiles[fromY][fromX]));

            TileView SwapedTile = _tiles[fromY][fromX];
            _tiles[fromY][fromX] = _tiles[toY][toX];
            _tiles[toY][toX] = SwapedTile;

            return swapSequence;
        }

        // ========================== Destroy ============================

        public Tween DestroyTiles(List<Vector2Int> matchedPosition)
        {
            for (int i = 0; i < matchedPosition.Count; i++)
            {
                Vector2Int position = matchedPosition[i];
                // TODO: Queue this to a pool
                Destroy(_tiles[position.y][position.x].gameObject);
                _tiles[position.y][position.x] = null;
            }
            return DOVirtual.DelayedCall(0.2f, () => { });
        }

        // ========================== Move ============================

        public Tween MoveTiles(List<MovedTileInfo> movedTiles)
        {
            TileView[][] tiles = new TileView[_tiles.Length][];
            for (int y = 0; y < _tiles.Length; y++)
            {
                tiles[y] = new TileView[_tiles[y].Length];
                for (int x = 0; x < _tiles[y].Length; x++)
                {
                    tiles[y][x] = _tiles[y][x];
                }
            }

            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < movedTiles.Count; i++)
            {
                MovedTileInfo movedTileInfo = movedTiles[i];

                Vector2Int from = movedTileInfo.from;
                Vector2Int to = movedTileInfo.to;

                sequence.Join(_tileSpots[to.y][to.x].AnimatedSetTile(_tiles[from.y][from.x]));

                tiles[to.y][to.x] = _tiles[from.y][from.x];
            }

            _tiles = tiles;
            return sequence;
        }

        // ========================== Create ============================

        public Tween CreateTile(List<AddedTileInfo> addedTiles)
        {
            Sequence seq = DOTween.Sequence();
            for (int i = 0; i < addedTiles.Count; i++)
            {
                AddedTileInfo addedTileInfo = addedTiles[i];
                Vector2Int position = addedTileInfo.position;

                TileSpotView tileSpot = _tileSpots[position.y][position.x];

                TileView tilePrefab = _tilePrefabRepository.GetTileFromType(addedTileInfo.type);
                TileView tile = Instantiate(tilePrefab);
                tileSpot.SetTile(tile);

                _tiles[position.y][position.x] = tile;

                tile.transform.localScale = Vector2.zero;
                seq.Join(tile.transform.DOScale(1.0f, 0.2f));
            }

            return seq;
        }

        // ========================== Dispose ============================

        private void OnDestroy()
        {
            _onTileClickCallback = null;
        }
    }
}