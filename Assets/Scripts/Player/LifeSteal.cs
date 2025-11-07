using UnityEngine;

[DisallowMultipleComponent]
public class LifeSteal : MonoBehaviour {

    [Header("Stat"),Range(0.0f, 1.0f)] // 체력 배율 설정
    public float lifeStealRate = 0.2f;

    // 흡혈 실행 함수
    public float ExecuteSteal(float damage) {
        if (lifeStealRate <= 0.0f) {
            return 0.0f;
        }

        float healAmount = damage * lifeStealRate; // 흡혈량 계산

        Health health = GetComponent<Health>();
        if (health is null) {
            return 0.0f;
        }

        float heal = health.Heal(healAmount); // 플레이어 회복

        Debug.Log($"{gameObject.name} 흡혈: {heal}"); // 로그 출력

        return heal;
    }

}
