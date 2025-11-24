using System.Collections;
using UnityEngine;

public class Pattern : MonoBehaviour {

    [Header("Stat")] // 수치 설정
    public string pattern = "Pattern";
    public float damage = 10;
    public float hitTime = 0.5f;
    [Range(0, 100)]
    public float chance = 20;
    public float cool = 10;
    public float duration = 1.0f;
    public float speed = 10;

    [Header("Object")]
    public GameObject player;
    public GameObject hitbox;
    
    [Header("Animator")]
    public Animator animator;
    
    private float _timer;
    private bool _enabled;
    private float _cooldown;
    private Rigidbody2D _rigidbody;
    private Vector2 _position;

    private void Start()
    {
        Reset();
        
        _rigidbody = hitbox.GetComponent<Rigidbody2D>();
        _position = _rigidbody.position;
        
        hitbox.SetActive(false);
    }

    private void Reset()
    {
        _enabled = true;
        _cooldown = 0;
    }
    
    private void Update() {
        Health health = player.GetComponent<Health>();
        if (health == null || health.isDead)
        {
            return;
        }
        
        if (_enabled) {
            _timer += Time.deltaTime;

            if (_timer > 1.0f) {
                _timer = 0.0f;

                if (!gameObject.tag.Equals("UsedPattern") && Random.value * 100 < chance) {
                    _timer = 0;
                    _enabled = false;

                    gameObject.tag = "UsedPattern";

                    EnemyAttackHitbox enemyAttackHitbox = hitbox.GetComponent<EnemyAttackHitbox>();
                    if (enemyAttackHitbox == null)
                    {
                        return;
                    }
                    
                    StartCoroutine(UsePattern(enemyAttackHitbox));
                    
                    Debug.Log($"{gameObject.name} 패턴 사용: {pattern}");
                    return;
                }
            }
        }
        
        _cooldown += Time.deltaTime;
        
        if (_cooldown < cool) {
            return;
        }
        
        Reset();
    }
    
    private IEnumerator UsePattern(EnemyAttackHitbox enemyAttackHitbox)
    {
        hitbox.SetActive(true);
        
        _rigidbody.MovePosition(_position);
        
        yield return new WaitForSeconds(0.5f);

        enemyAttackHitbox.StartAttack(player, damage, hitTime, duration);

        if (animator != null)
        {
            animator.SetBool("Shooting", true);
        }

        switch (pattern)
        {
            case "front":
                _rigidbody.linearVelocity = new Vector2(-1 * speed, 0);
                break;
            
            case "smash":
                _rigidbody.linearVelocity = new Vector2(-1 * speed, 0);
                break;
        }

        yield return new WaitForSeconds(duration - 0.5f);

        gameObject.tag = "Boss";

        _rigidbody.linearVelocity = Vector2.zero;
        
        if (animator != null)
        {
            animator.SetBool("Shooting", false);
        }

        hitbox.SetActive(false);
    }

}