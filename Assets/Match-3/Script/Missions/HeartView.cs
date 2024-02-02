using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace match3.missions
{
    public class HeartView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private Image _fillImage;

        public bool filled => _fillImage.fillAmount == 1;

        // ========================== Animations  ============================

        private void AnimateFill(float from, float to)
        {
            _fillImage.fillAmount = from;

            DOTween.To(
                () => _fillImage.fillAmount,
                fillAmount => _fillImage.fillAmount = fillAmount,
                to, .5f);
        }

        public void Fill() => AnimateFill(0, 1);
        public void Empty() => AnimateFill(1, 0);
    }
}
