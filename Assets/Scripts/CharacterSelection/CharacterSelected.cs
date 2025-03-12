using Assets.Scripts.Managers;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.CharacterSelection
{
    public class CharacterSelected : MonoBehaviour
    {
        public TextMeshProUGUI characterLevelText;
        public TextMeshProUGUI characterBaseClassText;
        public TextMeshProUGUI characterAdvClassText;
        public TextMeshProUGUI characterJobText;
        public TextMeshProUGUI characterHealthText;
        public TextMeshProUGUI characterManaText;

        public PlayerSelection selectedPlayer;

        public static CharacterSelected Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void SelectCharacter(PlayerSelection player)
        {
            selectedPlayer = player;
            characterLevelText.text = $"Level: {player.Level}";
            characterBaseClassText.text = $"Class: {player.BaseClass}";
            characterAdvClassText.text = $"Adv Class: {player.AdvClass}";
            characterJobText.text = $"Job: {player.Job}";
            characterHealthText.text = $"Health: {player.Health}";
            characterManaText.text = $"Mana: {player.Mana}";
            CreationAndAuthManager.Instance.loginButton.gameObject.SetActive(true);
        }
    }
}