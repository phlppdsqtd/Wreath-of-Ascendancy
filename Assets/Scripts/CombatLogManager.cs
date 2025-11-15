using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class CombatLogManager : MonoBehaviour
{
    public static CombatLogManager Instance { get; private set; }

    [SerializeField] private TMP_Text combatLogText;
    [SerializeField] private int maxLines = 4; // optional limit to avoid text overflow

    private readonly List<string> logLines = new();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddLog(string message)
    {
        logLines.Add(message);

        // Keep only the last N lines (optional)
        if (logLines.Count > maxLines)
            logLines.RemoveAt(0);

        // Join all lines with newline characters
        combatLogText.text = string.Join("\n", logLines);
    }
}
