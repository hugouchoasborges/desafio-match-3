using match3.tile;
using System.Collections.Generic;
using UnityEngine;

namespace match3.board
{
    public class Board
    {
        public int columns { get; private set; }
        public int lines { get; private set; }

        public List<List<Tile>> tiles;
        public int tilesCount { get; private set; } = 0;

        // Indexer to access tiles by [x][y]
        public List<Tile> this[int idx]
        {
            get { return tiles[idx]; }
            set { tiles[idx] = value; }
        }

        public Board(int columns, int lines)
        {
            this.columns = columns;
            this.lines = lines;

            Reset();
        }

        private void Reset()
        {
            tiles = new List<List<Tile>>(lines);
            tilesCount = 0;

            for (int y = 0; y < lines; y++)
            {
                tiles.Add(new List<Tile>(columns));
                for (int x = 0; x < columns; x++)
                {
                    tiles[y].Add(new Tile(id: -1, type: TileType.NONE));
                    tilesCount++;
                }
            }
        }

        public Board Clone()
        {
            Board clone = new Board(columns, lines);

            for (int y = 0; y < lines; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    clone[y][x].SetID(this[y][x].id);
                    clone[y][x].SetType(this[y][x].type);
                }
            }
            return clone;
        }


        // ========================== Helper Methods ============================

        public Vector2Int[] GetNeighborTilePositions(int x, int y)
        {
            List<Vector2Int> neighbors = new List<Vector2Int>();

            if (x > 0) neighbors.Add(new Vector2Int(x - 1, y));
            if (x < columns - 1) neighbors.Add(new Vector2Int(x + 1, y));

            if (y > 0) neighbors.Add(new Vector2Int(x, y - 1));
            if (y < lines - 1) neighbors.Add(new Vector2Int(x, y + 1));

            return neighbors.ToArray();
        }


        // ========================== Match identifying ============================

        public bool IsMatch(int x, int y)
        {
            return
                IsHorizontalRightMatch(x, y)
                || IsHorizontalMiddleMatch(x, y)
                || IsHorizontalLeftMatch(x, y)
                || IsVerticalBottomMatch(x, y)
                || IsVerticalMiddleMatch(x, y)
                || IsVerticalTopMatch(x, y);
        }

        public bool IsHorizontalRightMatch(int x, int y)
        {
            if (x > 1
                && tiles[y][x].type == tiles[y][x - 1].type
                && tiles[y][x - 1].type == tiles[y][x - 2].type)
            {
                return true;
            }

            return false;
        }

        public bool IsHorizontalMiddleMatch(int x, int y)
        {
            if (x > 0 && x < columns - 1
                && tiles[y][x].type == tiles[y][x - 1].type
                && tiles[y][x - 1].type == tiles[y][x + 1].type)
            {
                return true;
            }

            return false;
        }

        public bool IsHorizontalLeftMatch(int x, int y)
        {
            if (x < columns - 2
                && tiles[y][x].type == tiles[y][x + 1].type
                && tiles[y][x + 1].type == tiles[y][x + 2].type)
            {
                return true;
            }

            return false;
        }

        public bool IsVerticalBottomMatch(int x, int y)
        {
            if (y > 1
                && tiles[y][x].type == tiles[y - 1][x].type
                && tiles[y - 1][x].type == tiles[y - 2][x].type)
            {
                return true;
            }

            return false;
        }

        public bool IsVerticalMiddleMatch(int x, int y)
        {
            if (y > 0 && y < lines - 1
                && tiles[y][x].type == tiles[y - 1][x].type
                && tiles[y - 1][x].type == tiles[y + 1][x].type)
            {
                return true;
            }

            return false;
        }

        public bool IsVerticalTopMatch(int x, int y)
        {
            if (y < lines - 2
                && tiles[y][x].type == tiles[y + 1][x].type
                && tiles[y + 1][x].type == tiles[y + 2][x].type)
            {
                return true;
            }

            return false;
        }
    }
}
