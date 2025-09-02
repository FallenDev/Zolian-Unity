using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AnyRPG {
    public class ActionButton : NavigableElement, IClickable {

        // A reference to the useable on the actionbutton
        protected IUseable useable = null;

        // keep track of the last usable that was on this button in case an ability is re-learned
        protected IUseable savedUseable = null;

        [Header("Action Button")]

        [SerializeField]
        protected TextMeshProUGUI stackSizeText = null;

        [SerializeField]
        protected TextMeshProUGUI keyBindText = null;

        [SerializeField]
        protected Image backgroundImage = null;

        [SerializeField]
        protected Image icon = null;

        [SerializeField]
        protected Image coolDownIcon = null;

        [SerializeField]
        protected Image backGroundImage = null;

        [SerializeField]
        protected Image rangeIndicator = null;

        [SerializeField]
        protected Button button = null;

        protected int count = 0;

        protected int actionButtonIndex = 0;
        protected bool gamepadButton = false;

        protected Coroutine monitorCoroutine = null;

        protected CloseableWindowContents windowPanel = null;

        protected RectTransform tooltipTransform = null;

        // game manager references
        protected UIManager uIManager = null;
        protected SystemEventManager systemEventManager = null;
        protected PlayerManager playerManager = null;
        protected HandScript handScript = null;
        protected ActionBarManager actionBarManager = null;
        //InventoryManager inventoryManager = null;

        public Image Icon { get => icon; set => icon = value; }
        public int Count { get => count; }
        public TextMeshProUGUI StackSizeText { get => stackSizeText; }
        public TextMeshProUGUI KeyBindText { get => keyBindText; }
        public IUseable SavedUseable { get => savedUseable; set => savedUseable = value; }
        public IUseable Useable { get => useable; }
        public Image CoolDownIcon { get => coolDownIcon; set => coolDownIcon = value; }
        public Coroutine MonitorCoroutine { get => monitorCoroutine; set => monitorCoroutine = value; }
        public Image BackgroundImage { get => backgroundImage; set => backgroundImage = value; }
        public Image RangeIndicator { get => rangeIndicator; }
        public int ActionButtonIndex { get => actionButtonIndex; }

        public override void Configure(SystemGameManager systemGameManager) {
            if (configureCount > 0) {
                return;
            }
            base.Configure(systemGameManager);

            uIManager = systemGameManager.UIManager;
            systemEventManager = systemGameManager.SystemEventManager;
            playerManager = systemGameManager.PlayerManager;
            handScript = uIManager.HandScript;
            actionBarManager = uIManager.ActionBarManager;

            // setting useable to null should not be necessary since useable is already null at start
            //Useable = null;
            button.onClick.AddListener(OnClickFromButton);
            DisableCoolDownIcon();

            systemEventManager.OnItemCountChanged += UpdateItemCount;
            HideRangeIndicator();
        }

        public void HideRangeIndicator() {
            rangeIndicator.color = hiddenColor;
        }


        public void SetIndex(int index, bool gamepadButton) {
            actionButtonIndex = index;
            // for now, we only set index on gamepad buttons, so this call can tell the button it's a gamepad button
            gamepadButton = true;
        }

        public void SetPanel(CloseableWindowContents windowPanel) {
            this.windowPanel = windowPanel;
        }

        public void SetTooltipTransform(RectTransform rectTransform) {
            tooltipTransform = rectTransform;
        }


        public void SetBackGroundColor(Color color) {
            if (backGroundImage != null) {
                backGroundImage.color = color;
            }
        }

        public void OnClickFromButton() {

            // do not allow clicks to have any effect when gamepad mode is active to prevent pickup with hand script
            if (controlsManager.GamePadModeActive == true) {
                //Debug.Log("ActionButton.OnClickFromButton() gamepad mode active, returning");
                return;
            }

            OnClick();
        }

        public void OnClick(bool fromKeyBind = false) {
            // this may seem like duplicate with the next method, but right now it is used to simulate click events when keypresses happen
            // it is also used when the player controller sends a click event from the gamepad

            if (!fromKeyBind) {
                // if we did come from a keybind, we don't want to ignore left shift
                if (Input.GetKey(KeyCode.LeftShift)) {
                    return;
                }
                if (handScript.Moveable != null) {
                    // if we have something in the handscript we are trying to drop an item, not use one
                    return;
                }
            }

            if (Useable != null) {
                Useable.ActionButtonUse(playerManager.UnitController);
            }
        }

        public override void OnPointerClick(PointerEventData eventData) {
            //Debug.Log("ActionButton.OnPointerClick()");

            // do not allow clicks to have any effect when gamepad mode is active to prevent pickup with hand script
            if (controlsManager.GamePadModeActive == true) {
                //Debug.Log("ActionButton.OnPointerClick() gamepad mode active, returning");
                return;
            }

            base.OnPointerClick(eventData);
            if (playerManager.ActiveUnitController != null) {
                if (playerManager.ActiveUnitController.ControlLocked == true) {
                    return;
                }
            }

            // left click
            if (eventData.button == PointerEventData.InputButton.Left) {

                if (Input.GetKey(KeyCode.LeftShift)) {
                    // attempt to pick up - the only valid option when shift is held down
                    if (Useable != null && actionBarManager.FromButton == null && handScript.Moveable == null) {
                        // left shift down, pick up a useable
                        //Debug.Log("ActionButton: OnPointerClick(): shift clicked and useable is not null. picking up");
                        handScript.TakeMoveable(Useable as IMoveable);
                        actionBarManager.FromButton = this;
                    }
                } else {
                    // attempt to put down
                    if (handScript.Moveable != null && handScript.Moveable is IUseable) {
                        if (actionBarManager.FromButton != this) {
                            if (actionBarManager.FromButton != null) {
                                //Debug.Log("ActionButton: OnPointerClick(): FROMBUTTON IS NOT NULL, SWAPPING ACTIONBAR ITEMS");
                                // this came from another action button slot.  now decide to swap (if we are not empty), or remove from original (if we are empty)
                                /*
                                if (Useable != null) {
                                    actionBarManager.FromButton.ClearUseable();
                                    actionBarManager.FromButton.SetUseable(Useable);
                                } else {
                                    actionBarManager.FromButton.ClearUseable();
                                }
                                */
                                actionBarManager.RequestMoveMouseUseable(actionBarManager.FromButton.actionButtonIndex, actionButtonIndex);
                            } else {
                                actionBarManager.RequestAssignMouseUseable(handScript.Moveable as IUseable, actionButtonIndex);
                            }
                                // no matter whether we sent our useable over or not, we can now clear our useable and set whatever is in the handscript
                            //ClearUseable();
                            //SetUseable(handScript.Moveable as IUseable);
                        }

                        handScript.Drop();
                    }
                }
            }
        }

        public void HandleDropCombat() {
            UpdateVisual();
        }

        public void HandleEnterCombat(Interactable interactable) {
            UpdateVisual();
        }

        public void HandleLeaveStealth() {
            UpdateVisual();
        }

        public void HandleEnterStealth() {
            UpdateVisual();
        }

        public void LoadUseable(IUseable newUseable) {
            useable = newUseable.GetFactoryUseable();
        }

        /// <summary>
        /// Sets the useable on the actionbutton
        /// </summary>
        /// <param name="newUseable"></param>
        public void SetUseable(IUseable newUseable, bool monitor = true) {
            //Debug.Log($"{gameObject.name}.ActionButton.SetUsable({(useable == null ? "null" : useable.DisplayName)}, {monitor})");

            ClearUseable();

            newUseable.AssignToActionButton(this);

            LoadUseable(newUseable);

            SubscribeToAbilityEvents();
            SubscribeToCombatEvents();
            SubscribeToStealthEvents();

            // there may be a global cooldown in progress.  if not, this call will still update the visual
            if (monitor == true) {
                ChooseMonitorCoroutine();
            }

            // there was the assumption that these were only being called when a player clicked to add an ability
            if (UIManager.MouseInRect(Icon.rectTransform)) {
                //uIManager.ShowToolTip(transform.position, useable as IDescribable);
                uIManager.ShowGamepadTooltip(tooltipTransform, transform, newUseable as IDescribable, "");

            }

        }

        public void SubscribeToAbilityEvents() {
            playerManager.UnitController.UnitEventController.OnAttemptPerformAbility += HandleAttemptPerformAbility;
            playerManager.UnitController.UnitEventController.OnPerformAbility += HandlePerformAbility;
            playerManager.UnitController.UnitEventController.OnBeginAbilityCoolDown += HandleBeginAbilityCooldown;
        }

        public void SubscribeToAutoAttackEvents() {
            //Debug.Log($"{gameObject.name}.ActionButton.SubscribeToAutoAttackEvents()");

            playerManager.UnitController.UnitEventController.OnActivateAutoAttack += HandleActivateAutoAttack;
        }


        public void UnsubscribeFromAutoAttackEvents() {
            //Debug.Log($"{gameObject.name}.ActionButton.UnsubscribeFromAutoAttackEvents()");

            playerManager.UnitController.UnitEventController.OnActivateAutoAttack -= HandleActivateAutoAttack;
        }

        private void HandleActivateAutoAttack() {
            //Debug.Log($"{gameObject.name}.ActionButton.HandleActivateAutoAttack()");

            if (monitorCoroutine == null && gameObject.activeInHierarchy == true) {
                monitorCoroutine = StartCoroutine(MonitorAutoAttack());
            }
        }

        public void SubscribeToCombatEvents() {
            if (Useable != null && Useable.RequireOutOfCombat == true) {
                playerManager.ActiveUnitController.UnitEventController.OnEnterCombat += HandleEnterCombat;
                playerManager.ActiveUnitController.UnitEventController.OnDropCombat += HandleDropCombat;
            }
        }

        public void SubscribeToStealthEvents() {
            if (Useable != null && Useable.RequireStealth == true) {
                playerManager.ActiveUnitController.UnitEventController.OnEnterStealth += HandleEnterStealth;
                playerManager.ActiveUnitController.UnitEventController.OnLeaveStealth += HandleLeaveStealth;
            }
        }

        public void HandleAttemptPerformAbility(AbilityProperties ability) {
            //Debug.Log("ActionButton.OnUseableUse(" + ability.DisplayName + ")");
            ChooseMonitorCoroutine();
        }

        public void HandleBeginAbilityCooldown(AbilityProperties abilityProperties, float coolDownLength) {
            //Debug.Log("ActionButton.OnUseableUse(" + ability.DisplayName + ")");
            ChooseMonitorCoroutine();
        }

        public void ChooseMonitorCoroutine() {
            // if this action button is empty, there is nothing to monitor
            if (useable == null) {
                return;
            }

            // if this object is disabled, then there is no reason to monitor
            if (gameObject.activeInHierarchy == false) {
                return;
            }

            if (monitorCoroutine == null) {
                monitorCoroutine = useable.ChooseMonitorCoroutine(this);
            }
            if (monitorCoroutine == null) {
                UpdateVisual();
            }
        }

        public void HandlePerformAbility(UnitController sourceUnitController, AbilityProperties ability) {
            //Debug.Log("ActionButton.OnUseableUse(" + ability.DisplayName + ")");
            ChooseMonitorCoroutine();
        }

        public IEnumerator MonitorAutoAttack() {
            //Debug.Log("ActionButton.MonitorautoAttack(" + ability.DisplayName + ")");
            yield return null;

            while (Useable != null
                && playerManager.UnitController.CharacterCombat.GetInCombat() == true
                && playerManager.UnitController.CharacterCombat.AutoAttackActive == true) {
                UpdateVisual();
                yield return new WaitForSeconds(0.5f);
            }
            //Debug.Log("ActionButton.MonitorAbility(" + ability.DisplayName + "): Done Monitoring");
            if (Useable != null) {
                // could switch buttons while an ability is on cooldown
                UpdateVisual();
            }
            //autoAttackCoRoutine = null;
            monitorCoroutine = null;
        }

        public IEnumerator MonitorCooldown(IUseable useable) {
            //Debug.Log("ActionButton.MonitorAbility(" + ability.DisplayName + ")");
            while (Useable != null
                && playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary.ContainsKey(useable.DisplayName)) {
                UpdateVisual();
                yield return null;
            }
            //Debug.Log("ActionButton.MonitorAbility(" + ability.DisplayName + "): Done Monitoring; Useable: " + (Useable == null ? "null" : Useable.DisplayName));
            if (Useable != null) {
                // could switch buttons while an ability is on cooldown
                UpdateVisual();
            }
            //abilityCoRoutine = null;
            monitorCoroutine = null;
        }

        //public IEnumerator MonitorAbility(BaseAbility ability) {
        public IEnumerator MonitorAbility(string abilityName) {
            //Debug.Log("ActionButton.MonitorAbility(" + ability.DisplayName + ")");
            while (Useable != null
                && (playerManager.UnitController.CharacterAbilityManager.RemainingGlobalCoolDown > 0f
                || playerManager.UnitController.CharacterAbilityManager.AbilityCoolDownDictionary.ContainsKey(abilityName))) {
                UpdateVisual();
                yield return null;
            }
            //Debug.Log("ActionButton.MonitorAbility(" + ability.DisplayName + "): Done Monitoring; Useable: " + (Useable == null ? "null" : Useable.DisplayName));
            if (Useable != null) {
                // could switch buttons while an ability is on cooldown
                UpdateVisual();
            }
            //abilityCoRoutine = null;
            monitorCoroutine = null;
        }

        public void DisableCoolDownIcon() {
            //Debug.Log("ActionButton.DisableCoolDownIcon() useable: " + (useable == null ? "null" : useable.DisplayName));
            // testing
            // this was preventing cooldown icons from being reset on logout
            //if (coolDownIcon.isActiveAndEnabled != false) {
            coolDownIcon.sprite = null;
            coolDownIcon.color = new Color32(0, 0, 0, 0);
            coolDownIcon.enabled = false;
            //}
        }

        /// <summary>
        /// attempt to remove unlearned spells from the button
        /// </summary>
        public void RemoveStaleActions() {
            //Debug.Log($"{gameObject.name}.ActionButton.RemoveStaleActions()");

            if (Useable != null && Useable.IsUseableStale(playerManager.UnitController) == true) {
                Debug.Log($"{gameObject.name}.ActionButton.RemoveStaleActions() removing useable: {Useable.ResourceName}");
                savedUseable = Useable;
                useable = null;
                UpdateVisual();
            }
        }

        /// <summary>
        /// Updates the visual representation of the actionbutton
        /// </summary>
        public void UpdateVisual() {
            //Debug.Log(gameObject.name + GetInstanceID() + ".ActionButton.UpdateVisual() useable: " + (useable == null ? "null" : useable.DisplayName));
            if (playerManager == null || playerManager.UnitController == null) {
                return;
            }

            if (Useable == null) {
                //Debug.Log("ActionButton.UpdateVisual(): useable is null. clearing stack count and setting icon to empty");

                // clear stack count
                uIManager.ClearStackCount(this);

                // clear icon
                Icon.sprite = null;
                Icon.color = icon.color = new Color32(0, 0, 0, 0);

                // clear background image
                backgroundImage.color = new Color32(0, 0, 0, 0);
                backgroundImage.sprite = null;

                // clear cooldown icon
                DisableCoolDownIcon();
                HideRangeIndicator();

                return;
            }

            if (Icon.sprite != Useable.Icon) {
                Icon.sprite = Useable.Icon;
            }
            if (Icon.color != Color.white) {
                Icon.color = Color.white;
            }

            //Debug.Log("ActionButton.UpdateVisual(): about to get useable count");
            UpdateChargeCount();
            Useable.UpdateActionButtonVisual(this);

            // if this object is disabled, then there is no reason to process pointer enter
            if (gameObject.activeInHierarchy == false) {
                return;
            }

            if (UIManager.MouseInRect(Icon.rectTransform)) {
                ProcessOnPointerEnter();
            }
        }

        public void UpdateChargeCount() {
            uIManager.UpdateStackSize(this, Useable.GetChargeCount(), useable is InstantiatedItem);
        }

        public void EnableFullCoolDownIcon() {
            //Debug.Log("ActionButton.EnableFullCoolDownIcon(): useable: " + (useable == null ? "null" : useable.DisplayName));
            if (coolDownIcon.isActiveAndEnabled == false) {
                coolDownIcon.enabled = true;
            }
            if (coolDownIcon.sprite != Icon.sprite) {
                coolDownIcon.sprite = Icon.sprite;
            }
            coolDownIcon.color = new Color32(0, 0, 0, 150);
            coolDownIcon.fillMethod = Image.FillMethod.Radial360;
            //coolDownIcon.fillOrigin = Image.Origin360.Top;
            coolDownIcon.fillClockwise = false;
            //}
            float fillAmount = 1f;
            if (coolDownIcon.fillAmount != fillAmount) {
                coolDownIcon.fillAmount = fillAmount;
            }
        }

        public void UpdateItemCount(UnitController unitController, Item item) {

            if (item is IUseable) {
                UpdateVisual();
            }
        }

        public override void OnPointerEnter(PointerEventData eventData) {
            base.OnPointerEnter(eventData);
            //Debug.Log(gameObject + ".ActionButton.OnPointerEnter()");

            ProcessOnPointerEnter();
        }

        public void ProcessOnPointerEnter() {
            IDescribable tmp = null;

            if (Useable != null && Useable is IDescribable) {
                tmp = (IDescribable)Useable;
            }
            if (tmp != null) {
                //uIManager.ShowToolTip(transform.position, tmp);
                uIManager.ShowGamepadTooltip(tooltipTransform, transform, useable as IDescribable, "");

            }
        }

        public override void OnPointerExit(PointerEventData eventData) {
            base.OnPointerExit(eventData);
            uIManager.HideToolTip();
        }

        public void UnsubscribeFromCombatEvents() {
            if (Useable != null && Useable.RequireOutOfCombat == true) {
                playerManager.ActiveUnitController.UnitEventController.OnEnterCombat -= HandleEnterCombat;
                playerManager.ActiveUnitController.UnitEventController.OnDropCombat -= HandleDropCombat;
            }
        }

        public void UnsubscribeFromStealthEvents() {
            if (Useable != null && Useable.RequireStealth == true) {
                playerManager.ActiveUnitController.UnitEventController.OnEnterStealth -= HandleEnterStealth;
                playerManager.ActiveUnitController.UnitEventController.OnLeaveStealth -= HandleLeaveStealth;
            }
        }


        public void ClearUseable() {
            //Debug.Log($"{gameObject.name}.ActionButton.ClearUseable()");

            if (useable != null) {
                //Debug.Log($"{gameObject.name}.ActionButton.ClearUseable() useable: {Useable.ResourceName}");
                UnsubscribeFromAbilityEvents();
                UnsubscribeFromCombatEvents();
                UnsubscribeFromStealthEvents();
                useable.HandleRemoveFromActionButton(this);

                savedUseable = Useable;
                useable = null;

                UpdateVisual();
            }
        }

        public void UnsubscribeFromAbilityEvents() {
            playerManager.UnitController.UnitEventController.OnAttemptPerformAbility -= HandleAttemptPerformAbility;
            playerManager.UnitController.UnitEventController.OnPerformAbility -= HandlePerformAbility;
            playerManager.UnitController.UnitEventController.OnBeginAbilityCoolDown -= HandleBeginAbilityCooldown;
        }

        public override void Select() {
            base.Select();
            if (useable != null) {
                owner.SetControllerHints("Move", "Clear", "", "", "", "");
                if (tooltipTransform != null) {
                    uIManager.ShowGamepadTooltip(tooltipTransform, transform, useable as IDescribable, "");
                }
            } else {
                owner.HideControllerHints();
                uIManager.HideToolTip();
            }
        }

        public override void DeSelect() {
            base.DeSelect();
            owner.HideControllerHints();
            uIManager.HideToolTip();
        }

        public override void JoystickButton2() {
            base.JoystickButton2();
            if (useable == null) {
                return;
            }
            actionBarManager.RequestClearGamepadUseable(actionButtonIndex);
            uIManager.HideToolTip();
            owner.HideControllerHints();
        }

        public override void Accept() {
            base.Accept();
            if (useable == null) {
                return;
            }
            actionBarManager.StartUseableAssignment(useable, actionButtonIndex);

            // ensure the assignment window is set to the same navigation controller and element index so the move starts in the same spot on the screen
            (uIManager.assignToActionBarsWindow.CloseableWindowContents as AssignToActionBarsUI).SetNavigationControllerByIndex(windowPanel.GetNavigationControllerIndex());
            (uIManager.assignToActionBarsWindow.CloseableWindowContents as AssignToActionBarsUI).CurrentNavigationController.SetCurrentIndex(windowPanel.CurrentNavigationController.CurrentIndex);

            uIManager.assignToActionBarsWindow.OpenWindow();
        }
    }

}