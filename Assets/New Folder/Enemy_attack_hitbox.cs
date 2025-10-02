using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    public float hitboxActiveDuration = 0.5f;
    private GameObject playerTarget;
    private bool hasHit = false;

    void Start()
    {
        // ⭐ 추가: 시작 시 플래그를 초기화하여 이전 실행 상태의 잔여물을 제거합니다.
        hasHit = false;
    }

    public void StartAttack(GameObject player)
    {
        playerTarget = player;
        hasHit = false; // 공격 시작 시 플래그 초기화
        this.gameObject.SetActive(true);

        Invoke("DisableHitbox", hitboxActiveDuration);
    }

    void DisableHitbox()
    {
        this.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (other.gameObject.CompareTag("Player"))
        {
            hasHit = true;

            GameObject hitTarget = other.gameObject;
            PlayerComboAttack playerAttack = hitTarget.GetComponentInParent<PlayerComboAttack>();

            if (playerAttack != null)
            {
                playerAttack.OnHitReceived(hitboxActiveDuration);
                Debug.Log("주인공 피격 감지!");
            }
        }
    }
}