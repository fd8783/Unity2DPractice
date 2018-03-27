using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class feather : MonoBehaviour {

    [SerializeField]
    private Vector2[] WayPoint = new Vector2[5];
    [SerializeField]
    private Vector2 fixwaypt;

    private Quaternion currot, targetrot;
    private Vector2 curpos, targetdir;
    //private Rigidbody2D body;
    private float speed = 0.2f, targetangle;
    private int ptcount, curpt, rotatecount;
    private bool settled = false, topoint, toend, fired, hitsth;
    private Transform target;
    private LayerMask targetlayer = (1 << 8) | (1 << 9) | (1 << 10) | (1 << 14) | (1 << 16) | (1 << 17); //normal,enemy,player,bodyfragment,deadbody, interactable
    private RaycastHit2D hitpt;
    private Transform hittarget;
    private SpriteRenderer featherimg;
    private BoxCollider2D hitcollider;
    private TrailRenderer trail;
    private GameObject bloodsprayeffect;

    private Vector2 contactpt;
	// Use this for initialization
    void Awake()
    {
        featherimg = transform.Find("img").GetComponent<SpriteRenderer>();
        hitcollider = GetComponent<BoxCollider2D>();
        WayPoint = new Vector2[5];
        trail = transform.Find("Trail").GetComponent<TrailRenderer>();
        bloodsprayeffect = Resources.Load("effect/BloodSpray") as GameObject;
    }

	void Start ()
    {

    }
	
	// Update is called once per frame
	void FixedUpdate () {
        curpos = transform.localPosition;
		if (settled)
        {
            if (fired)
            {
                FiredMove();
            }
            else
            {
                if (toend)
                {
                    Ready();
                }
                else
                {
                    Move();
                }
            }
        }
	}

    void Move()
    {
        if (Vector2.Distance(curpos, (curpt == 4 ? fixwaypt : WayPoint[curpt])) < 0.03f)
        {
            if (curpt >= (ptcount - 1))
            {
                toend = true;
                //Debug.Log(" " + curpt);
            }
            else
            {
                curpt++;
                //Debug.Log(Time.time + " " + curpt);
            }
        }
        else //Move
        {
            if (curpt == 4)
            {
                targetdir = (fixwaypt - curpos).normalized;

                curpos = Vector2.Lerp(curpos, fixwaypt, 0.1f);
            }
            else
            {
                targetdir = (WayPoint[curpt] - curpos).normalized;

                curpos = Vector2.Lerp(curpos, WayPoint[curpt], 0.1f);
            }
            transform.localPosition = curpos;

            Rotate();
        }
    }

    void Rotate()
    {
        targetangle = Mathf.Atan2(targetdir.y, targetdir.x) * Mathf.Rad2Deg; // use to rotate weapon
        targetrot = Quaternion.Euler(new Vector3(0f, 0f, targetangle - 90));// * cursca.x + (cursca.x > 0 ? 0 : 180)
        transform.rotation = Quaternion.Lerp(transform.rotation, targetrot, 0.4f * Time.deltaTime * 60);
    }

    void Ready()
    {
        targetdir = (target.position - transform.position).normalized;
        Rotate();
    }

    public void Settle(Vector2[] pointlist, Transform target)
    {
        ptcount = pointlist.Length;
        WayPoint = pointlist;
        //Debug.Log(WayPoint[4].x+" "+ WayPoint[4].y);
        fixwaypt = WayPoint[4];
        this.target = target;
        settled = true;
    }

    public void Fire()
    {
        targetdir = (target.position - transform.position).normalized;
        Rotate();
        fired = true;
        hitcollider.enabled = true;
        transform.parent = null;
        StartCoroutine("DestoryCount",3f);
    }

    void FiredMove()
    {
        curpos = curpos + targetdir * 0.2f;
        transform.localPosition = curpos;
    }

    IEnumerator DestoryCount(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("normal") || col.CompareTag("Player"))
        {
            hitpt = Physics2D.Raycast(transform.position, transform.up, 0.1f, targetlayer);
            if (hitpt)
            {
                screenctrl.ShakeScreen(0.05f);
                hittarget = hitpt.transform;
                transform.position = hitpt.point;
                featherimg.transform.parent = hittarget;
                featherimg.sortingOrder = 6;
                /*settled = false;
                trail.enabled = false;
                hitcollider.enabled = false;
                hitsth = true;*/
                Instantiate(bloodsprayeffect, hitpt.point, transform.rotation, featherimg.transform);
                hittarget.GetComponent<baseai>().hit(targetdir);
                hittarget.GetComponent<health>().Hurt(100f);
                Destroy(gameObject);
            }
        }
    }
}


/*   	void Start ()
    {
        Transform temp;
        temp = transform.parent.Find("featherwaypt");
        WayPoint = new Vector2[temp.childCount];
        ptcount = temp.childCount;
        for (int i =0; i < temp.childCount; i++)
        {
            WayPoint[i] = temp.GetChild(i).localPosition;
        }
        settled = true;

    }
 *   
 *   
 *   
 *    void Move()
    {
        if (Vector2.Distance(curpos, WayPoint[curpt]) < 0.02f)
        {
            if (curpt >= (ptcount - 1))
            {
                toend = true;
            }
            else
            {
                curpt++;
                topoint = true;
                rotatecount = 0;
                targetdir = (WayPoint[curpt] - curpos).normalized;
            }
        }
        else //Move
        {
            targetdir = WayPoint[curpt];

            curpos = Vector2.Lerp(curpos, targetdir, 0.1f);
            transform.localPosition = curpos;
        }
    }

    void Rotate()
    {
        targetangle = Mathf.Atan2(targetdir.y, targetdir.x) * Mathf.Rad2Deg; // use to rotate weapon
        targetrot = Quaternion.Euler(new Vector3(0f, 0f, targetangle - 90));// * cursca.x + (cursca.x > 0 ? 0 : 180)
        transform.rotation = Quaternion.Lerp(transform.rotation, targetrot, 0.4f * Time.deltaTime * 60);
    }


    */
