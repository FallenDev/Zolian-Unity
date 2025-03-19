using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reallusion.Import;
using UnityEditor;
using UnityEngine;

// ToDo: Not used, kept as a reference
namespace Assets.Scripts.Entity
{
    public class Customizer : MonoBehaviour
    {
        public CharacterSO ScriptableData;
        public GameObject BaseHead;
        public GameObject ArmsLower;
        public GameObject ArmsUpper;
        public GameObject Feet;
        public GameObject Hands;
        public GameObject Hips;
        public GameObject LegsLower;
        public GameObject LegsUpper;
        public GameObject LegsKnee;
        public GameObject Shoulders;
        public GameObject Neck;
        public GameObject Chest;
        public GameObject Abdomen;
        public SkinnedMeshRenderer HeadBlendShapes;
        // BlendShapes
        public float Happy01Weight;
        public float Happy02Weight;
        public float Happy03Weight;
        public float Sad01Weight;
        public float Sad02Weight;
        public float Sad03Weight;
        public float Angry01Weight;
        public float Angry02Weight;
        public float Angry03Weight;

        private void Start()
        {

        }

        private void Update()
        {

        }

        public void Randomizer()
        {
            var happyOneInd = HeadBlendShapes.sharedMesh.GetBlendShapeIndex("happy_01");
            if (happyOneInd != -1)
            {
                HeadBlendShapes.SetBlendShapeWeight(happyOneInd, Happy01Weight);
            }
            var happyTwoInd = HeadBlendShapes.sharedMesh.GetBlendShapeIndex("happy_02");
            if (happyTwoInd != -1)
            {
                HeadBlendShapes.SetBlendShapeWeight(happyTwoInd, Happy02Weight);
            }
            var happyThreeInd = HeadBlendShapes.sharedMesh.GetBlendShapeIndex("happy_03");
            if (happyThreeInd != -1)
            {
                HeadBlendShapes.SetBlendShapeWeight(happyThreeInd, Happy03Weight);
            }
            var sadOneInd = HeadBlendShapes.sharedMesh.GetBlendShapeIndex("sad_01");
            if (sadOneInd != -1)
            {
                HeadBlendShapes.SetBlendShapeWeight(sadOneInd, Sad01Weight);
            }
            var sadTwoInd = HeadBlendShapes.sharedMesh.GetBlendShapeIndex("sad_02");
            if (sadTwoInd != -1)
            {
                HeadBlendShapes.SetBlendShapeWeight(sadTwoInd, Sad02Weight);
            }
            var sadThreeInd = HeadBlendShapes.sharedMesh.GetBlendShapeIndex("sad_03");
            if (sadThreeInd != -1)
            {
                HeadBlendShapes.SetBlendShapeWeight(sadThreeInd, Sad03Weight);
            }
            var angryOneInd = HeadBlendShapes.sharedMesh.GetBlendShapeIndex("angry_01");
            if (angryOneInd != -1)
            {
                HeadBlendShapes.SetBlendShapeWeight(angryOneInd, Angry01Weight);
            }
            var angryTwoInd = HeadBlendShapes.sharedMesh.GetBlendShapeIndex("angry_02");
            if (angryTwoInd != -1)
            {
                HeadBlendShapes.SetBlendShapeWeight(angryTwoInd, Angry02Weight);
            }
            var angryThreeInd = HeadBlendShapes.sharedMesh.GetBlendShapeIndex("angry_03");
            if (angryThreeInd != -1)
            {
                HeadBlendShapes.SetBlendShapeWeight(angryThreeInd, Angry03Weight);
            }

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
