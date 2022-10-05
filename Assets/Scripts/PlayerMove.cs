using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    //선언해둔 것들
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;
    public GameManager gameManager;

    //이동관련 선언
    float input_x; //누른 키패드
    float isRight = 1; //바라보는 방향 1=오른쪽 -1 왼쪽

    //체력 관련 선언
    [SerializeField]
    private float maxHP = 4;
    private float curTime;
    private float currentHP;
    private bool underzeroHP;
    private bool die;

    //스피드선언
    [SerializeField]
    private float speed;
    public float maxSpeed;

    //점프 파워 선언
    [SerializeField]
    private float jumpPower;

    // 공격 관련 선언
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

    //그라운드 관련
    public Transform groundChkFront;
    public Transform groundChkBack;
    bool isGround;
    public float chkDistance;
    public LayerMask g_Layer;
    bool ground_front;
    bool ground_back;

    // 벽 관련 선언
    public Transform wallChk;
    public float wallChkDistance;
    public LayerMask w_Layer;
    public float wallJumpPower;
    bool isWall;
    public float slidingSpeed;
    bool isWallJump;

    void Awake()
    {
        //컴포넌트따오기
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        currentHP = maxHP;
    }

    //이동같은 아주 기본적인 최우선 되어야 하는 내용은 FixedUpdate에 주로 넣음 단, 점프 공격 같은
    //짧은 프레임을 사용하는 것들은 Update에 넣기도 함.
    void FixedUpdate()
    {
        //공격중이 아니고, 벽 점프 중이 아닐 때.
        //input_x = 플레이어가 누르고 있는 화살표 오른쪽 = 1 왼쪽 = -1
        //isRight = 바라보고 있는 방향, 오른쪽 =1, 왼쪽 = -1
        //시작시 오른쪽 보며 시작
        //if 문을 해석하면 입력하고 있는 좌우화살표와 바라보는 방향이 다를 때
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
    void FreezeX() // 일정 시간 후 벽점프 상태를 false로 만들기 위한 함수(Invoke 함수를 이용하기 위해 함수로 만듬)
    {
        isWallJump = false;
    }

    void FlipPlayer() //플레이어 방향전환 함수화
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

        //그라운드 레이캐스트 
        ground_front = Physics2D.Raycast(groundChkFront.position, Vector2.down, chkDistance, g_Layer);
        ground_back = Physics2D.Raycast(groundChkBack.position, Vector2.down, chkDistance, g_Layer);
        //앞또는 뒤 쪽에 바닥이 감지되면 isGround를 True 로 아니면 False로
        if (ground_front || ground_back)
        {
            isGround = true;
            anim.SetFloat("AirSpeedY", 0);
        }
        else
        {
            isGround = false;
            //점프를 했을 때 Fall 애니메이션 발동
            anim.SetFloat("AirSpeedY", -1);
        }
        anim.SetBool("Grounded", isGround);

        //Wall 레이캐스트 WallSlide 벽 메달렸을 때 느리게 되는 애니메이션
        isWall = Physics2D.Raycast(wallChk.position, Vector2.right * isRight, wallChkDistance, w_Layer);

        //점프 상태에서 앞 또는 뒤쪽에 바닥이 감지되면 바닥에 붙어서 이동하게 변경
        //if (!isGround && (ground_front || ground_back))
        //rigid.velocity = new Vector2(rigid.velocity.x, 0); 현재 사용 안함 상태 아래쪽의 콜리더에 걸려서 맞춰짐 



        //벽에 붙어있을 때 느리게 내려오는 코드 + 벽점프
        if (isWall && Input.GetAxisRaw("Horizontal") != 0)
        {
            anim.SetBool("WallSlide", true);
            isWallJump = false;
            //올라오다가 슬라이딩스피드 적용 되어버림
            //원하는 결과 점프했을 때 속도의 방해를 받지 않고 그대로 갔으면 좋겠음.
            //조건 점프 후 
            rigid.velocity = new Vector2(rigid.velocity.x, rigid.velocity.y * slidingSpeed);
            if (Input.GetButtonDown("Jump") && !isGround) //점프를 눌렀는가?
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


        //Stop Speed  버튼에서 떼었을 때 ㅇㅇ 속도 바꾸기
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


    //플레이어 콜리전 겹쳤을 때 ㅇㅇ 태그로 Portal일 때 다음 스테이지 이동
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Portal")
        {
            gameManager.NextStage();
        }
    }

    //속도 0으로 만듬 보통 다음 스테이지 갈 때 속도 초기화용ㅇㅇ
    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }

    //걷기 달리기 함수
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
                    } //데미지

                }
                //hp>0일때만 공격가능
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

    //플레이어 공격범위 표시 
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
            Attack = true; //못움직이게

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
                    } //데미지

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


