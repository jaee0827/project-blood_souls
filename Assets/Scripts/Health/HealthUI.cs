using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour {
    
    [Header("Components")] // 컴포넌트 지정
    public Health health;
    public Slider healthBar;
    public TMP_Text healthText;

    void Start() {
        if (health != null) {
            health.OnHealthChange.AddListener(UpdateUI); // 이벤트 리스너 등록

            UpdateUI(health.currentHealth, health.maxHealth); // UI 초기 설정
        }
    }

    // UI 설정 함수
    void UpdateUI(float current, float max) {
        if (healthBar != null) {
            healthBar.value = max > 0 ? current / max : 0f; // 바 설정
        }
        
        if (healthText != null) {
            healthText.text = $"{current:0} / {max:0}"; // 텍스트 설정
        }
    }
}
