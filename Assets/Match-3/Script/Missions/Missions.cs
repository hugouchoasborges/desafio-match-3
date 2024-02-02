using System;

namespace match3.missions
{
    public class Missions
    {
        public int lifes { get; private set; }

        public bool isGameOver => lifes <= 0;

        public Missions(int lifes)
        {
            this.lifes = lifes;
        }

        public void ConsumeLife()
        {
            lifes = Math.Max(0, lifes - 1);
        }
    }
}
