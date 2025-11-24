using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    private bool _enabled;
    private GameObject _player;
    private float _damage;
    private float _hitTime;
    private bool _hasHit;

    private void Start()
    {
        _enabled = false;
        _hasHit = false;
    }
    
    public void StartAttack(GameObject player, float damage, float hitTime, float duration) {
        _enabled = true;
        _player = player;
        _damage = damage;
        _hitTime = hitTime;
        _hasHit = false; // 공격 시작 시 플래그 초기화
        
        Invoke(nameof(Disable), duration);
    }

    private void OnTriggerEnter2D(Collider2D target) {
        if (!_enabled || _hasHit) {
            return;
        }
        
        GameObject targetObject = target.gameObject;
        if (targetObject.Equals(_player)) {
            _hasHit = true;

            PlayerComboAttack playerAttack = targetObject.GetComponentInParent<PlayerComboAttack>();

            if (playerAttack != null)
            {
                Debug.Log($"{targetObject.name} {(playerAttack.OnHitReceived(_hitTime, _damage) ? "피격" : "회피")}: {gameObject.name} ({_damage})");
            }
        }
    }

    private void Disable()
    {
        _enabled = false;
    }

}

