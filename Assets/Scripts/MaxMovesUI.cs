using TMPro;
using UnityEngine;

public class MaxMovesUI : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] TMP_Text maxMovesText;

    void Start()
    {
        if (player && maxMovesText)
        {
            maxMovesText.text = $"Max Moves: {player.maxMoves}";
        }
    }
}
