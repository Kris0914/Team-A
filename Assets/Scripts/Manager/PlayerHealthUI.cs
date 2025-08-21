using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public Image hpBarFill; // Inspector에 Drag & Drop

    private int maxHealth = 100;
    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (hpBarFill != null) // 널 체크
            hpBarFill.fillAmount = (float)currentHealth / maxHealth;
    }
}




