using match3.board;
using match3.progress;
using match3.settings;
using match3.special;
using match3.tile;
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
        public Special clearLinesSpecial { get; private set; }
        public Special explosionSpecial { get; private set; }
        public Special colorClearSpecial { get; private set; }
        public Special tipSpecial { get; private set; }

        public Level currentLevel { get; private set; }

        private List<TileType> _tilesTypes;
        private int _tileCount;

        public Board StartGame(Level level)
        {
            currentLevel = level;

            _tilesTypes = new List<TileType>();
            foreach (var newTileType in level.availableTileTypes)
                if (newTileType != TileType.NONE && !_tilesTypes.Contains(newTileType)) // Shouldn't allow duplicates
                    _tilesTypes.Add(newTileType);

            _board = CreateBoard(level.boardWidth, level.boardHeight, _tilesTypes);

            // Progress
            progress = new Progress();
            progress.SetLevel(level.level);
            progress.SetGoal(level.goalScore);


            return _board;
        }

        public void SetSpecials(
            int clearLinesDurationSec, int clearLinesWarmupSec,
            int explosionDurationSec, int explosionWarmupSec,
            int colorClearDurationSec, int colorClearWarmupSec,
            int tipDurationSec, int tipWarmupSec
            )
        {
            // Specials
            clearLinesSpecial = new Special(clearLinesDurationSec, clearLinesWarmupSec);
            explosionSpecial = new Special(explosionDurationSec, explosionWarmupSec);
            colorClearSpecial = new Special(colorClearDurationSec, colorClearWarmupSec);
            tipSpecial = new Special(tipDurationSec, tipWarmupSec);
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

        /// <summary>
        /// Finds all possible 1-away matches
        /// </summary>
        /// <returns>A list of positions that are 1-away from a match </returns>
        public List<Vector2Int> GetMatchTipsBruteForce() => GetMatchTipsBruteForce(_board);
        private List<Vector2Int> GetMatchTipsBruteForce(Board board)
        {
            // TODO: Optimize this
            List<Vector2Int> matchedTipPositions = new List<Vector2Int>();

            for (int y = 0; y < board.lines; y++)
            {
                for (int x = 0; x < board.columns; x++)
                {
                    Vector2Int[] neighbors = board.GetNeighborTilePositions(x, y);
                    foreach (Vector2Int neighbor in neighbors)
                    {
                        Board newBoard = board.Clone();

                        Tile switchedTile = newBoard[y][x];
                        newBoard[y][x] = newBoard[neighbor.y][neighbor.x];
                        newBoard[neighbor.y][neighbor.x] = switchedTile;

                        if (newBoard.IsMatch(x, y))
                        {
                            matchedTipPositions.Add(new Vector2Int(neighbor.x, neighbor.y));
                        }
                    }
                }
            }

            return matchedTipPositions;
        }

        public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
        {
            Board newBoard = _board.Clone();

            Tile switchedTile = newBoard[fromY][fromX];
            newBoard[fromY][fromX] = newBoard[toY][toX];
            newBoard[toY][toX] = switchedTile;

            int totalScore = progress.score;
            int loopCount = 0;
            List<BoardSequence> boardSequences = new List<BoardSequence>();
            List<List<bool>> matchedTiles;
            while (HasMatch(matchedTiles = FindMatches(newBoard)))
            {
                loopCount++;

                //Cleaning the matched tiles
                List<Vector2Int> matchedPosition = new List<Vector2Int>();
                for (int y = 0; y < newBoard.lines; y++)
                    for (int x = 0; x < newBoard.columns; x++)
                        if (matchedTiles[y][x])
                            matchedPosition.Add(new Vector2Int(x, y));

                if (loopCount == 1 && clearLinesSpecial.active)
                    SwapTileClearLinesSpecial(newBoard, matchedPosition);

                if (loopCount == 1 && explosionSpecial.active)
                    SwapTileExplosionSpecial(newBoard, matchedPosition);

                if (loopCount == 1 && colorClearSpecial.active)
                    SwapTileColorClearSpecial(newBoard, matchedPosition);

                // Remove duplicated matches
                matchedPosition = GetDistinctMatchedPositions(matchedPosition);

                // Update the new board with the distinct matched positions
                foreach (var match in matchedPosition)
                    newBoard[match.y][match.x] = new Tile();

                // Dropping the tiles
                List<MovedTileInfo> movedTilesList = new List<MovedTileInfo>();
                Queue<int> availableSlots = new Queue<int>();

                for (int x = newBoard.columns - 1; x > -1; x--)
                {
                    availableSlots.Clear();

                    // Go up every column, dropping valid tiles to the lowest available slot
                    // Look at this as a TETRIS game
                    for (int y = newBoard.lines - 1; y > -1; y--)
                    {
                        // Mark empty slots as free\available
                        if (newBoard[y][x].type == TileType.NONE)
                        {
                            if (!availableSlots.Contains(y))
                                availableSlots.Enqueue(y);
                        }
                        else if (availableSlots.Count > 0)
                        {
                            // If there are free slots, move the current tile to the bottom
                            Tile movedTile = newBoard[y][x];
                            int availableY = availableSlots.Dequeue();
                            newBoard[availableY][x] = movedTile;

                            // Create the MoveTileInfo for this action
                            MovedTileInfo movedTileInfo = new MovedTileInfo
                            {
                                from = new Vector2Int(x, y),
                                to = new Vector2Int(x, availableY)
                            };
                            movedTilesList.Add(movedTileInfo);

                            // Now add the current slot as free
                            newBoard[y][x] = new Tile();
                            if (!availableSlots.Contains(y))
                                availableSlots.Enqueue(y);
                        }
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

            progress.SetScore(totalScore);
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
                    if (board.IsHorizontalRightMatch(x, y))
                    {
                        matchedTiles[y][x] = true;
                        matchedTiles[y][x - 1] = true;
                        matchedTiles[y][x - 2] = true;
                    }
                    if (board.IsVerticalBottomMatch(x, y))
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
            clearLinesSpecial.SetActive(active);
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
            explosionSpecial.SetActive(active);
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
            }
        }


        // ========================== Color Clear ============================


        public void SetSpecialColorClearActive(bool active)
        {
            colorClearSpecial.SetActive(active);
        }

        private void SwapTileColorClearSpecial(Board newBoard, List<Vector2Int> matchedPosition)
        {
            // If the color clear special is active, look for matches and 
            // include the all other tiles from the same type as matches

            List<TileType> tileTypesToClear = new List<TileType>();
            foreach (var match in matchedPosition)
                if (!tileTypesToClear.Contains(newBoard[match.y][match.x].type))
                    tileTypesToClear.Add(newBoard[match.y][match.x].type);

            // Mark all tiles of the same type as a valid match
            for (int y = 0; y < newBoard.lines; y++)
                for (int x = 0; x < newBoard.columns; x++)
                    if (tileTypesToClear.Contains(newBoard[y][x].type))
                        matchedPosition.Add(new Vector2Int(x, y));
        }


        // ========================== Tip ============================

        public void SetSpecialTipActive(bool active)
        {
            tipSpecial.SetActive(active);
        }
    }
}