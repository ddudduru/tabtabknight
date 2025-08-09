using System;
using UnityEngine;
using TMPro;
using static SoundManager;
using static UnityEngine.EventSystems.EventTrigger;

public class Enemy : MonoBehaviour
{
    // -------------------------------------------------
    // 1) 새로 추가된 필드: 이 Enemy가 어떤 몬스터 타입인지
    // -------------------------------------------------
    public MonsterType monsterType;

    private Rigidbody rgbd;
    private Animator anim;
    public GameObject scoreUpText;

    // Enemy가 풀로 돌아갈 때 호출되는 이벤트
    public Action OnDespawn;

    private void Awake()
    {
        rgbd = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        // Animation 속도 랜덤화
        float randSpd = UnityEngine.Random.Range(0.7f, 1f);
        anim.SetFloat("anim_spd", randSpd);
    }

    private void Update()
    {
        // 몬스터는 매 프레임 게임 속도에 맞춰 Z축으로 이동
        transform.position += new Vector3(0f, 0f, GameManager.instance.gameSpd) * Time.deltaTime;
    }

    public void HitEnemy()
    {
        // 타입별 점수 및 추가 처리
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

        // 사운드 재생
        if (sound != SoundType.None)
            SoundManager.instance.Play_SoundEffect(sound);

        // 점수 증가
        if (score > 0)
            GameManager.instance.PointUp(score);

        // 점수 텍스트 표시
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

        // 풀 반환
        Despawn();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어 공격(슬래시 또는 스킬)에 맞았을 때

        // 맵 끝(DeadZoneETC)에 들어가면 그대로 풀로 반환
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
    /// Pool로 복귀시키는 메서드
    /// </summary>
    public void Despawn()
    {
        OnDespawn?.Invoke();
        OnDespawn = null;
    }
}
