using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reallusion.Import;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Entity
{
    public class Customizer : MonoBehaviour
    {
        public CharacterSO Models;
        public SkinnedMeshRenderer SkinColor;
        public SkinnedMeshRenderer HairColor;
        public SkinnedMeshRenderer HairHighlightColor;
        public SkinnedMeshRenderer EyeColor;
        public SkinnedMeshRenderer Hair;
        public SkinnedMeshRenderer HairBangs;
        public SkinnedMeshRenderer HairFacial;
        public SkinnedMeshRenderer UpperBody;
        public SkinnedMeshRenderer LowerBody;
        public SkinnedMeshRenderer Shoes;
        public SkinnedMeshRenderer Helmets;

        private void Start()
        {

        }

        private void Update()
        {

        }

        public void Randomizer()
        {
            Hair.sharedMesh = Models.Hair[UnityEngine.Random.Range(0, Models.Hair.Length)];
            HairBangs.sharedMesh = Models.HairBangs[UnityEngine.Random.Range(0, Models.HairBangs.Length)];
            HairFacial.sharedMesh = Models.HairFacial[UnityEngine.Random.Range(0, Models.HairFacial.Length)];
        }

        public void CustomizeCharacter()
        {
            // Implement customization logic
        }

        public void ResetCustomization()
        {
            // Implement reset logic
        }

        public void SaveCustomization()
        {
            // Implement save logic
        }

        public void LoadCustomization()
        {
            // Implement load logic
        }
    }

    [CustomEditor(typeof(Customizer))]
    public class CustomizerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var myScript = (Customizer)target;
            if (GUILayout.Button("Randomize"))
            {
                myScript.Randomizer();
            }
        }
    }
}
