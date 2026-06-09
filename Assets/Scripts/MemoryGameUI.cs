using TMPro;
using UnityEngine;

public class MemoryGameUI : MonoBehaviour
{
    public TextMeshProUGUI instructionText;

    public void SetText(string text)
{
        if (instructionText != null)
        {
            instructionText.text = text;
        }
    }
}
