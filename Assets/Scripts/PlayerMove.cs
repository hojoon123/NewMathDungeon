using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    //�����ص� �͵�
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    public GameManager gameManager;

    //�̵����� ����
    float input_x; //���� Ű�е�
    float isRight = 1; //�ٶ󺸴� ���� 1=������ -1 ����

    //ü�� ���� ����
    [SerializeField]
    private float maxHP = 4;
    private float curTime;
    private float currentHP;
    private bool underzeroHP;
    private bool die;

    //���ǵ弱��
    [SerializeField]
    private float speed;
    public float maxSpeed;

    //���� �Ŀ� ����
    [SerializeField]
    private float jumpPower;

    // ���� ���� ����
    [SerializeField]
    private KeyCode keyCodeAttack = KeyCode.A;
    private bool Attack;
    public Transform pos;
    public Vector2 boxSize;
    [SerializeField]
    private float coolTime;
    public int ComboAttackCounter = 0;
    float lastClickedTime = 0;
    public float maxComboDelay = 0.9f;

    //�׶��� ����
    public Transform groundChkFront;
    public Transform groundChkBack;
    bool isGround;
    public float chkDistance;
    public LayerMask g_Layer;
    bool ground_front;
    bool ground_back;

    // �� ���� ����
    public Transform wallChk;
    public float wallChkDistance;
    public LayerMask w_Layer;
    public float wallJumpPower;
    bool isWall;
    public float slidingSpeed;
    bool isWallJump;

    void Awake()
    {
        //������Ʈ������
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        currentHP = maxHP;
    }

    //�̵����� ���� �⺻���� �ֿ켱 �Ǿ�� �ϴ� ������ FixedUpdate�� �ַ� ���� ��, ���� ���� ����
    //ª�� �������� ����ϴ� �͵��� Update�� �ֱ⵵ ��.
    void FixedUpdate()
    {
        //�������� �ƴϰ�, �� ���� ���� �ƴ� ��.
        //input_x = �÷��̾ ������ �ִ� ȭ��ǥ ������ = 1 ���� = -1
        //isRight = �ٶ󺸰� �ִ� ����, ������ =1, ���� = -1
        //���۽� ������ ���� ����
        //if ���� �ؼ��ϸ� �Է��ϰ� �ִ� �¿�ȭ��ǥ�� �ٶ󺸴� ������ �ٸ� ��
        if (!Attack)
        {
            if (!isWallJump)
            {
                Run();
                if ((input_x > 0 && isRight < 0) || (input_x < 0 && isRight > 0))
                    FlipPlayer();
            }
        }
    }
    void FreezeX() // ���� �ð� �� ������ ���¸� false�� ����� ���� �Լ�(Invoke �Լ��� �̿��ϱ� ���� �Լ��� ����)
    {
        isWallJump = false;
    }

    void FlipPlayer() //�÷��̾� ������ȯ �Լ�ȭ
    {
        transform.eulerAngles = new Vector3(0, Mathf.Abs(transform.eulerAngles.y - 180), 0);
        isRight = isRight * -1; 
    }

    // Update is called once per frame
    void Update()
    {
        OnComboAttack();

        Attack1();

        JumpAttack();

        Jump();

        //�׶��� ����ĳ��Ʈ 
        ground_front = Physics2D.Raycast(groundChkFront.position, Vector2.down, chkDistance, g_Layer);
        ground_back = Physics2D.Raycast(groundChkBack.position, Vector2.down, chkDistance, g_Layer);
        //�նǴ� �� �ʿ� �ٴ��� �����Ǹ� isGround�� True �� �ƴϸ� False��
        if (ground_front || ground_back)
        {
            isGround = true;
            anim.SetFloat("AirSpeedY", 0);
        }
        else
        {
            isGround = false;
            //������ ���� �� Fall �ִϸ��̼� �ߵ�
            anim.SetFloat("AirSpeedY", -1);
        }
        anim.SetBool("Grounded", isGround);

        //Wall ����ĳ��Ʈ WallSlide �� �޴޷��� �� ������ �Ǵ� �ִϸ��̼�
        isWall = Physics2D.Raycast(wallChk.position, Vector2.right * isRight, wallChkDistance, w_Layer);

        //���� ���¿��� �� �Ǵ� ���ʿ� �ٴ��� �����Ǹ� �ٴڿ� �پ �̵��ϰ� ����
        //if (!isGround && (ground_front || ground_back))
        //rigid.velocity = new Vector2(rigid.velocity.x, 0); ���� ��� ���� ���� �Ʒ����� �ݸ����� �ɷ��� ������ 



        //���� �پ����� �� ������ �������� �ڵ� + ������
        if (isWall && Input.GetAxisRaw("Horizontal") != 0)
        {
            anim.SetBool("WallSlide", true);
            isWallJump = false;
            //�ö���ٰ� �����̵����ǵ� ���� �Ǿ����
            //���ϴ� ��� �������� �� �ӵ��� ���ظ� ���� �ʰ� �״�� ������ ������.
            //���� ���� �� 
            rigid.velocity = new Vector2(rigid.velocity.x, rigid.velocity.y * slidingSpeed);
            if (Input.GetButtonDown("Jump") && !isGround) //������ �����°�?
            {
                anim.SetBool("WallSlide", false);
                isGround = false;
                isWallJump = true;
                Invoke("FreezeX", 0.3f);
                rigid.velocity = new Vector2(wallJumpPower * -isRight, 0.9f * wallJumpPower);
                FlipPlayer();
            }
        }
        else
        {
            anim.SetBool("WallSlide", false);
        }


        //Stop Speed  ��ư���� ������ �� ���� �ӵ� �ٲٱ�
        if (Input.GetButtonUp("Horizontal"))
        {
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        }

        if (isGround)
        {
            if (Mathf.Abs(rigid.velocity.x) < 0.3f)
                anim.SetInteger("AnimState", 0);
            else
                anim.SetInteger("AnimState", 1);

        }

    }


    //�÷��̾� �ݸ��� ������ �� ���� �±׷� Portal�� �� ���� �������� �̵�
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Portal")
        {
            gameManager.NextStage();
        }
    }

    //�ӵ� 0���� ���� ���� ���� �������� �� �� �ӵ� �ʱ�ȭ�뤷��
    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }

    //�ȱ� �޸��� �Լ�
    void Run()
    {
        input_x = Input.GetAxisRaw("Horizontal");

        rigid.velocity = (new Vector2(input_x * speed, rigid.velocity.y));

        if (rigid.velocity.x > maxSpeed)
        {
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        }
        else if (rigid.velocity.x < maxSpeed * (-1))
        {
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);

        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGround)
        {

            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetTrigger("Jump");

        }
    }
    void Attack1()
    {
        if (curTime <= 0 && anim.GetBool("Grounded"))
        {
            if (Input.GetKeyDown(keyCodeAttack))
            {
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
                foreach (Collider2D collider in collider2Ds)
                {
                    if (collider.tag == "Enemy")
                    {
                        collider.GetComponent<EnemyBehavior>().TakeDamage(1);
                    } //������

                }
                //hp>0�϶��� ���ݰ���
                if (!underzeroHP)
                {
                    Attack = true;
                    anim.SetTrigger("Attack1");
                    curTime = coolTime;
                    Invoke("BoolAttack", coolTime);
                }

            }
        }
        else
        {
            curTime -= Time.deltaTime;
        }
    }

    void BoolAttack()
    {
        Attack = false;
    }

    //�÷��̾� ���ݹ��� ǥ�� 
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(pos.position, boxSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(wallChk.position, Vector2.right * isRight * wallChkDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(groundChkFront.position, Vector2.down * chkDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(groundChkBack.position, Vector2.down * chkDistance);
    }

    public void TakeDamage(float damage)
    {
        if (currentHP <= 0)
        {
            underzeroHP = true;
            OnDie();
        }

        else
        {
            anim.SetTrigger("Hurt");
            currentHP -= damage;
        }

    }

    void OnDie()
    {
        if (!die)
        {
            Attack = true; //�������̰�

            anim.SetTrigger("Death");

            Destroy(gameObject, 1.5f);

            //gameObject.GetComponent<BoxCollider>().enabled = false;

            die = true;
        }

    }
    void JumpAttack()
    {
        if (!anim.GetBool("Grounded") && curTime <= 0)
        {
            if (Input.GetKeyDown(keyCodeAttack))
            {
                Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
                foreach (Collider2D collider in collider2Ds)
                {
                    if (collider.tag == "Enemy")
                    {
                        collider.GetComponent<EnemyBehavior>().TakeDamage(1);
                    } //������

                }

                if (!underzeroHP)
                {
                    anim.SetTrigger("AirAttackUp");
                    curTime = coolTime;
                }
            }
        }
    }

    void OnComboAttack()
    {

        if (Time.deltaTime - lastClickedTime > maxComboDelay)
        {
            ComboAttackCounter = 0;
        }

        if (Input.GetKeyDown(KeyCode.D) && anim.GetBool("Grounded"))
        {
            Collider2D[] collider2Ds = Physics2D.OverlapBoxAll(pos.position, boxSize, 0);
            foreach (Collider2D collider in collider2Ds)
            {
                if (collider.tag == "Enemy")
                {
                    collider.GetComponent<EnemyBehavior>().TakeDamage(1);
                }
            }

            lastClickedTime = Time.deltaTime;
            ComboAttackCounter++;
            if (ComboAttackCounter == 1 && !underzeroHP)
            {
                anim.SetBool("Attack11", true);
                Attack = true;
                Invoke("BoolAttack", coolTime);
            }

            ComboAttackCounter = Mathf.Clamp(ComboAttackCounter, 0, 3);

        }


    }

    public void return1()
    {
        if (ComboAttackCounter >= 2 && !underzeroHP)
        {
            anim.SetBool("Attack11", false);
            anim.SetBool("Attack22", true);
            Attack = true;
            Invoke("BoolAttack", coolTime);
        }
        else
        {
            Debug.Log("attack11false");
            anim.SetBool("Attack11", false);
            ComboAttackCounter = 0;
        }
    }
    public void return2()
    {
        if (ComboAttackCounter >= 3 && !underzeroHP)
        {
            anim.SetBool("Attack22", false);
            anim.SetBool("Attack33", true);
            Attack = true;
            Invoke("BoolAttack", coolTime);
        }
        else
        {
            Debug.Log("attack22false");
            anim.SetBool("Attack22", false);
            ComboAttackCounter = 0;
        }
    }
    public void return3()
    {

        anim.SetBool("Attack11", false);
        anim.SetBool("Attack22", false);
        anim.SetBool("Attack33", false);
        ComboAttackCounter = 0;

    }
}


