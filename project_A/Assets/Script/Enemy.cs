using System;
using UnityEngine;
using TMPro;
using static SoundManager;
using static UnityEngine.EventSystems.EventTrigger;

public class Enemy : MonoBehaviour
{
    // -------------------------------------------------
    // 1) ���� �߰��� �ʵ�: �� Enemy�� � ���� Ÿ������
    // -------------------------------------------------
    public MonsterType monsterType;

    private Rigidbody rgbd;
    private Animator anim;
    public GameObject scoreUpText;

    // Enemy�� Ǯ�� ���ư� �� ȣ��Ǵ� �̺�Ʈ
    public Action OnDespawn;

    private void Awake()
    {
        rgbd = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        // Animation �ӵ� ����ȭ
        float randSpd = UnityEngine.Random.Range(0.7f, 1f);
        anim.SetFloat("anim_spd", randSpd);
    }

    private void Update()
    {
        // ���ʹ� �� ������ ���� �ӵ��� ���� Z������ �̵�
        transform.position += new Vector3(0f, 0f, GameManager.instance.gameSpd) * Time.deltaTime;
    }

    public void HitEnemy()
    {
        // Ÿ�Ժ� ���� �� �߰� ó��
        int score = 0;
        SoundType sound = SoundType.None;
        switch (monsterType)
        {
            case MonsterType.Ghost:
                score = 20;
                sound = SoundType.Sword_Slash_Hit_Normal;
                break;
            case MonsterType.Skeleton:
                score = 30;
                sound = SoundType.Sword_Slash_Hit_Normal;
                break;
            case MonsterType.Bat:
                score = 15;
                sound = SoundType.Sword_Slash_Hit_Normal;
                break;
            case MonsterType.Slime:
                score = 5;
                sound = SoundType.Sword_Slash_Hit_Normal;
                break;
        }

        // ���� ���
        if (sound != SoundType.None)
            SoundManager.instance.Play_SoundEffect(sound);

        // ���� ����
        if (score > 0)
            GameManager.instance.PointUp(score);

        // ���� �ؽ�Ʈ ǥ��
        if (scoreUpText != null)
        {
            GameObject so = Instantiate(
                scoreUpText,
                transform.position + new Vector3(0f, 4f, 0f),
                Quaternion.Euler(50f, 0f, 0f)
            );
            var textComp = so.transform.GetChild(0)
                                  .GetChild(0)
                                  .GetComponent<TextMeshProUGUI>();
            if (textComp != null)
                textComp.text = score.ToString();
            Destroy(so, 1f);
        }

        // Ǯ ��ȯ
        Despawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        // �÷��̾� ����(������ �Ǵ� ��ų)�� �¾��� ��

        // �� ��(DeadZoneETC)�� ���� �״�� Ǯ�� ��ȯ
        if (other.CompareTag(ConstData.AttackSlashTag))
        {
            Despawn();
        }
        else if (other.CompareTag(ConstData.DeadZoneETCTag))
        {
            Despawn();
        }
    }

    /// <summary>
    /// Pool�� ���ͽ�Ű�� �޼���
    /// </summary>
    public void Despawn()
    {
        OnDespawn?.Invoke();
        OnDespawn = null;
    }
}
