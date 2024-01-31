using UnityEngine;
using UnityEngine.UI;

namespace match3.settings
{
    [CreateAssetMenu(fileName = "SpecialRepository", menuName = "Gameplay/SpecialRepository")]
    public class SpecialRepository : ScriptableObject
    {
        [SerializeField] private SpecialItem _clearLineSpecial;
        [SerializeField] private SpecialItem _explosionSpecial;
        [SerializeField] private SpecialItem _colorClearSpecial;

        public SpecialItem clearLinesSpecial => _clearLineSpecial;
        public SpecialItem explosionSpecial => _explosionSpecial;
        public SpecialItem colorClearSpecial => _colorClearSpecial;
    }

    [System.Serializable]
    public class SpecialItem
    {

        [SerializeField] private Sprite _icon;
        [SerializeField] private string _name;
        [SerializeField][Range(0, 30)] private int _warmupSeconds = 10;
        [SerializeField][Range(0, 30)] private int _durationSeconds = 10;

        public Sprite icon => _icon;
        public string name => _name;
        public int warmupSeconds => _warmupSeconds;
        public int durationSeconds => _durationSeconds;
    }
}