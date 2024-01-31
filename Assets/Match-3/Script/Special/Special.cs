namespace match3.special
{
    public class Special
    {
        public string name { get; private set; }
        public bool active { get; private set; }

        public Special(string name)
        {
            this.name = name;
            this.active = false;
        }

        public void SetActive(bool active)
        {
            this.active = active;
        }
    }
}
