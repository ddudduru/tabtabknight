using System;
using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour
{
    private Rigidbody rgbd;
    private Animator anim;
    public GameObject scoreUpText;
    public event Action OnReturnToPool;

    private void Awake()
    {
        rgbd = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        float randSpd = UnityEngine.Random.Range(0.7f, 1f);
        anim.SetFloat("anim_spd", randSpd);
    }

    private void Update()
    {
        transform.position += new Vector3(0f, 0f, GameManager.instance.gameSpd) * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player_Attack_Slash") || other.CompareTag("Player_Attack_Skill"))
        {
            SoundManager.instance.Play_SoundEffect(1);
            GameManager.instance.PointUp(10);
            GameObject so = Instantiate(scoreUpText, transform.position + new Vector3(0f, 4f, 0f),
                Quaternion.Euler(50f, 0f, 0f));
            so.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "10";
            Destroy(so, 1f);
            OnReturnToPool?.Invoke();
        }
        else if (other.CompareTag("DeadZoneETC"))
        {
            OnReturnToPool?.Invoke();
        }
    }
}
