using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public int type;
    // type 0 : 스킬, // type 1 : 포워드 // type 2 : 체력회복


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
