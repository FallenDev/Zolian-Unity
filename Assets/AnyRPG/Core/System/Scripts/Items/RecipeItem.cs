using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    [CreateAssetMenu(fileName = "RecipeItem", menuName = "AnyRPG/Inventory/Items/RecipeItem", order = 1)]
    public class RecipeItem : Item {

        [Header("Recipe Item")]

        [Tooltip("The power resource to refill when this potion is used")]
        [SerializeField]
        [ResourceSelector(resourceType = typeof(Recipe))]
        private string recipeName = string.Empty;

        private Recipe recipe = null;

        public Recipe Recipe { get => recipe; }

        public override InstantiatedItem GetNewInstantiatedItem(SystemGameManager systemGameManager, int itemId, Item item, ItemQuality usedItemQuality) {
            if ((item is RecipeItem) == false) {
                return null;
            }
            return new InstantiatedRecipeItem(systemGameManager, itemId, item as RecipeItem, usedItemQuality);
        }


        public override string GetDescription(ItemQuality usedItemQuality, int usedItemLevel) {
            string returnString = base.GetDescription(usedItemQuality, usedItemLevel);
            if (recipe != null) {
                string alreadyKnownString = string.Empty;
                if (playerManager.UnitController.CharacterRecipeManager.RecipeList.ContainsValue(recipe)) {
                    alreadyKnownString = "<color=red>already known</color>\n";
                }
                string abilityKnownString = string.Empty;
                if (playerManager.UnitController.CharacterAbilityManager.AbilityList.ContainsValue(recipe.CraftAbility)) {
                    abilityKnownString = "<color=white>Requires: " + recipe.CraftAbility.DisplayName  + "</color>\n";
                } else {
                    abilityKnownString = "<color=red>Requires: " + recipe.CraftAbility.DisplayName + "</color>\n";
                }
                returnString += string.Format("\n<color=green>Recipe</color>\n{0}{1}{2}", alreadyKnownString, abilityKnownString, recipe.Output.GetDescription());
            }
            return returnString;
        }

        public override void SetupScriptableObjects(SystemGameManager systemGameManager) {
            //Debug.Log("RecipeItem.SetupScriptableObjects():");
            base.SetupScriptableObjects(systemGameManager);

            if (recipeName != null && recipeName != string.Empty) {
                Recipe tmpRecipe = systemDataFactory.GetResource<Recipe>(recipeName);
                if (tmpRecipe != null) {
                    recipe = tmpRecipe;
                } else {
                    Debug.LogError("RecipeItem.SetupScriptableObjects(): Could not find recipe : " + recipeName + " while inititalizing " + ResourceName + ".  CHECK INSPECTOR");
                }
            }

        }
    }

}