using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private int levelNumberToLoad;
    [SerializeField] private GameObject bgLock;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Button button;

    private void Start()
    {
        Refresh();
        button.onClick.AddListener(TryLoadLevel);
    }

    private void TryLoadLevel()
    {
        string sceneToLoad = "Level" + levelNumberToLoad;

        if (SceneManager.GetActiveScene().name == sceneToLoad)
            return;

        if (LevelUnlockManager.instance.IsLevelUnlocked(levelNumberToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    public void Refresh()
    {
        bool isUnlocked = LevelUnlockManager.instance.IsLevelUnlocked(levelNumberToLoad);
        bgLock.SetActive(!isUnlocked);
        lockIcon.SetActive(!isUnlocked);
        button.interactable = isUnlocked;
    }

}