using match3.tile;
using UnityEngine;

namespace match3.settings
{
    [CreateAssetMenu(fileName = "TilePrefabRepository", menuName = "Gameplay/TilePrefabRepository")]
    public class TilePrefabRepository : ScriptableObject
    {
        public TileView[] tileTypePrefabList;
    }
}