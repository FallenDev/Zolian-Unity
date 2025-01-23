using UnityEngine;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    public InputField usernameInput;
    public InputField passwordInput;
    public Button loginButton;
    
    void Start()
    {
        // Hook up the button to the login logic
        loginButton.onClick.AddListener(OnLoginButtonClick);
    }

    void OnLoginButtonClick()
    {
        string username = usernameInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Username or Password cannot be empty!");
            return;
        }

        // Call your networking logic here for authentication
        AuthenticateUser(username, password);
    }

    void AuthenticateUser(string username, string password)
    {
        // Example for sending credentials to the server:
        // You can now send the username and password over the network
        Debug.Log($"Authenticating user: {username}");

        // Simulate server authentication (you'll replace this with your networking call)
        if (username == "test" && password == "password123")
        {
            Debug.Log("Login successful!");
            // Proceed to the next scene or the main menu
            // Example: SceneManager.LoadScene("MainMenu");
        }
        else
        {
            Debug.LogError("Invalid login credentials.");
        }
    }
}