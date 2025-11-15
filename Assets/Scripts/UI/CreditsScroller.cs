using UnityEngine;
using UnityEngine.UI;

public class CreditsScroller : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scrollSpeed = 0.2f;

    void Update()
    {
        if (scrollRect.verticalNormalizedPosition > 0)
        {
            scrollRect.verticalNormalizedPosition -= scrollSpeed * Time.deltaTime;
        }
    }
}
