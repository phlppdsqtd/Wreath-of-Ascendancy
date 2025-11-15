using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public System.Action<int> OnWaveStarted;
    public System.Action OnAllWavesCleared;

    [System.Serializable]
    public class Wave
    {
        public List<GameObject> enemies; // Assign manually in the Inspector
    }

    [Header("Wave Settings")]
    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private float delayBetweenWaves = 3f; // ⏱ delay in seconds
    [SerializeField] private WaveAnnouncementUI waveUI;

    private int currentWaveIndex = 0;
    public bool AllWavesCleared { get; private set; } = false;

    void Start()
    {
        // Disable all enemies at start
        foreach (var wave in waves)
        {
            foreach (var enemy in wave.enemies)
            {
                if (enemy != null) enemy.SetActive(false);
            }
        }
    }

    public void StartWave(int index)
    {
        if (index >= waves.Count)
        {
            //Debug.Log("✅ All waves cleared!");
            AllWavesCleared = true;
            OnAllWavesCleared?.Invoke(); // 👈 Notify TypingBattleManager
            return;
        }

        // Disable all previous enemies
        foreach (var wave in waves)
        {
            foreach (var enemy in wave.enemies)
            {
                if (enemy != null) enemy.SetActive(false);
            }
        }

        currentWaveIndex = index;
        AllWavesCleared = false;

        // Activate enemies for this wave
        foreach (var enemy in waves[index].enemies)
        {
            if (enemy != null)
            {
                enemy.SetActive(true);
                UIManager.Instance?.RegisterEnemy(enemy); // register for counters
            }
        }

        //Debug.Log($"🔥 Wave {index + 1} started!");
        OnWaveStarted?.Invoke(index);
    }

    public void BeginWaves()
    {
        StartWave(0);
    }

    public void OnEnemyDied(GameObject enemy)
    {
        var currentWave = waves[currentWaveIndex];
        CombatLogManager.Instance?.AddLog($"<color=red>{enemy.name}</color> defeated!");
        currentWave.enemies.Remove(enemy);

        if (currentWave.enemies.Count == 0)
        {
            //Debug.Log($"Wave {currentWaveIndex + 1} cleared!");

            // 🧠 Tell TypingBattleManager to disable input while waiting
            TypingBattleManager.Instance?.SetTypingActive(false);

            StartCoroutine(NextWaveDelay());
        }
    }

    private IEnumerator NextWaveDelay()
    {
        //Debug.Log($"Waiting {delayBetweenWaves} seconds before next wave...");
        // show cleared message
        if (waveUI != null)
            waveUI.ShowMessage($"Wave {currentWaveIndex + 1} Cleared!");
        // wait the configured delay (this is when the announcement is visible)
        yield return new WaitForSeconds(delayBetweenWaves);

        currentWaveIndex++;

        if (currentWaveIndex < waves.Count)
        {
            if (waveUI != null)
                waveUI.ShowMessage($"Wave {currentWaveIndex + 1} Starting!");
            StartWave(currentWaveIndex);
        }
        else
        {
            waveUI?.ShowMessage("All Waves Cleared!");
            //Debug.Log("✅ All waves cleared!");
            AllWavesCleared = true;
            // notify listeners
            OnAllWavesCleared?.Invoke();
        }
    }

    public GameObject GetFirstAliveEnemy()
    {
        if (AllWavesCleared) return null;

        foreach (var enemy in waves[currentWaveIndex].enemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
                return enemy;
        }

        return null;
    }
}
