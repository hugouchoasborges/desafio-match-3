using DG.Tweening;
using match3.board;
using match3.tile;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace match3.core
{
    public class GameHandler : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private GameController _gameController;
        [SerializeField] public BoardView boardView;

        [Header("Board Settings")]
        [SerializeField] private TileType[] _availableTileTypes;
        [SerializeField][Range(2, 20)] private int _boardWidth = 10;
        [SerializeField][Range(2, 20)] private int _boardHeight = 10;

        // Internal animation\movement control
        private int _selectedX, _selectedY = -1;
        private bool _isAnimating;


        private void Awake()
        {
            // TODO: Remove this event dependency, maybe create something like a FSM event dispatch?
            boardView.onTileClick += OnTileClick;
        }

        private void Start()
        {
            _gameController = new GameController();

            // TODO: Maybe create a Board class to that holds List<List<Tile>> ??? 
            List<List<Tile>> board = _gameController.StartGame(_availableTileTypes, _boardWidth, _boardHeight);
            boardView.CreateBoard(board);
        }

        private void OnTileClick(int x, int y)
        {
            if (_isAnimating) return;

            if (_selectedX > -1 && _selectedY > -1)
            {
                if (Mathf.Abs(_selectedX - x) + Mathf.Abs(_selectedY - y) > 1)
                {
                    // TODO: Enter here also if the same slot was selected
                    _selectedX = -1;
                    _selectedY = -1;
                }
                else
                {
                    _isAnimating = true;
                    boardView.SwapTiles(_selectedX, _selectedY, x, y).onComplete += () =>
                    {
                        // TODO: This code is creating a deep copy of the entire board just to check if the movement was valid
                        // Change it to check valid movements BEFORE them were made, not deep copying the entire board
                        bool isValid = _gameController.IsValidMovement(_selectedX, _selectedY, x, y);
                        if (!isValid)
                        {
                            boardView.SwapTiles(x, y, _selectedX, _selectedY)
                            .onComplete += () => _isAnimating = false;
                        }
                        else
                        {
                            // Finish swaping the tiles (control layer)
                            List<BoardSequence> swapResult = _gameController.SwapTile(_selectedX, _selectedY, x, y);

                            // Then animate the new updated board 
                            AnimateBoard(swapResult, 0, () => _isAnimating = false);
                        }

                        _selectedX = -1;
                        _selectedY = -1;
                    };
                }
            }
            else
            {
                _selectedX = x;
                _selectedY = y;
            }
        }

        private void AnimateBoard(List<BoardSequence> boardSequences, int i, Action onComplete)
        {
            // TODO: Remove this 'int i' parameter
            // Outside methods shouldn't have to help this method
            Sequence sequence = DOTween.Sequence();

            BoardSequence boardSequence = boardSequences[i];
            sequence.Append(boardView.DestroyTiles(boardSequence.matchedPosition));
            sequence.Append(boardView.MoveTiles(boardSequence.movedTiles));
            sequence.Append(boardView.CreateTile(boardSequence.addedTiles));

            i++;
            if (i < boardSequences.Count)
            {
                sequence.onComplete += () => AnimateBoard(boardSequences, i, onComplete);
            }
            else
            {
                sequence.onComplete += () => onComplete();
            }
        }
    }
}