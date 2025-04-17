using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Obstacls_Control : MonoBehaviour
{
    public enum Type { tree,rock,log};
    public Type type;

    public GameObject score_up_txt;
    public void Setting_Random()
    {
        float rand_x = Random.Range(-40f, -10f);
        float rand_y = Random.Range(0f,360f);
        transform.localPosition = new Vector3(rand_x, transform.localPosition.y, transform.localPosition.z);
        transform.localRotation = Quaternion.Euler(new Vector3(0f, rand_y, 0f));
    }
    private void Update()
    {
        if (type == Type.log)
        {
            transform.position += new Vector3(0f, 0f, GameManager.instance.gameSpd)*1.2f * Time.deltaTime;
        }
        if (type == Type.rock)
        {
            transform.position += new Vector3(0f, 0f, GameManager.instance.gameSpd) * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player_Attack_Skill")&&type==Type.tree)
        {
            GameManager.instance.PointUp(50);
            SoundManager.instance.Play_SoundEffect(2);
            GameObject so = Instantiate(score_up_txt, transform.position + new Vector3(0f, 4f, 0f), Quaternion.Euler(new Vector3(50f, 0f, 0f)));
            so.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + 50;
            Destroy(so, 1f);
            Destroy(this.gameObject);
        }

        if (other.CompareTag("Player_Attack_Skill") && type == Type.rock)
        {
            GameManager.instance.PointUp(200);
            SoundManager.instance.Play_SoundEffect(4);
            SoundManager.instance.Play_SoundEffect(5);
            Player_Control.instance.HitObtacle(2);
            GameObject so = Instantiate(score_up_txt, transform.position + new Vector3(0f, 4f, 0f), Quaternion.Euler(new Vector3(50f, 0f, 0f)));
            so.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + 200;
            Destroy(so, 1f);
            Destroy(this.gameObject);
        }

        if (other.CompareTag("DeadZoneETC")&&(type==Type.log|| type == Type.rock))
        {
            Destroy(this.gameObject);
        }
    }
}
