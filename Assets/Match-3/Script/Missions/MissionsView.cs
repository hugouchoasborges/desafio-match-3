using System.Collections.Generic;
using UnityEngine;

namespace match3.missions
{
    public class MissionsView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private HeartView _prefab;

        private List<HeartView> _hearts;

        private void Start()
        {
            _hearts = new List<HeartView>();
        }

        private void DestroyHearts()
        {
            for (int i = _hearts.Count - 1; i >= 0; i--)
            {
                Destroy(_hearts[i].gameObject);
            }

            _hearts.Clear();
        }

        public void Setup(Missions missions)
        {
            if (_hearts.Count > 0)
                DestroyHearts();

            for (int i = 0; i < missions.lifes; i++)
            {
                HeartView newHeart = Instantiate(_prefab, transform);
                newHeart.Fill();

                _hearts.Add(newHeart);
            }
        }

        public void UpdateView(Missions missions)
        {
            for (int i = _hearts.Count - 1; i >= 0; i--)
            {
                if ((i + 1) > missions.lifes)
                {
                    if (_hearts[i].filled)
                        _hearts[i].Empty();
                }

                else if (!_hearts[i].filled)
                    _hearts[i].Fill();
            }
        }
    }
}
