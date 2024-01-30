using match3.tile;
using System.Collections.Generic;

namespace match3.board
{
    public class Board
    {
        public int columns { get; private set; }
        public int lines { get; private set; }

        public List<List<Tile>> tiles;

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

            for (int y = 0; y < lines; y++)
            {
                tiles.Add(new List<Tile>(columns));
                for (int x = 0; x < columns; x++)
                {
                    tiles[y].Add(new Tile(id: -1, type: TileType.NONE));
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

    }
}
