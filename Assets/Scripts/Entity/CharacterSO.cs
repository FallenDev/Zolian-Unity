using UnityEngine;

namespace Assets.Scripts.Entity
{
    [CreateAssetMenu(menuName = "Character Customizer")]
    public class CharacterSO : ScriptableObject
    {
        // Body Colors
        public Color[] SkinColor;
        public Color[] HairColor;
        public Color[] HairHighlightColor;
        public Color[] EyeColor;

        // Body Meshes
        public GameObject[] Hair;
        public GameObject[] HairBangs;
        public GameObject[] HairBeard;
        public GameObject[] HairMustache;
    }
}
