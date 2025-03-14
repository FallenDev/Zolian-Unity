using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rukha93.ModularAnimeCharacter
{
    [ExecuteInEditMode]
    public class BlendshapeSync : MonoBehaviour
    {
        [System.Serializable]
        public class BlendshapeItem
        {
            [HideInInspector] public string name;
            [HideInInspector] public List<(SkinnedMeshRenderer, int)> renderers;
            [HideInInspector] public float oldValue;
            [Range(0, 100)] public float value;

            public BlendshapeItem(string name, float value)
            {
                this.name = name;
                this.oldValue = this.value = value;
                renderers = new List<(SkinnedMeshRenderer, int)>();
            }

            public void UpdateBlendshape()
            {
                if (oldValue != value)
                {
                    oldValue = value;
                    for (int i = 0; i < renderers.Count; i++)
                        renderers[i].Item1.SetBlendShapeWeight(renderers[i].Item2, value);
                }
            }
        }

        [SerializeField] private BlendshapeItem[] m_Blendshapes;

        private void OnEnable()
        {
            var renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            var blendshapeMap = new Dictionary<string, BlendshapeItem>();

            //get all available blendshapes and map them
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                SkinnedMeshRenderer renderer = renderers[rendererIndex];
                Mesh sharedMesh = renderer.sharedMesh;

                for (int blendshapeIndex = 0; blendshapeIndex < sharedMesh.blendShapeCount; blendshapeIndex++)
                {
                    string blendshapeName = sharedMesh.GetBlendShapeName(blendshapeIndex);

                    if (!blendshapeMap.ContainsKey(blendshapeName))
                        blendshapeMap[blendshapeName] = new BlendshapeItem(blendshapeName, renderer.GetBlendShapeWeight(blendshapeIndex));
                    
                    blendshapeMap[blendshapeName].renderers.Add((renderer, blendshapeIndex));
                }
            }

            //store the blendshapes
            m_Blendshapes = blendshapeMap.Values.ToArray();
        }

        private void OnValidate()
        {
            UpdateBlendshapes();
        }

        private void UpdateBlendshapes()
        {
            for (int i = 0; i < m_Blendshapes.Length; i++)
                m_Blendshapes[i].UpdateBlendshape();
        }
    }
}
