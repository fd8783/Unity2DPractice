using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class thieftocarai : enemyai, ableincar {

    private Vector2 curpos, targetpos;

    //use for movement && settle issue//
    private float speed = 0.6f, curweaponweight;
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
    private Vector2 runningright = new Vector2(1,0);
    private float rightwallptx;
    private Transform detectwallpt;
    private GameObject thiefbag;
    private Rigidbody2D thiefbagRB;
    //********************************//

    //use for weapon issues//
    public string[] defaultweapon = { "melee", "bag", "12" };
    public bool abletoattack = false;
    public Vector2 weaponpos;
    public float reloadtime = 0.5f, nextattacktime = 0f;
    //*********************//

    //***for incar***//
    private GameObject cargetin;
    private bool incar = false;
    private float slowpercent, incarpercent = 0f;
    private Vector2 incarcurpos, incartarpos;
    private float movedis = 0.2f;
    //***************//

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
        thiefbag = Resources.Load("item/thiefbag") as GameObject;
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
        if (incar)
        {
            MoveInCar();
        }
        else
        {
            if (isdeath || picked)
            {

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
                                Move();
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
        
        targetspeed = targetspeed * speed * (10 / curweaponweight);
        curspeed = body.velocity;
        curspeed.x = Mathf.Lerp(curspeed.x, targetspeed.x, 0.2f);
        curspeed.y = Mathf.Lerp(curspeed.y, targetspeed.y, 0.2f);

        body.velocity = curspeed;
        speedforanim = Mathf.Max(Mathf.Abs(curspeed.x), Mathf.Abs(curspeed.y));
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

            targetspeed = targetspeed * speed * (10 / curweaponweight) * 1.2f;
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
            cargetin.GetComponent<carctrl>().getonein(transform ,isdeath);
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
        if (damage >= 50 || (Mathf.Pow(hitvel.x,2) + Mathf.Pow(hitvel.y, 2)) > 30) //30 is near 6^2 = 36 which 6 is max power of default weapon brand
        {
            Scared(true);
            DropWeapon();
        }
    }

    override public void Interact(Transform player)
    {

    }

    override public void DropWeapon()
    {
        hand.gameObject.SetActive(true);
        weapon.gameObject.SetActive(false);
        thiefbagRB = Instantiate(thiefbag, weapon.transform.position, Quaternion.identity).GetComponent<Rigidbody2D>();
        thiefbagRB.velocity = body.velocity / 2;
    }

    public void getincar(GameObject car, float slowpercent)
    {
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
}
