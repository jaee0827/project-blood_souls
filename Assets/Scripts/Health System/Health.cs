using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Health : MonoBehaviour {

    [Header("Stats")]
    public float maxHealth = 100.0f;
    public float currentHealth;

    [Header("Events")]
    public UnityEvent<float, float> OnHealthChange;
    public UnityEvent OnDied;

    void Start() {
        currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth);
    }

    void Reset() => currentHealth = maxHealth;

    public float ChangeHealth(float amount) {
        float preHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0.0f, maxHealth);
        OnHealthChange?.Invoke(currentHealth, maxHealth);

        Debug.Log($"{gameObject.name} 체력 변화: {preHealth} -> {currentHealth} ({currentHealth - preHealth})");

        if (currentHealth <= 0.0f) {
            Debug.Log($"{gameObject.name} 사망");
            OnDied?.Invoke();
        }

        return currentHealth - preHealth;
    }

    public float Damage(float amount) {
        if (amount <= 0.0f) {
            return 0.0f;
        }

        return ChangeHealth(-amount);
    }

    public float Heal(float amount) {
        if (amount <= 0.0f) {
            return 0.0f;
        }

        return ChangeHealth(amount);
    }

    public bool IsAlive() => currentHealth > 0.0f;
}
