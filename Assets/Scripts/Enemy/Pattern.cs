using System.Collections;
using System.Collections.Generic;
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
    public GameObject effect;
    
    [Header("Animator")]
    public Animator animator;
    
    private float _timer;
    private bool _enabled;
    private float _cooldown;
    private Rigidbody2D _rigidbody;
    private Vector2 _position;
    private float _rotation;
    private Vector3 _scale;

    private void Start()
    {
        Reset();
        
        _rigidbody = hitbox.GetComponent<Rigidbody2D>();
        _position = _rigidbody.position;
        _rotation = _rigidbody.rotation;
        _scale = hitbox.transform.localScale;
        
        hitbox.SetActive(false);
        
        effect.SetActive(false);
    }

    private void Reset()
    {
        _enabled = true;
        _cooldown = 0;
    }
    
    private void Update() {
        if (player.GetComponent<Health>().isDead || gameObject.GetComponent<Health>().isDead)
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
        _rigidbody.rotation = _rotation;
        hitbox.transform.localScale = _scale;
        
        effect.SetActive(true);
        
        effect.transform.position = _position;
        
        yield return new WaitForSeconds(0.5f);
        
        effect.SetActive(false);
        
        enemyAttackHitbox.StartAttack(player, damage, hitTime, duration);

        if (animator != null)
        {
            animator.SetBool("Shooting", true);
        }

        float delay = 0.0f;
        
        List<GameObject> clones = new List<GameObject>();
        switch (pattern)
        {
            case "front":
                _rigidbody.linearVelocity = new Vector2(-1 * speed, 0);
                break;
            
            case "down":
                _rigidbody.linearVelocity = new Vector2(0, speed);
                
                for (int i = 0; i < 8; i++)
                {
                    GameObject clone = Instantiate(hitbox, new Vector2(_position.x + 2 * (i + 1), _position.y), Quaternion.identity);
                    clones.Add(clone);
                    
                    clone.transform.rotation = Quaternion.Euler(0, 0, -90);
                    clone.transform.localScale = new Vector3(1.5f, 1.5f, 1);
                    
                    yield return new WaitForSeconds(0.3f);
                    
                    clone.GetComponent<Animator>().SetBool("Shooting", true);
                    
                    clone.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, speed);
                    clone.GetComponent<EnemyAttackHitbox>().StartAttack(player, damage, hitTime, duration);
                }
                
                delay = 2.4f;
                break;
            
            case "smash":
                _rigidbody.angularVelocity = 180;
                
                yield return new WaitForSeconds(0.5f);
                
                _rigidbody.angularVelocity = 0;
                _rigidbody.rotation = 90;
                
                delay = 0.5f;
                break;
            
            case "push":
                for (int i = 0; i < 90; i++)
                {
                    Vector3 scale = hitbox.transform.localScale;
                    hitbox.transform.localScale = new Vector3(scale.x, scale.y + 0.01f, scale.z);
                    
                    yield return new WaitForSeconds(0.01f);
                }
                
                delay = 0.9f;
                break;
        }

        yield return new WaitForSeconds(duration - delay - 0.5f);

        gameObject.tag = "Boss";

        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0;
        
        clones.ForEach(Destroy);
        
        if (animator != null)
        {
            animator.SetBool("Shooting", false);
        }

        hitbox.SetActive(false);
    }

}