using UnityEngine;

public class Map_Control : MonoBehaviour
{
    public static Map_Control instance;
    public GameObject[] Map_parts;
    public GameObject[] tree_objs;
    public int tree_amount;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // 초기 트리 위치 랜덤화
        for (int i = 0; i < Map_parts.Length; i++)
        {
            Transform treesParent = Map_parts[i].transform.GetChild(0);
            for (int j = 0; j < treesParent.childCount; j++)
                treesParent.GetChild(j).GetComponent<Obstacls_Control>().Setting_Random();
        }
    }

    private void Update()
    {
        float speed = GameManager.instance.gameSpd * Time.deltaTime;
        for (int i = 0; i < Map_parts.Length; i++)
        {
            Transform part = Map_parts[i].transform;
            part.position += new Vector3(0f, 0f, speed);
            if (part.position.z >= 75f)
                RecyclePart(i);
        }
    }

    private void RecyclePart(int index)
    {
        Transform part = Map_parts[index].transform;
        part.position = new Vector3(0f, 0f, -74f);
        Transform treesParent = part.GetChild(0);
        if (treesParent.childCount < tree_amount)
        {
            int addCount = tree_amount - treesParent.childCount;
            for (int i = 0; i < addCount; i++)
            {
                int shape = UnityEngine.Random.Range(0, tree_objs.Length);
                GameObject to = Instantiate(tree_objs[shape], treesParent);
                float randY = UnityEngine.Random.Range(-1.23f, -1.18f);
                float randZ = UnityEngine.Random.Range(-20f, 20f);
                to.transform.localPosition = new Vector3(0f, randY, randZ);
            }
        }
        for (int i = 0; i < treesParent.childCount; i++)
            treesParent.GetChild(i).GetComponent<Obstacls_Control>().Setting_Random();
    }
}