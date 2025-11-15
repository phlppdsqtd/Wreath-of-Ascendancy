using UnityEngine;

public class Mana : MonoBehaviour
{
    [Header("Mana")]
    [SerializeField] private float startingMana;
    public float currentMana { get; private set; }

    private void Awake()
    {
        currentMana = startingMana;
    }

    public bool UseMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            return true;
        }
        return false;
    }

    public void AddMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, startingMana);
    }

    public void ResetMana()
    {
        currentMana = startingMana;
    }

    public float GetCurrentMana()
    {
        return currentMana;
    }
}
