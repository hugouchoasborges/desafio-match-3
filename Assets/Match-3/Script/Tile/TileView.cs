using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace match3.tile
{
    public class TileView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image _selectionIcon;

        public void SetParent(Transform parentTransform, bool worldPositionStays = false)
        {
            transform.SetParent(parentTransform, worldPositionStays);
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetSelected(bool selected)
        {
            _selectionIcon.enabled = selected;
        }

        public void SetSelectedTip(bool selected)
        {
            _selectionIcon.enabled = selected;
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