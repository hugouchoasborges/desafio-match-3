using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace match3.tile
{
    public class TileView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image _selectionIcon;
        [SerializeField] private Image _tipSelectionIcon;
        [SerializeField] private ParticleSystem _destroyParticles;

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
            _tipSelectionIcon.enabled = selected;
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

        // ========================== Particles ============================

        [ContextMenu("Play Destroy Particles")]
        public void PlayDestroyParticles(bool destroyGameobject)
        {
            _destroyParticles?.Play();
            if (destroyGameobject)
            {
                DOVirtual.DelayedCall(1f, () =>
                {
                    if (this != null && this.gameObject != null)
                        Destroy(this.gameObject);
                });
            }
        }
    }
}