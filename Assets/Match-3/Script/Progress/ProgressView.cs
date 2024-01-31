using TMPro;
using UnityEngine;

namespace match3.progress
{
    public class ProgressView : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _goalText;

        public void UpdateProgress(Progress progress)
        {
            SetLevel(progress.level);
            SetScore(progress.score);
            SetGoal(progress.goal);
        }


        // ========================== Setters ============================

        public void SetLevel(int value)
        {
            _levelText.text = "Level: " + value;
        }

        public void SetScore(int value)
        {
            _scoreText.text = "Score: " + value;
        }
        public void SetGoal(int value)
        {
            _goalText.text = "Goal: " + value;
        }
    }
}
