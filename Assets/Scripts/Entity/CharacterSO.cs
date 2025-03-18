using UnityEngine;

namespace Assets.Scripts.Entity
{
    [CreateAssetMenu(menuName = "Player Model Customizer")]
    public class CharacterSO : ScriptableObject
    {
        // Body Colors
        public Color[] SkinColor;
        public Color[] HairColor;
        public Color[] HairHighlightColor;
        public Color[] EyeColor;

        // Body Meshes
        public Mesh[] Hair;
        public Mesh[] HairBangs;
        public Mesh[] HairFacial;

        // Accessories
        public Mesh[] UpperBody;
        public Mesh[] LowerBody;
        public Mesh[] Shoes;
        public Mesh[] Helmets;
    }
}
