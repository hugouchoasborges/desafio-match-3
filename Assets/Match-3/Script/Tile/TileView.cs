using DG.Tweening;
using UnityEngine;

namespace match3.tile
{
    public class TileView : MonoBehaviour
    {
        public void SetParent(Transform parentTransform, bool worldPositionStays = false)
        {
            transform.SetParent(parentTransform, worldPositionStays);
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        /// <summary>
        /// Stops all running transform tweening operations then 
        /// moves to the corresponding location and duration
        /// </summary>
        /// <param name="to"></param>
        /// <param name="duration"></param>
        /// <returns>The DG.Tweening.Tween created from this operation</returns>
        public Tween DOMove(Vector3 to, float duration)
        {
            transform.DOKill();
            return transform.DOMove(to, duration);
        }
    }
}