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
      
 
        //한칸앞
        Vector2 frontVec = new Vector2(rigid.position.x + nextMove, rigid.position.y);
        //Ray
        Debug.DrawRay(frontVec, Vector3.down, new Color(0, 1, 0));
        //Ray 쏴서 맞은 오브젝트를 탐지
        RaycastHit2D raycast = Physics2D.Raycast(frontVec, Vector3.down, 2, LayerMask.GetMask("Floor"));

        if(raycast.collider == null)
        {
            Debug.Log("경고! 이앞은 낭떨어지");
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
        // -1 왼쪽이동, 1 오른쪾이동, 0 멈춤
        if (currentHP > 0)
        {
            nextMove = Random.Range(-1, 2);

            float time = Random.Range(2f, 5f);
            Invoke("Think", time);

            Debug.Log("생각해");
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
        //상자안에 플레이어가들어오면 공격 쿨타임 기준으로 공격주기
        Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(AttackRangepos.position, AttackRangeboxSize, 0);
        foreach (Collider2D collider in collider2Ds)
        {
            if(collider.tag == "Player" && curTime <= 0)
            {

                
                if (currentHP > 0)
                {
                    collider.GetComponent<PlayerMove>().TakeDamage(1);
                    Debug.Log("범위안으로들ㅇ어모");//플레이어데미지추가
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
    // 공격범위상자그리기
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(AttackRangepos.position, AttackRangeboxSize);
    }

    void OnDie()
    {

        CancelInvoke();
        anim.SetTrigger("Death");
        Debug.Log("죽어어어");

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
