using UnityEngine;

[DisallowMultipleComponent]
public class LifeSteal : MonoBehaviour {

    [Tooltip("흡혈: 주는 피해량의 일부를 자신의 체력으로 회복")]
    [Range(0.0f, 1.0f)] public float lifeStealRate = 0.2f;

    public float OnDealDamage(float damage) {
        if (damage <= 0.0f || lifeStealRate <= 0.0f) {
            return 0.0f;
        }

        float healAmount = damage * lifeStealRate;

        var health = GetComponent<Health>();
        if (health == null) {
            return 0.0f;
        }

        return health.Heal(healAmount);
    }
}
