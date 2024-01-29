using System.Collections.Generic;
using UnityEngine;

public class BoardSequence
{
    // TODO: All of this should be private with accessors
    public List<Vector2Int> matchedPosition;
    public List<AddedTileInfo> addedTiles;
    public List<MovedTileInfo> movedTiles;

    public override string ToString()
    {
        // TODO: StringBuilder
        string log;
        log = "matchedPosition: \n";
        for (int i = 0; i < matchedPosition.Count; i++)
        {
            log += $"{matchedPosition[i]}, ";
        }

        log += "\naddedTiles: \n";
        for (int i = 0; i < addedTiles.Count; i++)
        {
            log += $"{addedTiles[i].position}, ";
        }

        log += $"\nmovedTiles: {movedTiles.Count}\n";
        for (int i = 0; i < movedTiles.Count; i++)
        {
            log += $"{movedTiles[i].from} - {movedTiles[i].to}, ";
        }

        //log = $"matchedPosition: {matchedPosition.Count} - addedTiles: {addedTiles.Count} - movedTiles: {movedTiles.Count}";
        return log;
    }
}