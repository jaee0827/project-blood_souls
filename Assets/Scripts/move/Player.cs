using UnityEngine;

public class AdvancedPlayerMovement : MonoBehaviour
{
    // 이동 설정
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    // 점프 설정
    [Header("Jump Settings")]
    public float jumpForce = 8f;                 // 점프 힘 (Impulse 기준)
    public LayerMask groundMask;                 // 바닥 레이어
    public Vector2 groundCheckOffset = new(0f, -0.5f); // 바닥 체크 위치 오프셋
    public float groundCheckRadius = 0.2f;       // 바닥 체크 반경

    // 조작감 향상 기능
    [Header("Feel Improvements")]
    [Tooltip("땅에서 떨어진 후 점프가 가능한 시간 (관대함)")]
    public float coyoteTime = 0.15f;
    [Tooltip("점프 버튼을 미리 눌러도 인정되는 시간 (준비성)")]
    public float jumpBufferTime = 0.1f;

    // 내부 상태 변수
    private Rigidbody2D rigid;
    private Vector2 inputVec;
    private bool isGrounded;

    // 시간 측정 타이머
    private float coyoteTimer;   // 코요테 타임 타이머
    private float jumpBufferTimer; // 점프 버퍼 타이머

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. 입력 처리 (WASD 및 스페이스바)
        inputVec.x = Input.GetAxisRaw("Horizontal");
        // inputVec.y는 필요 없으므로 사용하지 않음

        // 2. 점프 입력 버퍼 처리
        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferTimer = jumpBufferTime; // 점프 입력을 기록
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime; // 시간 감소
        }
    }

    private void FixedUpdate()
    {
        // 1. 바닥 체크 및 코요테 타임 처리
        CheckGround();

        // 2. 점프 실행 로직
        // 점프가 가능한 조건: (점프 버퍼가 남아있음) AND (코요테 타임이 남아있음)
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            PerformJump();
        }

        // 3. 수평 이동
        MoveHorizontal();
    }

    // 바닥 체크 및 코요테 타임 업데이트 함수
    private void CheckGround()
    {
        Vector2 checkPos = rigid.position + groundCheckOffset;
        isGrounded = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundMask) != null;

        if (isGrounded)
        {
            coyoteTimer = coyoteTime; // 땅에 닿으면 코요테 타임 초기화
        }
        else
        {
            coyoteTimer -= Time.fixedDeltaTime; // 공중에선 시간 감소
        }
    }

    // 점프 실행 함수
    private void PerformJump()
    {
        // 수직 속도 초기화 후 점프 (점프 높이 고정)
        rigid.linearVelocity = new Vector2(rigid.linearVelocity.x, 0f);
        rigid.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // 점프가 실행되었으므로 두 타이머 모두 초기화
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
    }

    // 수평 이동 함수
    private void MoveHorizontal()
    {
        // WASD (수평) 이동 (MovePosition 대신 Velocity를 사용하여 벽타기 방지 및 물리 엔진 활용)
        Vector2 currentVelocity = rigid.linearVelocity;

        // 이동할 목표 속도 계산 (수직 속도는 유지)
        float targetVelocityX = inputVec.x * moveSpeed;

        // X축 속도만 변경
        rigid.linearVelocity = new Vector2(targetVelocityX, currentVelocity.y);
    }

    // 에디터에서 바닥 체크 Gizmo 보기 (디버깅)
    void OnDrawGizmosSelected()
    {
        // Gizmo가 제대로 작동하려면 이 스크립트가 Rigidbody2D가 있는 오브젝트에 붙어있어야 합니다.
        if (rigid == null && Application.isPlaying) return;

        Vector2 center = Application.isPlaying
            ? (Vector2)rigid.position + groundCheckOffset
            : (Vector2)transform.position + groundCheckOffset;

        // 플레이 중일 때는 접지 여부에 따라 색상 변경
        if (Application.isPlaying)
            Gizmos.color = coyoteTimer > 0f ? Color.green : Color.red; // 코요테 타임 기준으로 색상 표시
        else
            Gizmos.color = Color.yellow; // 에디터에서는 노란색

        Gizmos.DrawWireSphere(center, groundCheckRadius);
    }
}