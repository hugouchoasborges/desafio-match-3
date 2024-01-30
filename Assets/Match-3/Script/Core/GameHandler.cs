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

        private void Start()
        {
            _gameController = new GameController();

            Board board = _gameController.StartGame(_availableTileTypes, _boardWidth, _boardHeight);
            boardView.CreateBoard(board, OnTileClick);
        }

        private void OnTileClick(int x, int y)
        {
            if (_isAnimating) return;

            if (_selectedX > -1 && _selectedY > -1)
            {
                if (Mathf.Abs(_selectedX - x) + Mathf.Abs(_selectedY - y) != 1)
                {
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
                            AnimateBoardSequences(swapResult, () => _isAnimating = false);
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

        private void AnimateBoardSequences(List<BoardSequence> boardSequences, Action onComplete)
        {
            if (boardSequences.Count == 0) return;

            // Retrieve the current sequence and remove it from the list
            BoardSequence currentBoardSequence = boardSequences[0];
            boardSequences.Remove(currentBoardSequence);

            // Create an animation sequence
            Sequence sequence = CreateBoardAnimationSequence(currentBoardSequence);

            // Link it to the next animation sequence OR to the onComplete callback
            if (boardSequences.Count > 0)
                sequence.onComplete += () => AnimateBoardSequences(boardSequences, onComplete);
            else
                sequence.onComplete += () => onComplete();
        }

        private Sequence CreateBoardAnimationSequence(BoardSequence boardSequence)
        {
            Sequence sequence = DOTween.Sequence();

            sequence.Append(boardView.DestroyTiles(boardSequence.matchedPosition));
            sequence.Append(boardView.MoveTiles(boardSequence.movedTiles));
            sequence.Append(boardView.CreateTile(boardSequence.addedTiles));

            return sequence;
        }
    }
}