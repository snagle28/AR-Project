using TMPro;
using UnityEngine;

public class MemoryGameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI instructionText;

    public void SetText(string text)
    {
        if (instructionText != null)
        {
            instructionText.text = text;
        }
    }
}
