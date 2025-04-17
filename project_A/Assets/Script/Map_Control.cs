using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Map_Control : MonoBehaviour
{
    static public Map_Control instance;
    public GameObject[] Map_parts;
    public GameObject[] tree_objs;
    public GameObject log_obj;
    public GameObject rock_obj;
    public int tree_amount;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        StartCoroutine(SpawnLog());
        StartCoroutine(SpawnRock());
        for (int i = 0; i < 3; i++)
        {
            
            Transform trees_parent = Map_parts[i].transform.GetChild(0);
            for (int q = 0; q < trees_parent.childCount; q++)
            {
                trees_parent.GetChild(q).GetComponent<Obstacls_Control>().Setting_Random();
            }
        }
    }
    IEnumerator SpawnLog()
    {
        while (true)
        {
            float rand_x = Random.Range(-10f, 5f);
            GameObject lo = Instantiate(log_obj, transform.position, transform.rotation);
            lo.transform.position = new Vector3(rand_x, 0.8f, -15f);
            yield return new WaitForSeconds(20f);
        }
    }

    IEnumerator SpawnRock()
    {
        while (true)
        {
            float rand_x = Random.Range(-10f, 5f);
            GameObject lo = Instantiate(rock_obj, transform.position, transform.rotation);
            lo.transform.position = new Vector3(rand_x, 1.03f, -15f);
            yield return new WaitForSeconds(10f);
        }
    }

    void Setting_new(int index)
    {
        Map_parts[index].transform.position = new Vector3(0f, 0f, -74f);
        Transform trees_parent = Map_parts[index].transform.GetChild(0);
        if(trees_parent.childCount < tree_amount)
        {
            int add_count = tree_amount - trees_parent.childCount;
            for (int i=0;i<add_count; i++)
            {
                int tree_shape = Random.Range(0, 4);
                GameObject to = Instantiate(tree_objs[tree_shape], transform.position, transform.rotation);
                to.transform.parent = trees_parent;
                float rand_y = Random.Range(-1.18f, -1.23f);
                float rand_z = Random.Range(20f, -20f);
                to.transform.localPosition = new Vector3(0f, rand_y, rand_z);
               

            }
        }
        for (int i=0;i< trees_parent.childCount; i++)
        {
            trees_parent.GetChild(i).GetComponent<Obstacls_Control>().Setting_Random();
        }
    }
    // Update is called once per frame
    void Update()
    {
        
        
        for (int i = 0; i < 3; i++)
        {
            Map_parts[i].transform.position += new Vector3(0f, 0f, GameManager.instance.gameSpd) *Time.deltaTime;
            if (Map_parts[i].transform.position.z >= 75f) { Setting_new(i); }
        }
        
    }
}
