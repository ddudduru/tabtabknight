using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpanwer : MonoBehaviour
{
    public GameObject item_;
    void Start()
    {
        StartCoroutine(ItemSpawn_Cycle());
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Spawn();
        }
    }
    IEnumerator ItemSpawn_Cycle()
    {
        while (true)
        {
            Spawn();
            yield return new WaitForSeconds(7.5f);
        }
    }
    void Spawn()
    {
        for (int i = 0; i < 3+(GameManager.instance.gameLevel); i++)
        {
            GameObject item_obj = Instantiate(item_, transform.position, transform.rotation);
            int rand_type = Random.Range(0, 3);
            item_obj.GetComponent<Item>().type = rand_type;
            for (int a = 0; a < 3; a++)
            {
                if (a == rand_type)
                {
                    item_obj.transform.GetChild(a).gameObject.SetActive(true);
                }
                else
                {
                    item_obj.transform.GetChild(a).gameObject.SetActive(false);
                }
            }
            float rand_x = Random.Range(-9f, 5f);
            float rand_z = Random.Range(-45f, -20f);
            item_obj.transform.position = new Vector3(rand_x, item_obj.transform.position.y, rand_z);
        }
    }
    
}
