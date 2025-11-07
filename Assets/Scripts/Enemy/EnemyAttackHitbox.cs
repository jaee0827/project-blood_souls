using UnityEditor.Media;
using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour {

    [Header("Stat")]
    public float hitboxActiveDuration = 0.5f;
    public float damage = 10.0f; // 피해량 설정
    private GameObject playerTarget;
    private bool hasHit;

    void Start() {
        // ⭐ 추가: 시작 시 플래그를 초기화하여 이전 실행 상태의 잔여물을 제거합니다.
        hasHit = false;
    }

    public void StartAttack(GameObject player) {
        playerTarget = player;
        hasHit = false; // 공격 시작 시 플래그 초기화
        this.gameObject.SetActive(true);

        Invoke("DisableHitbox", hitboxActiveDuration);
    }

    void DisableHitbox() {
        this.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D target) {
        if (hasHit) {
            return;
        }

        if (target.gameObject.CompareTag("Player")) {
            hasHit = true;

            GameObject hitTarget = target.gameObject;
            PlayerComboAttack playerAttack = hitTarget.GetComponentInParent<PlayerComboAttack>();

            if (playerAttack != null) {
                playerAttack.OnHitReceived(hitboxActiveDuration, damage);

                Debug.Log($"{gameObject.name} 피격");
            }
        }
    }

};