using System;
using UnityEngine;
using TMPro;

public class Obstacls_Control : MonoBehaviour
{
    public enum Type { Tree, Rock, Log }
    public Type type;

    [Header("Ǯ������ ������ �� ȣ���� �ݹ�")]
    public Action OnDespawn;

    [Header("���� ǥ�� ������(�ڽ����� ���� �� 1�� �� �ı�)")]
    public GameObject scoreUpText;

    [Header("Log Ÿ���� �� �߰��� �� ������ �̵���ų �ӵ�(�� �ӵ��� �� ��)")]
    public float logExtraSpeedMultiplier = 1.2f;
    // ��: 1.2f��, �⺻ gameSpd * 1.2 �ӵ��� �̵�

    /// <summary>
    /// �� ��Ʈ ���ο��� ���� X ��ġ�� ȸ���� ���� ������ �ݴϴ�.
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
        // �⺻������ ��ֹ��� MapController�� �ڽ� ������Ʈ�� 
        // Translate(Vector3.forward * gameSpd) �� �ָ� ���� �����Դϴ�.
        // ��, Log Ÿ�Ը� �߰� �ӵ��� ���� �ݴϴ�.
        if (type == Type.Log)
        {
            float extraSpeed = GameManager.instance.gameSpd * (logExtraSpeedMultiplier - 1f);
            transform.Translate(Vector3.forward * extraSpeed * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1) �÷��̾� ��ų�� �¾��� �� -> Ǯ������ ����
        if (other.CompareTag("Player_Attack_Skill"))
        {
            int score = 0;
            int soundType = -1;

            // Tree�� �� 50��, Rock�� �� 200��, Log�� ���� ����(0)
            switch (type)
            {
                case Type.Tree:
                    score = 50;
                    soundType = 2; // ���� �´� �Ҹ�
                    break;
                case Type.Rock:
                    score = 200;
                    soundType = 4; // ���� �´� �Ҹ�
                    break;
                case Type.Log:
                    // Log�� �¾Ƶ� ���� ����(�ʰ� ���ϸ� ���� �߰�)
                    score = 0;
                    soundType = 4; // ����/�볪�� �´� �Ҹ�(����)
                    break;
            }

            // ���� ����
            if (score > 0)
            {
                GameManager.instance.PointUp(score);
            }

            // ���� ���
            if (soundType >= 0)
            {
                SoundManager.instance.Play_SoundEffect(soundType);
            }

            // ȭ�鿡 ���� �ؽ�Ʈ ���� �ֱ� (Tree, Rock�� �ǹ�)
            if (score > 0 && scoreUpText != null)
            {
                ShowScore(score);
            }

            // Ǯ�� ȸ��
            Despawn();
        }
        // 2) DeadZone�� ���� �� -> Ǯ������ ����
        else if (other.CompareTag("DeadZoneETC"))
        {
            Despawn();
        }
    }

    /// <summary>
    /// ���� ǥ�ÿ� �������� ȭ�鿡 ���� 1�� �� �ı��մϴ�.
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
    /// OnDespawn �ݹ��� ȣ���� Ǯ�� �Ŵ����� �� ������Ʈ�� ȸ���ϵ��� �մϴ�.
    /// </summary>
    public void Despawn()
    {
        // �ߺ� ȣ�� ������ ���� �ѹ��� ȣ��
        OnDespawn?.Invoke();
        OnDespawn = null;
    }
}
