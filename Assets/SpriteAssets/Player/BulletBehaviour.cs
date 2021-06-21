using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    [Header("攻擊對象")]
    public string receiver;
    [Header("攻擊力")]
    public float attackPower =  0.1f;
    [Header("飛行速度")]
    public float bulletSpeed = 10f;
    [Header("是否擊中消失")]
    public bool isVanish = true;
    [Header("是否有rigidbody")]
    public bool ifRigidbody = true;
    // Start is called before the first frame update
    private Vector2 shootVector = Vector2.right;
    private Rigidbody2D rigidbody2D;

    void Awake()
    {
        // Debug.Log("Awake");
        // Debug.Log(this.shootVector);
        if(ifRigidbody)
            rigidbody2D = GetComponent<Rigidbody2D>();
    }
    void Start()
    {
        // Debug.Log("Start");
        // Debug.Log(this.shootVector);
        if(ifRigidbody)
        {
            rigidbody2D.velocity = this.shootVector * bulletSpeed;
            Destroy(this.gameObject, 5f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void setShootVector(Vector2 v)
    {
        // Debug.Log("Set");
        this.shootVector = v;
        // Debug.Log(this.shootVector);
    }

    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        if(collider2D.tag != this.receiver && collider2D.tag != "Shield") return;

        if(collider2D.tag == "Enemy")
            collider2D.GetComponent<EnemyBehaviour>().takeDamage(-this.attackPower);
        else if(collider2D.tag == "Player")
            collider2D.GetComponentInParent<PlayerBehaviour>().takeDamage(-this.attackPower);
        else if(collider2D.tag == "Shield")
        {
            Debug.Log("Shield!");
            if(this.isVanish) Destroy(this.gameObject);
        }
        if(this.isVanish) Destroy(this.gameObject);
    }
}
