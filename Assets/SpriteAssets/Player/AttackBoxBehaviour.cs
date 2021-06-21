using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackBoxBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("敵人標籤")]
    public string enemyTag;
    private List<Collider2D> touchingEnemyBox;

    void Awake()
    {
        touchingEnemyBox = new List<Collider2D>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<Collider2D> GetTouchingEnemyBox()
    {
        return this.touchingEnemyBox;
    }

    private void OnTriggerEnter2D(Collider2D collider2D)
    {
        // Debug.Log("Enter!");
        if(collider2D.gameObject.tag == this.enemyTag)
        {
            touchingEnemyBox.Add(collider2D);
        }
    }

    private void OnTriggerExit2D(Collider2D collider2D)
    {
        // Debug.Log("Exit!");
        if(collider2D.gameObject.tag == this.enemyTag)
        {
            touchingEnemyBox.Remove(collider2D);
        }
    }
}
