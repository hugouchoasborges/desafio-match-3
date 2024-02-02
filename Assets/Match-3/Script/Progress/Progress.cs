using System;

namespace match3.progress
{
    public class Progress
    {
        public int level { get; private set; }
        public int score { get; private set; }
        public int goal { get; private set; }

        public bool IsLevelFinished => score >= goal;

        public Progress(int level = 1, int score = 0, int goal = 0)
        {
            this.level = level;
            this.score = score;
            this.goal = goal;
        }


        // ========================== Setters ============================

        public void AddScore(int value)
        {
            SetScore(score + value);
        }

        public void SetScore(int value)
        {
            score = Math.Max(0, value);
        }

        public void SetLevel(int value)
        {
            level = Math.Max(0, value);
        }

        public void SetGoal(int value)
        {
            goal = value;
        }
    }
}
