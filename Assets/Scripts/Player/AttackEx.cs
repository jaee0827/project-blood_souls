using UnityEngine;

public class AttackEx : MonoBehaviour {

    public float damage = 10.0f; // 피해량 설정

    private bool hasHit = false;

    void Update() {
        if (Input.GetKey(KeyCode.X) && !hasHit) {
            hasHit = true;

            GameObject enemy = GameObject.FindGameObjectWithTag("Enemy"); // 적 지정
            if (enemy == null) {
                return;
            }

            Health health = enemy.GetComponent<Health>();
            if (health == null || health.isDead || this.damage <= 0.0f) {
                return;
            }

            float damage = -health.Damage(this.damage); // 적 피해

            if (damage >= 0) {
                LifeSteal lifeSteal = GetComponent<LifeSteal>();
                if (lifeSteal != null) {
                    lifeSteal.ExecuteSteal(damage); // 플레이어 흡혈
                }
            }

        } else if (Input.GetKey(KeyCode.Z)) {
            hasHit = false; // 연속 입력 방지
        }
    }

}