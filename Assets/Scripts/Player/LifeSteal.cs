using UnityEngine;

[DisallowMultipleComponent]
public class LifeSteal : MonoBehaviour {

    [Header("Stat"),Range(0, 100)] // 체력 배율 설정
    public float lifeStealRate = 20;

    // 흡혈 실행 함수
    public void ExecuteSteal(float damage) {
        if (lifeStealRate <= 0) {
            return;
        }

        float healAmount = damage * lifeStealRate * 0.01f; // 흡혈량 계산

        Health health = GetComponent<Health>();
        if (health == null) {
            return;
        }

        float heal = health.Heal(healAmount); // 플레이어 회복

        Debug.Log($"{gameObject.name} 흡혈: {heal}"); // 로그 출력
    }

}
