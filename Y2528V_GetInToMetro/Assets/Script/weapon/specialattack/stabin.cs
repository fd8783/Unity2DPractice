using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stabin : MonoBehaviour {

    private LayerMask targetlayer = (1 << 8) | (1 << 9) | (1 << 10) | (1 << 14) | (1 << 16) | (1 << 17); //normal,enemy,player,bodyfragment,deadbody
    private RaycastHit2D hitpt;
    private Transform hittarget;
    private Collider2D[] allcollider; //all BOX collider
    private int colcount;
    private Collider2D hitcollider;
    private TrailRenderer trail;
    private bool hitsth, throwout;
    private Rigidbody2D body;
    private Transform img;
    private GameObject bloodsprayeffect; //, tempbloodspray;
    //private ParticleSystem bloodspray;
    //private ParticleSystem.ShapeModule bloodsprayshape;
    

    // Use this for initialization
    void Awake () {
        allcollider = GetComponents<BoxCollider2D>();
        colcount = allcollider.Length;
        trail = transform.Find("Trail").GetComponent<TrailRenderer>();
        for (int i = 0; i < colcount; i++)
        {
            if (allcollider[i].isTrigger)
            {
                hitcollider = allcollider[i];
                break;
            }
        }
        body = GetComponent<Rigidbody2D>();
        img = transform.Find("img");
        bloodsprayeffect = Resources.Load("effect/BloodSpray") as GameObject;

	}
	
	// Update is called once per frame
	void Update () {
        //Debug.DrawRay(transform.position, transform.up, Color.blue);
        if (throwout)
        {
            if (Vector2.Distance(body.velocity, Vector2.zero) < 0.05f)
            {
                hitcollider.enabled = false;
                trail.enabled = false;
                throwout = false;
            }
        }
	}

    public void Settle(Vector2 vel)
    {
        hitcollider.enabled = true;
        trail.enabled = true;
        hitsth = false;
        body.velocity = vel;
        throwout = true;
    }

    IEnumerator DestoryCount(float time)
    {
        yield return new WaitForSeconds(time);
        if (!hitsth)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("normal") || col.CompareTag("Player") || col.CompareTag("normal enemy") || col.CompareTag("bodyfragment"))
        {
            hitpt = Physics2D.Raycast(transform.position, transform.up, 0.5f, targetlayer);
            if (hitpt)
            {
                /*body.isKinematic = true;
                body.velocity = Vector2.zero;
                screenctrl.ShakeScreen(0.05f);
                hittarget = hitpt.transform;
                img.parent = hittarget;
                transform.position = hitpt.point;
                trail.enabled = false;
                for ()
                hitcollider.enabled = false;
                hitsth = true;
                throwout = false;
                hittarget.GetComponent<baseai>().hit(body.velocity.normalized);
                hittarget.GetComponent<health>().Hurt(50f);*/
                hittarget = hitpt.transform;
                transform.position = hitpt.point;
                img.parent = hittarget;
                Instantiate(bloodsprayeffect, hitpt.point, transform.rotation, img);
                //tempbloodspray.transform.localRotation = Quaternion.identity;
                if (!col.CompareTag("bodyfragment"))
                    hittarget.GetComponent<baseai>().hit(body.velocity.normalized);
                hittarget.GetComponent<health>().Hurt(50f);
                Destroy(gameObject);
            }
        }
    }
}
