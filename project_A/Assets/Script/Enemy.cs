using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour
{
    Rigidbody rgbd;// Start is called before the first frame update
    Animator anim;

    public GameObject score_up_txt;
    void Awake()
    {
        rgbd = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        float rand_value = Random.Range(0.7f, 1f);
        anim.SetFloat("anim_spd",rand_value);
    }

    private void FixedUpdate()
    {
        rgbd.velocity = Vector3.zero;
    }
    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(0f, 0f, GameManager.instance.gameSpd) * Time.deltaTime;
        //rgbd.position += new Vector3(0f, 0f, 1f*Time.deltaTime);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player_Attack_Slash"))
        {
            SoundManager.instance.Play_SoundEffect(1);
            GameManager.instance.PointUp(10);
            GameObject so = Instantiate(score_up_txt, transform.position+new Vector3(0f,4f,0f), Quaternion.Euler(new Vector3(50f, 0f, 0f)));
            so.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = ""+10;
            Destroy(so, 1f);
            Destroy(this.gameObject);
        }
        if (other.CompareTag("Player_Attack_Skill"))
        {
            SoundManager.instance.Play_SoundEffect(1);
            GameManager.instance.PointUp(10);
            GameObject so = Instantiate(score_up_txt, transform.position + new Vector3(0f, 4f, 0f), Quaternion.Euler(new Vector3(50f, 0f, 0f)));
            so.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + 10;
            Destroy(so, 1f);
            Destroy(this.gameObject);
        }
        if (other.CompareTag("DeadZoneETC"))
        {
            Destroy(this.gameObject);
        }
    }
}
