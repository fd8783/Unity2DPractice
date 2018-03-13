using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemymeleehitbox : MonoBehaviour {
    public float hitcoefficient = 1f;
    public float damagecoefficient = 1f;
    public float basepower = 1f, maxhitcharge = 5f; // full charge = 1+5(100/100) = 6
    public float basedamage = 5f, maxdamagecharge = 20f;

    private Transform parent;
    private enemyai parentscript;
    private Vector2 targetdir, hitvel;
    private Transform hitbody;
    private normalai normalscript;
    private movementctrl playerscript;

    // Use this for initialization
    void Start()
    {
        parent = transform.parent.parent.parent.parent;
        parentscript = parent.GetComponent(typeof(enemyai)) as enemyai;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D col)
    {
        targetdir = parentscript.gettargetdir();

        if (col.CompareTag("normal"))
        {
            hitbody = col.transform;
            normalscript = hitbody.GetComponent<normalai>();
            //if (normalscript.checkincar())
            //{
            //    normalscript.hitincar(hitcoefficient * (1 - hitbody.GetComponent<normalai>().checkslowpercent()));
            //}
            //else {}
            hitvel = hitcoefficient * targetdir * (basepower + (maxhitcharge));
            normalscript.hit(hitvel);

            hitbody.GetComponent<health>().Hurt(damagecoefficient * (basedamage + (maxdamagecharge)));
            screenctrl.StopScreen(0.01f);
            return;
        }

        if (col.CompareTag("Player"))
        {
            hitbody = col.transform;
            playerscript = hitbody.GetComponent<movementctrl>();

            hitvel = hitcoefficient * targetdir * (basepower + (maxhitcharge));
            playerscript.hit(hitvel);

            hitbody.GetComponent<health>().Hurt(damagecoefficient * (basedamage + (maxdamagecharge)));
            screenctrl.StopScreen(0.01f);
            return;
        }
    }
}




/*void OnCollisionEnter2D(Collision2D col)
{
    if (col.gameObject.CompareTag("normal"))
    {
        hitbody = col.transform.GetComponent<Rigidbody2D>();
        mousedir = parentscript.gettargetdir();
        if (hitbody.GetComponent<normalai>().checkincar())
        {
            hitbody.GetComponent<normalai>().hitincar(coefficient * (1 - hitbody.GetComponent<normalai>().checkslowpercent()));
        }
        else
        {
            hitbody.GetComponent<normalai>().hit();
            hitvel = coefficient * mousedir * (basepower + (hitpower));
            hitbody.velocity = hitvel;
        }
    }
}*/
