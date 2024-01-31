using DG.Tweening;
using match3.board;
using match3.progress;
using match3.settings;
using match3.special;
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
        [SerializeField] private BoardView _boardView;
        [SerializeField] private ProgressView _progressView;
        [SerializeField] private SpecialView _specialView;
        [SerializeField] private SpecialRepository _specialRepository;

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
            _gameController.SetSpecials(
                _specialRepository.clearLinesSpecial.durationSeconds, _specialRepository.clearLinesSpecial.warmupSeconds,
                _specialRepository.explosionSpecial.durationSeconds, _specialRepository.explosionSpecial.warmupSeconds,
                _specialRepository.colorClearSpecial.durationSeconds, _specialRepository.colorClearSpecial.warmupSeconds
                );

            _boardView.CreateBoard(board, OnTileClick);
            _progressView.UpdateProgress(_gameController.progress);

            // Specials
            _specialView.SetOnClickEvents(OnSpecialClearLinesClick, OnSpecialExplosionClick, OnSpecialColorClearClick);

            _specialView.SetupClearLines(_specialRepository.clearLinesSpecial.name, _specialRepository.clearLinesSpecial.icon);
            _specialView.SetupExplosion(_specialRepository.explosionSpecial.name, _specialRepository.explosionSpecial.icon);
            _specialView.SetupColorClear(_specialRepository.colorClearSpecial.name, _specialRepository.colorClearSpecial.icon);
        }

        // ========================== Tile Click ============================

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
                    _boardView.SwapTiles(_selectedX, _selectedY, x, y).onComplete += () =>
                    {
                        // TODO: This code is creating a deep copy of the entire board just to check if the movement was valid
                        // Change it to check valid movements BEFORE them were made, not deep copying the entire board
                        bool isValid = _gameController.IsValidMovement(_selectedX, _selectedY, x, y);
                        if (!isValid)
                        {
                            _boardView.SwapTiles(x, y, _selectedX, _selectedY)
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


        // ========================== Specials Trigger ============================

        private void OnSpecialClearLinesClick()
        {
            _gameController.SetSpecialClearLinesActive(true);
            _specialView.AnimateClearLinesButton(_gameController.ClearLinesSpecial,
                onEffectFinishedCallback: () => _gameController.SetSpecialClearLinesActive(false)
                );
        }

        private void OnSpecialColorClearClick()
        {
            throw new NotImplementedException();
        }

        private void OnSpecialExplosionClick()
        {
            throw new NotImplementedException();
        }


        // ========================== Board Animation ============================


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
                sequence.onComplete += () =>
                {
                    AnimateBoardSequences(boardSequences, onComplete);
                    _progressView.SetScore(currentBoardSequence.score);
                };
            else
                sequence.onComplete += () =>
                {
                    _progressView.UpdateProgress(_gameController.progress);
                    onComplete();
                };
        }

        private Sequence CreateBoardAnimationSequence(BoardSequence boardSequence)
        {
            Sequence sequence = DOTween.Sequence();

            sequence.Append(_boardView.DestroyTiles(boardSequence.matchedPosition));
            sequence.Append(_boardView.MoveTiles(boardSequence.movedTiles));
            sequence.Append(_boardView.CreateTile(boardSequence.addedTiles));

            return sequence;
        }
    }
}