using UnityEngine;

namespace Assets.Scripts.Entity.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Character Customizer")]
    public class CharacterSO : ScriptableObject
    {
        // Body Colors
        public Color[] SkinColor;
        public Color[] HairColor;
        public Color[] HairHighlightColor;
        public Color[] EyeColor;

        // Body Meshes GameObject[1] allows null values for the first entry
        public GameObject[] Hair = new GameObject[1];
        public GameObject[] HairBangs = new GameObject[1];
        public GameObject[] HairBeard = new GameObject[1];
        public GameObject[] HairMustache = new GameObject[1];
    }
}
