using System;
using UnityEngine;
using TMPro;
using static SoundManager;

public class Obstacls_Control : MonoBehaviour
{
    public enum Type { Tree, Rock, Log }
    public Type type;

    [Header("풀링에서 복귀할 때 호출할 콜백")]
    public Action OnDespawn;

    [Header("점수 표시 프리팹(자식으로 생성 후 1초 뒤 파괴)")]
    public GameObject scoreUpText;

    [Header("Log 타입일 때 추가로 더 빠르게 이동시킬 속도(맵 속도의 몇 배)")]
    public float logExtraSpeedMultiplier = 1.2f;
    // 예: 1.2f면, 기본 gameSpd * 1.2 속도로 이동

    /// <summary>
    /// 맵 파트 내부에서 로컬 X 위치와 회전을 랜덤 설정해 줍니다.
    /// </summary>
    public void Setting_Random()
    {
        transform.localPosition = new Vector3(
            UnityEngine.Random.Range(-40f, -10f),
            transform.localPosition.y,
            transform.localPosition.z
        );
        transform.localRotation = Quaternion.Euler(
            0f,
            UnityEngine.Random.Range(0f, 360f),
            0f
        );
    }

    private void Update()
    {
        // 기본적으로 장애물은 MapController가 자식 오브젝트를 
        // Translate(Vector3.forward * gameSpd) 해 주면 따라 움직입니다.
        // 단, Log 타입만 추가 속도를 더해 줍니다.
        if (type == Type.Log)
        {
            float extraSpeed = GameManager.instance.gameSpd * (logExtraSpeedMultiplier - 1f);
            transform.Translate(Vector3.forward * extraSpeed * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1) 플레이어 스킬에 맞았을 때 -> 풀링으로 복귀
        if (other.CompareTag(ConstData.AttackSkillTag))
        {
            int score = 0;
            SoundType soundType = SoundType.None;

            bool isRemove = true;
            // Tree일 때 50점, Rock일 때 200점, Log는 점수 없음(0)
            switch (type)
            {
                case Type.Tree:
                    score = 50;
                    soundType = SoundType.Sword_Slash_Hit_Wood; // 나무 맞는 소리
                    break;
                case Type.Rock:
                    score = 200;
                    soundType = SoundType.Sword_Slash_Hit_Rock; // 바위 맞는 소리
                    Player_Control.instance.HitObtacle(type);
                    break;
                case Type.Log:
                    // Log는 맞아도 점수 없음(너가 원하면 점수 추가)
                    score = 0;
                    soundType = SoundType.Sword_Slash_Hit_Rock; // 바위/통나무 맞는 소리(예시)
                    isRemove = false;
                    break;
            }

            // 사운드 재생
            if (soundType >= 0)
            {
                SoundManager.instance.Play_SoundEffect(soundType);
            }

            // 화면에 점수 텍스트 보여 주기 (Tree, Rock만 의미)
            if (score > 0)
            {
                GameManager.instance.PointUp(score);
                ShowScore(score);
            }

            if (isRemove)
            {
                // 풀링 회수
                Despawn();
            }
        }
        else if (other.CompareTag(ConstData.PlayerTag))
        {
            if (!Player_Control.instance.isImmortal && !Player_Control.instance.isHit)
            {
                switch (type)
                {
                    case Type.Tree:
                    case Type.Log:
                        {
                            SoundManager.instance.Play_SoundEffect(SoundType.Hit_Player);
                            Player_Control.instance.HitObtacle(type);
                            Despawn();
                        }
                        break;
                    case Type.Rock:
                        {

                        }
                        break;

                }
            }
        }
        // 2) DeadZone에 들어갔을 때 -> 풀링으로 복귀
        else if (other.CompareTag(ConstData.DeadZoneETCTag))
        {
            Despawn();
        }
    }

    /// <summary>
    /// 점수 표시용 프리팹을 화면에 띄우고 1초 뒤 파괴합니다.
    /// </summary>
    private void ShowScore(int amount)
    {
        if (scoreUpText == null) return;

        GameObject so = Instantiate(
            scoreUpText,
            transform.position + Vector3.up * 4f,
            Quaternion.Euler(50f, 0f, 0f)
        );
        var textComp = so.transform
                         .GetChild(0)
                         .GetChild(0)
                         .GetComponent<TextMeshProUGUI>();
        if (textComp != null)
        {
            textComp.text = amount.ToString();
        }
        Destroy(so, 1f);
    }

    /// <summary>
    /// OnDespawn 콜백을 호출해 풀링 매니저가 이 오브젝트를 회수하도록 합니다.
    /// </summary>
    public void Despawn()
    {
        // 중복 호출 방지를 위해 한번만 호출
        OnDespawn?.Invoke();
        OnDespawn = null;
    }
}
