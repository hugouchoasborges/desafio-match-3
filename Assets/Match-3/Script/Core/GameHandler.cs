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
        [SerializeField] private BoardOffsetView _boardOffsetView;
        [SerializeField] private BoardContentFitter _boardContentFitter;

        [Header("Repositories")]
        [SerializeField] private SpecialRepository _specialRepository;
        [SerializeField] private LevelRepository _levelRepository;

        [Header("levels")]
        [SerializeField][Range(1, 100)] private int _startLevel = 1;

        // Internal animation\movement control
        private int _selectedX, _selectedY = -1;
        private bool _isAnimating;

        private static int _currentLevel = -1;

        private void Start()
        {
            _currentLevel = _startLevel - 2;
            _gameController = new GameController();

            // Specials Settings
            _gameController.SetSpecials(
                _specialRepository.clearLinesSpecial.durationSeconds, _specialRepository.clearLinesSpecial.warmupSeconds,
                _specialRepository.explosionSpecial.durationSeconds, _specialRepository.explosionSpecial.warmupSeconds,
                _specialRepository.colorClearSpecial.durationSeconds, _specialRepository.colorClearSpecial.warmupSeconds,
                _specialRepository.tipSpecial.durationSeconds, _specialRepository.tipSpecial.warmupSeconds
                );

            _specialView.SetOnClickEvents(OnSpecialClearLinesClick, OnSpecialExplosionClick, OnSpecialColorClearClick, OnSpecialTipClick);

            _specialView.SetupClearLines(_specialRepository.clearLinesSpecial.name, _specialRepository.clearLinesSpecial.icon);
            _specialView.SetupExplosion(_specialRepository.explosionSpecial.name, _specialRepository.explosionSpecial.icon);
            _specialView.SetupColorClear(_specialRepository.colorClearSpecial.name, _specialRepository.colorClearSpecial.icon);
            _specialView.SetupTip(_specialRepository.tipSpecial.name, _specialRepository.tipSpecial.icon);

            StartLevelNext();
        }
        private void StartLevelNext()
        {
            _currentLevel++;

            Board board = _gameController.StartGame(_levelRepository[_currentLevel]);

            _boardView.CreateBoard(board, OnTileClick);
            _progressView.UpdateProgress(_gameController.progress);

            _boardContentFitter.UpdateLayout();
            _boardOffsetView.AnimateTransitionDown();
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

                    _boardView.SetTileSelected(_selectedX, _selectedY);
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
                            .onComplete += OnBoardAnimationFinished;
                        }
                        else
                        {
                            // Finish swaping the tiles (control layer)
                            List<BoardSequence> swapResult = _gameController.SwapTile(_selectedX, _selectedY, x, y);

                            // Then animate the new updated board 
                            AnimateBoardSequences(swapResult, OnBoardAnimationFinished);
                        }

                        _selectedX = -1;
                        _selectedY = -1;

                        _boardView.SetTileSelected(_selectedX, _selectedY);
                    };
                }
            }
            else
            {
                _selectedX = x;
                _selectedY = y;

                _boardView.SetTileSelected(_selectedX, _selectedY);
            }
        }

        private void OnBoardAnimationFinished()
        {
            _isAnimating = false;

            if (_gameController.progress.IsLevelFinished)
            {
                _boardOffsetView.AnimateTransitionUp(onComplete: () =>
                {
                    _boardView.DestroyBoard();
                    StartLevelNext();
                });
            }
        }


        // ========================== Specials Trigger ============================

        private void SetAllSpecialsInteractable(bool interactable, bool affectAnimating = false)
        {
            _specialView.SetAllSpecialsInteractable(interactable, affectAnimating);
        }

        private bool IsAnySpecialActive()
        {
            return _specialView.IsAnySpecialActive();
        }

        private void OnSpecialClearLinesClick()
        {
            SetAllSpecialsInteractable(false);
            _gameController.SetSpecialClearLinesActive(true);
            _specialView.AnimateClearLinesButton(_gameController.clearLinesSpecial,
                onEffectFinishedCallback: () =>
                {
                    _gameController.SetSpecialClearLinesActive(false);
                    SetAllSpecialsInteractable(true);
                },
                onWarmupFinishedCallback: () =>
                {
                    if (!IsAnySpecialActive())
                        SetAllSpecialsInteractable(true);
                }
                );
        }

        private void OnSpecialColorClearClick()
        {
            SetAllSpecialsInteractable(false);
            _gameController.SetSpecialColorClearActive(true);
            _specialView.AnimateColorClearButton(_gameController.colorClearSpecial,
                onEffectFinishedCallback: () =>
                {
                    _gameController.SetSpecialColorClearActive(false);
                    SetAllSpecialsInteractable(true);
                },
                onWarmupFinishedCallback: () =>
                {
                    if (!IsAnySpecialActive())
                        SetAllSpecialsInteractable(true);
                }
                );
        }

        private void OnSpecialExplosionClick()
        {
            SetAllSpecialsInteractable(false);
            _gameController.SetSpecialExplosionActive(true);
            _specialView.AnimateExplosionButton(_gameController.explosionSpecial,
                onEffectFinishedCallback: () =>
                {
                    _gameController.SetSpecialExplosionActive(false);
                    SetAllSpecialsInteractable(true);
                },
                onWarmupFinishedCallback: () =>
                {
                    if (!IsAnySpecialActive())
                        SetAllSpecialsInteractable(true);
                }
                );
        }

        private void OnSpecialTipClick()
        {
            SetAllSpecialsInteractable(false);
            _gameController.SetSpecialTipActive(true);
            _specialView.AnimateTipButton(_gameController.tipSpecial,
                onEffectFinishedCallback: () =>
                {
                    _gameController.SetSpecialTipActive(false);
                    SetAllSpecialsInteractable(true);
                },
                onWarmupFinishedCallback: () =>
                {
                    if (!IsAnySpecialActive())
                        SetAllSpecialsInteractable(true);
                }
                );

            List<Vector2Int> foundMatches = _gameController.GetMatchTipsBruteForce();
            foreach (Vector2Int match in foundMatches)
            {
                _boardView.SetTileSelectedTip(match.x, match.y, true);
            }
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