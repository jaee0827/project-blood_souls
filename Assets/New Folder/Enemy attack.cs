using UnityEngine;

public class EnemyController : MonoBehaviour
{
    // === 인스펙터 설정 변수 ===
    [Header("컴포넌트 연결")]
    public Animator anim;
    // Enemy_Hitbox 오브젝트를 인스펙터에서 연결하세요.
    public GameObject hitboxObject;

    [Header("공격 설정")]
    public float attackDuration = 0.5f;

    private bool isAttacking = false;
    private EnemyAttackHitbox hitboxScript;

    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();

        if (hitboxObject != null)
        {
            hitboxScript = hitboxObject.GetComponent<EnemyAttackHitbox>();
            hitboxObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && !isAttacking)
        {
            Attack();
        }
    }

    void Attack()
    {
        if (anim == null || hitboxScript == null)
        {
            Debug.LogError("적 설정 오류: 컴포넌트 연결을 확인하세요.");
            return;
        }

        isAttacking = true;
        anim.SetTrigger("EnemyAttack");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // ⭐ 추가: 주인공 Rigidbody를 깨워서 충돌 감지 누락을 방지합니다.
            Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.WakeUp();
            }

            hitboxScript.StartAttack(player);
        }
        else
        {
            Debug.LogError("주인공 오브젝트(태그: Player)를 찾을 수 없습니다.");
        }

        Invoke("ResetAttack", attackDuration);
    }

    void ResetAttack()
    {
        isAttacking = false;
    }
}