using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour {

    private GameObject _playerTarget;
    private float _damage;
    private float _hitTime;
    private bool _hasHit;

    void Start() {
        // ⭐ 추가: 시작 시 플래그를 초기화하여 이전 실행 상태의 잔여물을 제거합니다.
        _hasHit = false;
    }

    public void StartAttack(GameObject player, float damage, float hitTime) {
        _playerTarget = player;
        _damage = damage;
        _hitTime = hitTime;
        _hasHit = false; // 공격 시작 시 플래그 초기화
        gameObject.SetActive(true);

        Invoke(nameof(DisableHitbox), _hitTime);
    }

    void DisableHitbox() {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D target) {
        if (_hasHit) {
            return;
        }

        if (target.gameObject.Equals(_playerTarget)) {
            _hasHit = true;

            GameObject hitTarget = target.gameObject;
            PlayerComboAttack playerAttack = hitTarget.GetComponentInParent<PlayerComboAttack>();

            if (playerAttack != null) {
                playerAttack.OnHitReceived(_hitTime, _damage);

                Debug.Log($"{gameObject.name} 피격");
            }
        }
    }

};