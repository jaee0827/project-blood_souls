using System.Collections;
using UnityEngine;

// 이 스크립트는 Rigidbody2D와 Health 컴포넌트가 필요합니다.
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public class PlayerComboAttack : MonoBehaviour
{
    // === 인스펙터 설정 변수 ===
    [Header("컴포넌트 연결")]
    public Animator anim;
    public GameObject attackHitbox;

    [Header("콤보 타이밍 설정")]
    public float comboDelay = 1.0f;
    public float attack1Duration = 0.5f;
    public float attack2Duration = 0.6f;
    public float attack3Duration = 0.8f;

    [Header("피격 및 사망 설정")]
    public float hitDuration = 0.5f;      // 피격 애니메이션 길이
    public float dieDuration = 2.0f;        // 사망 애니메이션 길이

    [Header("구르기 설정")]
    public float dodgeDuration = 0.4f;      // 구르기 애니메이션 총 시간
    public float dodgeSpeed = 12f;          // 구르기 이동 속도
    public float dodgeCooldown = 0.5f;      // 구르기 후 다음 구르기까지 딜레이
    public KeyCode dodgeKey = KeyCode.LeftControl; // 구르기 키

    [Header("Hitbox 타이밍 (Attack 1 예시)")]
    public float hitbox1Start = 0.1f;
    public float hitbox1Duration = 0.1f;
    // (참고: 2, 3타 히트박스 타이밍은 AttackRoutine 안에 하드코딩 되어있습니다)

    // === 내부 상태 변수 ===
    private Rigidbody2D rb;
    private Health playerHealth;            // Health.cs 스크립트
    private bool isAttacking = false;
    private bool isHit = false;             // 피격 상태
    private bool isRolling = false;         // 구르기 상태
    private int comboStep = 0;
    private Coroutine currentActionCoroutine; // 현재 행동(공격, 피격, 구르기)을 제어
    private float lastAttackTime = 0f;
    private bool shouldAdvanceCombo = false;
    private float lastDodgeTime = -1f;      // 마지막 구르기 시간
    private bool isFacingRight = true;      // 캐릭터가 바라보는 방향

    void Start()
    {
        // 컴포넌트 자동 할당
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<Health>(); // Health.cs

        if (attackHitbox != null)
            attackHitbox.SetActive(false);
        else
            Debug.LogError("attackHitbox가 연결되지 않았습니다!", this.gameObject);
    }

    void Update()
    {
        // 콤보 타이머 체크 (공격 중일 때만)
        if (isAttacking && comboStep > 0 && Time.time > lastAttackTime + comboDelay)
        {
            ResetCombo();
        }

        // 캐릭터 방향 체크 (구르기 방향 결정을 위해)
        // (참고: 이 부분은 실제 캐릭터 이동 스크립트에서 관리하는 것이 더 좋습니다)
        if (transform.localScale.x > 0) isFacingRight = true;
        if (transform.localScale.x < 0) isFacingRight = false;

        // === 1. 구르기 입력 ===
        // (피격/구르기/공격 중이 아니고, 쿨타임이 지났을 때)
        if (Input.GetKeyDown(dodgeKey) && !isRolling && !isAttacking && !isHit && Time.time >= lastDodgeTime + dodgeCooldown)
        {
            StartDodge();
            return; // 구르기 입력이 들어오면, 아래의 공격 처리는 무시
        }

        // === 2. 공격 입력 ===
        if (Input.GetKeyDown(KeyCode.X))
        {
            // 구르기 중이거나 피격 중이면 공격 불가
            if (isRolling || isHit) return;

            if (!isAttacking)
                StartCombo();
            else // 콤보 입력 시간(comboDelay) 안에 눌렀다면
                TryNextCombo();
        }
    }

    // =========================================================================
    // 피격 로직 (적이 플레이어를 때렸을 때 호출됨)
    // =========================================================================

    /// <summary>
    /// 적의 히트박스에서 이 함수를 호출해야 합니다. (데미지만 받음)
    /// Health.cs 스크립트가 데미지 처리를 하도록 수정해야 합니다.
    /// 이 함수는 "피격 상태"로 만드는 역할만 합니다.
    /// </summary>
    public void OnHitReceived()
    {
        // 구르기(무적) 중이거나, 이미 피격 상태거나, 죽었으면 무시
        if (isRolling || isHit || (playerHealth != null && playerHealth.isDead))
        {
            return;
        }

        // 1. 현재 행동(공격, 구르기 등) 중지
        StopCurrentAction();

        isHit = true; // 피격 상태로 전환

        // 2. Health.cs 스크립트가 "Hit" 애니메이션을 처리할 것입니다.
        //    (Health.cs의 Damage 함수가 호출되었다고 가정합니다)

        // 3. 피격 모션 시간(hitDuration)만큼 대기 후 상태 초기화
        // (죽지 않았을 때만 피격 상태에서 풀려남)
        if (playerHealth != null && !playerHealth.isDead)
        {
            currentActionCoroutine = StartCoroutine(WaitAndReset(hitDuration, "HitEnd"));
        }
        else if (playerHealth != null && playerHealth.isDead)
        {
            // 사망 시, dieDuration 후에 상태 리셋 (필요시)
            currentActionCoroutine = StartCoroutine(WaitAndReset(dieDuration, null));
        }
    }

    /// <summary>
    /// 일정 시간(duration)이 지난 후 상태를 초기화하는 범용 코루틴
    /// </summary>
    IEnumerator WaitAndReset(float duration, string endTrigger)
    {
        yield return new WaitForSeconds(duration);

        if (!string.IsNullOrEmpty(endTrigger) && anim != null)
        {
            // anim.SetTrigger(endTrigger); // 피격/구르기 끝났다는 트리거 (선택사항)
        }

        isHit = false;
        isRolling = false;
        currentActionCoroutine = null;
    }

    /// <summary>
    /// 현재 진행 중인 모든 행동(공격, 구르기, 피격대기)을 중지시킵니다.
    /// </summary>
    void StopCurrentAction()
    {
        if (currentActionCoroutine != null)
        {
            StopCoroutine(currentActionCoroutine);
            currentActionCoroutine = null;
        }

        // 하던 행동들의 상태를 강제로 초기화
        isAttacking = false;
        isRolling = false;
        comboStep = 0;

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        if (playerHealth != null)
            playerHealth.isInvincible = false; // 무적 상태 강제 해제

        // (중요) 물리 속도를 0으로 되돌려야 할 수 있음
        // rb.velocity = new Vector2(0, rb.velocity.y);
    }


    // =========================================================================
    // 구르기 (회피) 로직
    // =========================================================================

    void StartDodge()
    {
        // 다른 행동(공격 등) 코루틴이 실행 중이면 중지
        StopCurrentAction();

        isRolling = true;
        lastDodgeTime = Time.time;

        anim.SetTrigger("Dodge"); // "Dodge"라는 이름의 애니메이션 트리거 실행

        // 구르기 코루틴 시작
        currentActionCoroutine = StartCoroutine(DodgeRoutine());
    }

    IEnumerator DodgeRoutine()
    {
        // --- 1. 무적 상태(I-frames) 시작 ---
        if (playerHealth != null)
        {
            playerHealth.isInvincible = true; // Health.cs에 isInvincible 변수 필요!
        }

        // --- 2. 이동 ---
        float rollDirection = isFacingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(rollDirection * dodgeSpeed, rb.linearVelocity.y);

        // --- 3. 구르기 지속 시간 (애니메이션 8장 길이) ---
        yield return new WaitForSeconds(dodgeDuration);

        // --- 4. 구르기 종료 ---
        // (주의: 구르기 직후 바로 멈추게 할지, 관성을 줄지는 기획에 따름)
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // 구르기 후 속도 초기화
        isRolling = false;

        // 무적 상태(I-frames) 종료
        if (playerHealth != null)
        {
            playerHealth.isInvincible = false;
        }

        currentActionCoroutine = null; // 현재 행동 종료
    }


    // =========================================================================
    // 공격 로직 (콤보)
    // =========================================================================

    void StartCombo()
    {
        isAttacking = true;
        comboStep = 1;

        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);
        currentActionCoroutine = StartCoroutine(AttackRoutine());
    }

    void TryNextCombo()
    {
        if (comboStep < 3)
        {
            shouldAdvanceCombo = true;
        }
    }

    void ResetCombo()
    {
        // 콤보가 중간에 끊기거나(시간 초과), 피격당했을 때 호출됨
        isAttacking = false;
        comboStep = 0;
        lastAttackTime = 0f;
        shouldAdvanceCombo = false;

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        currentActionCoroutine = null;
    }

    IEnumerator AttackRoutine()
    {
        // --- 1단 공격 ---
        comboStep = 1; // 1단 공격 시작
        anim.SetTrigger("Attack1"); lastAttackTime = Time.time; shouldAdvanceCombo = false;
        yield return StartCoroutine(HitboxManagement(hitbox1Start, hitbox1Duration));

        float remainingTime1 = attack1Duration - (hitbox1Start + hitbox1Duration);
        yield return StartCoroutine(WaitForComboInput(remainingTime1 > 0 ? remainingTime1 : 0f));

        if (!shouldAdvanceCombo) goto ComboEnd; // 콤보 입력 없으면 종료

        // --- 2단 공격 ---
        comboStep = 2; // 2단 공격 시작
        anim.SetTrigger("Attack2"); lastAttackTime = Time.time; shouldAdvanceCombo = false;

        // (2단 공격 히트박스 타이밍 - 임시 값, 인스펙터로 빼는 것 추천)
        yield return StartCoroutine(HitboxManagement(0.1f, 0.2f));

        float remainingTime2 = attack2Duration - (0.1f + 0.2f);
        yield return StartCoroutine(WaitForComboInput(remainingTime2 > 0 ? remainingTime2 : 0f));

        if (!shouldAdvanceCombo) goto ComboEnd; // 콤보 입력 없으면 종료

        // --- 3단 공격 ---
        comboStep = 3; // 3단 공격 시작
        anim.SetTrigger("Attack3"); lastAttackTime = Time.time; shouldAdvanceCombo = false;

        // (3단 공격 히트박스 타이밍 - 임시 값)
        yield return StartCoroutine(HitboxManagement(0.1f, 0.3f));

        float waitDuration = attack3Duration - (0.1f + 0.3f);
        if (waitDuration > 0) yield return new WaitForSeconds(waitDuration);

        ComboEnd:
        // 콤보가 자연스럽게 끝났을 때
        ResetCombo();
    }

    /// <summary>
    /// 다음 콤보 입력을 기다립니다.
    /// </summary>
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


    /// <summary>
    /// 히트박스를 정해진 시간 후에 켜고, 지속 시간이 지나면 끕니다.
    /// </summary>
    IEnumerator HitboxManagement(float startTime, float duration)
    {
        // startTime 만큼 대기 (애니메이션 선딜레이)
        yield return new WaitForSeconds(startTime);

        // 히트박스 활성화
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(true);
        }

        // duration 만큼 대기 (히트박스 판정 시간)
        yield return new WaitForSeconds(duration);

        // 히트박스 비활성화
        if (attackHitbox != null)
        {
            attackHitbox.SetActive(false);
        }
    }
}