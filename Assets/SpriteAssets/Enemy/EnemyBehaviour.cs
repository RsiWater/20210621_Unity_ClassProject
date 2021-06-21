using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private const int MAX_ACTION_MODE = 5;
    private int[] LIMITED_ATTACK_COUNT_BY_MODE = {3, 20, 3, 1, 1, 1, 1};
    private const float JUMP_ATTACK_RATE = 20f;
    [Header("menu")]
    public GameObject menu;

    [Header("玩家")]
    public GameObject player;
    [Header("攻擊box")]
    public Collider2D attackbox;
    [Header("攻擊子彈")]
    public GameObject bullet;
    [Header("雷射子彈")]
    public GameObject laserBullet;
    [Header("Laser警告號誌")]
    public GameObject laserWarning;
    [Header("Thunder警告號誌")]
    public GameObject ThunderWarning;
    [Header("雷擊")]
    public GameObject Thunder;
    [Header("射擊點")]
    public GameObject shootPoint;
    [Header("HP血條")]
    public Canvas HPbar;
    [Header("移動速度")]
    [Range(0.01f, 0.5f)]
    public float movingSpeed = 0.1f;
    private Rigidbody2D rigidbody2D;
    private UnityEngine.Animator anim;
    [Header("當前血量")]
    [Range(.01f, 100f)]
    public float HealthPoint = 100f;
    private float maxHealthPoint;
    private float[] shootRate = {0.5f, 0.05f};
    private float nextShootTime = 0.0f, preserveShootAngle = 0f;
    private float[] meleeAttackRate = {0.2f, 0.2f, 0.5f};
    private float nextMeleeAttackTime = 0f;

    private int attackCount = 0;
    private bool spillFlag = true, isAttacking = false, allowChangeDir = true, firstTry = true, castingOver = false;
    private float jumpAttackRNG = 0.4f, nextJumpAttackTime = 0f;
    private float nextResetBehaviorTimer = 0f;
    private int actionMode = 0;
    private int castingMode = 0;
    private int bloodMode = 0;

    void Awake()
    {
        this.anim = GetComponent<Animator>();
        this.HealthPoint = 100f;
        this.maxHealthPoint = 100f;
        this.rigidbody2D = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        this.actionMode = -1;
    }

    // Update is called once per frame
    void Update()
    {
        this.faceToPlayer();
        if(Time.time >= this.nextResetBehaviorTimer)
        {
            if(this.actionMode < 5 || this.castingOver)
            {
                this.resetBehaviorVar();
                this.nextResetBehaviorTimer = Time.time + UnityEngine.Random.Range(4f, 5f);   
            }      
        }

        // this.actionMode = 5;
        if(this.actionMode < 2) this.jumpAwayJudge();
        this.normalAttack(this.actionMode);
        // this.OnEnemyMove?.Invoke(this, EventArgs.Empty);
    }

    private bool moveToPlayer()
    {
        // if player don't act with enemy:
        // if(Time.time < this.nextMovingTime) return false;
        if(this.isInAttackRange() || this.isAttacking) return true;
        // this.nextMovingTime = Time.time + UnityEngine.Random.Range(2.5f, 4f);
        
        anim.SetBool("isRun", true);
        if(this.player.transform.position.x - this.gameObject.transform.position.x > 0)
        {
            this.gameObject.transform.position += new Vector3(this.movingSpeed, 0);
            this.gameObject.transform.rotation = new Quaternion(0, 0, 0, 0);
        }
        else
        {
            this.gameObject.transform.position += new Vector3(-this.movingSpeed, 0);
            this.gameObject.transform.rotation = new Quaternion(0, 180, 0, 0);
        }

        return false;
    }
    private IEnumerator moveToCertainPos(int mode)
    {
        if(mode == 0)
        {
            this.rigidbody2D.gravityScale = 0f;
            Vector3 dest = new Vector3(0f, 1f, 0f);
            Vector3 oriPos = this.gameObject.transform.position;
            while((dest - this.gameObject.transform.position).magnitude > .1f)
            {
                Vector3 dir = Vector3.Scale((dest - oriPos), new Vector3(.01f, .01f, .01f));
                this.gameObject.transform.position = this.gameObject.transform.position + dir;

                yield return new WaitForSeconds(0.8f);
            }
            this.gameObject.transform.position = dest;
            yield return new WaitForSeconds(2f);
            // this.readyToCast = true;
            if(this.castingMode < 1) this.castingMode = 1;
        }
        else
        {
            this.rigidbody2D.gravityScale = 5f;
            yield return new WaitForSeconds(1f);
            anim.SetBool("isJump", false);
            anim.SetBool("isDown", true);
            yield return new WaitForSeconds(6f);
            anim.SetBool("isDown", false);
            yield return new WaitForSeconds(1f);
            anim.SetTrigger("idle");
            this.castingMode = 4;   
            while(this.actionMode == 3)
                this.actionMode = Mathf.FloorToInt(UnityEngine.Random.Range(0, MAX_ACTION_MODE));
            this.castingOver = true;
            Debug.Log("casting over");
        }
    }
    private IEnumerator castAttack_Circle()
    {
        Vector3 sampleDirection = new Vector3(1, 0, 0);
        float shootAngle = 0f;
        bool reverseFlag = false;
        const int ATTACK_COUNT = 50;
        int currentAttackCount = ATTACK_COUNT / 2;

        // this.isCasting = true;

        for(int i = 0;i < currentAttackCount;i++)
        {
            if(!reverseFlag && i > currentAttackCount / 2) reverseFlag = true;
            for(float j = 0;j < 360f; j += 22.5f)
            {
                this.Shoot(sampleDirection, shootAngle + j, 10f);
            }
            if(reverseFlag)
                shootAngle += 10f;
            else
                shootAngle -= 10f;

            yield return new WaitForSeconds(.5f);
        }

        currentAttackCount = ATTACK_COUNT;
        for(int i = 0;i < currentAttackCount;i++)
        {
            if(!reverseFlag && i > currentAttackCount / 2) reverseFlag = true;
            for(float j = 0;j < 360f; j += 22.5f)
            {
                this.Shoot(sampleDirection, shootAngle + j, 10f);
            }
            if(reverseFlag)
                shootAngle += 10f;
            else
                shootAngle -= 10f;

            yield return new WaitForSeconds(.2f);
        }

        // this.isCastingOver = true;
        this.castingMode = 3;
    }
    private IEnumerator castAttack_Spill()
    {
        Vector3 sampleDirection = new Vector3(1, 0, 0), oriShootPointPos = this.shootPoint.transform.position;
        float shootAngle = 0f;
        bool reverseFlag = false;
        const int ATTACK_COUNT = 250;
        const float X_RANGE = 22f;

        for(int i = 0;i < ATTACK_COUNT; i++)
        {
            if(Mathf.Abs(this.shootPoint.transform.position.x) > X_RANGE)
            {
                if(!reverseFlag)
                    this.shootPoint.transform.position = new Vector3(21f, 0f, 0f);
                else
                    this.shootPoint.transform.position = new Vector3(-21f, 0f, 0f);
                reverseFlag = !reverseFlag;
            }

            if(reverseFlag)
                this.shootPoint.transform.position = this.shootPoint.transform.position - new Vector3(UnityEngine.Random.Range(3f, 5f), 0f, 0f);
            else
                this.shootPoint.transform.position = this.shootPoint.transform.position + new Vector3(UnityEngine.Random.Range(3f, 5f), 0f, 0f);

            for(int j = 0;j < Mathf.Floor(UnityEngine.Random.Range(2f, 3f)); j++)
            {
                this.Shoot(sampleDirection, UnityEngine.Random.Range(75f, 105f), UnityEngine.Random.Range(10f, 20f), true);
                yield return new WaitForSeconds(.005f);
            }
            yield return new WaitForSeconds(.015f);
        }

        this.shootPoint.transform.position = oriShootPointPos;
        this.castingMode = 3;
    }

    private IEnumerator laserAttack_Deal()
    {
        Vector3 sampleDirection = new Vector3(1, 0, 0);
        bool reverseFlag = false;
        const int ATTACK_COUNT = 3;
        GameObject temp;

        anim.SetBool("isLookUp", true);
        for(int i = 0;i < ATTACK_COUNT; i++)
        {
            reverseFlag = UnityEngine.Random.Range(0f, 1f) < 0.5 ? true : false;
            int shootLevel = Mathf.FloorToInt(UnityEngine.Random.Range(0f, 3f));

            temp = Instantiate(this.laserWarning, new Vector3(0, -7f + (shootLevel*2.5f), 0f), new Quaternion(0, 0, 0, 0));
            Destroy(temp, .5f);
            yield return new WaitForSeconds(.5f);

            if(reverseFlag)
                this.laserShoot(new Vector3(-44f, -11f + (shootLevel*2.5f), 0f), sampleDirection, 0f, 120f);
            else
                this.laserShoot(new Vector3(44f, -11f + (shootLevel*2.5f), 0f), -sampleDirection, 0f, 120f);

            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(3f);
        this.castingMode = 2;
        anim.SetBool("isLookUp", false);
    }
    private IEnumerator thunderAttack_Deal()
    {
        Vector3 sampleDirection = new Vector3(1, 0, 0);
        const int ATTACK_COUNT = 5;
        GameObject temp;

        anim.SetBool("isLookUp", true);
        for(int i = 0;i < ATTACK_COUNT; i++)
        {
            Vector3 playerPos = new Vector3(this.player.transform.position.x, this.player.transform.position.y + 12f, this.player.transform.position.z);
            temp = Instantiate(this.ThunderWarning, playerPos, new Quaternion(0, 0, 0, 0));
            Destroy(temp, .6f);
            yield return new WaitForSeconds(.2f);

            playerPos = new Vector3(playerPos.x, playerPos.y - 2f, playerPos.z);
            StartCoroutine(thunderAttack_Landing(playerPos));
            
        }
        yield return new WaitForSeconds(.5f);
        anim.SetBool("isLookUp", false);
        this.castingMode = 2;
    }
    private IEnumerator thunderAttack_Landing(Vector3 pos)
    {
        yield return new WaitForSeconds(.4f);
        GameObject temp = Instantiate(this.Thunder, pos, new Quaternion(0, 0, 0, 0));
        Destroy(temp, .5f);
    }
    private void jumpAwayJudge()
    {
        if(this.DistanceBetweenPlayer() >= 10f || Time.time < this.nextJumpAttackTime)
            return;

        if(UnityEngine.Random.Range(0f, 1f) < this.jumpAttackRNG)
        {
            this.jumpawayFromPlayer();

            this.preserveShootAngle = Vector3.Angle(this.gameObject.transform.position, this.player.transform.position);
            this.actionMode = 1;
            this.attackCount = 0;

            this.nextJumpAttackTime = Time.time + JUMP_ATTACK_RATE;
        }
        else
        {
            this.nextJumpAttackTime = Time.time + (JUMP_ATTACK_RATE / 2);
        }
    }

    private void normalAttack(int mode)
    {   
        if(this.attackCount >= this.LIMITED_ATTACK_COUNT_BY_MODE[mode]) return;
        bool isAttack = false;
        switch(mode)
        {
            case 0:
                isAttack = sectorAttack();
                break;
            case 1:
                isAttack = spillAttack();
                break;
            case 2:
                isAttack = meleeAttack();
                break;
            case 3:
                isAttack = thunderAttack();
                break;
            case 4:
                isAttack = laserAttack();
                break;
            case 5:
                isAttack = castAttack(0);
                break;
            case 6:
                isAttack = castAttack(1);
                break;
            default:
                break;
        }
        if(isAttack) this.attackCount++;
    }
    private bool sectorAttack()
    {
        if(Time.time < this.nextShootTime) return false;
        this.nextShootTime = Time.time + this.shootRate[0];

        anim.SetTrigger("shoot");
        Vector3 sampleDirection = new Vector3(this.player.transform.position.x - this.attackbox.transform.position.x, 0f, 0f);
        for(int i = -20; i <= 20; i+= 20)
            this.Shoot(sampleDirection, i + UnityEngine.Random.Range(-5f, 5f));
        return true;
    }
    private bool spillAttack()
    {
        if(Time.time < this.nextShootTime) return false;
        this.nextShootTime = Time.time + this.shootRate[1];

        anim.SetTrigger("shoot");
        const float SHOOT_ANGLE = 20f, LIMITED_ANGLE = 60f;

        Vector3 sampleDirection = new Vector3(this.player.transform.position.x - this.attackbox.transform.position.x, 0f, 0f);

        this.Shoot(sampleDirection, this.preserveShootAngle + UnityEngine.Random.Range(-2.5f, 2.5f), 15f);
        this.preserveShootAngle += (this.spillFlag ? SHOOT_ANGLE : -SHOOT_ANGLE);
        if(this.preserveShootAngle >= LIMITED_ANGLE || this.preserveShootAngle <= -LIMITED_ANGLE) this.spillFlag = !this.spillFlag;
        return true;
    }
    private bool meleeAttack()
    {
        bool isInRange = this.moveToPlayer();
        if(!isInRange) return false;

        if(Time.time < this.nextMeleeAttackTime) return false;
        this.nextMeleeAttackTime = Time.time + this.meleeAttackRate[this.attackCount];

        anim.SetBool("isRun", false);
        anim.SetTrigger("attack");
        this.isAttacking = true;

        List<Collider2D> touchingEnemyBox = this.attackbox.GetComponent<AttackBoxBehaviour>().GetTouchingEnemyBox();
        foreach(Collider2D ele in touchingEnemyBox)
        {
            if(ele.gameObject.tag != "Player" || ele == null) continue;

            ele.GetComponentInParent<PlayerBehaviour>().takeDamage(-10f);
        }
        return true;
    }
    private bool castAttack(int mode)
    {
        this.allowChangeDir = false;
        // int castMethod = Mathf.FloorToInt(UnityEngine.Random.Range(0, 2));
        int castMethod = mode;
        anim.SetBool("isJump", true);
        // this.gameObject.transform.position = new Vector3(0f, 1f, 0f);
        StartCoroutine(this.moveToCertainPos(0));
        if(this.castingMode < 1)
            return false;
        
        if(this.castingMode == 1)
        {
            this.castingMode = 2;

            if(castMethod == 0)
                StartCoroutine(this.castAttack_Circle());
            else
                StartCoroutine(this.castAttack_Spill());
        }
        if(this.castingMode < 3)
            return false;
        
        StartCoroutine(this.moveToCertainPos(1));
        if(this.castingMode < 3)
            return false;
        
        return true;
    }
    private bool laserAttack()
    {
        if(this.castingMode == 0)
        {
            this.castingMode = 1;
            StartCoroutine(laserAttack_Deal());
        }
        if(this.castingMode < 2)
            return false;
        return true;
    }
    private bool thunderAttack()
    {
        if(this.castingMode == 0)
        {
            this.castingMode = 1;
            StartCoroutine(this.thunderAttack_Deal());
        }
        if(this.castingMode < 2)
            return false;
        return true;
    }
    private void Shoot(Vector3 direction, float detAngle)
    {
        Vector3 shootVector = direction.normalized;
        shootVector = Quaternion.AngleAxis(detAngle, Vector3.forward) * shootVector;
        // Debug.Log(shootVector);
        GameObject newObject = Instantiate(this.bullet, this.shootPoint.transform.position, Quaternion.Euler(0, 0, detAngle));
        newObject.GetComponent<BulletBehaviour>().setShootVector(shootVector);
    }
    private void Shoot(Vector3 direction, float detAngle, float bulletSpeed)
    {
        Vector3 shootVector = direction.normalized;
        shootVector = Quaternion.AngleAxis(detAngle, Vector3.forward) * shootVector;
        // Debug.Log(shootVector);
        GameObject newObject = Instantiate(this.bullet, this.shootPoint.transform.position, Quaternion.Euler(0, 0, detAngle));
        newObject.GetComponent<BulletBehaviour>().setShootVector(shootVector);
        newObject.GetComponent<BulletBehaviour>().bulletSpeed = bulletSpeed;
    }
    private void Shoot(Vector3 direction, float detAngle, float bulletSpeed, bool isGravity)
    {
        Vector3 shootVector = direction.normalized;
        shootVector = Quaternion.AngleAxis(detAngle, Vector3.forward) * shootVector;
        // Debug.Log(shootVector);
        GameObject newObject = Instantiate(this.bullet, this.shootPoint.transform.position, Quaternion.Euler(0, 0, detAngle));
        newObject.GetComponent<BulletBehaviour>().setShootVector(shootVector);
        newObject.GetComponent<BulletBehaviour>().bulletSpeed = bulletSpeed;

        if(isGravity) newObject.GetComponent<Rigidbody2D>().gravityScale = 1f;
    }
    private void laserShoot(Vector3 OriPos, Vector3 direction, float detAngle, float bulletSpeed)
    {
        Vector3 shootVector = direction.normalized;
        shootVector = Quaternion.AngleAxis(detAngle, Vector3.forward) * shootVector;
        // Debug.Log(shootVector);
        GameObject newObject = Instantiate(this.laserBullet, OriPos, Quaternion.Euler(0, 0, detAngle));
        newObject.GetComponent<BulletBehaviour>().setShootVector(shootVector);
        newObject.GetComponent<BulletBehaviour>().bulletSpeed = bulletSpeed;
    }

    private void jumpawayFromPlayer()
    {
        if(!isGrounded()) return;

        Vector2 dir;
        if(this.player.transform.position.x - this.gameObject.transform.position.x > 0) dir = Vector2.left;
        else dir = Vector2.right;
        this.rigidbody2D.velocity = (Vector2.up * 30f) + (dir * 5f);
    }
    private void faceToPlayer()
    {
        if(!this.allowChangeDir) return;

        if(this.player.transform.position.x - this.gameObject.transform.position.x > 0)
            this.gameObject.transform.rotation = new Quaternion(0, 0, 0 , 0);
        else
            this.gameObject.transform.rotation = new Quaternion(0, 180, 0 , 0);
    }
    private void resetBehaviorVar()
    {
        if(this.firstTry && actionMode < 5)
        {
            actionMode++;
        }
        if(this.firstTry && actionMode >= 5)
            this.firstTry = false;
        if(!this.firstTry)
            this.actionMode = Mathf.FloorToInt(UnityEngine.Random.Range(0, MAX_ACTION_MODE));

        this.castingOver = false;
        this.allowChangeDir = true;
        this.castingMode = 0;
        this.preserveShootAngle = 0f;
        this.isAttacking = false;
        this.attackCount = 0;
        if(this.bloodMode == 0 && this.HealthPoint < (this.maxHealthPoint / 3) * 2)
        {
            this.actionMode = 5;
            this.bloodMode++;
            Debug.Log("BloodMode 1");
        }
        if(this.bloodMode == 1 && this.HealthPoint < (this.maxHealthPoint / 3))
        {
            this.actionMode = 6;
            this.bloodMode++;
            Debug.Log("BloodMode 2");
        }
        Debug.Log("Action mode: " + this.actionMode);
    }
    private bool isGrounded()
    {
        if(rigidbody2D.velocity.y == 0)
        {
            return true;
        }
        else
            return false;
    }
    private bool isInAttackRange()
    {
        if((this.gameObject.transform.position - this.player.transform.position).magnitude < 4.5f)
        {
            // Debug.Log("OK");
            return true;
        }
        else
            return false;
    }
    private float DistanceBetweenPlayer()
    {
        // Debug.Log(this.gameObject.transform.position - this.player.transform.position);
        return (this.gameObject.transform.position - this.player.transform.position).magnitude;
    }
    public void takeDamage(float damage)
    {
        this.HealthPoint += damage;
        Debug.Log("Hit.");
        Debug.Log(this.HealthPoint);

        UnityEngine.UI.Slider slider = this.HPbar.GetComponentsInChildren<CanvasRenderer>()[3].GetComponent<UnityEngine.UI.Slider>();

        slider.value = this.HealthPoint / this.maxHealthPoint;

        if(this.HealthPoint <= 0f)
            StartCoroutine(this.Dead());
    }
    private IEnumerator Dead()
    {
        anim.SetTrigger("die");
        yield return new WaitForSeconds(.5f);
        this.menu.GetComponent<MenuScript>().Victory();
    }
}
