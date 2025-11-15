using UnityEngine;

public class LevelUnlockManager : MonoBehaviour
{
    public static LevelUnlockManager instance;

    private const string SkillPointsKey = "SkillPoints";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private string GetUnlockKey(int levelNumber) => $"Level{levelNumber}";

    public void UnlockLevelIfNotAlready(int levelNumber)
    {
        string key = GetUnlockKey(levelNumber);
        if (PlayerPrefs.GetInt(key, 0) == 0)
        {
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }
    }

    public bool IsLevelUnlocked(int levelNumber)
    {
        return PlayerPrefs.GetInt(GetUnlockKey(levelNumber), 0) == 1;
    }

    private string GetCompletedKey(string levelName) => $"Completed_{levelName}";

    public void MarkLevelComplete(string levelName)
    {
        string completeKey = GetCompletedKey(levelName);

        if (PlayerPrefs.GetInt(completeKey, 0) == 0)
        {
            PlayerPrefs.SetInt(completeKey, 1);
            PlayerPrefs.Save();
            AddSkillPoint(1);
        }
    }

    public bool HasCompletedLevel(string levelName)
    {
        return PlayerPrefs.GetInt(GetCompletedKey(levelName), 0) == 1;
    }

    public void AddSkillPoint(int amount)
    {
        if (amount <= 0) return;

        int current = PlayerPrefs.GetInt(SkillPointsKey, 0);
        int newTotal = current + amount;

        PlayerPrefs.SetInt(SkillPointsKey, newTotal);
        PlayerPrefs.Save();
    }

    public void SpendSkillPoint()
    {
        int current = PlayerPrefs.GetInt(SkillPointsKey, 0);
        if (current > 0)
        {
            PlayerPrefs.SetInt(SkillPointsKey, current - 1);
            PlayerPrefs.Save();
        }
    }

    public int GetSkillPoints()
    {
        return PlayerPrefs.GetInt(SkillPointsKey, 0);
    }

    public int GetHighestUnlockedLevel()
    {
        int highest = 1;
        for (int i = 2; i <= 6; i++)
        {
            if (IsLevelUnlocked(i)) highest = i;
        }
        return highest;
    }

}