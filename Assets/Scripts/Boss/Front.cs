using UnityEngine;

public class Front : MonoBehaviour {

    private float damage;
    private float hitboxDuration;
    private bool hasHit;
    
    private void Start() {
        hasHit = false;
    }
    
    public void StartAttack(float damage, float hitboxDuration) {
        this.damage = damage;
        this.hitboxDuration = hitboxDuration;
        hasHit = false;
        gameObject.SetActive(true);

        Invoke("DisableHitbox", hitboxDuration);
    }

    void DisableHitbox() {
        gameObject.SetActive(false);
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
                playerAttack.OnHitReceived(hitboxDuration, damage);

                Debug.Log($"{gameObject.name} 피격");
            }
        }
    }

    
}
