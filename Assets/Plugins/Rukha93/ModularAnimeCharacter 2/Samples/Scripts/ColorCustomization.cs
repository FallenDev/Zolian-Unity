using System.Collections.Generic;
using UnityEngine;

namespace Rukha93.ModularAnimeCharacter.Customization
{
    [ExecuteInEditMode]
    public class ColorCustomization : MonoBehaviour
    {
        [System.Serializable]
        public class ColorData
        {
            [HideInInspector]
            public string name;

            [HideInInspector]
            public Material sharedMaterial;
            
            public Color shadeColor_A, mainColor_A;
            public Color shadeColor_B, mainColor_B;
            public Color shadeColor_C, mainColor_C;
        }

        [SerializeField] private Renderer[] m_Renderers;
        [SerializeField] private List<ColorData> m_Colors;

        private void OnEnable()
        {
            //initialize dependencies
            m_Renderers = GetComponentsInChildren<Renderer>();                            
            if (m_Colors == null)     
                m_Colors = new List<ColorData>();

            
            //initialize the colors or refresh them in case a mesh has been enabled or disabled
            GetColors();
            ApplyColors();
        }

        private void OnDisable()
        {
            //disable the colors override when the component is disabled
            ResetColors();            
        }
        
        private void OnValidate()
        {            
            //apply the new colors to the renderers propertyblocks 
            ApplyColors();
        }

        private void GetColors()
        {
            //get the current material colors
            for(int rendererIndex = 0; rendererIndex < m_Renderers.Length; rendererIndex++)
            {
                for (int materialIndex = 0; materialIndex < m_Renderers[rendererIndex].sharedMaterials.Length; materialIndex++)
                {
                    Material material = m_Renderers[rendererIndex].sharedMaterials[materialIndex];
                    if (material == null)
                    {
                        Debug.LogError($"missing material at {m_Renderers[rendererIndex].name}.sharedMaterials[{materialIndex}]");
                        continue;
                    }

                    if (IsMaterialMapped(material))
                        continue;
                    
                    ColorData colorData = GetColorData(material);
                    colorData.sharedMaterial = material;
                    colorData.name = material.name;

                    m_Colors.Add(colorData);
                    
                    MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                    m_Renderers[rendererIndex].GetPropertyBlock(propertyBlock, materialIndex);

                    //check if its using the color customization shader
                    if (material.HasColor("_Color_A_1"))
                    {
                        //get the color in the property block if available, else get the one in the material
                        colorData.shadeColor_A = GetColor("_Color_A_1", material, propertyBlock);
                        colorData.mainColor_A = GetColor("_Color_A_2", material, propertyBlock);
                        colorData.shadeColor_B = GetColor("_Color_B_1", material, propertyBlock);
                        colorData.mainColor_B = GetColor("_Color_B_2", material, propertyBlock);
                        colorData.shadeColor_C = GetColor("_Color_C_1", material, propertyBlock);
                        colorData.mainColor_C = GetColor("_Color_C_2", material, propertyBlock);

                        continue;
                    }

                    //standard shaders
                    if (material.HasColor("_Color"))
                    {
                        colorData.mainColor_C = GetColor("_Color", material, propertyBlock);
                        continue;
                    }

                    if (material.HasColor("_MainColor"))
                    {
                        colorData.mainColor_C = GetColor("_MainColor", material, propertyBlock);
                        continue;
                    }

                    if (material.HasColor("_BaseColor"))
                    {
                        colorData.mainColor_C = GetColor("_BaseColor", material, propertyBlock);
                        continue;
                    }
                }
            }
        }

        private void ApplyColors()
        {
            //update material colors
            for(int rendererIndex = 0; rendererIndex < m_Renderers.Length; rendererIndex++)
            {
                for (int materialIndex = 0; materialIndex < m_Renderers[rendererIndex].sharedMaterials.Length; materialIndex++)
                {
                    Material material = m_Renderers[rendererIndex].sharedMaterials[materialIndex];
                    if (material == null)
                        continue;
                    
                    ColorData colorData = GetColorData(material);

                    MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
                    m_Renderers[rendererIndex].GetPropertyBlock(propertyBlock, materialIndex);

                    if (material.HasColor("_Color_A_1"))
                    {
                        propertyBlock.SetColor("_Color_A_1", colorData.shadeColor_A);
                        propertyBlock.SetColor("_Color_A_2", colorData.mainColor_A);
                        propertyBlock.SetColor("_Color_B_1", colorData.shadeColor_B);
                        propertyBlock.SetColor("_Color_B_2", colorData.mainColor_B);
                        propertyBlock.SetColor("_Color_C_1", colorData.shadeColor_C);
                        propertyBlock.SetColor("_Color_C_2", colorData.mainColor_C);
                    }
                    else if (material.HasColor("_Color"))
                    {
                        propertyBlock.SetColor("_Color", colorData.mainColor_C);
                    }
                    else if (material.HasColor("_MainColor"))
                    {
                        propertyBlock.SetColor("_MainColor", colorData.mainColor_C);
                    }
                    else if (material.HasColor("_BaseColor"))
                    {
                        propertyBlock.SetColor("_BaseColor", colorData.mainColor_C);
                    }

                    m_Renderers[rendererIndex].SetPropertyBlock(propertyBlock, materialIndex);
                }
            }
        }

        private void ResetColors()
        {
            //clear the property blocks for each material in the renderers
            for(int rendererIndex = 0; rendererIndex < m_Renderers.Length; rendererIndex++)
                for (int materialIndex = 0; materialIndex < m_Renderers[rendererIndex].sharedMaterials.Length; materialIndex++)
                    m_Renderers[rendererIndex].SetPropertyBlock(new MaterialPropertyBlock(), materialIndex);
        }
    
        private bool IsMaterialMapped(Material material)
        {
            for (int i = 0; i < m_Colors.Count; i++)
                if (m_Colors[i].sharedMaterial == material)
                    return true;
            return false;
        }

        private ColorData GetColorData(Material material)
        {
            for (int i = 0; i < m_Colors.Count; i++)
                if (m_Colors[i].sharedMaterial == material)
                    return m_Colors[i];

            return new ColorData()
            {
                name = material?.name,
                sharedMaterial = material,
                shadeColor_A = Color.black,
                mainColor_A = Color.grey,
                shadeColor_B = Color.black,
                mainColor_B = Color.grey,
                shadeColor_C = Color.black,
                mainColor_C = Color.grey,
            };
        }

        private Color GetColor(string id, Material material, MaterialPropertyBlock propertyBlock)
        {
            return propertyBlock.HasColor(id) ?
                propertyBlock.GetColor(id) : material.GetColor(id);
        }
    }
}
