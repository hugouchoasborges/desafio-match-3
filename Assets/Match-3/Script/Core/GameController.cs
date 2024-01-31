using match3.board;
using match3.progress;
using match3.special;
using match3.tile;
using System;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

namespace match3.core
{
    public class GameController
    {
        private Board _board;
        public Progress progress { get; private set; }

        // Specials
        public Special ClearLinesSpecial { get; private set; }
        public Special ExplosionSpecial { get; private set; }
        public Special ColorClearSpecial { get; private set; }

        private List<TileType> _tilesTypes;
        private int _tileCount;

        public Board StartGame(TileType[] _availableTileTypes, int boardWidth, int boardHeight)
        {
            _tilesTypes = new List<TileType>();
            foreach (var newTileType in _availableTileTypes)
                if (newTileType != TileType.NONE && !_tilesTypes.Contains(newTileType)) // Shouldn't allow duplicates
                    _tilesTypes.Add(newTileType);

            _board = CreateBoard(boardWidth, boardHeight, _tilesTypes);

            // Progress
            progress = new Progress();

            return _board;
        }

        public void SetSpecials(
            int clearLinesDurationSec, int clearLinesWarmupSec,
            int explosionDurationSec, int explosionWarmupSec,
            int colorClearDurationSec, int colorClearWarmupSec
            )
        {
            // Specials
            ClearLinesSpecial = new Special(clearLinesDurationSec, clearLinesWarmupSec);
            ExplosionSpecial = new Special(explosionDurationSec, explosionWarmupSec);
            ColorClearSpecial = new Special(colorClearDurationSec, colorClearWarmupSec);
        }

        public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
        {
            // TODO: Change this method to work without deep-copying the entire board
            Board newBoard = _board.Clone();

            Tile switchedTile = newBoard[fromY][fromX];
            newBoard[fromY][fromX] = newBoard[toY][toX];
            newBoard[toY][toX] = switchedTile;

            for (int y = 0; y < newBoard.lines; y++)
            {
                for (int x = 0; x < newBoard.columns; x++)
                {
                    if (x > 1
                        && newBoard[y][x].type == newBoard[y][x - 1].type
                        && newBoard[y][x - 1].type == newBoard[y][x - 2].type)
                    {
                        return true;
                    }
                    if (y > 1
                        && newBoard[y][x].type == newBoard[y - 1][x].type
                        && newBoard[y - 1][x].type == newBoard[y - 2][x].type)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
        {
            Board newBoard = _board.Clone();

            Tile switchedTile = newBoard[fromY][fromX];
            newBoard[fromY][fromX] = newBoard[toY][toX];
            newBoard[toY][toX] = switchedTile;

            int totalScore = 0;
            List<BoardSequence> boardSequences = new List<BoardSequence>();
            List<List<bool>> matchedTiles;
            while (HasMatch(matchedTiles = FindMatches(newBoard)))
            {
                //Cleaning the matched tiles
                List<Vector2Int> matchedPosition = new List<Vector2Int>();
                for (int y = 0; y < newBoard.lines; y++)
                    for (int x = 0; x < newBoard.columns; x++)
                        if (matchedTiles[y][x])
                            matchedPosition.Add(new Vector2Int(x, y));

                if (ClearLinesSpecial.active)
                    SwapTileClearLinesSpecial(newBoard, matchedPosition);

                if (ExplosionSpecial.active)
                    SwapTileExplosionSpecial(newBoard, matchedPosition);

                // Remove duplicated matches
                matchedPosition = GetDistinctMatchedPositions(matchedPosition);

                // Update the new board with the distinct matched positions
                foreach (var match in matchedPosition)
                    newBoard[match.y][match.x] = new Tile();

                // Dropping the tiles
                Dictionary<int, MovedTileInfo> movedTiles = new Dictionary<int, MovedTileInfo>();
                List<MovedTileInfo> movedTilesList = new List<MovedTileInfo>();
                for (int i = 0; i < matchedPosition.Count; i++)
                {
                    int x = matchedPosition[i].x;
                    int y = matchedPosition[i].y;
                    if (y > 0)
                    {
                        for (int j = y; j > 0; j--)
                        {
                            Tile movedTile = newBoard[j - 1][x];
                            newBoard[j][x] = movedTile;
                            if (movedTile.type > TileType.NONE)
                            {
                                if (movedTiles.ContainsKey(movedTile.id))
                                {
                                    movedTiles[movedTile.id].to = new Vector2Int(x, j);
                                }
                                else
                                {
                                    MovedTileInfo movedTileInfo = new MovedTileInfo
                                    {
                                        from = new Vector2Int(x, j - 1),
                                        to = new Vector2Int(x, j)
                                    };
                                    movedTiles.Add(movedTile.id, movedTileInfo);
                                    movedTilesList.Add(movedTileInfo);
                                }
                            }
                        }

                        newBoard[0][x] = new Tile();
                    }
                }

                // Filling the board
                List<AddedTileInfo> addedTiles = new List<AddedTileInfo>();
                for (int y = newBoard.lines - 1; y > -1; y--)
                {
                    for (int x = newBoard.columns - 1; x > -1; x--)
                    {
                        if (newBoard[y][x].type == TileType.NONE)
                        {
                            int tileTypeIdx = Random.Range(0, _tilesTypes.Count);
                            Tile tile = newBoard[y][x];
                            tile.SetID(_tileCount++);
                            tile.SetType(_tilesTypes[tileTypeIdx]);
                            addedTiles.Add(new AddedTileInfo
                            {
                                position = new Vector2Int(x, y),
                                type = tile.type
                            });
                        }
                    }
                }

                totalScore += matchedPosition.Count;
                BoardSequence sequence = new BoardSequence(
                    matchedPosition,
                    addedTiles,
                    movedTilesList,
                    totalScore
                    );
                boardSequences.Add(sequence);
            }

            progress.AddScore(totalScore);
            _board = newBoard;
            return boardSequences;
        }

        private List<Vector2Int> GetDistinctMatchedPositions(List<Vector2Int> matchedPosition)
        {
            HashSet<Vector2Int> uniquePositions = new HashSet<Vector2Int>();
            List<Vector2Int> uniqueList = new List<Vector2Int>();

            foreach (Vector2Int pos in matchedPosition)
            {
                if (uniquePositions.Add(pos))
                {
                    uniqueList.Add(pos);
                }
            }

            return uniqueList;
        }

        private static bool HasMatch(List<List<bool>> list)
        {
            for (int y = 0; y < list.Count; y++)
                for (int x = 0; x < list[y].Count; x++)
                    if (list[y][x])
                        return true;
            return false;
        }

        private static List<List<bool>> FindMatches(Board board)
        {
            List<List<bool>> matchedTiles = new List<List<bool>>();
            for (int y = 0; y < board.lines; y++)
            {
                matchedTiles.Add(new List<bool>(board.columns));
                for (int x = 0; x < board.columns; x++)
                {
                    matchedTiles[y].Add(false);
                }
            }

            for (int y = 0; y < board.lines; y++)
            {
                for (int x = 0; x < board.columns; x++)
                {
                    if (x > 1
                        && board[y][x].type == board[y][x - 1].type
                        && board[y][x - 1].type == board[y][x - 2].type)
                    {
                        matchedTiles[y][x] = true;
                        matchedTiles[y][x - 1] = true;
                        matchedTiles[y][x - 2] = true;
                    }
                    if (y > 1
                        && board[y][x].type == board[y - 1][x].type
                        && board[y - 1][x].type == board[y - 2][x].type)
                    {
                        matchedTiles[y][x] = true;
                        matchedTiles[y - 1][x] = true;
                        matchedTiles[y - 2][x] = true;
                    }
                }
            }

            return matchedTiles;
        }

        private Board CreateBoard(int width, int height, List<TileType> tileTypes)
        {
            // Create uninitialized board
            Board board = new Board(width, height);

            // Now fill the board with only no-matching tiles
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    List<TileType> noMatchTypes = new List<TileType>(tileTypes.Count);
                    for (int i = 0; i < tileTypes.Count; i++)
                    {
                        // All tile types
                        noMatchTypes.Add(_tilesTypes[i]);
                    }

                    // If the previous two tiles are already matching,
                    // the current one can't be of the same type -- Vertically and Horizontally
                    if (x > 1
                        && board[y][x - 1].type == board[y][x - 2].type)
                    {
                        noMatchTypes.Remove(board[y][x - 1].type);
                    }
                    if (y > 1
                        && board[y - 1][x].type == board[y - 2][x].type)
                    {
                        noMatchTypes.Remove(board[y - 1][x].type);
                    }

                    board[y][x].SetID(_tileCount++);
                    board[y][x].SetType(noMatchTypes[Random.Range(0, noMatchTypes.Count)]);
                }
            }

            return board;
        }



        // ----------------------------------------------------------------------------------
        // ========================== Specials ============================
        // ----------------------------------------------------------------------------------

        // ========================== Clear Lines============================

        public void SetSpecialClearLinesActive(bool active)
        {
            ClearLinesSpecial.SetActive(active);
        }

        private void SwapTileClearLinesSpecial(Board newBoard, List<Vector2Int> matchedPosition)
        {
            // If the clear lines special is active, look for matches and 
            // include the entire row as a valid match

            List<int> linesToClear = new List<int>();
            foreach (var match in matchedPosition)
            {
                if (!linesToClear.Contains(match.y))
                    linesToClear.Add(match.y);
            }

            matchedPosition.Clear();

            foreach (int y in linesToClear)
            {
                // Mark the entire line as a match
                for (int x = 0; x < newBoard.columns; x++)
                {
                    matchedPosition.Add(new Vector2Int(x, y));
                    newBoard[y][x] = new Tile();
                }
            }
        }

        // ========================== Explosion ============================

        public void SetSpecialExplosionActive(bool active)
        {
            ExplosionSpecial.SetActive(active);
        }

        private void SwapTileExplosionSpecial(Board newBoard, List<Vector2Int> matchedPosition)
        {
            // If the explosion special is active, mark random tiles as valid matches

            int explosionsCount = Random.Range(1, (newBoard.tilesCount - matchedPosition.Count) / 2);
            //explosionsCount = 3;

            for (int i = 0; i < explosionsCount; i++)
            {
                Vector2Int randomV2I = new Vector2Int(
                        Random.Range(0, newBoard.columns),
                        Random.Range(0, newBoard.lines));

                matchedPosition.Add(randomV2I);
                newBoard[randomV2I.x][randomV2I.y] = new Tile();
            }
        }


        // ========================== Color Clear ============================


        public void SetSpecialColorClearActive(bool active)
        {
            // TODO
            throw new NotImplementedException();
        }

        private void SwapTileColorClearSpecial(Board newBoard, List<Vector2Int> matchedPosition)
        {
            // If the clear lines special is active, look for matches and 
            // include the entire row as a valid match

            List<int> linesToClear = new List<int>();
            foreach (var match in matchedPosition)
            {
                if (!linesToClear.Contains(match.y))
                    linesToClear.Add(match.y);
            }

            matchedPosition.Clear();

            foreach (int y in linesToClear)
            {
                // Mark the entire line as a match
                for (int x = 0; x < newBoard.columns; x++)
                {
                    matchedPosition.Add(new Vector2Int(x, y));
                    newBoard[y][x] = new Tile();
                }
            }
        }
    }
}