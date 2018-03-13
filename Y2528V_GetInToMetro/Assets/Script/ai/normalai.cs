using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class normalai : MonoBehaviour, ableincar, baseai, interactable {

    //kind of buff/debuff stuffs//
    public bool gethit = false;
    private bool scared = false;
    private bool isdeath = false;
    private bool picked = false;
    //***************************//
    private dropweapon dropweaponscript;

    private bool settled = false; //check is this normal choose first car to go,if no , it will not starting moving
    private bool rotating = false; //seem useless now

    //***use for stun things***//
    private bool isstun = false;
    private GameObject stunbar;
    private float curstuntime = 0, endstuntime = 0;
    //*************************//

    //normal info && settle issue things//
    private Transform train;
    private Transform[] car;
    private Rigidbody2D body;
    private GameObject clothing, face;
    private Animator anim;
    private Collider2D[] cols;
    private float colcount;
    private float speedforanim;
    private float yellowlineposx;

    private Quaternion targetrot;
    private float rotangle;
    [SerializeField]
    private Vector3[] carpos;
    private int carcount;
    //////////////////////////////////////
    private float speed = 0.2f;
    private Vector3 curspeed, targetpos, curpos;
    private Vector2 zeroVector, cursca;
    [SerializeField]
    private int prefercar;
    private float closestdis;

    private int[] curdir = { 1, -1 };

    //**scaredmove**//
    private Vector2 lastcolpt, coldir, scaredmovedir;
    private float nextchangedirtime, endscaredtime;
    //**************//

    //***for incar***//
    private GameObject cargetin;
    private bool incar = false;
    [SerializeField]
    private float slowpercent, incarpercent = 0f;
    private Vector2 incarcurpos, incartarpos;
    private float movedis = 0.12f;
    //***************//

    //***use to fix healthbar filp problem***//
    private Transform healthbar;
    private Vector3 lockpossca;
    //***************************************//

    private Transform colobject; // use for onCollision2DEnter

    // Use this for initialization
    void Awake()
    {
        Random.seed = System.Guid.NewGuid().GetHashCode();
        anim = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
        dropweaponscript = GetComponent<dropweapon>();
        train = GameObject.Find("train").transform;
        carcount = train.Find("carctrl").childCount;
        car = new Transform[carcount];
        carpos = new Vector3[carcount];
        targetrot = transform.rotation;
        zeroVector = Vector2.zero;
        clothing = transform.Find("clothing/cloth").gameObject;
        clothing.GetComponent<SpriteRenderer>().color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.6f, 1f);
        face = transform.Find("face").gameObject;
        healthbar = transform.Find("UI/healthbar");
        stunbar = transform.Find("UI/stunbar").gameObject;
        yellowlineposx = GameObject.Find("ground/yellowline").transform.position.x;
        cols = GetComponents<Collider2D>();
        colcount = cols.Length;
    }

    void Start()
    {
        
    }

    void Update()
    {
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        curpos = transform.position;
        speedforanim = 0;
        if (incar)
        {
            MoveInCar(); //this function need to put here because death hit pushing will be counted in this function
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
                            // if ( Time.time > endscaredtime) { Scared(false); }  make it comment first so it scared forever unless get kill or interact by player

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
                                else
                                {
                                    MoveToYellowLine();
                                }
                            }
                        }
                    }
                    Rotate(); //rotate when gethit = false (not flying around)
                }
            }
        }
        anim.SetFloat("speed", speedforanim);
        // Debug.Log(body.velocity);
    }

    void Move()
    {
        targetpos = carpos[prefercar] - curpos;
        checkdir(targetpos);
        targetpos = (targetpos.normalized) * speed;
        curspeed = body.velocity;
        curspeed.x = Mathf.Lerp(curspeed.x, targetpos.x, 0.2f);
        curspeed.y = Mathf.Lerp(curspeed.y, targetpos.y, 0.2f);

        body.velocity = curspeed;
        speedforanim = Mathf.Max(Mathf.Abs(curspeed.x), Mathf.Abs(curspeed.y));
        //transform.position += new Vector3(targetpos.x/55,targetpos.y/55,0);

        //targetrot = Quaternion.LookRotation(targetpos);
        //Debug.Log(rotangle);
    }

    void MoveToYellowLine()
    {
        if (curpos.x > yellowlineposx + 0.3f)
        {
            targetpos = new Vector3(yellowlineposx, curpos.y, 0f) - curpos;
            checkdir(targetpos);
            targetpos = (targetpos.normalized) * speed;
            curspeed = body.velocity;
            curspeed.x = Mathf.Lerp(curspeed.x, targetpos.x, 0.2f);
            curspeed.y = Mathf.Lerp(curspeed.y, targetpos.y, 0.2f);

            body.velocity = curspeed;
            speedforanim = Mathf.Max(Mathf.Abs(curspeed.x), Mathf.Abs(curspeed.y));
        }
    }

    void ScaredMove()
    {
        if (Time.time > nextchangedirtime)
        {
            ChangeMoveDir(body.velocity);
        }
        else
        {
            targetpos = scaredmovedir * speed * 2;
            checkdir(targetpos);
            curspeed = body.velocity;
            curspeed.x = Mathf.Lerp(curspeed.x, targetpos.x, 0.2f);
            curspeed.y = Mathf.Lerp(curspeed.y, targetpos.y, 0.2f);

            body.velocity = curspeed;
            speedforanim = Mathf.Max(Mathf.Abs(curspeed.x), Mathf.Abs(curspeed.y));
        }
        Debug.DrawLine(curspeed, curpos, Color.red); //it draw the wrong line really confused me wtf holy shit ruined my fking day
    }

    void ChangeMoveDir(Vector2 lastcoldir)
    {
        if (lastcoldir == Vector2.zero)
        {
            lastcoldir = Random.insideUnitCircle;
        }
        coldir = lastcoldir.normalized;
        coldir = coldir.Rotate(Random.Range(-80f, 80f));
        scaredmovedir = coldir * -1;
        nextchangedirtime = Time.time + Random.Range(2f, 4f);
    }

    void Rotate()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, targetrot, 0.2f * Time.deltaTime * 60);
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
        for (int i = 0; i < carcount ; i++) 
        {
            car[i] = train.Find("carctrl").GetChild(i);
            carpos[i] = car[i].position;
        }
        settled = true;
        FindClosestCar();
    }

    public void MoveInCar()
    {
        if (!isdeath) // if death, then no auto move
        {
            incarpercent += 1f * (1 - slowpercent); 
        }
        incartarpos.x = incarcurpos.x - (movedis * (incarpercent / 100));
        if (incarpercent >= 100f)
        {
            cargetin.GetComponent<carctrl>().getonein(transform ,isdeath);
            Destroy(gameObject);
        }
        transform.position = incartarpos;
    }

    public void hit(Vector2 hitvel)
    {
        gethit = true;
        anim.SetBool("scared", gethit);
        body.velocity = hitvel;
        if (scared)
        {
            ChangeMoveDir(hitvel * - 1);
        }
        else
        {
            if (hitvel.x > 2f || hitvel.y > 2f)
            {
                //Scared(true);
            }
        }
    }

    public void hit()
    {
        gethit = true;
        anim.SetBool("scared", gethit);
    }

    public void Scared(bool isscared)
    {
        if (!isdeath)
        {
            scared = isscared;
            anim.SetBool("scared", scared);
            if (isscared)
            {
                ChangeMoveDir(body.velocity * -1);

                //able to interact
                gameObject.layer = LayerMask.NameToLayer("Interactable");
                //dropweaponscript.enabled = true;
                endscaredtime = Time.time + 20f;
            }
            else
            {
                gameObject.layer = LayerMask.NameToLayer("Normal");
                //dropweaponscript.enabled = false;
            }
        }
    }

    public void Interact(Transform player) //deal with all interact issuse here with player i hope it can
    {
        if (scared)
        {
            Scared(false);
        }
    }

    public void Stun(float time)
    {
        if (time > 0.2f)
        {
            gethit = true; //it's hard code because stun check after gethit, if I only call stun when it don't gethit, it don't stun
            isstun = true;
            curstuntime += time;
            Debug.Log(curstuntime);
        }
    }

    public void hitincar(float vel)
    {
        incarpercent += vel;
        //if (incarpercent > 100)
        //{
        //    incarpercent = 99f;
        //}
    }

    public void getincar(GameObject car, float slowpercent)
    {
        this.slowpercent = slowpercent/100;
        incar = true;
        hitincar(Mathf.Abs(body.velocity.x) * 15 * (1 - this.slowpercent));
        //Debug.Log(body.velocity.x + " " + incarpercent);
        body.bodyType = RigidbodyType2D.Static;
        incarcurpos = curpos;
        incartarpos = incarcurpos;
        cargetin = car;
        MoveInCar();
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

    public void PickUp(Transform picker)
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

    public void PutDown(Vector2 vel)
    {
        picked = false;

        /*body.isKinematic = false;
        for (int i = 0; i < colcount; i++)
        {
            cols[i].enabled = true;
        }
        transform.parent = null;*/
        hit(vel);
    }

    public void Death() // need to reset most of the state here
    {
        isdeath = true;
        endstuntime = 0f;
        isstun = false;
        stunbar.SetActive(false);
        gameObject.layer = LayerMask.NameToLayer("Deadbody");
        //dropweaponscript.enabled = true;
    }

    public void Rescued()
    {
        isdeath = false;
        Stun(2f);
        Scared(false); // << this function do next two line's thing
        //gameObject.layer = LayerMask.NameToLayer("Normal");
        //dropweaponscript.enabled = false;
        dirrepair();
    }

    public void Rescued(Vector2 hitvel)
    {
        isdeath = false;
        Stun(2f);
        hit(hitvel);
        Scared(false); // << this function do next two line's thing
        //gameObject.layer = LayerMask.NameToLayer("Normal");
        //dropweaponscript.enabled = false;
        dirrepair();
    }

    public bool checkincar()
    {
        return incar;
    }

    public float checkslowpercent()
    {
        return slowpercent;
    }

    void checkdir(Vector2 targetdir)
    {
        if (Mathf.Sign(targetdir.x) != curdir[0])
        {
            FilpHor();
            curdir[0] *= -1;
        }
        if (Mathf.Sign(targetdir.y) != curdir[1])
        {
            FilpVer();
            curdir[1] *= -1;
        }
    }

    void FilpHor()
    {
        cursca = transform.localScale;
        cursca.x *= -1;
        transform.localScale = cursca;
        healthbar.localScale = cursca;
        //weapon.localScale = cursca;
    }

    void FilpVer()
    {
        //face.SetActive(!face.activeSelf);
        //change dir of face seem weird for everyone
    }

    void dirrepair()
    {
        if (Mathf.Sign(transform.localScale.x) != curdir[0])
        {
            FilpHor();
        }
    }

    bool InRange(float num, float min, float max)
    {
        if (num >= min && num <= max)
            return true;
        else
            return false;
    }

    public bool IsScared()
    {
        return scared;
    }

    public bool IsHit()
    {
        return gethit;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (scared)
        {
            foreach (ContactPoint2D contact in col.contacts)
            {
                Debug.DrawLine(contact.point, curpos, Color.black);
                ChangeMoveDir(contact.point - (Vector2) curpos);
            }
        }

        if (gethit)
        {
            colobject = col.transform;
            if (colobject.CompareTag("normal"))
            {
                colobject.GetComponent<normalai>().hit();
                return;
            }
            if (colobject.CompareTag("normal enemy"))
            {
                //colobject.GetComponent<enemyai>().hit();
                return;
            }
            if (colobject.CompareTag("wall"))
            {
                Stun(Mathf.Max(Mathf.Abs(body.velocity.x),Mathf.Abs(body.velocity.y))/2); //add stun time
                return;
            }
        }
    }

    public Vector2 Vector2FromAngle(float a)
    {
        a *= Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(a), Mathf.Sin(a));
    }

}

public static class Vector2Extension
{
    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        return Quaternion.Euler(0, 0, degrees) * v;
    }
}
    
/*void OnCollisionStay2D(Collision2D col)
    {
        foreach (ContactPoint2D contact in col.contacts)
        {
            Debug.DrawLine(contact.point, curpos, Color.black);
            Vector2 aaa = (contact.point - (Vector2)curpos).normalized;
            //float angle = Mathf.Atan2(aaa.y, aaa.x) * Mathf.Rad2Deg;
            aaa = aaa.Rotate(90f);
            Debug.DrawLine(aaa, curpos, Color.red);
        }
    }*/
