using UnityEngine;
using UnityEngine.UI;

public class Healthbar3D : MonoBehaviour
{
    [SerializeField] private Health3D playerHealth;
    [SerializeField] private Image totalHealthBar;
    [SerializeField] private Image currentHealthBar;

    void Start()
    {
        if (playerHealth != null)
            totalHealthBar.fillAmount = 1f; // full at start
    }

    void Update()
    {
        if (playerHealth != null)
            currentHealthBar.fillAmount = playerHealth.currentHealth / playerHealth.MaxHealth;
    }

}
