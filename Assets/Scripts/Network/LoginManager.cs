using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Network
{
    public class LoginManager : MonoBehaviour
    {
        public TMP_InputField steamId;
        public Button loginButton;

        private void Start()
        {
            // Hook up the button to the login logic
            loginButton.onClick.AddListener(OnLoginButtonClick);
        }

        private void OnLoginButtonClick()
        {
            var successful = int.TryParse(steamId.text, out var localSteamId);

            if (!successful)
            {
                PopupManager.Instance.ShowMessage("No account exists, would you like to create one?");
                return;
            }

            // Call your networking logic here for authentication
            LoginClient.Instance.SendLoginCredentials(localSteamId);
        }
    }
}