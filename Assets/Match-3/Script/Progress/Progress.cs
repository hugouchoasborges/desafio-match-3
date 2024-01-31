using System;

namespace match3.progress
{
    public class Progress
    {
        public int level { get; private set; }
        public int score { get; private set; }
        public int goal { get; private set; }

        public Progress(int level = 1, int score = 0, int goal = 0)
        {
            this.level = level;
            this.score = score;
            this.goal = goal;
        }


        // ========================== Setters ============================

        public void AddScore(int value)
        {
            score = Math.Max(0, score + value);
        }

        public void AddLevel(int value)
        {
            level = Math.Max(0, level + value);
        }

        public void SetGoal(int value)
        {
            goal = value;
        }
    }
}
