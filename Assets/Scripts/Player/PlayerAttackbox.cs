using UnityEngine;
using System.Collections.Generic;

public class PlayerAttackHitbox : MonoBehaviour
{
    // Health.cs의 Damage 함수가 float를 받으므로 float로 변경
    [Header("공격 설정")]
    public float attackDamage = 10.0f;

    [Header("디버깅")]
    [Tooltip("한 번의 공격에 이 태그를 가진 대상만 감지합니다.")]
    public string targetTag = "Enemy";

    // 한 번의 스윙(활성화)에서 중복 히트를 방지하기 위한 리스트
    private List<Collider2D> alreadyHit;

    void OnEnable()
    {
        // 히트박스가 활성화될 때 (공격 애니메이션이 시작될 때)
        // '이미 때린' 목록을 초기화합니다.
        if (alreadyHit == null)
        {
            alreadyHit = new List<Collider2D>();
        }
        alreadyHit.Clear();
    }

    // 이 히트박스(Trigger)에 누군가 들어왔을 때 호출됨
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 이미 이번 스윙에서 때린 대상이면 무시
        if (alreadyHit.Contains(other))
        {
            return;
        }

        // 2. 부딪힌 대상이 "Enemy" 태그를 가지고 있는지 확인
        if (other.CompareTag(targetTag))
        {
            // 3. 적의 'Health.cs' 스크립트를 찾습니다. (EnemyHealth.cs 아님!)
            Health enemyHealth = other.GetComponent<Health>();

            if (enemyHealth != null && !enemyHealth.isDead)
            {
                // 4. Health 스크립트의 'Damage' 함수를 호출합니다.
                enemyHealth.Damage(attackDamage);

                // 5. '이미 때린' 목록에 추가 (중복 히트 방지)
                alreadyHit.Add(other);

                Debug.Log($"플레이어 공격 적중! -> {other.name}에게 {attackDamage} 데미지");
            }
            else if (enemyHealth == null)
            {
                Debug.LogError(other.name + "에는 Health 스크립트가 없습니다.");
            }
        }
    }
}