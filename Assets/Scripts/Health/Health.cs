using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour {

    private const float DefaultHealth = 100.0f; // 체력 기본값

    [Header("Stat")] // 수치 설정
    public float maxHealth = DefaultHealth;
    public float currentHealth = DefaultHealth;
    public float drainDamage = 0.0f;

    [Header("Animator")] // 애니메이터 설정
    public Animator animator;

    [Header("Event")] // 이벤트 설정
    public UnityEvent<float, float> onHealthChange;

    [HideInInspector]
    public bool isDead = false; // 사망 상태 변수

    void Start() {
        currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth); // 잘못된 현재 체력 정정

        if (animator == null) {
            animator = GetComponent<Animator>();
        }
    }

    // 초기화 함수
    void Reset() => currentHealth = maxHealth;

    void Update() {
        if (drainDamage > 0.0f && !isDead) {
            Damage(drainDamage * Time.deltaTime, true);
        }
    }

    public float ChangeHealth(float amount, bool isDrain) {
        if (isDead) {
            return 0;
        }

        float preHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0.0f, maxHealth); // 체력 계산
        onHealthChange?.Invoke(currentHealth, maxHealth); // 이벤트 호출 (UI 변화 전용)

        if (currentHealth <= 0.0f) {
            Die(); // 체력 0 이하면 사망 처리

        } else if (!isDrain && amount < 0 && animator != null) {
            animator.SetTrigger("Hit"); // 피격 애니메이션
        }

        if (!isDrain) {
            string prefix = currentHealth > preHealth ? "+" : "";
            Debug.Log($"{gameObject.name} 체력 변화: {preHealth} -> {currentHealth} ({prefix}{currentHealth - preHealth})"); // 로그 출력
        }

        return currentHealth - preHealth;
    }

    // 피해 함수
    public float Damage(float amount, bool isDrain = false) {
        if (amount <= 0.0f) {
            return 0.0f;
        }

        return ChangeHealth(-amount, isDrain);
    }

    // 회복 함수
    public float Heal(float amount, bool isDrain = false) {
        if (amount <= 0.0f) {
            return 0.0f;
        }

        return ChangeHealth(amount, isDrain);
    }

    // 사망 함수
    public void Die() {
        isDead = true;

        if (animator != null) {
            animator.SetTrigger("Death"); // 사망 애니메이션
        }

        Debug.Log($"{gameObject.name} 사망"); // 로그 출력

    }

}
