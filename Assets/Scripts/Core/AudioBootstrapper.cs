using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioBootstrapper : MonoBehaviour
{
    void Start()
    {
        if (SoundManager.instance == null)
        {
            GameObject soundManagerObject = Instantiate(Resources.Load("SoundManager")) as GameObject;
            DontDestroyOnLoad(soundManagerObject);

            SoundManager sm = soundManagerObject.GetComponent<SoundManager>();
            sm.OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
    }
}