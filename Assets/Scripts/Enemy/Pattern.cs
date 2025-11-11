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

    [Header("Component")]
    public GameObject player;
    public GameObject hitbox;
    
    private float _timer;
    private bool _enabled;
    private float _cooldown;
    private Transform _enemyTransform;

    private void Start()
    {
        Reset();
    }

    private void Reset()
    {
        _enabled = true;
        _cooldown = 0;
        _enemyTransform = transform;
    }
    
    private void FixedUpdate() {
        if (_enabled) {
            _timer += Time.deltaTime;

            if (_timer > 1.0f) {
                _timer = 0.0f;

                if (!gameObject.tag.Equals("UsedPattern") && Random.value * 100 < chance) {
                    _timer = 0;
                    _enabled = false;

                    gameObject.tag = "UsedPattern";

                    EnemyAttackHitbox enemyAttackHitbox = hitbox.GetComponent<EnemyAttackHitbox>();
                    if (enemyAttackHitbox == null || !UsePattern())
                    {
                        return;
                    }
                    
                    enemyAttackHitbox.StartAttack(player, damage, hitTime);
                    
                    Debug.Log($"{gameObject.name} 패턴 사용: {pattern}");

                    Invoke(nameof(ChangeTag), duration);
                    return;
                }
            }
        }
        
        _cooldown += Time.fixedDeltaTime;
        
        if (_cooldown < cool) {
            return;
        }
        
        Reset();
    }
    
    private bool UsePattern()
    {
        switch (pattern)
        {
            case "front":
                for (int i = 0; i < 5; i++)
                {
                    Invoke(nameof(ExecuteDelayed), 0.5f);
                }

                return true;
            
            case "":
                return true;
        }
        
        return false;
    }

    private void ExecuteDelayed()
    {
        switch (pattern)
        {
            case "front":
                hitbox.transform.position = (Vector2)_enemyTransform.position + new Vector2(1.0f, 0.0f);
                break;
        }
    }
    
    private void ChangeTag() {
        gameObject.tag = "Boss";
    }

}