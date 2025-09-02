using AnyRPG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace AnyRPG {

    [System.Serializable]
    public class UseInteractableObjective : QuestObjective {

        [SerializeField]
        protected string interactableName = null;

        public override string ObjectiveName { get => interactableName; }

        public override Type ObjectiveType {
            get {
                return typeof(UseInteractableObjective);
            }
        }

        // if just interacting is not enough, but actually finishing using an interactable is required.
        public bool requireCompletion = false;

        public void CheckInteractionComplete(UnitController sourceUnitController, InteractableOptionComponent interactableOption) {
            CheckInteractableName(sourceUnitController, interactableOption.DisplayName, true);
            CheckInteractableName(sourceUnitController, interactableOption.Interactable.DisplayName, true);
        }

        public void CheckInteractionWithOptionStart(UnitController sourceUnitController, InteractableOptionComponent interactableOption, int optionIndex, int choiceIndex) {
            CheckInteractableName(sourceUnitController, interactableOption.DisplayName, false);
            CheckInteractableName(sourceUnitController, interactableOption.Interactable.DisplayName, false);
        }

        public void CheckInteractableName(UnitController sourceUnitController, string interactableName, bool interactionComplete) {
            //Debug.Log("UseInteractableObjective.CheckInteractableName()");
            bool completeBefore = IsComplete(sourceUnitController);
            if (completeBefore) {
                return;
            }
            if (SystemDataUtility.MatchResource(interactableName, this.interactableName)) {
                if (!interactionComplete && requireCompletion == true) {
                    return;
                }
                if (requireCompletion == false && interactionComplete) {
                    return;
                }
                if (CurrentAmount(sourceUnitController) < Amount) {
                    SetCurrentAmount(sourceUnitController, CurrentAmount(sourceUnitController) +1);
                }
                if (CurrentAmount(sourceUnitController) <= Amount && questBase.PrintObjectiveCompletionMessages && CurrentAmount(sourceUnitController) != 0) {
                    sourceUnitController.WriteMessageFeedMessage(string.Format("{0}: {1}/{2}", DisplayName, Mathf.Clamp(CurrentAmount(sourceUnitController), 0, Amount), Amount));
                }
                if (completeBefore == false && IsComplete(sourceUnitController) && questBase.PrintObjectiveCompletionMessages) {
                    sourceUnitController.WriteMessageFeedMessage(string.Format("{0}: Objective Complete", DisplayName));
                }
                questBase.CheckCompletion(sourceUnitController);
            }
        }


        public override void OnAcceptQuest(UnitController sourceUnitController, QuestBase quest, bool printMessages = true) {
            //Debug.Log("UseInteractableObjective.OnAcceptQuest()");
            base.OnAcceptQuest(sourceUnitController, quest, printMessages);

            // don't forget to remove these later
            //systemEventManager.OnInteractionStarted += CheckInteractionStart;
            sourceUnitController.UnitEventController.OnStartInteractWithOption += CheckInteractionWithOptionStart;
            sourceUnitController.UnitEventController.OnCompleteInteractWithOption += CheckInteractionComplete;
        }

        public override void OnAbandonQuest(UnitController sourceUnitController) {
            //Debug.Log("UseInteractableObjective.OnAbandonQuest()");
            base.OnAbandonQuest(sourceUnitController);
            //systemEventManager.OnInteractionStarted -= CheckInteractionStart;
            sourceUnitController.UnitEventController.OnStartInteractWithOption -= CheckInteractionWithOptionStart;
            sourceUnitController.UnitEventController.OnCompleteInteractWithOption -= CheckInteractionComplete;
        }

    }
}