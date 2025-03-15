using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SoftKitty.InventoryEngine
{
    /// <summary>
    /// This script is an example to show how to use InventoryEngine.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        #region References
        public GameObject LootPackPrefab;
        public InventoryHolder MerchantNpc;
        public InventoryHolder StorageContainer;
        public InventoryHolder NpcEquipment;
        public InventoryHolder NpcWorker;
        public ListItem DebugItem;
        private float SkyBoxRot = 21F;
        #endregion

        #region Visual Effects
        private void Update()
        {
            //Make the background looks more alive.
            RenderSettings.skybox.SetFloat("_Exposure", 1.15F + Mathf.Sin(Time.time * 0.7F) * 0.15F);
            RenderSettings.skybox.SetFloat("_Rotation", SkyBoxRot);
            SkyBoxRot = Mathf.Lerp(SkyBoxRot, 21F + (Screen.width * 0.5F - InputProxy.mousePosition.x) * 0.003F, Time.deltaTime);
        }
        #endregion

        void Start()
        {
            ItemManager.PlayerInventoryHolder.RegisterItemUseCallback(OnItemUse);//Register callback to trigger when player's item being use.
            ActionBarUi.instance.SetProgressHint("Level. 1"); // Set the text on the bottom progress bar of the ActionBar UI.
            ActionBarUi.instance.SetProgress(1230, 2000); // Set the bottom progress bar of the ActionBar UI.
            ActionBarUi.instance.SavePath= Application.dataPath + "/../SaveData/ActionBar.sav";//Set the save data path for the ActionBar.
            MerchantNpc.SavePath = Application.dataPath + "/../SaveData/" + MerchantNpc.Name.Replace(" ", "") + ".sav";//Set the save data path for the merchant.
            ItemManager.PlayerInventoryHolder.SavePath= Application.dataPath + "/../SaveData/PlayerInventory.sav";//Set the save data path for the player's inventory.
            ItemManager.PlayerEquipmentHolder.SavePath = Application.dataPath + "/../SaveData/PlayerEquipment.sav";//Set the save data path for the player's equipment.
            ItemManager.PlayerEquipmentHolder.RegisterItemChangeCallback(OnEuqipmentItemChange);//Register callback for player's equipment
            ItemManager.PlayerInventoryHolder.RegisterItemDropCallback(OnItemDrop);//Register callback for player drop item
            ItemManager.PlayerEquipmentHolder.RegisterItemDropCallback(OnItemDrop);//Register callback for player drop item
        }

        public void OnItemDrop(Item _droppedItem, int _number)
        {
            DynamicMsg.PopMsg("Player dropped" + " [" + _droppedItem.nameWithAffixing + "] x "+_number.ToString());
        }
       

#if MASTER_CHARACTER_CREATOR
        public MasterCharacterCreator.CharacterEntity Player; //If you have Master Character Creator installed, drag the player entity to this slot.
#endif
        public void OnEuqipmentItemChange(Dictionary<Item, int> _changedItems)
        {
            foreach (var _item in _changedItems.Keys) {
#if MASTER_CHARACTER_CREATOR
                if (Player != null)
                {
                    if (_changedItems[_item] > 0)
                    {
                        Player.Equip(_item.equipAppearance); // Master Character Creator Equip
                    }
                    else
                    {
                        Player.Unequip(_item.equipAppearance.Type); // Master Character Creator Unequip
                    }
                }
#endif
                DynamicMsg.PopMsg("Player "+ (_changedItems[_item]>0?"equipped":"unequipped")+" ["+ _item.nameWithAffixing + "]");
            }
        }

        public void OnItemUse(string _action, int _id, int _index) //When player using an item, this callback will be called.
        {
            if (_action == "equip" && WindowsManager.getOpenedWindow(ItemManager.PlayerEquipmentHolder)==null)
            {
                InventoryHolder.EquipItem(ItemManager.PlayerInventoryHolder, ItemManager.PlayerEquipmentHolder, _id, _index);//Equip item when click though action bar.
                return;
            }

            if (_action.Contains("Add")|| _action.Contains("buff") || _action.Contains("permanent"))
            {
                SoundManager.Play2D("UseItem");
            }
            else if (_action.Contains("attck")|| _action.Contains("Skill"))
            {
                SoundManager.Play2D("Attack");
            }
           
            //Create item icon in the center of the screen to show how the callback works.
            DebugItem.transform.parent.SetAsLastSibling();
            GameObject _newItem = Instantiate(DebugItem.gameObject, DebugItem.transform.parent);
            _newItem.GetComponent<ListItem>().mImages[0].color = ItemManager.itemDic[_id].GetTypeColor();
            _newItem.GetComponent<ListItem>().mRawimages[0].texture = ItemManager.itemDic[_id].icon;
            _newItem.GetComponent<ListItem>().mTexts[0].text = _action;
            _newItem.gameObject.SetActive(true);
        }

        private void OnDestroy() // If you ever called RegisterItemUseCallback(), don't forget to call UnRegisterItemUseCallback() when the gameobject going to be destroyed.
        {
            ItemManager.PlayerInventoryHolder.UnRegisterItemUseCallback(OnItemUse);
            ItemManager.PlayerEquipmentHolder.UnRegisterItemChangeCallback(OnEuqipmentItemChange);
        }

        public void OpenPlayerInventory()
        {
            if (ItemManager.instance != null && ItemManager.PlayerInventoryHolder != null) ItemManager.PlayerInventoryHolder.OpenWindow();
        }

        public void OpenNpcWorker()
        {
            NpcWorker.OpenForgeWindow(true,false,false,false,2F);
        }

        public void OpenPlayerEquipment()
        {
            ItemManager.PlayerEquipmentHolder.OpenWindow();
        }

        public void OpenNpcTeamEquipment()
        {
            NpcEquipment.OpenWindow();
        }

        public void OpenCraft()
        {
            ItemManager.PlayerInventoryHolder.OpenForgeWindow(true, true, true, true, 1F,"Forge"); //Open crafting window
        }

        public void KillMonster()
        {
            SoundManager.Play2D("Kill");
            Instantiate(LootPackPrefab).GetComponent<LootPack>().OpenPack(); //Create a loot pack and open it
        }

        public void OpenSkills()
        {
            ItemManager.PlayerInventoryHolder.OpenWindowByName("Skills","Skills"); //An example to use "Hidden Items" to achive "Skills" management.
        }
        public void TalkToMerchant()
        {
            MerchantNpc.OpenWindow();
            SoundManager.Play2D("Merchant");
        }

        public void OpenStorage()
        {
            StorageContainer.OpenWindow();
        }
    }
}
