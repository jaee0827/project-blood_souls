using UnityEngine;

public class AttackEx : MonoBehaviour {

    public float attackDamage = 10.0f;

    void OnTriggerEnter2D(Collider2D target) {
        var targetHealth = target.GetComponent<Health>();
        if (targetHealth == null) {
            return;
        }

        if (!targetHealth.IsAlive()) {
            return;
        }

        float damage = -targetHealth.Damage(attackDamage);
        if (damage <= 0.0f) {
            return;
        }

        var lifeSteal = GetComponent<LifeSteal>();
        if (lifeSteal != null) {
            float heal = lifeSteal.OnDealDamage(damage);
            Debug.Log($"{gameObject.name} 흡혈: {heal}");
        }
    }
}
