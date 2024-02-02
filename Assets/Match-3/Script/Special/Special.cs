namespace match3.special
{
    public class Special
    {
        public bool active { get; private set; }
        public int warmupSeconds { get; private set; }
        public int durationSeconds { get; private set; }

        public Special(int durationSeconds, int warmupSeconds)
        {
            this.warmupSeconds = warmupSeconds;
            this.durationSeconds = durationSeconds;

            this.active = false;
        }

        public void SetActive(bool active)
        {
            this.active = active;
        }
    }
}
