using UnityEngine;

public class IconBillboardScaler : MonoBehaviour
{
    [Header("Scaling Settings")]
    [SerializeField] public Vector3 smallScale = Vector3.one * 0.8f;
    [SerializeField] public Vector3 largeScale = Vector3.one * 1.2f;
    [SerializeField] public float duration = 0.5f;

    private Coroutine scalingCoroutine;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        scalingCoroutine = StartCoroutine(ScaleLoop());
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        // -------------------------
        // ⭐ Billboard (upright only)
        // -------------------------
        Vector3 camForward = cam.transform.forward;
        camForward.y = 0;                    // keep upright
        transform.forward = camForward;      // face camera
    }

    // -------------------------
    // ⭐ Scale pulsing loop
    // -------------------------
    private System.Collections.IEnumerator ScaleLoop()
    {
        while (true)
        {
            yield return StartCoroutine(ScaleOverTime(smallScale, largeScale, duration));
            yield return StartCoroutine(ScaleOverTime(largeScale, smallScale, duration));
        }
    }

    private System.Collections.IEnumerator ScaleOverTime(Vector3 start, Vector3 end, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            transform.localScale = Vector3.Lerp(start, end, elapsed / time);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = end;
    }
}
