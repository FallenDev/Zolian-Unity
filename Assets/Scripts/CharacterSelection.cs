using Assets.Scripts.Managers;
using Assets.Scripts.Network;
using Assets.Scripts.Network.PacketArgs.ReceiveFromServer;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
    public PlayerSelection player;
    private Button button;
    private TextMeshProUGUI nameText;

    private void Awake()
    {
        button = GetComponent<Button>();
        nameText = GetComponentInChildren<TextMeshProUGUI>();

        if (button != null)
            button.onClick.AddListener(OnCharacterSelected);
    }

    public void Initialize(PlayerSelection newPlayer)
    {
        player = newPlayer;
        nameText.text = player.Name;
    }

    private void OnCharacterSelected()
    {
        Debug.Log($"Character {player.Name} selected!");
        Debug.Log($"{player.Serial} {LoginClient.Instance.SteamId}");
        CharacterSelectionManager.Instance.SelectCharacter(player);
    }
}