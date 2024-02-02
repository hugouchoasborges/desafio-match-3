using match3.tile;
using UnityEngine;

namespace match3.settings
{
    [CreateAssetMenu(fileName = "LevelRepository", menuName = "Gameplay/LevelRepository")]
    public class LevelRepository : ScriptableObject
    {
        [SerializeField] private Level[] _levels;

        public Level this[int idx]
        {
            get { return _levels[System.Math.Min(idx, _levels.Length - 1)]; }
        }
    }

    [System.Serializable]
    public class Level
    {
        [SerializeField] private int _level;
        [SerializeField] private int _goalScore;
        [SerializeField] private TileType[] _availableTileTypes;
        [SerializeField][Range(2, 20)] private int _boardWidth = 10;
        [SerializeField][Range(2, 20)] private int _boardHeight = 10;

        public int goalScore => _goalScore;
        public int level => _level;
        public TileType[] availableTileTypes => _availableTileTypes;
        public int boardWidth => _boardWidth;
        public int boardHeight => _boardHeight;
    }
}