using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;

public class TypingBattleManager : MonoBehaviour
{
    public static TypingBattleManager Instance { get; private set; }
    private bool ignoreNextInputChange = false;

    [Header("References")]
    public WaveManager waveManager;
    public PlayerAttack3D playerAttack;
    public GameObject currentTarget;

    [Header("UI References")]
    [SerializeField] private TMP_Text wordPromptText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject typingUIContainer;
    [SerializeField] private float victoryDelay = 2.0f;

    [Header("Word Settings")]
    [Tooltip("If empty, defaults will be used")]
    [SerializeField] private string[] wordList = { };

    // ---------- Stats ----------
    private bool sessionActive = false;
    private float sessionStartTime = 0f;
    private float sessionEndTime = 0f;

    // typingActiveTime aggregates only the seconds during which a wave's enemies are present.
    private float typingActiveTime = 0f;   // total active typing time across waves
    private float lastActiveStart = 0f;    // last time we started counting (wave start)
    private bool waveActive = false;       // is typing currently being counted for a wave?

    private int totalKeystrokes = 0;
    private int correctKeystrokes = 0;
    private int currentStreak = 0;
    private int longestStreak = 0;

    private List<string> unusedEasyWords = new List<string>();
    private List<string> unusedMediumWords = new List<string>();
    private List<string> unusedHardWords = new List<string>();

    // ---------- Victory UI ----------
    [Header("Victory Stats UI")]
    [SerializeField] private TMP_Text statAccuracyText;
    [SerializeField] private TMP_Text statWpmText;
    [SerializeField] private TMP_Text statTimeText;
    [SerializeField] private TMP_Text statLongestStreakText;

    private List<string> availableWords;
    private string currentWord;
    private string previousInput = "";

    // ---------- Word lists ----------
    private readonly string[] easyWords = {
        "valor","faith","oath","spire","relic","summit","tower","honor","trial","wreath",
        "blade","ember","flame","stone","light","storm","frost","heart","crest","crown",
        "shade","spark","bloom","vigor","chant","dream","flint","grace","haven","might",
        "noble","peace","quest","river","spear","truth","vital","whirl","blaze","guard",
        "bound","dawn","glory","soul","brave","torch","roots","faithful","steady","shield"
    };

    private readonly string[] mediumWords = {
        "vengeance","destiny","fracture","binding","sanctum","resolve","forsaken","torment",
        "redemption","legacy","awakening","eternal","divine","barrier","command","crimson",
        "defiant","prophecy","miracle","serpent","radiance","guardian","cataclysm","purified",
        "sanctify","evermore","hallowed","reversal","avenging","enchant","illusion","lamented",
        "silencer","brutality","solstice","revelate","captive","spectrum","eclipse","revenger",
        "redesign","forsworn","conquest","obsidian","reformer","sapphire","celestial",
        "reclaimer","reverent","vigilant"
    };

    private readonly string[] hardWords = {
        "ascension","imperium","resilience","sovereign","sacrifice","corruption","divinity",
        "redemption","salvation","phantom","retribution","transcend","domination","cataclysm",
        "purification","incantation","benevolent","deliverance","annihilate","desolation",
        "manifested","rejuvenate","insurgence","everlasting","malevolent","unrelenting",
        "righteous","incorrupt","reincarnate","revolution","obliterate","enlightened",
        "vengefulness","sanctifier","crucifixion","revelation","propagation","desecration",
        "magnificent","providence","reclamation","omnipotent","sovereignty","vindication",
        "sacrificial","prescience","damnation","invincible","deliverant","infallible"
    };

    // ---------- Unity Lifecycle ----------
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        unusedEasyWords.AddRange(easyWords);
        unusedMediumWords.AddRange(mediumWords);
        unusedHardWords.AddRange(hardWords);

        GenerateNewWord();
        FocusInput(true);
        inputField.onValueChanged.AddListener(OnInputChanged);

        if (waveManager != null)
        {
            waveManager.OnWaveStarted += HandleWaveStart;
            waveManager.OnAllWavesCleared += HandleAllWavesCleared;
            waveManager.StartWave(0);
            StartSession();
            FocusInput(true);
            SetTarget(waveManager.GetFirstAliveEnemy());
        }
    }

    // ---------- Word Generation ----------
    private string GetRandomWordForLevel(int level)
    {
        float easyChance = 0.5f, mediumChance = 0.3f, hardChance = 0.2f;
        switch (level)
        {
            case 2: easyChance = 0.4f; mediumChance = 0.4f; hardChance = 0.2f; break;
            case 3: easyChance = 0.3f; mediumChance = 0.4f; hardChance = 0.3f; break;
            case 4: easyChance = 0.25f; mediumChance = 0.45f; hardChance = 0.3f; break;
            case 5: easyChance = 0.2f; mediumChance = 0.4f; hardChance = 0.4f; break;
            case 6: easyChance = 0.15f; mediumChance = 0.35f; hardChance = 0.5f; break;
        }

        float roll = Random.value;
        List<string> pool = unusedEasyWords;
        string[] fullSet = easyWords;

        if (roll < easyChance) { pool = unusedEasyWords; fullSet = easyWords; }
        else if (roll < easyChance + mediumChance) { pool = unusedMediumWords; fullSet = mediumWords; }
        else { pool = unusedHardWords; fullSet = hardWords; }

        if (pool.Count == 0) pool.AddRange(fullSet);

        int index = Random.Range(0, pool.Count);
        string chosen = pool[index];
        pool.RemoveAt(index);
        return chosen;
    }

    private void GenerateNewWord()
    {
        int currentLevel = LevelUnlockManager.instance != null ?
            Mathf.Clamp(LevelUnlockManager.instance.GetHighestUnlockedLevel(), 1, 6) : 1;

        currentWord = GetRandomWordForLevel(currentLevel);
        wordPromptText.text = currentWord;
        ignoreNextInputChange = true;
        inputField.text = "";
        inputField.ActivateInputField();
        previousInput = "";
    }

    // ---------- Typing Logic ----------
    private void OnInputChanged(string input)
    {
        if (ignoreNextInputChange)
        {
            ignoreNextInputChange = false;
            previousInput = input;
            return;
        }

        if (!sessionActive && !string.IsNullOrEmpty(input))
            StartSession();

        TrackKeystrokes(previousInput, input);
        previousInput = input;

        if (playerAttack == null || playerAttack.playerHealth == null) return;
        if (playerAttack.playerHealth.currentHealth <= 0) return;
        if (!inputField.interactable) return;
        if (currentTarget == null) return;

        if (input == currentWord)
        {
            float damage = playerAttack.DamageForWord(currentWord);
            CombatLogManager.Instance?.AddLog($"<color=yellow>The Barbarian</color> attacks <color=red>{currentTarget.name}</color> for <b>{damage}</b> damage!");
            playerAttack.DealDamageTo(currentTarget, damage);

            currentStreak++;
            if (currentStreak > longestStreak) longestStreak = currentStreak;

            GenerateNewWord();
        }
        else if (!currentWord.StartsWith(input))
        {
            CombatLogManager.Instance?.AddLog($"<b>MISTYPE!</b> Attack cancelled!");
            ignoreNextInputChange = true;
            inputField.text = "";
            previousInput = "";
            currentStreak = 0;
        }
    }

    // ---------- Target Highlight ----------
    public void SetTarget(GameObject newTarget)
    {
        if (currentTarget != null)
        {
            var oldOutline = currentTarget.GetComponent<Outline>();
            if (oldOutline != null)
                oldOutline.enabled = false;
        }

        currentTarget = newTarget;

        if (currentTarget != null)
        {
            var outline = currentTarget.GetComponent<Outline>();
            if (outline == null)
                outline = currentTarget.AddComponent<Outline>();

            outline.OutlineColor = Color.red;
            outline.OutlineWidth = 3f;
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.enabled = true;
        }
    }

    // ---------- Death & Wave Handling ----------
    public void OnEntityDied(Health3D deadEntity)
    {
        if (deadEntity == null) return;

        if (deadEntity.CompareTag("Player"))
        {
            // stop active typing timer if player dies mid-wave
            if (waveActive)
            {
                typingActiveTime += Time.time - lastActiveStart;
                waveActive = false;
            }

            inputField.interactable = false;
            StartCoroutine(ShowDefeatLogAfterFrame());
            return;
        }

        if (deadEntity.CompareTag("Enemy"))
        {
            // Notify wave manager about this enemy death (existing behavior)
            waveManager.OnEnemyDied(deadEntity.gameObject);

            // After telling waveManager, check if wave has any alive enemies left.
            // If none, that means the wave just ended — pause typing timer.
            var nextAlive = waveManager.GetFirstAliveEnemy();
            if (nextAlive == null)
            {
                // wave ended — stop counting typing time for this wave
                if (waveActive)
                {
                    typingActiveTime += Time.time - lastActiveStart;
                    waveActive = false;
                }
            }

            // Update current target to the first alive (may be null)
            SetTarget(nextAlive);
        }
    }

    private IEnumerator ShowDefeatLogAfterFrame()
    {
        yield return null; // allow the last attack log to show first
        CombatLogManager.Instance?.AddLog("<b><color=red>You were defeated...</color></b>");
        yield return new WaitForSeconds(2.5f);
        UIManager.Instance.GameOver();
    }

    private void HandleWaveStart(int waveIndex)
    {
        // Called when waveManager starts a wave (first enemy spawn for that wave)
        SetTarget(waveManager.GetFirstAliveEnemy());
        GenerateNewWord();
        SetTypingActive(true);
        FocusInput(true);

        // Start (or resume) counting typing time for this wave
        if (!waveActive)
        {
            lastActiveStart = Time.time;
            waveActive = true;
        }
    }

    private void HandleAllWavesCleared()
    {
        // If a wave was active, finalize typing time
        if (waveActive)
        {
            typingActiveTime += Time.time - lastActiveStart;
            waveActive = false;
        }

        CombatLogManager.Instance?.AddLog("All waves cleared!");
        SetTypingActive(false);
        EndSession();
        StartCoroutine(ShowVictoryWithDelay());
    }

    private IEnumerator ShowVictoryWithDelay()
    {
        yield return new WaitForSeconds(victoryDelay);
        UIManager.Instance.ShowVictory();
    }

    // ---------- Stats ----------
    private void StartSession()
    {
        sessionActive = true;
        sessionStartTime = Time.time;

        // Reset aggregated typing time and counters
        typingActiveTime = 0f;
        lastActiveStart = 0f;
        waveActive = false;

        totalKeystrokes = 0;
        correctKeystrokes = 0;
        currentStreak = 0;
        longestStreak = 0;

        if (statAccuracyText) statAccuracyText.text = "Accuracy: 0%";
        if (statWpmText) statWpmText.text = "WPM: 0";
        if (statTimeText) statTimeText.text = "Time: 0.0s";
        if (statLongestStreakText) statLongestStreakText.text = "Longest Streak: 0";
    }

    private void EndSession()
    {
        if (!sessionActive) return;
        sessionActive = false;

        // if wave is still active at session end, finalize its time
        if (waveActive)
        {
            typingActiveTime += Time.time - lastActiveStart;
            waveActive = false;
        }

        sessionEndTime = Time.time;
        PopulateVictoryStats();
    }

    private void TrackKeystrokes(string oldInput, string newInput)
    {
        if (oldInput == newInput) return;

        // Backspaces/deletions
        if (newInput.Length < oldInput.Length)
        {
            totalKeystrokes += oldInput.Length - newInput.Length;
            return;
        }

        // Additions
        for (int i = oldInput.Length; i < newInput.Length; i++)
        {
            totalKeystrokes++;
            if (i < currentWord.Length && currentWord[i] == newInput[i])
                correctKeystrokes++;
        }
    }

    private void PopulateVictoryStats()
    {
        // Use aggregated typingActiveTime (seconds spent actively in waves)
        float elapsed = Mathf.Max(0.001f, typingActiveTime);
        float minutes = elapsed / 60f;

        float accuracy = totalKeystrokes > 0 ? 100f * (float)correctKeystrokes / (float)totalKeystrokes : 100f;
        float wpm = minutes > 0f ? (correctKeystrokes / 5f) / minutes : 0f;

        if (statAccuracyText) statAccuracyText.text = $"Accuracy: {accuracy:F1}%";
        if (statWpmText) statWpmText.text = $"WPM: {wpm:F1}";
        if (statTimeText) statTimeText.text = $"Active Time: {elapsed:F1}s";
        if (statLongestStreakText) statLongestStreakText.text = $"Longest Streak: {longestStreak}";
    }

    // ---------- Utility ----------
    public void SetTypingActive(bool active)
    {
        inputField.interactable = active;
        if (typingUIContainer) typingUIContainer.SetActive(active);

        if (active)
        {
            inputField.ActivateInputField();
            FocusInput(false);
        }
    }

    private void OnDestroy()
    {
        if (waveManager != null)
        {
            waveManager.OnWaveStarted -= HandleWaveStart;
            waveManager.OnAllWavesCleared -= HandleAllWavesCleared;
        }
    }

    public void FocusInput(bool clear = false)
    {
        if (inputField == null) return;
        if (clear)
        {
            ignoreNextInputChange = true;
            inputField.text = "";
        }
        inputField.interactable = true;
        inputField.ActivateInputField();
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        }
    }
}
