using UnityEngine;

public class SkillBootstrapper : MonoBehaviour
{
    public GameObject playerSkillManagerPrefab;

    void Start()
    {
        if (PlayerSkillManager.instance == null && playerSkillManagerPrefab != null)
        {
            GameObject skillManagerObject = Instantiate(playerSkillManagerPrefab);
            DontDestroyOnLoad(skillManagerObject);
        }
    }
}
