using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GameHandler : MonoBehaviour
{
    // TODO: Should it be serialized?
    // TODO: [Header("Components")]
    // TODO: private naming convention
    [SerializeField] private GameController gameController;

    // TODO: [Header("Board Settings")] [Range(min, max)]
    [SerializeField] public int boardWidth = 10;

    [SerializeField] public int boardHeight = 10;

    [SerializeField] public BoardView boardView;

    private void Awake()
    {
        // TODO: Move instantiation to Start -- Use awake only for Get\Add Component operations
        gameController = new GameController();

        // TODO: Remove this event dependency, maybe create something like a FSM event dispatch?
        boardView.onTileClick += OnTileClick;
    }

    private void Start()
    {
        // TODO: Maybe create a Board class to that holds List<List<Tile>> ??? 
        List<List<Tile>> board = gameController.StartGame(boardWidth, boardHeight);
        boardView.CreateBoard(board);
    }

    // TODO: Move to the top
    // TODO: private naming convention
    private int selectedX, selectedY = -1;

    private bool isAnimating;

    private void OnTileClick(int x, int y)
    {
        if (isAnimating) return;

        if (selectedX > -1 && selectedY > -1)
        {
            if (Mathf.Abs(selectedX - x) + Mathf.Abs(selectedY - y) > 1)
            {
                // TODO: Enter here also if the same slot was selected
                selectedX = -1;
                selectedY = -1;
            }
            else
            {
                isAnimating = true;
                boardView.SwapTiles(selectedX, selectedY, x, y).onComplete += () =>
                {
                    // TODO: This code is creating a deep copy of the entire board just to check if the movement was valid
                    // Change it to check valid movements BEFORE them were made, not deep copying the entire board
                    bool isValid = gameController.IsValidMovement(selectedX, selectedY, x, y);
                    if (!isValid)
                    {
                        boardView.SwapTiles(x, y, selectedX, selectedY)
                        .onComplete += () => isAnimating = false;
                    }
                    else
                    {
                        // Finish swaping the tiles (control layer)
                        List<BoardSequence> swapResult = gameController.SwapTile(selectedX, selectedY, x, y);

                        // Then animate the new updated board 
                        AnimateBoard(swapResult, 0, () => isAnimating = false);
                    }

                    selectedX = -1;
                    selectedY = -1;
                };
            }
        }
        else
        {
            selectedX = x;
            selectedY = y;
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
