using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy_slime;

    void Start()
    {
        StartCoroutine(Spawn(0, 5+5*GameManager.instance.gameLevel));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator Spawn(int type,int amount)
    {
        while (true)
        {
            if (type == 0)
            {// ΩΩ∂Û¿”
                for (int i = 0; i < amount; i++)
                {
                    GameObject slime_obj = Instantiate(enemy_slime, transform.position, transform.rotation);
                    float rand_x = Random.Range(-9f, 5f);
                    float rand_z = Random.Range(-35f, -20f);
                    slime_obj.transform.position = new Vector3(rand_x, slime_obj.transform.position.y, rand_z);
                }
            }
            yield return new WaitForSeconds(5f);
        }
    }
}
