using UnityEngine;

[CreateAssetMenu(fileName = "TilePrefabRepository", menuName = "Gameplay/TilePrefabRepository")]
public class TilePrefabRepository : ScriptableObject
{
    public TileView[] tileTypePrefabList;
}