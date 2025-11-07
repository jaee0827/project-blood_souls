using System.Collections;
using UnityEngine;

public class PlayerPlatformFall : MonoBehaviour // (스크립트 이름은 다를 수 있습니다)
{
    private bool isDropping = false; // 현재 내려가는 중인지 확인
    private int playerLayer;
    private int platformLayer;

    void Start()
    {
        // 레이어 이름을 숫자로 변환 (이래야 빠름)
        playerLayer = LayerMask.NameToLayer("Player");
        platformLayer = LayerMask.NameToLayer("Platform");
    }

    void Update()
    {
        // 아래 키(S키 또는 아래 화살표)를 누르고, 현재 내려가는 중이 아닐 때
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            if (!isDropping)
            {
                StartCoroutine(DropDown());
            }
        }
    }

    private IEnumerator DropDown()
    {
        // 1. 내려가는 상태로 변경
        isDropping = true;

        // 2. 플레이어와 플랫폼 레이어의 충돌을 "무시" (핵심!)
        Physics2D.IgnoreLayerCollision(playerLayer, platformLayer, true);

        // 3. 아주 잠깐 (0.3초) 기다림 (플랫폼을 통과할 시간)
        yield return new WaitForSeconds(0.3f);

        // 4. 충돌을 다시 "활성화"
        Physics2D.IgnoreLayerCollision(playerLayer, platformLayer, false);

        // 5. 내려가는 상태 해제
        isDropping = false;
    }
}