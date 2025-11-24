using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // === 인스펙터 설정 변수 ===
    [Header("컴포넌트 연결")]
    public Animator anim;
    public GameObject hitboxObject;

    [Header("공격 설정")]
    public float attackDuration = 0.5f;

    private bool isAttacking = false;
    private EnemyAttackHitbox hitboxScript;

    void Start()
    {
        // 1. Animator를 스스로 찾는지 확인
        if (anim == null)
        {
            Debug.Log(this.gameObject.name + "에서 Animator를 스스로 찾습니다.");
            anim = GetComponent<Animator>();
        }

        // 2. Hitbox Object가 인스펙터에서 연결되었는지 확인
        if (hitboxObject == null)
        {
            Debug.LogError(this.gameObject.name + "의 Inspector 창에서 Hitbox Object가 비어있습니다!", this.gameObject);
        }
        else
        {
            // 3. 연결된 Hitbox Object에서 스크립트를 제대로 찾아오는지 확인
            hitboxScript = hitboxObject.GetComponent<EnemyAttackHitbox>();
            if (hitboxScript == null)
            {
                Debug.LogError(this.gameObject.name + "의 hitboxObject(" + hitboxObject.name + ")에는 연결했지만, 그 안에 EnemyAttackHitbox 스크립트가 없습니다!", hitboxObject);
            }
            else
            {
                Debug.Log(this.gameObject.name + "의 hitboxScript가 성공적으로 연결되었습니다.");
                hitboxObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            Attack();
        }
    }

    void Attack()
    {
        // 4. Attack 함수 실행 시점에 각 변수가 비어있는지 따로따로 확인
        bool hasError = false;
        if (anim == null)
        {
            Debug.LogError("Attack 실패: Animator(anim)가 비어있습니다! 어느 오브젝트인지 확인하세요.", this.gameObject);
            hasError = true;
        }
        if (hitboxScript == null)
        {
            Debug.LogError("Attack 실패: EnemyAttackHitbox(hitboxScript)가 비어있습니다! 어느 오브젝트인지 확인하세요.", this.gameObject);
            hasError = true;
        }

        // 에러가 있으면 여기서 함수를 중단하고 에디터를 일시정지
        if (hasError)
        {
            Debug.Break(); // 에디터를 일시정지시켜서 문제 상황을 바로 확인할 수 있게 함
            return;
        }

        // --- 기존 Attack 로직 ---
        isAttacking = true;
        anim.SetTrigger("EnemyAttack");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.WakeUp();
            }
            hitboxScript.StartAttack(player, 10, 0.5f, 0.5f);
        }
        else
        {
            Debug.LogError("주인공 오브젝트(태그: Player)를 찾을 수 없습니다.");
        }

        Invoke(nameof(ResetAttack), attackDuration);
    }

    void ResetAttack()
    {
        isAttacking = false;
    }
}