using UnityEngine;

/// <summary>
/// Ensures the application process is killed on quit, preventing lingering processes.
/// </summary>
public class ApplicationQuitHandler : MonoBehaviour
{
    private void OnApplicationQuit()
    {
        if (!Application.isEditor)
            System.Diagnostics.Process.GetCurrentProcess().Kill();
    }
}