using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionScrolling : MonoBehaviour
{
    public Scrollbar horizontalScrollbar;
    public float scrollStep = 0.1f; // Adjust this for the percentage of scrolling

    public void ScrollLeft()
    {
        if (horizontalScrollbar != null)
        {
            horizontalScrollbar.value = Mathf.Clamp01(horizontalScrollbar.value + scrollStep);
        }
    }

    public void ScrollRight()
    {
        if (horizontalScrollbar != null)
        {
            horizontalScrollbar.value = Mathf.Clamp01(horizontalScrollbar.value - scrollStep);
        }
    }
}