using match3.tile;
using UnityEngine;

namespace match3.settings
{
    [CreateAssetMenu(fileName = "TilePrefabRepository", menuName = "Gameplay/TilePrefabRepository")]
    public class TilePrefabRepository : ScriptableObject
    {
        [SerializeField] private TileSpotView _tileSpotPrefab;
        [SerializeField] private TilePrefabItem[] _tilePrefabList;

        public TileSpotView TileSpotPrefab => _tileSpotPrefab;

        public TileView GetTileFromType(TileType tileType)
        {
            foreach (var tile in _tilePrefabList)
            {
                if (tile.tileType == tileType)
                    return tile.prefab;
            }
            return null;
        }
    }

    [System.Serializable]
    public class TilePrefabItem
    {
        [SerializeField] private TileType _tileType;
        [SerializeField] private TileView _prefab;

        public TileType tileType => _tileType;
        public TileView prefab => _prefab;
    }
}