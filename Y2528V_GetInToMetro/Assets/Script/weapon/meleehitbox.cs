using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class meleehitbox : MonoBehaviour {

    public float hitcoefficient = 1f;
    public float damagecoefficient = 1f;
    public float basepower = 1f, maxhitcharge = 5f; // full charge = 1+5(100/100) = 6
    public float basedamage = 5f, maxdamagecharge = 20f;

    private Transform parent;
    private movementctrl parentscript;
    private Vector2 mousedir, hitvel;
    private Transform hitbody;
    private normalai normalscript;
    private enemyai enemyscript;
    private ableincar incarscript;
    private float charge, damage, targetweight;

    //***** use to stop game *****//
    private float pauseEndTime, stopinterval = 1f; // useless now
    //****************************//

    // Use this for initialization
    void Start () {
        parent = transform.root.root.root;
        parentscript = parent.GetComponent<movementctrl>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}



    void OnTriggerEnter2D(Collider2D col)
    {
        mousedir = parentscript.getmousedir();
        charge = parentscript.getcharging();

        if (col.CompareTag("normal"))
        {
            hitbody = col.transform;
            normalscript = hitbody.GetComponent<normalai>();
            if (normalscript.checkincar())
            {
                normalscript.hitincar(hitcoefficient * 1.5f * charge * (1 - normalscript.checkslowpercent()));
            }
            else
            {
                hitvel = hitcoefficient * mousedir * (basepower + (maxhitcharge * (charge / 100)));
                normalscript.hit(hitvel);
            }

            hitbody.GetComponent<health>().Hurt(damagecoefficient * (basedamage + (maxdamagecharge * (charge / 100))));
            if (charge == 100)
            {
                screenctrl.StopScreen(0.05f);
            }
            return;
        }

        if (col.CompareTag("normal enemy"))
        {
            hitbody = col.transform;

            damage = damagecoefficient * (basedamage + (maxdamagecharge * (charge / 100)));
            hitbody.GetComponent<health>().Hurt(damage);

            incarscript = hitbody.GetComponent<ableincar>();
            if (incarscript == null)
            {
                enemyscript = hitbody.GetComponent<enemyai>();
                hitvel = hitcoefficient * mousedir * (basepower + (maxhitcharge * (charge / 100)));
                enemyscript.hit(hitvel, parent);
                enemyscript.CheckWillScared(damage, hitvel);
            }
            else
            {
                if (incarscript.checkincar())
                {
                    incarscript.hitincar(hitcoefficient * 1.5f * charge * (1 - incarscript.checkslowpercent()));
                }
                else
                {
                    enemyscript = hitbody.GetComponent<enemyai>();
                    hitvel = hitcoefficient * mousedir * (basepower + (maxhitcharge * (charge / 100)));
                    enemyscript.hit(hitvel, parent);
                    enemyscript.CheckWillScared(damage, hitvel);
                }
            }


            
            if (charge == 100)
            {
                screenctrl.StopScreen(0.05f);
            }
            return;
        }

        if (col.CompareTag("bodyfragment"))
        {
            hitvel = hitcoefficient * 1.5f * mousedir * (basepower + (maxhitcharge * (charge / 100)));
            col.GetComponent<Rigidbody2D>().velocity = hitvel;
            return;
        }

        if (col.CompareTag("chest") || col.CompareTag("dropweapon"))
        {
            targetweight = col.GetComponent<dropweapon>().getweight();
            hitvel = hitcoefficient * 1.5f * mousedir * (basepower + (maxhitcharge * (charge / 100))) * (10/targetweight);
            col.GetComponent<Rigidbody2D>().velocity = hitvel;
            return;
        }
    }




    //******** Not updating now ************//
    /*void OnCollisionEnter2D(Collision2D col)  // maybe not good for the melee weapon to use collision2D
    {
        if (col.gameObject.CompareTag("normal"))
        {
            hitbody = col.transform.GetComponent<Rigidbody2D>();
            mousedir = parentscript.getmousedir();
            if (hitbody.GetComponent<normalai>().checkincar())
            {
                hitbody.GetComponent<normalai>().hitincar(hitcoefficient * parentscript.getcharging() * (1 - hitbody.GetComponent<normalai>().checkslowpercent()));
            }
            else
            {
                hitbody.GetComponent<normalai>().hit();
                hitvel = hitcoefficient * mousedir * (basepower + (hitpower * (parentscript.getcharging() / 100)));
                hitbody.velocity = hitvel;
            }
        }
    }
        IEnumerator pasuegame(float time)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(time);
        Time.timeScale = 1f;
        Debug.Log("back");
    }

    IEnumerator stoprecover(float time)
    {
        yield return new WaitForSecondsRealtime(time);
        stoped = false;
        Debug.Log("backed");
    }
     
     
     */
}
