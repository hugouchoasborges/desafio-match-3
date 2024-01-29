namespace match3.tile
{
    public class Tile
    {
        public int id { get; private set; }
        public TileType type { get; private set; }

        public void SetID(int id) { this.id = id; }
        public void SetType(TileType type) { this.type = type; }

        public Tile(int id = -1, TileType type = TileType.NONE)
        {
            this.id = id;
            this.type = type;
        }
    }
}