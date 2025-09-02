using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnyRPG {
    public class CharacterRequestData {

        public int accountId = -1;
        public ICharacterRequestor characterRequestor;
        public GameMode requestMode;
        public CharacterConfigurationRequest characterConfigurationRequest;
        public bool isServerOwned = false;
        public bool isServer = false;
        public bool isOwner = false;
        public AnyRPGSaveData saveData = null;

        public CharacterRequestData(ICharacterRequestor characterRequestor, GameMode requestMode, CharacterConfigurationRequest characterConfigurationRequest) {
            //Debug.Log($"CharacterRequestData.CharacterRequestData({characterRequestor}, {requestMode})");

            this.characterRequestor = characterRequestor;
            this.requestMode = requestMode;
            //this.unitProfile = unitProfile;
            //this.unitControllerMode = unitControllerMode;
            this.characterConfigurationRequest = characterConfigurationRequest;
        }

    }
}

