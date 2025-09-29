using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour {
    
    public Health health;
    public Slider healthBar;
    public TMP_Text healthText;

    void Start() {
        if (health != null) {
            health.OnHealthChange.AddListener(UpdateUI);
            UpdateUI(health.currentHealth, health.maxHealth);
        }
    }

    void UpdateUI(float current, float max) {
        if (healthBar != null) {
            healthBar.value = max > 0 ? current / max : 0f;
        }
        
        if (healthText != null) {
            healthText.text = $"{current:0} / {max:0}";
        }
    }
}
