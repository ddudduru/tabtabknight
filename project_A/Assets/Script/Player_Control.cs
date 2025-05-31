using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player_Control : MonoBehaviour
{
    static public Player_Control instance;

    Transform tr;
    Rigidbody rgbd;
    Animator anim;



    int isForward = 0;
    public int cur_dir; //
    float cur_acceleration;
    float max_acceleration;

    bool isAttack;
    bool isAttackReady;
    float attackDealy = 0.2f;

    bool isHit;

    public bool isImmortal;//isSkill;
    public float skill_amount;
    public BoxCollider attack_range;
    public TrailRenderer attack_effect;

    public GameObject score_up_txt;



    private void Awake()
    {
        instance = this;
        tr = this.transform;

        rgbd = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();

        cur_acceleration = 7f;
        max_acceleration = 10f;
    }
    void Start()
    {
        cur_dir = 1;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {

            if (isDizzy)
                dizzy_amount -= 1f;
            else
                Move_ChangeDir();

        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            anim.SetBool("attack_rotate", true);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            anim.SetBool("attack_rotate", false);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            anim.SetTrigger("doSlash");
        }
        if (cur_dir == 1) anim.SetBool("move_dir", true);
        else anim.SetBool("move_dir", false);
        cur_acceleration += Time.deltaTime * 4f;
        if (cur_acceleration >= max_acceleration) cur_acceleration = max_acceleration;

        attackDealy += Time.deltaTime;
        if (attackDealy >= 0.2f) { isAttackReady = true; }
        else { isAttackReady = false; }

        if (dizzy_amount > 0f) { isDizzy = true; anim.SetBool("isDizzy", true); }
        else { isDizzy = false; anim.SetBool("isDizzy", isDizzy); }

        if (skill_amount > 0f)
        {
            skill_amount -= Time.deltaTime;
            isImmortal = true;
            anim.SetBool("attack_rotate", true);
            skill_range.enabled = true;
        }
        else
        {
            isImmortal = false;
            anim.SetBool("attack_rotate", false);
            skill_range.enabled = false;
        }


        if (!isDizzy)
        {
            rgbd.linearVelocity = isForward * (transform.forward * 3f) + new Vector3(cur_dir * cur_acceleration, 0f, 0f);
        }
        else
        {
            rgbd.linearVelocity = new Vector3(0f, 0f, GameManager.instance.gameSpd);
        }





        RaycastHit hit;
        if (!isHit && !isDizzy && !isImmortal)
            if (Physics.Raycast(tr.position + new Vector3(0f, 1f, 0f), tr.forward, out hit, 3) || Physics.Raycast(tr.position + new Vector3(0f, 1f, 0f), new Vector3(-1f, 0f, 0f), out hit, 3) || Physics.Raycast(tr.position + new Vector3(0f, 1f, 0f), new Vector3(1f, 0f, 0f), out hit, 3))
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    if (isAttackReady && !isAttack)
                    {
                        StartCoroutine(nameof(DoSlash));
                    }
                }
            }

    }
    public float dizzy_amount;
    public bool isDizzy;
    public ParticleSystem hit_Obstacle_effect;
    public void Move_ChangeDir()
    {
        cur_dir = cur_dir == 1 ? -1 : 1;
        cur_acceleration = 7f;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            Item it = other.GetComponent<Item>();
            int getPoint = 50;
            switch (it.type)
            {
                case 0: // ��ų
                    skill_amount = 4f;
                    break;
                case 1: // ������
                    isForward = 1;
                    Invoke(nameof(ForwardEnd), 3f);

                    break;
                case 2: // ����(����)
                    getPoint = 150;
                    break;
            }
            SoundManager.instance.Play_SoundEffect(3);


            GameObject so = Instantiate(score_up_txt, transform.position + new Vector3(0f, 4f, 0f), Quaternion.Euler(new Vector3(50f, 0f, 0f)));
            so.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "" + getPoint;
            Destroy(so, 1f);

            Destroy(other.gameObject);

        }
        if (other.CompareTag("DeadZone"))
        {
            UI_Control.instance.Finish_game();

            //
        }

    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("CannotGoZone"))
        {
            isForward = 0;
        }
    }

    public BoxCollider skill_range;
    void ForwardEnd()
    {
        isForward = 0;
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Obstacle") && !isImmortal && !isHit)
        {
            var obstacls = col.gameObject.GetComponent<Obstacls_Control>();
            if (obstacls != null)
            {
                Obstacls_Control.Type tp = obstacls.type;
                if (tp == Obstacls_Control.Type.Tree)
                {
                    SoundManager.instance.Play_SoundEffect(7);
                    HitObtacle(0);
                    obstacls.Despawn();
                }
                else if (tp == Obstacls_Control.Type.Log)
                {
                    SoundManager.instance.Play_SoundEffect(7);
                    HitObtacle(1);
                    obstacls.Despawn();
                }
                else
                {

                }
            }
        }

    }
    void HitFalse()
    {
        isHit = false;
    }
    public void HitObtacle(int type)
    {


        isHit = true;
        skill_amount = 0;
        Invoke(nameof(HitFalse), 0.1f);
        anim.SetTrigger("doDizzy");
        hit_Obstacle_effect.Play();

        switch (type)
        {
            case 0: // tree
                dizzy_amount += 3f;
                break;
            case 1: // log
                dizzy_amount += 7f;
                break;
            case 2: // rock
                dizzy_amount += 5f;
                break;
        }


    }
    IEnumerator DoSlash()
    {
        attackDealy = 0f;
        isAttack = true;

        int rand_type = Random.Range(0, 2);
        anim.SetInteger("slashType", rand_type);
        anim.SetTrigger("doSlash");
        //attack_effect.enabled = true;
        SoundManager.instance.Play_SoundEffect(0);
        yield return new WaitForSeconds(0.05f);

        attack_range.enabled = true;
        yield return new WaitForSeconds(0.1f);
        attack_range.enabled = false;
        //attack_effect.enabled = false;

        yield return new WaitForSeconds(0.2f);
        isAttack = false;
    }

}
