using match3.board;
using match3.tile;
using System.Collections.Generic;
using UnityEngine;

namespace match3.core
{
    public class GameController
    {
        private List<List<Tile>> _boardTiles;
        private List<TileType> _tilesTypes;
        private int _tileCount;

        public List<List<Tile>> StartGame(TileType[] _availableTileTypes, int boardWidth, int boardHeight)
        {
            _tilesTypes = new List<TileType>();
            foreach (var newTileType in _availableTileTypes)
                if (newTileType != TileType.NONE && !_tilesTypes.Contains(newTileType)) // Shouldn't allow duplicates
                    _tilesTypes.Add(newTileType);

            _boardTiles = CreateBoard(boardWidth, boardHeight, _tilesTypes);
            return _boardTiles;
        }

        public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
        {
            // TODO: Change this method to work without deep-copying the entire board
            List<List<Tile>> newBoard = CopyBoard(_boardTiles);

            Tile switchedTile = newBoard[fromY][fromX];
            newBoard[fromY][fromX] = newBoard[toY][toX];
            newBoard[toY][toX] = switchedTile;

            // TODO: No need for 2 nested for loops
            for (int y = 0; y < newBoard.Count; y++)
            {
                for (int x = 0; x < newBoard[y].Count; x++)
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
            List<List<Tile>> newBoard = CopyBoard(_boardTiles);

            Tile switchedTile = newBoard[fromY][fromX];
            newBoard[fromY][fromX] = newBoard[toY][toX];
            newBoard[toY][toX] = switchedTile;

            List<BoardSequence> boardSequences = new List<BoardSequence>();
            List<List<bool>> matchedTiles;
            while (HasMatch(matchedTiles = FindMatches(newBoard)))
            {
                //Cleaning the matched tiles
                List<Vector2Int> matchedPosition = new List<Vector2Int>();
                for (int y = 0; y < newBoard.Count; y++)
                {
                    for (int x = 0; x < newBoard[y].Count; x++)
                    {
                        if (matchedTiles[y][x])
                        {
                            matchedPosition.Add(new Vector2Int(x, y));
                            newBoard[y][x] = new Tile();
                        }
                    }
                }

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
                for (int y = newBoard.Count - 1; y > -1; y--)
                {
                    for (int x = newBoard[y].Count - 1; x > -1; x--)
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

                BoardSequence sequence = new BoardSequence(
                    matchedPosition,
                    addedTiles,
                    movedTilesList
                    );
                boardSequences.Add(sequence);
            }

            _boardTiles = newBoard;
            return boardSequences;
        }

        private static bool HasMatch(List<List<bool>> list)
        {
            for (int y = 0; y < list.Count; y++)
                for (int x = 0; x < list[y].Count; x++)
                    if (list[y][x])
                        return true;
            return false;
        }

        private static List<List<bool>> FindMatches(List<List<Tile>> board)
        {
            List<List<bool>> matchedTiles = new List<List<bool>>();
            for (int y = 0; y < board.Count; y++)
            {
                matchedTiles.Add(new List<bool>(board[y].Count));
                for (int x = 0; x < board.Count; x++)
                {
                    matchedTiles[y].Add(false);
                }
            }

            for (int y = 0; y < board.Count; y++)
            {
                for (int x = 0; x < board[y].Count; x++)
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

        /// <summary>
        /// Deep copies an entire board
        /// TODO: Move this to the new Board class
        /// </summary>
        /// <param name="boardToCopy"></param>
        /// <returns></returns>
        private static List<List<Tile>> CopyBoard(List<List<Tile>> boardToCopy)
        {
            List<List<Tile>> newBoard = new List<List<Tile>>(boardToCopy.Count);
            for (int y = 0; y < boardToCopy.Count; y++)
            {
                newBoard.Add(new List<Tile>(boardToCopy[y].Count));
                for (int x = 0; x < boardToCopy[y].Count; x++)
                {
                    Tile tile = boardToCopy[y][x];
                    newBoard[y].Add(new Tile(id: tile.id, type: tile.type));
                }
            }

            return newBoard;
        }

        private List<List<Tile>> CreateBoard(int width, int height, List<TileType> tileTypes)
        {
            // Create uninitialized board
            List<List<Tile>> board = new List<List<Tile>>(height);
            _tileCount = 0;
            for (int y = 0; y < height; y++)
            {
                board.Add(new List<Tile>(width));
                for (int x = 0; x < width; x++)
                {
                    board[y].Add(new Tile(id: -1, type: TileType.NONE));
                }
            }

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
    }
}