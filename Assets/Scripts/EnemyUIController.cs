using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(Health3D))]
public class EnemyUIController : MonoBehaviour
{
    public static List<EnemyUIController> ActiveEnemies = new List<EnemyUIController>();

    [Header("UI Prefab")]
    public GameObject enemyUIPrefab; // Assign your EnemyUI prefab here (child of Canvas)

    [Header("Follow Offset")]
    public Vector3 uiOffset = new Vector3(0f, 2.0f, 0f);

    // Runtime references
    private GameObject spawnedUI;
    private TMP_Text nameText;
    private EnemyUIFollow follow;
    private Health3D health;
    private Image hpFillImage;

    void Awake()
    {
        health = GetComponent<Health3D>();
    }

    void Start()
    {
        StartCoroutine(WaitForIntroThenSpawnUI());

        /*
        if (enemyUIPrefab == null)
        {
            Debug.LogWarning("EnemyUIController: No enemyUIPrefab assigned.");
            return;
        }

        Canvas c = FindFirstObjectByType<Canvas>();
        if (c == null)
        {
            Debug.LogWarning("EnemyUIController: No Canvas found in scene.");
            return;
        }

        spawnedUI = Instantiate(enemyUIPrefab, c.transform, false);

        // ✅ Corrected paths
        hpFillImage = spawnedUI.transform.Find("EnemyHealthbar/EnemyHealthbarCurrent")?.GetComponent<Image>();
        nameText = spawnedUI.transform.Find("EnemyNameText")?.GetComponent<TMP_Text>();

        if (nameText != null)
            nameText.text = gameObject.name;

        if (hpFillImage != null && health != null)
            hpFillImage.fillAmount = health.currentHealth / health.MaxHealth;

        follow = spawnedUI.GetComponent<EnemyUIFollow>();
        if (follow != null)
        {
            follow.target = this.transform;
            follow.offset = uiOffset;
        }
        */
    }

    void Update()
    {
        if (spawnedUI == null || health == null) return;

        // Update HP fill amount
        if (hpFillImage != null)
        {
            hpFillImage.fillAmount = health.currentHealth / health.MaxHealth;
            //Debug.Log($"{gameObject.name} HP fill: {hpFillImage.fillAmount}");
        }

        // Remove UI when enemy dies or deactivates
        if (!gameObject.activeInHierarchy || health.currentHealth <= 0f)
        {
            CleanupUI();
        }
    }

    public void CleanupUI()
    {
        if (spawnedUI != null)
        {
            Destroy(spawnedUI);
            spawnedUI = null;
        }
    }

    void OnDestroy()
    {
        CleanupUI();
    }

    void OnEnable()
    {
        if (!ActiveEnemies.Contains(this))
            ActiveEnemies.Add(this);
    }

    void OnDisable()
    {
        ActiveEnemies.Remove(this);
        CleanupUI();
    }

    public void SetUIVisible(bool visible)
    {
        if (spawnedUI != null)
            spawnedUI.SetActive(visible);
    }

    private IEnumerator WaitForIntroThenSpawnUI()
    {
        // Wait while intro is active
        while (UIManager.Instance != null && UIManager.Instance.floorIntroActive)
            yield return null;

        // Intro finished — now spawn UI
        SpawnUI();
    }

    private void SpawnUI()
    {
        if (enemyUIPrefab == null) return;

        Canvas c = FindFirstObjectByType<Canvas>();
        if (c == null) return;

        spawnedUI = Instantiate(enemyUIPrefab, c.transform, false);

        hpFillImage = spawnedUI.transform.Find("EnemyHealthbar/EnemyHealthbarCurrent")?.GetComponent<Image>();
        nameText = spawnedUI.transform.Find("EnemyNameText")?.GetComponent<TMP_Text>();

        if (nameText != null)
            nameText.text = gameObject.name;

        if (hpFillImage != null && health != null)
            hpFillImage.fillAmount = health.currentHealth / health.MaxHealth;

        follow = spawnedUI.GetComponent<EnemyUIFollow>();
        if (follow != null)
        {
            follow.target = this.transform;
            follow.offset = uiOffset;
        }
    }

}
