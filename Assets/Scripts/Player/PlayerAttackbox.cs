using UnityEngine;
using System.Collections.Generic;

public class PlayerAttackHitbox : MonoBehaviour
{
    // Health.cs�� Damage �Լ��� float�� �����Ƿ� float�� ����
    [Header("���� ����")]
    public float attackDamage = 10.0f;

    [Header("�����")]
    [Tooltip("�� ���� ���ݿ� �� �±׸� ���� ��� �����մϴ�.")]
    public string targetTag = "Enemy";

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
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. �̹� �̹� �������� ���� ����̸� ����
        if (alreadyHit.Contains(other))
        {
            return;
        }

        // 2. �ε��� ����� "Enemy" �±׸� ������ �ִ��� Ȯ��
        if (other.CompareTag(targetTag))
        {
            // 3. ���� 'Health.cs' ��ũ��Ʈ�� ã���ϴ�. (EnemyHealth.cs �ƴ�!)
            Health enemyHealth = other.GetComponent<Health>();

            if (enemyHealth != null && !enemyHealth.isDead)
            {
                // 4. Health ��ũ��Ʈ�� 'Damage' �Լ��� ȣ���մϴ�.
                enemyHealth.Damage(attackDamage);

                // 5. '�̹� ����' ��Ͽ� �߰� (�ߺ� ��Ʈ ����)
                alreadyHit.Add(other);

                Debug.Log($"�÷��̾� ���� ����! -> {other.name}���� {attackDamage} ������");
            }
            else if (enemyHealth == null)
            {
                Debug.LogError(other.name + "���� Health ��ũ��Ʈ�� �����ϴ�.");
            }
        }
    }
}