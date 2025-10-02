using System.Collections;
using UnityEngine;

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

    [Header("패링 및 피격 설정")]
    public float parryMotionDuration = 0.3f; // 방패 드는 모션 길이
    public float parryGoodDuration = 0.8f;   // 패링 성공 모션 길이
    public float dieDuration = 2.0f;         // 사망 애니메이션 길이
    public float parryRecoveryTime = 0.1f;
    public float hitDuration = 0.5f;

    [Header("Hitbox 타이밍 (Attack 1 예시)")]
    public float hitbox1Start = 0.1f;
    public float hitbox1Duration = 0.1f;

    // === 내부 상태 변수 ===
    private bool isAttacking = false;
    private bool isParrying = false;
    private bool isHit = false;
    private int comboStep = 0;
    private Coroutine currentActionCoroutine;
    private float lastAttackTime = 0f;
    private bool shouldAdvanceCombo = false;
    private Coroutine parryTimerCoroutine;
    private bool canParryInput = false;

    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    void Update()
    {
        // 콤보 타이머 체크
        if (comboStep > 0 && Time.time > lastAttackTime + comboDelay)
        {
            ResetCombo();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            // 1. 패링 입력 체크 (Down + X)
            if (Input.GetKey(KeyCode.DownArrow))
            {
                if (isHit && canParryInput)
                {
                    // 피격 중이고 윈도우가 열렸을 때는 ParryWindowTimer가 판정하므로 return
                    return;
                }

                // 일반 패링 시도: 공격/피격/패링 중이 아닐 때만 방패 모션 발동
                if (!isAttacking && !isParrying && !isHit)
                {
                    StartParryMotion();
                    return;
                }
            }

            // 2. 일반 공격 (콤보) 체크
            if (isParrying || isHit) return;

            if (!isAttacking)
                StartCombo();
            else if (Time.time < lastAttackTime + comboDelay)
                TryNextCombo();
        }
    }

    // =========================================================================
    // 피격/패링 로직 (적 Hitbox 충돌 시 호출됨)
    // =========================================================================

    public void OnHitReceived(float parryWindow)
    {
        if (isHit) return;

        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);
        isAttacking = false;
        ResetCombo();

        isHit = true;
        isParrying = true;
        canParryInput = true;

        anim.SetTrigger("Parry");

        if (parryTimerCoroutine != null) StopCoroutine(parryTimerCoroutine);
        parryTimerCoroutine = StartCoroutine(ParryWindowTimer(parryWindow));
    }

    IEnumerator ParryWindowTimer(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.X))
            {
                SuccessParry();
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 실패: 타이머 끝
        FailParry();
    }

    void SuccessParry()
    {
        canParryInput = false;
        anim.SetTrigger("ParryGood");
        currentActionCoroutine = StartCoroutine(WaitAndReset(parryGoodDuration + parryRecoveryTime));
    }

    void FailParry()
    {
        canParryInput = false;
        anim.SetTrigger("Hit");

        currentActionCoroutine = StartCoroutine(WaitAndReset(dieDuration));
    }

    void StartParryMotion()
    {
        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);
        isParrying = true;
        isAttacking = false;

        anim.SetTrigger("Parry");
        currentActionCoroutine = StartCoroutine(WaitAndReset(parryMotionDuration + parryRecoveryTime));
    }

    IEnumerator WaitAndReset(float duration)
    {
        yield return new WaitForSeconds(duration);

        isHit = false;
        isParrying = false;
        currentActionCoroutine = null;
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
        if (currentActionCoroutine != null) StopCoroutine(currentActionCoroutine);

        isAttacking = false;
        comboStep = 0;
        lastAttackTime = 0f;

        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    IEnumerator AttackRoutine()
    {
        // --- 1단 공격 ---
        anim.SetTrigger("Attack1"); lastAttackTime = Time.time; shouldAdvanceCombo = false;
        yield return StartCoroutine(HitboxManagement(hitbox1Start, hitbox1Duration));
        float remainingTime1 = attack1Duration - hitbox1Start - hitbox1Duration;
        yield return StartCoroutine(WaitForComboInput(remainingTime1 > 0 ? remainingTime1 : 0f));
        if (comboStep == 1) goto ComboEnd;

        // --- 2단 공격 ---
        if (comboStep == 2)
        {
            anim.SetTrigger("Attack2"); lastAttackTime = Time.time; shouldAdvanceCombo = false;
            float remainingTime2 = attack2Duration;
            yield return StartCoroutine(WaitForComboInput(remainingTime2));
            if (comboStep == 2) goto ComboEnd;
        }

        // --- 3단 공격 ---
        if (comboStep == 3)
        {
            anim.SetTrigger("Attack3"); lastAttackTime = Time.time; shouldAdvanceCombo = false;
            float waitDuration = attack3Duration;
            float transitionBuffer = 0.05f;
            float firstWait = waitDuration - transitionBuffer;

            if (firstWait > 0) yield return new WaitForSeconds(firstWait);

            isAttacking = false;
            if (transitionBuffer > 0) yield return new WaitForSeconds(transitionBuffer);
        }

    ComboEnd:
        ResetCombo();
        currentActionCoroutine = null;
    }

    IEnumerator HitboxManagement(float startTime, float duration)
    {
        yield return new WaitForSeconds(startTime);
        if (attackHitbox != null) attackHitbox.SetActive(true);
        yield return new WaitForSeconds(duration);
        if (attackHitbox != null) attackHitbox.SetActive(false);
    }

    IEnumerator WaitForComboInput(float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            if (shouldAdvanceCombo)
            {
                comboStep++;
                shouldAdvanceCombo = false;
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }
}