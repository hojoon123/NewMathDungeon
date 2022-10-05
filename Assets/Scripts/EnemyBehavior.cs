using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{

    [SerializeField]
    private float coolTime;
    [SerializeField]
    private float maxHP = 4;

    private float curTime;
    private float currentHP;
    private int nextMove;
    public float maxSpeed;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    //private bool Attack;
    public Transform AttackRangepos;
    public Vector2 AttackRangeboxSize;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        currentHP = maxHP;
        Invoke("Think", 5);

    }

    private void FixedUpdate()
    {
        Walk();


        /*else
        {
            rigid.velocity = Vector2.zero;
        }*/
      
 
        //��ĭ��
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove, rigid.position.y);
        //Ray
        Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));
        //Ray ���� ���� ������Ʈ�� Ž��
        RaycastHit2D raycast = Physics2D.Raycast(frontVec, Vector3.down, 2, LayerMask.GetMask("Floor"));

        if(raycast.collider == null)
        {
            Debug.Log("���! �̾��� ��������");
            nextMove = nextMove * (-1);
            CancelInvoke();
            Invoke("Think", 5);
        }

    }
    // Update is called once per frame
    void Update()
    {
        Attack1();

        if(Mathf.Abs(rigid.velocity.x) < 0.3)
        {
            anim.SetInteger("AnimState", 0);
        }
        else if(Mathf.Abs(rigid.velocity.x) < 1.5)
        {
            anim.SetInteger("AnimState", 1);
        }

        if(nextMove == -1)
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            Debug.Log("-1");
        }
        
        else if(nextMove == 1)
        {
            transform.eulerAngles = new Vector3(0, 180, 0);
            Debug.Log("1");
        }

        else if (nextMove == 0)
        {
            rigid.velocity = Vector2.zero;
            Debug.Log("0");
        }
    }

    void Think()
    {
        // -1 �����̵�, 1 �����U�̵�, 0 ����
        if (currentHP > 0)
        {
            nextMove = Random.Range(-1, 2);

            float time = Random.Range(2f, 5f);
            Invoke("Think", time);

            Debug.Log("������");
        }
 
    }

    void Walk()
    {
        
        
        rigid.AddForce(Vector2.right * nextMove, ForceMode2D.Impulse);

        if (rigid.velocity.x > maxSpeed)
        {
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        }
        else if (rigid.velocity.x < maxSpeed * (-1))
        {
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);
        }
    }

    void Attack1()
    {
        //���ھȿ� �÷��̾������ ���� ��Ÿ�� �������� �����ֱ�
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(AttackRangepos.position, AttackRangeboxSize, 0);
        foreach (Collider2D collider in collider2Ds)
        {
            if(collider.tag == "Player" && curTime <= 0)
            {

                
                if (currentHP > 0)
                {
                    collider.GetComponent<PlayerMove>().TakeDamage(1);
                    Debug.Log("���������ε餷���");//�÷��̾�����߰�
                    anim.SetTrigger("Attack");
                    curTime = coolTime;
                    CancelInvoke();
                    nextMove = 0;
                    Invoke("Think", coolTime);
                }
            }

            else
            {
                curTime -= Time.deltaTime;               
            }
            
        }
    }
    /*void BoolAttack()
    {
        Attack = false;
    }*/
    // ���ݹ������ڱ׸���
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(AttackRangepos.position, AttackRangeboxSize);
    }

    void OnDie()
    {

        CancelInvoke();
        anim.SetTrigger("Death");
        Debug.Log("�׾���");

        Destroy(gameObject, 1f);
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;

        if (currentHP <= 0)
        {
            OnDie();
        }
        else
        {
            StopCoroutine("HitAnimation");
            StartCoroutine("HitAnimation");
        }

    }

    private IEnumerator HitAnimation()
    {
        anim.SetTrigger("Hurt");
        yield return new WaitForSeconds(0.05f);  
    }


}
