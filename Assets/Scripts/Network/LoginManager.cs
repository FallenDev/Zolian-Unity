using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Network
{
    public class LoginManager : MonoBehaviour
    {
        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;
        public Button loginButton;

        private void Start()
        {
            // Hook up the button to the login logic
            loginButton.onClick.AddListener(OnLoginButtonClick);
        }

        private void OnLoginButtonClick()
        {
            var username = usernameInput.text;
            var password = passwordInput.text;

            if (string.IsNullOrEmpty(username))
            {
                PopupManager.Instance.ShowMessage("Username cannot be empty!");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                PopupManager.Instance.ShowMessage("Password cannot be empty!");
                return;
            }

            // Call your networking logic here for authentication
            LoginClient.Instance.SendLoginCredentials(username, password);
        }
    }
}