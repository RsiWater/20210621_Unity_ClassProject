using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    [Header("Menu物件")]
    public GameObject menu;
    [Header("攻擊hitBox")]
    public BoxCollider2D attackBox;

    [Header("射擊point")]
    public GameObject shootPoint;
    
    [Header("移動速度")]
    [Range(0.01f, 0.5f)]
    public float movingSpeed = 0.1f;

    [Header("HP血條")]
    public Canvas HPbar;
    [Header("護盾")]
    public GameObject shield;

    [Header("子彈")]
    public GameObject bullet;

    private UnityEngine.Animator anim;
    private Rigidbody2D rigidbody2D;
    private BoxCollider2D boxCollider2D;
    private float MaxHealthPoint = 100f;
    [Header("初始血量")]
    [Range(1f, 100f)]
    public float HealthPoint = 100f;
    private int attackState = 0;
    private bool isJumping = false, isFacingRight = true, isParrying = false, isShooting = false;
    private float[] attackRate = {0.2f, 0.2f, 0.5f};
    private float[] attackDamage = {.5f, .5f, 10f};
    private float nextAttackTime = 0.0f, cooldownAttackTime = 0.5f;
    private float shootRate = 0.2f, nextShootTime = 0.0f;
    private float InvincibleRate = 2f, nextHurtTime = 0.0f;
    private float parryInvincibleRate = .2f, parryCDRate = 2f, nextParryTime = 0.0f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.resetVariableCheck();

        float currentMovingSpeed = this.movingSpeed;
        if(Input.GetKey(KeyCode.LeftShift))
        {
            currentMovingSpeed *= 0.75f;
        }

        if(Input.GetKey(KeyCode.Z))
        {
            this.Attack();
        }
        else if(Input.GetKey(KeyCode.C))
        {
            this.parry();
        }
        else
        {
            if(Input.GetKey(KeyCode.X))
            {
                this.isShooting = true;
                if(Input.GetKey(KeyCode.UpArrow))
                {
                    anim.SetBool("isLookUp", true);
                    this.Shoot(isFacingRight ? 90f : -90f);
                }
                else
                {
                    anim.SetTrigger("shoot");
                    this.Shoot(0f);
                }
            }
            else
            {
                anim.SetBool("isLookUp", false);
                this.isShooting = false;
            }
            
            if(Input.GetKey(KeyCode.LeftArrow))
            {
                this.isFacingRight = false;
                transform.position += new Vector3(-currentMovingSpeed, 0);
                transform.rotation = new Quaternion(0, 180, 0, 0);
                this.attackBox.transform.rotation = new Quaternion(0, 180, 0, 0);
                anim.SetBool("isRun", true);
            }
            else if(Input.GetKey(KeyCode.RightArrow))
            {
                this.isFacingRight = true;
                transform.position += new Vector3(currentMovingSpeed, 0);
                transform.rotation = new Quaternion(0, 0, 0, 0);
                this.attackBox.transform.rotation = new Quaternion(0, 0, 0, 0);
                anim.SetBool("isRun", true);
            }
            else if(!Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
            {
                anim.SetBool("isRun", false);
            }
        }
        
        if(Input.GetKey(KeyCode.UpArrow) && isGrounded() && !this.isShooting)
        {
            isJumping = true;
            rigidbody2D.velocity = Vector2.up * 30f;
        }
        else if(!Input.GetKey(KeyCode.UpArrow) && !isGrounded() && isJumping)
        {
            rigidbody2D.velocity = Vector2.up * 0f;
        }

        if(Input.GetKey(KeyCode.T))
        {
            this.menu.GetComponent<MenuScript>().Pause();
        }
        //debug
        if(Input.GetKey(KeyCode.Y))
        {
            this.menu.GetComponent<MenuScript>().Defeated();
        }

        if(!this.isGrounded()) anim.SetBool("isJump", true);
        else anim.SetBool("isJump", false);
    }
    private void resetVariableCheck()
    {
        if(Time.time > this.nextParryTime - this.parryCDRate + this.parryInvincibleRate)
        {
            this.isParrying = false;
            anim.SetBool("isLookUp", false);
        }
    }

    private bool isGrounded()
    {
        if(rigidbody2D.velocity.y == 0)
        {
            isJumping = false;
            return true;
        }
        else
            return false;
    }

    private void Attack()
    {
        if(!isGrounded() || Time.time <= nextAttackTime) return;

        if(Time.time - nextAttackTime >= this.cooldownAttackTime) this.attackState = 0;
        nextAttackTime = Time.time + attackRate[this.attackState];
        

        anim.SetTrigger("attack");

        List<Collider2D> touchingEnemyBox = attackBox.GetComponent<AttackBoxBehaviour>().GetTouchingEnemyBox();
        foreach(Collider2D ele in touchingEnemyBox)
        {
            if(ele.gameObject.tag != "Enemy" || ele == null) return;

            ele.GetComponent<EnemyBehaviour>().takeDamage(-this.attackDamage[this.attackState]);
        }

        if(this.attackState < 2) this.attackState++;
        else this.attackState = 0;
        //old method

        // BoxCollider2D[] contactBox = new BoxCollider2D[10];
        // ContactFilter2D rst = new ContactFilter2D();
        // Debug.Log(attackBox.OverlapCollider(rst, contactBox));
        // foreach(BoxCollider2D ele in contactBox)
        // {
        //     Debug.Log(ele.tag);
        //     if(ele.tag != "Enemy" || ele == null) return;

        //     ele.GetComponent<EnemyBehaviour>().takeDamage(10);
        // }
    }

    private void Shoot(float detAngle)
    {
        if(Time.time <= nextShootTime) return;

        nextShootTime = Time.time + shootRate;

        Vector2 sampleDir = (this.isFacingRight) ? Vector2.right : Vector2.left;
        Vector3 shootVector = Quaternion.AngleAxis(detAngle, Vector3.forward) * sampleDir;

        GameObject newBullet = Instantiate(this.bullet, this.shootPoint.transform.position, Quaternion.Euler(0, 0, detAngle));
        newBullet.GetComponent<BulletBehaviour>().setShootVector(shootVector);
    }
    private void parry()
    {
        if(Time.time < this.nextParryTime) return;
        this.nextParryTime = Time.time + this.parryCDRate;

        anim.SetBool("isLookUp", true);
        // Debug.Log("Parrying");
        Vector3 pos = this.gameObject.transform.position;
        pos.y += 2.3f;
        GameObject shield = Instantiate(this.shield, pos, this.gameObject.transform.rotation);
        Destroy(shield, this.parryInvincibleRate);
        this.isParrying = true;
        
    }

    public void takeDamage(float detValue)
    {
        if(Time.time <= this.nextHurtTime) return;
        if(isParrying)
        {
            Debug.Log("Perfect!");
            return;
        }

        this.nextHurtTime = Time.time + this.InvincibleRate;
        
        StartCoroutine(this.playerDamagedAnimation());
        UnityEngine.UI.Slider slider = this.HPbar.GetComponentInChildren<CanvasRenderer>().GetComponent<UnityEngine.UI.Slider>();

        Debug.Log("Hurt! " + this.HealthPoint);
        this.HealthPoint += detValue;
        slider.value = this.HealthPoint / this.MaxHealthPoint;

        if(this.HealthPoint <= 0) StartCoroutine(this.Dead());
    }

    private IEnumerator playerDamagedAnimation()
    {
        anim.SetTrigger("hurt");
        GameObject temp = gameObject.transform.GetChild(0).gameObject;
        for(int i = 0;i < temp.transform.childCount;i++)
        {
            GameObject ele = temp.transform.GetChild(i).gameObject;
            try
            {
                ele.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
            }
            catch(MissingComponentException)
            {
                break;
            }
        }
        
        yield return new WaitForSeconds(this.InvincibleRate);

        for(int i = 0;i < temp.transform.childCount;i++)
        {
            GameObject ele = temp.transform.GetChild(i).gameObject;
            try
            {
                ele.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            }
            catch(MissingComponentException)
            {
                break;
            }
        }
    }
    private IEnumerator Dead()
    {
        anim.SetTrigger("die");
        yield return new WaitForSeconds(.5f);
        this.menu.GetComponent<MenuScript>().Defeated();
    }
}
