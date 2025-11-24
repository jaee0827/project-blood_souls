using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    // Health.cs�� Damage �Լ��� float�� �����Ƿ� float�� ����
    [Header("Stat")]
    public float attackDamage = 10.0f;

    [Header("Object")]
    public GameObject player;
    public GameObject boss;

    // �� ���� ����(Ȱ��ȭ)���� �ߺ� ��Ʈ�� �����ϱ� ���� ����Ʈ
    private List<Collider2D> alreadyHit;

    void OnEnable()
    {
        // ��Ʈ�ڽ��� Ȱ��ȭ�� �� (���� �ִϸ��̼��� ���۵� ��)
        // '�̹� ����' ����� �ʱ�ȭ�մϴ�.
         if (alreadyHit == null)
         {
             alreadyHit = new List<Collider2D>();
         }

         alreadyHit.Clear();
    }

    // �� ��Ʈ�ڽ�(Trigger)�� ������ ������ �� ȣ���
    private void OnTriggerEnter2D(Collider2D target)
    {
        // 1. �̹� �̹� �������� ���� ����̸� ����
         if (alreadyHit.Contains(target))
         {
             return;
         }

        // 2. �ε��� ����� "Boss" �±׸� ������ �ִ��� Ȯ��
        if (target.gameObject.Equals(boss))
        {
            // 3. ���� 'Health.cs' ��ũ��Ʈ�� ã���ϴ�. (EnemyHealth.cs �ƴ�!)
            Health health = target.GetComponent<Health>();
            if (health == null || health.isDead)
            {
                return;
            }

            float damage = -health.Damage(attackDamage);

            if (health.isDead)
            {
                boss.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, -2);
            }

            // 4. Health ��ũ��Ʈ�� 'Damage' �Լ��� ȣ���մϴ�.
            StartCoroutine(ExecuteLifeSteal(damage));
            Debug.Log($"{player.gameObject.name} 공격: {target.name} ({attackDamage})");

            // 5. '�̹� ����' ��Ͽ� �߰� (�ߺ� ��Ʈ ����)
            //alreadyHit.Add(target);
        }
    }
    
    private IEnumerator ExecuteLifeSteal(float damage)
    {
        yield return new WaitForSeconds(0.001f);
        
        if (damage >= 0) {
            LifeSteal lifeSteal = player.GetComponent<LifeSteal>();
            if (lifeSteal != null) {
                lifeSteal.ExecuteSteal(damage);
            }
        }
    }
}