using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int type;
    // type 0 : ��ų, // type 1 : ������ // type 2 : ü��ȸ��


    void Update()
    {
        transform.position += new Vector3(0f, 0f, GameManager.instance.gameSpd) * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DeadZoneETC"))
        {
            Destroy(this.gameObject);
        }
    }

}
