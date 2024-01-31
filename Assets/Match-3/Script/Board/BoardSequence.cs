using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace match3.board
{
    public class BoardSequence
    {
        public List<Vector2Int> matchedPosition { get; private set; }
        public List<AddedTileInfo> addedTiles { get; private set; }
        public List<MovedTileInfo> movedTiles { get; private set; }
        public int score { get; private set; }

        public BoardSequence(
            List<Vector2Int> matchedPosition,
            List<AddedTileInfo> addedTiles,
            List<MovedTileInfo> movedTiles,
            int score
            )
        {
            this.matchedPosition = matchedPosition;
            this.addedTiles = addedTiles;
            this.movedTiles = movedTiles;
            this.score = score;
        }

        public override string ToString()
        {
            StringBuilder logSB = new StringBuilder();

            logSB.Append("matchedPosition: \n");
            for (int i = 0; i < matchedPosition.Count; i++)
            {
                logSB.Append($"{matchedPosition[i]}, ");
            }

            logSB.Append("\naddedTiles: \n");
            for (int i = 0; i < addedTiles.Count; i++)
            {
                logSB.Append($"{addedTiles[i].position}, ");
            }

            logSB.Append($"\nmovedTiles: {movedTiles.Count}\n");
            for (int i = 0; i < movedTiles.Count; i++)
            {
                logSB.Append($"{movedTiles[i].from} - {movedTiles[i].to}, ");
            }

            //log = $"matchedPosition: {matchedPosition.Count} - addedTiles: {addedTiles.Count} - movedTiles: {movedTiles.Count}";
            return logSB.ToString();
        }
    }
}