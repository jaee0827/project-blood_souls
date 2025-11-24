using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerComboAttack : MonoBehaviour
{
    // === 플레이어의 현재 상태 ===
    // 플레이어는 항상 이 중 하나의 상태에만 있게 됩니다.
    private enum PlayerState
    {
        Idle,       // 기본 상태 (공격, 구르기, 패링 가능)
        Attacking,  // 공격 중 (다른 행동 불가)
        Rolling,    // 구르기 중 (무적, 다른 행동 불가)
        Parrying,   // 패링 시도 중 (다른 행동 불가)
        HitStun,    // 피격 당함 (패링 성공 시도 가능)
        Dead        // 사망
    }
    private PlayerState currentState = PlayerState.Idle;

    // === 인스펙터 설정 변수 ===
    [Header("컴포넌트 연결")]
    public Animator anim;
    public GameObject attackHitbox;

    [Header("콤보 타이밍 설정")]
    public float comboDelay = 1.0f;
    public float attack1Duration = 0.5f;
    public float attack2Duration = 0.6f;
    public float attack3Duration = 0.8f;
    [Header("Hitbox 타이밍 (Attack 1 예시)")]
    public float hitbox1Start = 0.1f;
    public float hitbox1Duration = 0.1f;

    [Header("패링 및 피격 설정")]
    public float parryMotionDuration = 0.3f; // 방패 드는 모션 길이
    public float parryGoodDuration = 0.8f;   // 패링 성공 모션 길이
    public float parryRecoveryTime = 0.1f;
    public float hitDuration = 0.5f;

    [Header("구르기 설정")]
    public float dodgeDuration = 0.4f;
    public float dodgeSpeed = 12f;
    public float dodgeCooldown = 1.0f;
    [Tooltip("구르기 시 적용할 콜라이더 크기")]
    public Vector2 rollingColliderSize = new Vector2(1, 0.5f);
    [Tooltip("구르기 시 적용할 콜라이더 위치")]
    public Vector2 rollingColliderOffset = new Vector2(0, 0);
    
    [Header("키 설정")]
    public KeyCode attackKey = KeyCode.Mouse0;
    public KeyCode dodgeKey = KeyCode.LeftShift;

    // === 내부 시스템 변수 ===
    private Rigidbody2D rb;
    private Health playerHealth;
    private BoxCollider2D playerCollider;
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private bool isFacingRight = true;
    private float lastDodgeTime = -1f;

    // 콤보 관련
    private int comboStep = 0;
    private float lastAttackTime = 0f;
    private bool shouldAdvanceCombo = false;

    // 패링/피격 관련
    private float receivedDamage = 0.0f;
    private Coroutine parryTimerCoroutine;


    void Start()
    {
        // 컴포넌트 자동 할당
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<Health>();
        playerCollider = GetComponent<BoxCollider2D>();

        // 원본 콜라이더 값 저장
        if (playerCollider != null)
        {
            originalColliderSize = playerCollider.size;
            originalColliderOffset = playerCollider.offset;
        }

        if (attackHitbox != null)
            attackHitbox.SetActive(false);
    }

    void Update()
    {
        if (playerHealth.isDead)
        {
            currentState = PlayerState.Dead;
            return;
        }
        
        // 플레이어가 죽었으면 아무것도 안 함
        if (currentState == PlayerState.Dead)
        {
            return;
        }

        // 캐릭터 방향 체크
        if (transform.localScale.x > 0) isFacingRight = true;
        if (transform.localScale.x < 0) isFacingRight = false;

        // 콤보 타이머 (공격 중일 때만 체크)
        if (currentState == PlayerState.Attacking && comboStep > 0 && Time.time > lastAttackTime + comboDelay)
        {
            ResetCombo();
        }

        // --- 상태별 입력 처리 ---
        // 'switch'를 사용해 현재 상태에 따라 가능한 행동만 처리
        switch (currentState)
        {
            case PlayerState.Idle:
                // '기본' 상태일 때만 입력을 받음
                HandleIdleInput();
                break;

            case PlayerState.Attacking:
                // '공격 중'일 때는 다음 콤보 입력만 받음
                HandleAttackingInput();
                break;

            case PlayerState.HitStun:
                // '피격' 상태일 때는 패링 성공 입력만 받음
                HandleHitStunInput();
                break;

            // '구르기 중'이나 '패링 중'일 때는 아무 입력도 받지 않음
            case PlayerState.Rolling:
            case PlayerState.Parrying:
                break;
        }
    }

    // =========================================================================
    // 입력 처리 함수들 (Update에서 호출)
    // =========================================================================

    void HandleIdleInput()
    {
        // 1. 구르기 입력 (최우선)
        if (Input.GetKeyDown(dodgeKey) && Time.time >= lastDodgeTime + dodgeCooldown)
        {
            StartCoroutine(DodgeRoutine());
            return; // 구르기 시작
        }

        // 2. 공격 / 패링 입력 (N키)
        if (Input.GetKeyDown(attackKey))
        {
            StartCoroutine(AttackRoutine());
            // 2-1. 패링 입력 (M + N)
            // if (Input.GetKey(KeyCode.M))
            // {
            //     StartCoroutine(ParryMotionRoutine());
            // }
            // 2-2. 일반 공격 (N)
            //else
            //{
            //}
        }
    }

    void HandleAttackingInput()
    {
        // 콤보 다음타 입력 (N키)
        if (Input.GetKeyDown(attackKey) && comboStep < 3)
        {
            shouldAdvanceCombo = true;
        }
    }

    void HandleHitStunInput()
    {
        // 피격 중 패링 성공 입력 (M + N)
        if (Input.GetKey(KeyCode.M) && Input.GetKey(attackKey))
        {
            // 패링 타이머 코루틴이 돌고 있을 때만 성공
            if (parryTimerCoroutine != null)
            {
                StopCoroutine(parryTimerCoroutine);
                parryTimerCoroutine = null;
                StartCoroutine(SuccessParryRoutine());
            }
        }
    }

    // =========================================================================
    // 피격/패링 로직
    // =========================================================================

    public bool OnHitReceived(float parryWindow, float damage)
    {
        // 구르기(무적) 중이거나, 이미 피격/사망 상태면 무시
        if (currentState == PlayerState.Rolling || currentState == PlayerState.HitStun || currentState == PlayerState.Dead)
        {
            return false;
        }

        // 다른 모든 행동(공격, 패링 시도)을 즉시 중지
        StopAllCoroutines(); // (중요) 모든 코루틴을 중지시켜 상태를 강제 리셋
        ResetActionState();  // 콜라이더, 무적 등 상태 초기화

        currentState = PlayerState.HitStun; // '피격' 상태로 전환
        receivedDamage = damage;
        
        StartCoroutine(FailParryRoutine());
        // anim.SetTrigger("Parry"); // (피격 시 패링 시도 모션)

        // 패링 성공/실패를 결정하는 타이머 시작
        // parryTimerCoroutine = StartCoroutine(ParryWindowTimer(parryWindow));
        return true;
    }

    IEnumerator ParryWindowTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        // 타이머가 끝날 때까지 HandleHitStunInput()에서 성공 못 시켰으면 '실패'
        parryTimerCoroutine = null;
        StartCoroutine(FailParryRoutine());
    }

    IEnumerator SuccessParryRoutine()
    {
        currentState = PlayerState.Parrying; // '패링 성공' 상태 (잠시 무적)
        anim.SetTrigger("ParryGood");

        yield return new WaitForSeconds(parryGoodDuration + parryRecoveryTime);

        currentState = PlayerState.Idle; // '기본' 상태로 복귀
    }

    IEnumerator FailParryRoutine()
    {
        // (currentState는 여전히 HitStun)
        if (playerHealth != null && !playerHealth.isDead)
        {
            playerHealth.Damage(receivedDamage); // 플레이어 피해
            
            yield return new WaitForSeconds(hitDuration);
            
            if (currentState != PlayerState.Dead) // 그 사이에 죽지 않았다면
            {
                currentState = PlayerState.Idle; // '기본' 상태로 복귀
            }
        }
    }

    IEnumerator ParryMotionRoutine()
    {
        currentState = PlayerState.Parrying; // '패링 시도' 상태
        anim.SetTrigger("Parry");

        yield return new WaitForSeconds(parryMotionDuration + parryRecoveryTime);

        currentState = PlayerState.Idle; // '기본' 상태로 복귀
    }

    // =========================================================================
    // 구르기 (회피) 로직
    // =========================================================================

    IEnumerator DodgeRoutine()
    {
        currentState = PlayerState.Rolling; // '구르기' 상태
        lastDodgeTime = Time.time;
        anim.SetTrigger("Dodge");

        playerHealth.isInvincible = true; // 무적 시작
        playerCollider.size = rollingColliderSize;
        playerCollider.offset = rollingColliderOffset;

        float rollDirection = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(rollDirection * dodgeSpeed, rb.linearVelocity.y);

        yield return new WaitForSeconds(dodgeDuration);

        // (중요) 구르기가 끝났을 때만 상태 복귀 (피격 등으로 중단될 수 있음)
        if (currentState == PlayerState.Rolling)
        {
            ResetActionState(); // 콜라이더, 무적, 속도 초기화
            currentState = PlayerState.Idle; // '기본' 상태로 복귀
        }
    }

    // =========================================================================
    // 공격 로직 (콤보)
    // =========================================================================

    IEnumerator AttackRoutine()
    {
        currentState = PlayerState.Attacking; // '공격' 상태
        comboStep = 1;

        // --- 1단 공격 ---
        anim.SetTrigger("Attack1"); lastAttackTime = Time.time; shouldAdvanceCombo = false;
        yield return StartCoroutine(HitboxManagement(hitbox1Start, hitbox1Duration));
        float remainingTime1 = attack1Duration - hitbox1Start - hitbox1Duration;
        yield return StartCoroutine(WaitForComboInput(remainingTime1 > 0 ? remainingTime1 : 0f));

        if (!shouldAdvanceCombo) goto ComboEnd; // 콤보 입력 없으면 종료

        // --- 2단 공격 ---
        comboStep = 2;
        anim.SetTrigger("Attack2"); lastAttackTime = Time.time; shouldAdvanceCombo = false;
        yield return StartCoroutine(HitboxManagement(0.1f, 0.2f)); // (임시 값)
        float remainingTime2 = attack2Duration - 0.3f;
        yield return StartCoroutine(WaitForComboInput(remainingTime2 > 0 ? remainingTime2 : 0f));

        if (!shouldAdvanceCombo) goto ComboEnd;

        // --- 3단 공격 ---
        comboStep = 3;
        anim.SetTrigger("Attack3"); lastAttackTime = Time.time; shouldAdvanceCombo = false;
        yield return StartCoroutine(HitboxManagement(0.1f, 0.3f)); // (임시 값)
        float waitDuration = attack3Duration - 0.4f;
        if (waitDuration > 0) yield return new WaitForSeconds(waitDuration);

        ComboEnd:
        // 콤보가 자연스럽게 끝났을 때
        if (currentState == PlayerState.Attacking)
        {
            currentState = PlayerState.Idle; // '기본' 상태로 복귀
        }
        ResetCombo();
    }

    void ResetCombo()
    {
        comboStep = 0;
        lastAttackTime = 0f;
        shouldAdvanceCombo = false;
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    IEnumerator WaitForComboInput(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (shouldAdvanceCombo)
            {
                yield break; // 다음 콤보로 진입
            }
            timer += Time.deltaTime;
            yield return null;
        }
        // 콤보 입력 시간 초과 (shouldAdvanceCombo = false)
    }

    IEnumerator HitboxManagement(float startTime, float duration)
    {
        yield return new WaitForSeconds(startTime);
        if (attackHitbox != null) attackHitbox.SetActive(true);
        yield return new WaitForSeconds(duration);
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    // =========================================================================
    // 상태 초기화 헬퍼 함수
    // =========================================================================

    /// <summary>
    /// 피격, 구르기, 공격 등으로 꼬인 상태를 '기본'으로 되돌립니다.
    /// </summary>
    void ResetActionState()
    {
        // 물리 상태 초기화
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // 콜라이더 초기화
        playerCollider.size = originalColliderSize;
        playerCollider.offset = originalColliderOffset;

        // 무적/히트박스 초기화
        playerHealth.isInvincible = false;
        if (attackHitbox != null) attackHitbox.SetActive(false);

        // 콤보 초기화
        ResetCombo();

        // (currentState는 이 함수를 부른 곳에서 설정)
    }
}