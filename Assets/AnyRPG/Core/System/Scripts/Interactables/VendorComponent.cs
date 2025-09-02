using AnyRPG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.PostProcessing.SubpixelMorphologicalAntialiasing;

namespace AnyRPG {
    public class VendorComponent : InteractableOptionComponent {

        // client Id, VendorCollection
        private Dictionary<int, VendorCollection> buyBackCollection = new Dictionary<int, VendorCollection>();

        // game manager references
        private VendorManagerClient vendorManager = null;
        private MessageFeedManager messageFeedManager = null;
        private CurrencyConverter currencyConverter = null;

        public VendorProps Props { get => interactableOptionProps as VendorProps; }
        public Dictionary<int, VendorCollection> BuyBackCollection { get => buyBackCollection; set => buyBackCollection = value; }

        public VendorComponent(Interactable interactable, VendorProps interactableOptionProps, SystemGameManager systemGameManager) : base(interactable, interactableOptionProps, systemGameManager) {
            interactionPanelTitle = "Purchase Items";
            // pre populate collection zero so vendor UI works in single player game
            buyBackCollection.Add(0, ScriptableObject.CreateInstance(typeof(VendorCollection)) as VendorCollection);
        }

        public override void SetGameManagerReferences() {
            base.SetGameManagerReferences();

            vendorManager = systemGameManager.VendorManagerClient;
            messageFeedManager = systemGameManager.UIManager.MessageFeedManager;
            currencyConverter = systemGameManager.CurrencyConverter;
        }

        /*
        protected override void AddUnitProfileSettings() {
            if (unitProfile != null) {
                interactableOptionProps = unitProfile.VendorProps;
            }
            HandlePrerequisiteUpdates();
        }
        */

        public override bool ProcessInteract(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            base.ProcessInteract(sourceUnitController, componentIndex, choiceIndex);
            return true;
        }

        public override void ClientInteraction(UnitController sourceUnitController, int componentIndex, int choiceIndex) {
            if (!uIManager.vendorWindow.IsOpen) {
                //Debug.Log(source + " interacting with " + gameObject.name);

                vendorManager.SetProps(Props, this, componentIndex, choiceIndex);
                uIManager.vendorWindow.OpenWindow();
            }
        }


        public override void StopInteract() {
            base.StopInteract();
            uIManager.vendorWindow.CloseWindow();
        }

        public override bool PlayInteractionSound() {
            return true;
        }

        public override AudioClip GetInteractionSound(VoiceProps voiceProps) {
            return voiceProps.RandomStartVendorInteract;
        }

        public List<VendorCollection> GetLocalVendorCollections() {
            return GetVendorCollections(0);
        }

        public List<VendorCollection> GetVendorCollections(int accountId) {
            List<VendorCollection> returnList = new List<VendorCollection>();
            returnList.Add(buyBackCollection[accountId]);
            returnList.AddRange(Props.VendorCollections);
            return returnList;
        }

        public void AddToBuyBackCollection(UnitController sourceUnitController, int componentIndex, InstantiatedItem newInstantiatedItem) {
            VendorItem newVendorItem = new VendorItem();
            newVendorItem.Quantity = 1;
            newVendorItem.InstantiatedItem = newInstantiatedItem;
            if (playerManagerServer.ActivePlayerLookup.ContainsKey(sourceUnitController) == true) {
                int accountId = playerManagerServer.ActivePlayerLookup[sourceUnitController];
                if (buyBackCollection.ContainsKey(accountId) == false) {
                    buyBackCollection.Add(accountId, ScriptableObject.CreateInstance(typeof(VendorCollection)) as VendorCollection);
                }
                buyBackCollection[accountId].VendorItems.Add(newVendorItem);
                interactable.InteractableEventController.NotifyOnAddToBuyBackCollection(sourceUnitController, newInstantiatedItem);
                if (networkManagerServer.ServerModeActive == true) {
                    networkManagerServer.AdvertiseAddToBuyBackCollection(sourceUnitController, interactable, componentIndex, newInstantiatedItem);
                }
            }
        }

        public bool SellItemToVendor(UnitController sourceUnitController, int componentIndex, InstantiatedItem instantiatedItem) {
            if (instantiatedItem.Item.BuyPrice(sourceUnitController) <= 0 || instantiatedItem.Item.GetSellPrice(instantiatedItem, sourceUnitController).Key == null) {
                sourceUnitController.WriteMessageFeedMessage($"The vendor does not want to buy the {instantiatedItem.DisplayName}");
                return false;
            }
            KeyValuePair<Currency, int> sellAmount = instantiatedItem.Item.GetSellPrice(instantiatedItem, sourceUnitController);

            sourceUnitController.CharacterCurrencyManager.AddCurrency(sellAmount.Key, sellAmount.Value);
            AddToBuyBackCollection(sourceUnitController, componentIndex, instantiatedItem);
            instantiatedItem.Slot.RemoveItem(instantiatedItem);

            string priceString = currencyConverter.GetCombinedPriceString(sellAmount.Key, sellAmount.Value);
            sourceUnitController.WriteMessageFeedMessage($"Sold {instantiatedItem.DisplayName} for {priceString}");

            return true;
        }

        protected bool CanAfford(UnitController sourceUnitController, VendorItem vendorItem, bool buyBackButton) {
            if (buyBackButton == false) {
                if (currencyConverter.GetBaseCurrencyAmount(vendorItem.Item.Currency, vendorItem.BuyPrice(sourceUnitController)) <= sourceUnitController.CharacterCurrencyManager.GetBaseCurrencyValue(vendorItem.Item.Currency)) {
                    return true;
                }
                return false;
            }

            if (vendorItem.Item.GetSellPrice(vendorItem.InstantiatedItem, sourceUnitController).Value <= sourceUnitController.CharacterCurrencyManager.GetBaseCurrencyValue(vendorItem.Item.Currency)) {
                return true;
            }
            return false;

        }

        public void BuyItemFromVendor(UnitController sourceUnitController, int componentIndex, VendorItem vendorItem, int collectionIndex, int itemIndex) {
            //Debug.Log($"VendorComponent.BuyItemFromVendor({sourceUnitController.gameObject.name}, {componentIndex}, {vendorItem.Item.resourceName}, {collectionIndex}, {itemIndex})");

            if (vendorItem.BuyPrice(sourceUnitController) == 0
                                        || vendorItem.Item.Currency == null
                                        || CanAfford(sourceUnitController, vendorItem, collectionIndex == 0)) {
                InstantiatedItem tmpInstantiatedItem = null;
                if (collectionIndex == 0) {
                    // if this is a buyback, the item has already been instantiated so it is safe to reference it directly
                    tmpInstantiatedItem = vendorItem.InstantiatedItem;
                } else {
                    // if this is a new purchase, a new copy of the item must be instantiated since the button is referring to the original factory item template
                    tmpInstantiatedItem = sourceUnitController.CharacterInventoryManager.GetNewInstantiatedItem(vendorItem.Item.ResourceName, vendorItem.GetItemQuality());
                    //Debug.Log("Instantiated an item with id: " + tmpItem.GetInstanceID().ToString());
                }

                if (sourceUnitController.CharacterInventoryManager.AddItem(tmpInstantiatedItem, false)) {
                    if (collectionIndex != 0) {
                        tmpInstantiatedItem.DropLevel = sourceUnitController.CharacterStats.Level;
                    }
                    SellItemToPlayer(sourceUnitController, componentIndex, vendorItem, collectionIndex, itemIndex);
                    if (tmpInstantiatedItem is InstantiatedCurrencyItem) {
                        (tmpInstantiatedItem as InstantiatedCurrencyItem).Use(sourceUnitController);
                    }
                }
            } else {
                sourceUnitController.WriteMessageFeedMessage($"You cannot afford {vendorItem.Item.DisplayName}");
            }
        }

        private void SellItemToPlayer(UnitController sourceUnitController, int componentIndex, VendorItem vendorItem, int collectionIndex, int itemIndex) {
            //Debug.Log($"VendorComponent.SellItemToPlayer({sourceUnitController.gameObject.name}, {componentIndex}, {vendorItem.Item.ResourceName}, {collectionIndex}, {itemIndex})");

            string priceString = string.Empty;
            if (vendorItem.BuyPrice(sourceUnitController) == 0 || vendorItem.Item.Currency == null) {
                priceString = "FREE";
            } else {
                KeyValuePair<Currency, int> usedSellPrice = new KeyValuePair<Currency, int>();
                if (collectionIndex != 0) {
                    usedSellPrice = new KeyValuePair<Currency, int>(vendorItem.Item.Currency, vendorItem.BuyPrice(sourceUnitController));
                    priceString = vendorItem.BuyPrice(sourceUnitController) + " " + vendorItem.Item.Currency.DisplayName;
                } else {
                    // buyback collection
                    usedSellPrice = vendorItem.Item.GetSellPrice(vendorItem.InstantiatedItem, sourceUnitController);
                    priceString = currencyConverter.GetCombinedPriceString(usedSellPrice);
                }
                sourceUnitController.CharacterCurrencyManager.SpendCurrency(usedSellPrice.Key, usedSellPrice.Value);
            }
            ProcessQuantityNotification(vendorItem, (vendorItem.Unlimited ? vendorItem.Quantity : vendorItem.Quantity - 1));
            if (networkManagerServer.ServerModeActive == true) {
                networkManagerServer.AdvertiseSellItemToPlayer(sourceUnitController, interactable, componentIndex, collectionIndex, itemIndex, vendorItem.Item.ResourceName, vendorItem.Quantity);
            }

            sourceUnitController.WriteMessageFeedMessage($"Purchased {vendorItem.Item.DisplayName} for {priceString}");
        }

        public void ProcessQuantityNotification(VendorItem vendorItem, int newQuantity) {
            vendorItem.Quantity = newQuantity;
            interactable.InteractableEventController.NotifyOnSellItemToPlayer(vendorItem);
        }

    }

}