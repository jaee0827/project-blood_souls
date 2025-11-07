using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class Health : MonoBehaviour
{

    const float DefaultHealth = 100.0f; // 체력 기본값

    [Header("Stat")] // 수치 설정
    public float maxHealth = DefaultHealth;
    public float currentHealth = DefaultHealth;
    public float drainDamage = 0.0f;

    [Header("Animator")] // 애니메이터 설정
    public Animator animator;

    [Header("Event")] // 이벤트 설정
    public UnityEvent<float, float> OnHealthChange;

    [HideInInspector]
    public bool isDead; // 사망 상태 변수

    // --- [추가] 회피에 필요한 무적 변수 ---
    [HideInInspector]
    public bool isInvincible;
    // ----------------------------------


    void Start()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth); // 잘못된 현재 체력 정정

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        isDead = false;
        isInvincible = false;
    }

    // 초기화 함수
    void Reset() => currentHealth = maxHealth;

    // (사용자님이 추가하신 Update 함수)
    void FixedUpdate()
    {
        if (drainDamage > 0.0f && !isDead)
        {
            // (참고: isInvincible이 true면 Damage 함수가 알아서 0을 반환합니다)
            Damage(drainDamage * Time.fixedDeltaTime, true);
        }
    }

    // (사용자님이 수정한 ChangeHealth 함수)
    public float ChangeHealth(float amount, bool isDrain)
    {
        // --- [추가] 무적 상태이거나 죽었으면 데미지 무시 ---
        // (amount < 0 : 데미지를 입는 경우)
        if (isDead || (isInvincible && amount < 0))
        {
            return 0;
        }
        // ---------------------------------------------

        /* (기존 isDead 체크는 위 코드로 통합됨)
        if (isDead) {
            return 0;
        }
        */

        float preHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0.0f, maxHealth); // 체력 계산
        OnHealthChange?.Invoke(currentHealth, maxHealth); // 이벤트 호출 (UI 변화 전용)

        if (currentHealth <= 0.0f)
        {
            Die(); // 체력 0 이하면 사망 처리

        }
        else if (!isDrain && amount < 0 && animator != null)
        {
            // (오류 방지를 위해 HasParameter 체크를 추천합니다)
            animator.SetTrigger("Hit"); // 피격 애니메이션
        }

        if (!isDrain)
        {
            string prefix = currentHealth > preHealth ? "+" : "";
            Debug.Log($"{gameObject.name} 체력 변화: {preHealth} -> {currentHealth} ({prefix}{currentHealth - preHealth})"); // 로그 출력
        }

        return currentHealth - preHealth;
    }

    // (사용자님이 수정한 Damage 함수)
    public float Damage(float amount, bool isDrain = false)
    {
        if (amount <= 0.0f)
        {
            return 0.0f;
        }

        return ChangeHealth(-amount, isDrain);
    }

    // (사용자님이 수정한 Heal 함수)
    public float Heal(float amount, bool isDrain = false)
    {
        if (amount <= 0.0f)
        {
            return 0.0f;
        }

        return ChangeHealth(amount, isDrain);
    }

    // 사망 함수
    public void Die()
    {
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("Death"); // 사망 애니메이션
        }

        Debug.Log($"{gameObject.name} 사망"); // 로그 출력
    }
}