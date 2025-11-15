using UnityEngine;
using TMPro;
using System.Collections;

public class WaveAnnouncementUI : MonoBehaviour
{
    [SerializeField] private TMP_Text announcementText;
    [SerializeField] private float displayTime = 2f;

    private Coroutine displayRoutine;

    public void ShowMessage(string message)
    {
        if (displayRoutine != null)
            StopCoroutine(displayRoutine);
        displayRoutine = StartCoroutine(DisplayMessageRoutine(message));
    }

    private IEnumerator DisplayMessageRoutine(string message)
    {
        announcementText.gameObject.SetActive(true);
        announcementText.text = message;
        yield return new WaitForSeconds(displayTime);
        announcementText.gameObject.SetActive(false);
    }
}
