using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class InstantiatedEquipment : InstantiatedItem {

        private Equipment equipment = null;

        private List<int> randomStatIndexes = new List<int>();
        private List<ItemSecondaryStatNode> chosenSecondaryStats = new List<ItemSecondaryStatNode>();

        public List<int> RandomStatIndexes { get => randomStatIndexes; set => randomStatIndexes = value; }
        public List<ItemSecondaryStatNode> ChosenSecondaryStats { get => chosenSecondaryStats; set => chosenSecondaryStats = value; }
        public Equipment Equipment { get => equipment; }
        public List<ItemSecondaryStatNode> SecondaryStats {
            get {
                if (equipment.RandomSecondaryStats == true) {
                    return chosenSecondaryStats;
                }
                return equipment.SecondaryStats;
            }
        }

        public InstantiatedEquipment(SystemGameManager systemGameManager, int instanceId, Equipment equipment, ItemQuality itemQuality) : base(systemGameManager, instanceId, equipment, itemQuality) {
            this.equipment = equipment;
        }

        public override bool Use(UnitController sourceUnitController) {
            //Debug.Log($"{ResourceName}.InstantiatedEquipment.Use({sourceUnitController.gameObject.name})");

            if (sourceUnitController?.CharacterEquipmentManager != null) {
                bool returnValue = base.Use(sourceUnitController);
                if (returnValue == false) {
                    return false;
                }
                InventorySlot oldSlot = Slot;
                if (sourceUnitController.CharacterEquipmentManager.Equip(this) == true) {
                    RemoveFrom(oldSlot);
                    sourceUnitController.UnitModelController.RebuildModelAppearance();
                    return true;
                } else {
                    return false;
                }
            }
            return false;
        }

        public override InventorySlotSaveData GetSlotSaveData() {
            InventorySlotSaveData saveData = base.GetSlotSaveData();
            saveData.randomSecondaryStatIndexes = RandomStatIndexes;
            return saveData;
        }

        public override void LoadSaveData(InventorySlotSaveData inventorySlotSaveData) {
            base.LoadSaveData(inventorySlotSaveData);
            if (inventorySlotSaveData.randomSecondaryStatIndexes != null) {
                randomStatIndexes = inventorySlotSaveData.randomSecondaryStatIndexes;
                InitializeRandomStatsFromIndex();
            }
        }

        public virtual void LoadSaveData(EquipmentSaveData equipmentSaveData) {
            displayName = equipmentSaveData.DisplayName;
            dropLevel = equipmentSaveData.dropLevel;
            if (equipmentSaveData.randomSecondaryStatIndexes != null) {
                randomStatIndexes = equipmentSaveData.randomSecondaryStatIndexes;
                InitializeRandomStatsFromIndex();
            }
        }


        public override void PostInitialization() {
            base.PostInitialization();
            if (equipment.RandomSecondaryStats == false) {
                return;
            }
            if (ItemQuality == null) {
                return;
            }
            if (ItemQuality.RandomStatCount == 0) {
                return;
            }

            // get the max number, and cycling through the list and adding them to our current list and index
            PopulateRandomStatIndexes();
            InitializeRandomStatsFromIndex();

        }

        public void InitializeRandomStatsFromIndex() {
            chosenSecondaryStats.Clear();
            foreach (int randomIndex in randomStatIndexes) {
                chosenSecondaryStats.Add(equipment.SecondaryStats[randomIndex]);
            }
        }

        public void PopulateRandomStatIndexes() {
            int maxCount = Mathf.Min(equipment.SecondaryStats.Count, ItemQuality.RandomStatCount);
            while (randomStatIndexes.Count < maxCount) {
                int randomNumber = UnityEngine.Random.Range(0, equipment.SecondaryStats.Count);
                if (!RandomStatIndexes.Contains(randomNumber)) {
                    RandomStatIndexes.Add(randomNumber);
                }
            }
        }

        public override string GetDescription() {
            return base.GetDescription() + "\n\n" + equipment.GetEquipmentDescription(ItemQuality, GetItemLevel(playerManager.UnitController.CharacterStats.Level), SecondaryStats);
        }

    }

}