using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class exhibitionistai : enemyai, ableincar {

    private Vector2 curpos, targetpos;

    //use for movement && settle issue//
    private float speed = 0.2f, curweaponweight;
    private Vector2 curspeed, targetspeed;
    private float targetangle;
    private Quaternion startrot, targetrot;

    private Transform train;
    private Transform[] car;
    private Vector2[] carpos;
    private int carcount;

    private int prefercar;
    private float closestdis;

    //scared run//
    private Vector2 runningright = new Vector2(1, 0);
    private float rightwallptx;
    private Transform detectwallpt;
    //********************************//

    //***for incar***//
    private GameObject cargetin;
    private bool incar = false;
    private float slowpercent, incarpercent = 0f;
    private Vector2 incarcurpos, incartarpos;
    private float movedis = 0.2f;
    //***************//

    //****for change to exhibitionist****//
    private float startptx ,halfptx;
    private bool transforming = false, transformed = false;
    private Vector2 leftvec = new Vector2(-1, -1), rightvec = new Vector2(1, -1);
    private SpriteRenderer turneffect;
    //***********************************//

    // Use this for initialization
    new void Awake()
    {
        base.Awake();

        startrot = transform.rotation;
        curpos = transform.position;

        train = GameObject.Find("train").transform;
        carcount = train.Find("carctrl").childCount;
        car = new Transform[carcount];
        carpos = new Vector2[carcount];
        rightwallptx = GameObject.Find("airwall/rightwall").transform.position.x;
        detectwallpt = transform.Find("detectwall");
        turneffect = transform.Find("UI/turneffect").GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        turnvel = false; //then this character don't do filpvel
        startptx = transform.position.x;
    }

    // Update is called once per frame
    void Update () {
		
	}

    void FixedUpdate()
    {
        curpos = transform.position;
        speedforanim = 0;
        if (incar)
        {
            MoveInCar();
        }
        else
        {
            if (isdeath || picked)
            {
                if (transforming)
                {
                    Search(false);
                }
            }
            else
            {
                if (gethit)
                {
                    if (InRange(body.velocity.x, -0.15f, 0.15f) && InRange(body.velocity.y, -0.15f, 0.15f))
                    {
                        gethit = false;
                        if (!scared)
                        {
                            anim.SetBool("scared", gethit);
                        }

                        if (isstun)
                        {
                            endstuntime = Time.time + curstuntime;
                            stunbar.SetActive(true);
                        }

                        FindClosestCar();
                    }
                    else
                    {
                        if (transforming)
                        {
                            Search(false);
                        }
                    }
                }
                else
                {
                    if (isstun)
                    {
                        if (Time.time <= endstuntime)
                        {
                            curstuntime -= Time.fixedDeltaTime;
                        }
                        else
                        {
                            isstun = false;
                            stunbar.SetActive(false);
                        }
                    }
                    else
                    {
                        if (scared)
                        {
                            ScaredMove();
                        }
                        else
                        {
                            if (settled)
                            {
                                if (!transforming)
                                {
                                    Move();
                                }
                            }
                            else
                            {
                                if (trainctrl.arrived)
                                {
                                    Settle();
                                }
                            }
                        }
                    }
                    Rotate(); //rotate when don't get hit (not flying around)
                }
            }
        }

        anim.SetFloat("speed", speedforanim);
    }

    void Move()
    {
        targetpos = carpos[prefercar];
        targetspeed = (targetpos - curpos).normalized;
        checkdir(targetspeed); //function from parent script to filp

        targetspeed = targetspeed * speed * (transformed? 2 : 1); // * (10 / curweaponweight)
        curspeed = body.velocity;
        curspeed.x = Mathf.Lerp(curspeed.x, targetspeed.x, 0.2f);
        curspeed.y = Mathf.Lerp(curspeed.y, targetspeed.y, 0.2f);

        body.velocity = curspeed;
        speedforanim = Mathf.Max(Mathf.Abs(curspeed.x), Mathf.Abs(curspeed.y));
        if (!transformed)
        {
            if (curpos.x < halfptx)
            {
                Search(true);
            }
        }
    }

    void ScaredMove()
    {
        if (curpos.x < 4)
        {
            if (detectwallpt.position.x > rightwallptx)
            {
                tag = "runningenemy";
                gameObject.layer = 18; //18 is intouchable layer
            }

            targetpos = runningright;
            targetspeed = targetpos.normalized;
            checkdir(targetspeed); //function from parent script to filp

            targetspeed = targetspeed * speed * (transformed ? 2.4f : 1.2f);
            curspeed = body.velocity;
            curspeed.x = Mathf.Lerp(curspeed.x, targetspeed.x, 0.2f);
            curspeed.y = Mathf.Lerp(curspeed.y, targetspeed.y, 0.2f);

            body.velocity = curspeed;
            speedforanim = Mathf.Max(Mathf.Abs(curspeed.x), Mathf.Abs(curspeed.y));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MoveInCar()
    {
        if (!isdeath) // if death, then no auto move
        {
            incarpercent += 2f * (1 - slowpercent);
        }
        incartarpos.x = incarcurpos.x - (movedis * (incarpercent / 100));
        if (incarpercent >= 100f)
        {
            cargetin.GetComponent<carctrl>().getonein(transform, isdeath);
            Destroy(gameObject);
        }
        transform.position = incartarpos;
    }

    void Rotate()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, startrot, 0.2f * Time.deltaTime * 60);
    }

    void FindClosestCar() // finding closest car to get in 
    {
        prefercar = 0;
        closestdis = Vector2.Distance(curpos, carpos[prefercar]);
        for (int i = 1; i < carcount; i++)
        {
            if (Vector2.Distance(curpos, carpos[i]) < closestdis)
            {
                prefercar = i;
                closestdis = Vector2.Distance(curpos, carpos[i]);
            }
        }
    }

    void Settle() //get car's information after train arrived
    {
        for (int i = 0; i < carcount; i++)
        {
            car[i] = train.Find("carctrl").GetChild(i);
            carpos[i] = car[i].position;
        }
        settled = true;
        FindClosestCar();
        halfptx = ((startptx - carpos[prefercar].x) / 2) + carpos[prefercar].x; 
    }

    public void hitincar(float vel)
    {
        incarpercent += vel;
    }


    override public void Scared(bool isscared)
    {
        if (isscared)
        {
            scared = true;
            anim.SetBool("scared", true);

            //able to be pick up
            gameObject.layer = LayerMask.NameToLayer("Weapon");
            //dropweaponscript.enabled = true;
        }
        else
        {
            scared = false;
            anim.SetBool("scared", false);

            gameObject.layer = LayerMask.NameToLayer("Enemy");
            //dropweaponscript.enabled = false;
        }
    }

    override public void CheckWillScared(float damage, Vector2 hitvel)
    {
        if (damage >= 50 || (Mathf.Pow(hitvel.x, 2) + Mathf.Pow(hitvel.y, 2)) > 30) //30 is near 6^2 = 36 which 6 is max power of default weapon brand
        {
            Scared(true);
            anim.SetTrigger("transform");
            transformed = true;
            DropWeapon();
        }
    }

    override public void Interact(Transform player)
    {

    }

    override public void DropWeapon()
    {
        //don't have weapon
    }

    public void getincar(GameObject car, float slowpercent)
    {
        Search(false);
        this.slowpercent = slowpercent / 100;
        incar = true;
        hitincar(Mathf.Abs(body.velocity.x) * 15 * (1 - this.slowpercent));
        //Debug.Log(body.velocity.x + " " + incarpercent);
        body.bodyType = RigidbodyType2D.Static;
        incarcurpos = curpos;
        incartarpos = incarcurpos;
        cargetin = car;
        MoveInCar();

        //able to be pick up
        gameObject.layer = LayerMask.NameToLayer("Deadbody");
        //dropweaponscript.enabled = true;
    }

    public void getoutcar()
    {
        if (incar)
        {
            incar = false;
            incarpercent = 0;
            body.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public bool checkincar()
    {
        return incar;
    }

    public float checkslowpercent()
    {
        return slowpercent;
    }

    override public void PickUp(Transform picker)
    {
        picked = true;
        getoutcar();

        /*for (int i = 0; i < colcount; i++)
        {
            cols[i].enabled = false;
        }

        transform.parent = picker;
        body.isKinematic = true;

        // set pos rot to zero (include force)//
        body.velocity = zeroVector;
        transform.localPosition = zeroVector;
        body.angularVelocity = 0f;
        transform.localRotation = Quaternion.identity;
        //************************************/
    }

    void Search(bool start) //seems searching can be use like isstop
    {
        StopCoroutine("turning");
        StopCoroutine("waitforsec");
        if (start)
        {
            transforming = true;
            StartCoroutine("turning");
            StartCoroutine("waitforsec", 2.5f);
        }
        else
        {
            transforming = false;
        }
    }

    IEnumerator waitforsec(float time)
    {
        yield return new WaitForSeconds(time);
        anim.SetTrigger("transform");
        yield return new WaitForSeconds(0.1f);
        transforming = false;
        transformed = true;
    }

    IEnumerator turning()
    {
        while (transforming)
        {
            yield return new WaitForSeconds(0.3f);
            if (transforming)
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
