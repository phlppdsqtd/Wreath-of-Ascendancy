using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string nextSceneName;

    [Header("UI Settings")]
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageDisplayTime = 3f;

    [Header("Cinematic Settings")]
    [SerializeField] private GameObject cinematicRawImage;
    [SerializeField] private UnityEngine.Video.VideoPlayer videoPlayer;
    [SerializeField] private AudioSource videoAudio;


    private bool playerInRange = false;
    private bool isTransitioning = false;

    private void Update()
    {
        // Prevent multiple presses during transition
        if (isTransitioning)
            return;

        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            string msg = "A distant, unfamiliar voice whispers...\n\"Step through, ascendant.\"";
            StartCoroutine(ShowMessageAndLoadScene(msg));
        }
    }

    // ------------ 3D Collision ------------
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    // ------------ Message Display ------------
    private void ShowMessage(string msg)
    {
        if (messagePanel != null && messageText != null)
        {
            messageText.text = msg;
            messagePanel.SetActive(true);

            CancelInvoke(nameof(HideMessage));
            Invoke(nameof(HideMessage), messageDisplayTime);
        }
    }

    private void HideMessage()
    {
        if (messagePanel != null)
            messagePanel.SetActive(false);
    }

    // ------------ Transition Flow ------------
    private IEnumerator ShowMessageAndLoadScene(string msg)
    {
        isTransitioning = true;

        ShowMessage(msg);
        yield return new WaitForSeconds(messageDisplayTime);
        yield return StartCoroutine(PlayCinematic());


        // -----------------------------------------
        //  FUTURE SKILL UI SYSTEM (currently off)
        // -----------------------------------------
        /*
        string currentScene = SceneManager.GetActiveScene().name;
        LevelUnlockManager.instance.MarkLevelComplete(currentScene);

        UIManager.Instance.ShowSkillSelectionUI();

        // Wait until player closes skill screen
        while (UIManager.Instance.IsSkillUIOpen())
            yield return null;
        */

        // Load next level
        //SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator PlayCinematic()
    {
        cinematicRawImage.SetActive(true);

        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();

        while (videoPlayer.isPlaying)
            yield return null;

        SceneManager.LoadScene(nextSceneName);
    }

}
