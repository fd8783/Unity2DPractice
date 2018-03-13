using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class thiefai : enemyai {

    //use for getting information//
    private Vector2 curpos, targetpos;
    private SpriteRenderer turneffect;
    //***************************//

    //**use for vision**//
    public LayerMask targetlayer;
    //public LayerMask blocklayer;
    private GameObject stagemanger;
    private Vector2 topleftpt, downrightpt , coptopleftpt, copdownrightpt;
    private Collider2D[] visibletarget;
    private int vistarcount, closesttarnum;
    private float closestdis, newdis;
    [SerializeField]
    private bool searching = false;
    private health targethealthscript;
    private bool checkingdeathtarget = false;
    //******************//

    //use for movement//
    private float speed = 1.1f, curweaponweight;
    private Vector2 curspeed,targetspeed;
    private float targetangle;
    private Quaternion startrot, targetrot;
    private Vector2 leftvec = new Vector2(-1,-1) , rightvec = new Vector2(1,-1);
    //****************//

    //use for weapon issues//
    public string[] defaultweapon = { "melee", "knife", "9"};
    public bool abletoattack = false;
    public float reloadtime = 0.5f, nextattacktime = 0f, attackstoptime = 0f;
    private GameObject knife;
    private Rigidbody2D knifeRB;
    //*********************//

    //private int[] curdir = { 0, 0 };

    // Use this for initialization
    new void Awake()
    {
        base.Awake();
        stagemanger = GameObject.Find("stagemanager");
        topleftpt = stagemanger.transform.Find("stagearea/topleftpt").position;
        downrightpt = stagemanger.transform.Find("stagearea/downrightpt").position;
        coptopleftpt = stagemanger.transform.Find("coparea/topleftpt").position;
        copdownrightpt = stagemanger.transform.Find("coparea/downrightpt").position;
        //face = transform.Find("face").gameObject;
        turneffect = transform.Find("UI/turneffect").GetComponent<SpriteRenderer>();
        knife = Resources.Load("dropweapon/knife") as GameObject;

        startrot = transform.rotation;
        curpos = transform.position;
    }

    void Start ()
    {
        // these three need to run in start because some weapon value (i.e. startpos) need to be set in other script's Awake
        updateattackrange(defaultweapon[1]);
        curweaponanim = weapon.GetComponent<enemyweaponlist>().changeweapon(curweaponanim, defaultweapon[0], defaultweapon[1]);
        curweaponweight = int.Parse(defaultweapon[2]);
    }
	
	// Update is called once per frame
	void Update () {
    }

    void FixedUpdate()
    {
        curpos = transform.position;
        speedforanim = 0;
        if (isdeath)
        {
            if (searching)
            {
                Search(false);
            }
        }
        else
        {
            if (gethit || picked)
            {
                if (InRange(body.velocity.x, -0.15f, 0.15f) && InRange(body.velocity.y, -0.15f, 0.15f))
                {
                    gethit = false;
                    anim.SetBool("scared", gethit);

                    if (isstun)
                    {
                        endstuntime = Time.time + curstuntime;
                        stunbar.SetActive(true);
                    }
                }
                else
                {
                    if (searching)
                    {
                        Search(false);
                    }
                }
            }
            else
            {
                if (isstun)
                {
                    if (searching)
                    {
                        Search(false);
                    }
                    if (Time.time <= endstuntime)
                    {
                        curstuntime -= Time.fixedDeltaTime;
                    }
                    else
                    {
                        isstun = false;
                        stunbar.SetActive(false);
                        Search(true);
                    }
                }
                else
                {
                    if (!searching)
                    {
                        if (closesttarget == null)
                        {
                            Search(true);
                        }
                        else
                        {
                            if (Time.time > attackstoptime)
                            {
                                Move();
                            }
                            CheckTargetInRange(); //update bool abletoattack
                            if (abletoattack)
                            {
                                Attack();
                            }
                        }
                    }
                }
                Rotate(); //rotate when don't get hit (not flying around)
            }
        }

        anim.SetFloat("speed", speedforanim);
    }

    void Move()
    {
        targetpos = closesttarget.position;
        targetspeed = (targetpos - curpos).normalized;
        checkdir(targetspeed); //function from parent script to filp

        targetangle = Mathf.Atan2(targetspeed.y, targetspeed.x) * Mathf.Rad2Deg; // use to rotate weapon
        targetrot = Quaternion.Euler(new Vector3(0f, 0f, targetangle * cursca.x + (cursca.x > 0 ? 0 : 180)));
        weapon.localRotation = Quaternion.Lerp(weapon.localRotation, targetrot, 0.2f * Time.deltaTime * 60);

        targetspeed = targetspeed * speed * (10 / curweaponweight);
        curspeed = body.velocity;
        curspeed.x = Mathf.Lerp(curspeed.x, targetspeed.x, 0.2f);
        curspeed.y = Mathf.Lerp(curspeed.y, targetspeed.y, 0.2f);

        body.velocity = curspeed;
        speedforanim = Mathf.Max(Mathf.Abs(curspeed.x), Mathf.Abs(curspeed.y));
        
    }

    void Rotate()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, startrot, 0.2f * Time.deltaTime * 60);
    }

    void FindTarget()
    {
        visibletarget = Physics2D.OverlapAreaAll(topleftpt, downrightpt, targetlayer);
        vistarcount = visibletarget.Length;

        if (vistarcount > 0)
        {
            closestdis = Vector2.Distance(curpos, visibletarget[0].transform.position);
            closesttarnum = 0;
            closesttarget = visibletarget[0].transform;
            for (int i = 1; i < vistarcount; i++)
            {
                newdis = Vector2.Distance(curpos, visibletarget[i].transform.position);
                if (newdis < closestdis)
                {
                    closestdis = newdis;
                    closesttarnum = i;
                }
            }
            closesttarget = visibletarget[closesttarnum].transform;
            targethealthscript = closesttarget.GetComponent<health>();
        }
        else
        {
            Debug.Log("No Target Here");
        }
    }

    void CheckTargetInRange()
    {
        if (closesttarget.CompareTag("normal") && closesttarget.gameObject.layer == LayerMask.NameToLayer("Deadbody") ||  (closesttarget.CompareTag("Player") && closesttarget.gameObject.layer == LayerMask.NameToLayer("Interactable")))
        {
            abletoattack = false;
            Search(true);
        }
        else
        {
            if (Vector2.Distance(weapon.position, closesttarget.position) < attackrange)
            {
                abletoattack = true;
            }
            else
            {
                abletoattack = false;
            }
        }
        
    }

    void Attack()
    {
        if (Time.time >= nextattacktime)
        {
            nextattacktime = Time.time + reloadtime;
            attackstoptime = Time.time + reloadtime;
            anim.SetTrigger("attack");
        }
    }

    void Search(bool start) //seems searching can be use like isstop
    {
        StopCoroutine("turning");
        StopCoroutine("waitforsec");
        if (start)
        {
            searching = true;
            StartCoroutine("turning");
            StartCoroutine("waitforsec", 2.2f);
        }
        else
        {
            searching = false;
        }
    }

    override public void Scared(bool isscared)
    {

    }

    override public void CheckWillScared(float damage, Vector2 hitvel)
    {

    }

    override public void Interact(Transform player)
    {

    }

    override public void DropWeapon()
    {
        hand.gameObject.SetActive(true);
        weapon.gameObject.SetActive(false);
        knifeRB = Instantiate(knife, weapon.transform.position, Quaternion.identity).GetComponent<Rigidbody2D>();
        knifeRB.velocity = body.velocity ;
    }

    IEnumerator waitforsec(float time)
    {
        yield return new WaitForSeconds(time);
        searching = false;
        FindTarget();
    }

    IEnumerator turning()
    {
        while (searching)
        {
            yield return new WaitForSeconds(0.3f);
            if (searching)
            {
                if (transform.localScale.x > 0)
                {
                    checkdir(leftvec);
                }
                else
                {
                    checkdir(rightvec);
                }
                turneffect.enabled = true;
                yield return new WaitForSeconds(0.1f);
                turneffect.enabled = false;
                yield return new WaitForSeconds(0.4f);
            }
        }
    }
}



/*
 
    IEnumerator CheckIsTargetDeath()
    {
        yield return new WaitForSeconds(0.5f);
        if (closesttarget.gameObject.layer == LayerMask.NameToLayer("Deadbody"))
        {
            Search(true);
        }
        if (targethealthscript.IsDeath())
        {
        }
        checkingdeathtarget = false;
    }

*/