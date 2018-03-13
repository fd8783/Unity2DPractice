using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hussarsai : enemyai {

    //use for getting information//
    private Vector2 curpos, targetpos;
    private SpriteRenderer turneffect, backwing;
    private Animator horseanim;
    private bool startstun;
    //***************************//

    //**use for vision**//
    public LayerMask targetlayer;
    //public LayerMask blocklayer;
    private GameObject stagemanger;
    private Vector2 topleftpt, downrightpt;
    private Collider2D[] visibletarget;
    private int vistarcount, closesttarnum;
    private float closestdis, newdis;
    [SerializeField]
    private bool searching = false;
    private health targethealthscript;
    private bool checkingdeathtarget = false;
    //******************//

    //use for movement//
    private float speed = 1.5f, curweaponweight;
    private Vector2 curspeed, targetspeed;
    private float targetangle;
    private Quaternion startrot, targetrot;
    //****************//

    //use for weapon issues//
    public string[] defaultweapon = { "melee", "lance", "9" };
    public bool abletoattack = false;
    public float reloadtime = 0.5f, nextattacktime = 0f;
    private GameObject knife;
    private Rigidbody2D knifeRB;
    //*********************//


    //what to do//
    [SerializeField]
    private int[] priorityarr;
    private int priorityjob, jobcount = 7, currentdoingjob, lastjob;
    private bool wait;
    private lancestrung lancescript,throwstrungscript;
    private TrailRenderer RushTrail;
    private float targetdis;

    //for charging//
    private bool charge, charging, readytocharge, foundcorner;
    private Vector2 closesttargetpos, chargedir;

    //for catapult calling//
    private GameObject catapultcall, catapult;
    private bool catapultcalling, catapultset;

    //for throwhit//
    private bool throwhit, throwgood, throwhiting;

    //for featherattack//
    private Vector2[] waypoint = new Vector2[5];
    private Transform featherwaypt;
    private feather[] featherscript = new feather[5];
    private bool feathercalled, feathercalldone;
    private GameObject feather;

    //for horserush//
    private bool horserushing, readytorush;
    private int rushcount;

    //**********//

    //private int[] curdir = { 0, 0 };

    // Use this for initialization
    new void Awake()
    {
        base.Awake();
        stagemanger = GameObject.Find("stagemanager");
        topleftpt = stagemanger.transform.Find("stagearea/topleftpt").position;
        downrightpt = stagemanger.transform.Find("stagearea/downrightpt").position;
        //face = transform.Find("face").gameObject;
        turneffect = transform.Find("UI/turneffect").GetComponent<SpriteRenderer>();
        knife = Resources.Load("dropweapon/knife") as GameObject;

        startrot = transform.rotation;
        curpos = transform.position;

        body = transform.root.GetComponent<Rigidbody2D>();
        horseanim = transform.root.GetComponent<Animator>();
        backwing = transform.Find("clothing/backwing").GetComponent<SpriteRenderer>();
        priorityarr = new int[jobcount];
        RushTrail = transform.Find("Trails/Trail").GetComponent<TrailRenderer>();

        catapultcall = transform.Find("UI/catapultcall").gameObject;
        catapult = Resources.Load("Catapult") as GameObject;

        //priorityarr[3] = 1;
        lancescript = weapon.Find("melee/lance/lighthitbox").GetComponent<lancestrung>();
        throwstrungscript = weapon.Find("melee/lance/throwhitbox").GetComponent<lancestrung>();

        featherwaypt = transform.Find("featherwaypt");
        for (int i = 0; i < 4; i++)
        {
            waypoint[i] = featherwaypt.GetChild(i).localPosition;
        }
        feather = Resources.Load("feather") as GameObject;
    }

    void Start()
    {
        // these three need to run in start because some weapon value (i.e. startpos) need to be set in other script's Awake
        updateattackrange(defaultweapon[1]);
        curweaponanim = weapon.GetComponent<enemyweaponlist>().changeweapon(curweaponanim, defaultweapon[0], defaultweapon[1]);
        curweaponweight = int.Parse(defaultweapon[2]);
        FindTarget();
        StartCoroutine("IdleForSecond", 2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        curpos = transform.position;
        speedforanim = 0;

        if (closesttarget == null)
        {
            FindTarget();
        }

        if (isstun)
        {
            if (!startstun)
            {
                endstuntime = Time.time + curstuntime;
                stunbar.SetActive(true);
                startstun = true;
            }
            else
            {
                if (Time.time <= endstuntime)
                {
                    priorityarr[0] = 5; //5 is highest, stun must idle
                    curstuntime -= Time.fixedDeltaTime;
                }
                else
                {
                    isstun = false;
                    startstun = false;
                    stunbar.SetActive(false);
                    priorityarr[0] = 0;
                    StartCoroutine("IdleForSecond", 0.5f);
                }
            }
        }

        CheckWhatToDo();

        horseanim.SetFloat("speed", speedforanim);
    }

    void Idle()
    {
        if (closesttarget.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            if (!catapultset)
            {
                StopCoroutine("IdleForSecond");
                StartCoroutine("IdleForSecond", 1f);
                priorityarr[1] = 5;
                catapultset = true;
            }
        }
        else
        {
            catapultset = false;
            //JobChoose(); run it in IdleForSecond(), hope this work, will save some cpu time
        }
        targetrot = Quaternion.Euler(new Vector3(0f, 0f, 0f));
        weapon.localRotation = Quaternion.Lerp(weapon.localRotation, targetrot, 0.2f * Time.deltaTime * 60);
    }

    void JobChoose()
    {
        closestdis = Vector2.Distance(closesttarget.position, transform.position);
        Debug.Log(closestdis);
        priorityarr[4] = 0; //throwhit
        priorityarr[5] = 0; //horserush
        priorityarr[6] = 0; //featherattack
        if (closestdis < 0.7f)
        {
            if (lastjob == 4)
                priorityarr[5] = 3;
            else
                priorityarr[4] = 3;
        }
        else
        {
            if (closestdis < 1.3f)
            {
                if (lastjob == 5)
                    priorityarr[6] = 3;
                else
                    priorityarr[5] = 3;
            }
            else
            {
                if (closestdis < 2f)
                {
                    if (lastjob == 6)
                        priorityarr[3] = 3;
                    else
                        priorityarr[6] = 3;
                }
                else
                {
                    if (lastjob == 3)
                        priorityarr[6] = 3;
                    else
                        priorityarr[3] = 3;
                }
            }
        }
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

    void MoveAway()
    {
        targetpos = closesttarget.position;
        targetspeed = (targetpos - curpos).normalized * -1;
        checkdir(targetspeed); //function from parent script to filp

        targetangle = Mathf.Atan2(targetspeed.y, targetspeed.x) * Mathf.Rad2Deg; // use to rotate weapon
        targetrot = Quaternion.Euler(new Vector3(0f, 0f, targetangle * cursca.x + (cursca.x > 0 ? 0 : 180)));
        weapon.localRotation = Quaternion.Lerp(weapon.localRotation, targetrot, 0.2f * Time.deltaTime * 60);

        targetspeed = targetspeed * speed * 0.6f * (10 / curweaponweight);
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

    void CheckWhatToDo()
    {
        priorityjob = 0;
        for (int i = 1; i < jobcount; i++)
        {
            if (priorityarr[i] > priorityarr[priorityjob])
                priorityjob = i;
        }

        if (priorityjob != 0)
        {
            if (currentdoingjob != priorityjob)
            {
                // if priorityarr[currentdoingjob] != 0  >>> mean get interrupt 
                // may need to do another switch if a job is interrupt by other job
                currentdoingjob = priorityjob;
            }
        }

        switch (priorityjob)
        {
            case 0:
                Idle();
                break;
            case 1:
                CatapultAttack();
                break;
            case 3:
                Charge();
                break;
            case 4:
                ThrowHit();
                break;
            case 5:
                HorseRush();
                break;
            case 6:
                FeatherAttack();
                break;
            default:
                Debug.Log("don't have this job, go check your stupid code" + priorityjob);
                break;
        }
    }

    IEnumerator IdleForSecond(float time)
    {
        priorityarr[0] = 5; //set idle as highest priorityjob
        //MoveAway();
        //yield return new WaitForSeconds(0.3f);
        checkdir(targetspeed * -1f);
        yield return new WaitForSeconds(time);
        JobChoose();
        priorityarr[0] = 0;
    }

    void CatapultAttack()
    {
        if (!catapultcalling)
        {
            catapultcalling = true;
            checkdir(new Vector2(-1f,-1f));
            Instantiate(catapult, new Vector2(downrightpt.x + 0.3f, transform.position.y), Quaternion.identity);
            StartCoroutine("CallCatapult", 4f);
        }
    }

    IEnumerator CallCatapult(float time)
    {
        anim.SetBool("callcatapult", true);
        yield return new WaitForSeconds(time);
        anim.SetBool("callcatapult", false);
        ResetCatapultAttackstate();
    }

    void ResetCatapultAttackstate()
    {
        catapultcalling = false;
        priorityarr[1] = 0;
        lastjob = 1;
        StartCoroutine("IdleForSecond", 1f);
    }

    void Charge()
    {
        if (charge)
        {
            if (charging)
            {
                ChargeMove();
                /*CheckTargetInRange();
                if (abletoattack)
                {
                    //body.velocity = targetspeed * 2f;
                    charging = false;
                }*/
            }
            else
            {
                ResetChargeState();
            }
        }
        else
        {
            if (readytocharge)
            {
                Move();
                CheckTargetInRange();
                if (abletoattack)
                {
                    RushTrail.enabled = true;
                    charge = true;
                    chargedir = ((Vector2)closesttarget.position - curpos).normalized;
                }
            }
            else
            {
                if (foundcorner)
                {
                    MoveToCorner();
                }
                else
                {
                    FindCorner();
                }
            }
        }
    }

    void FindCorner()
    {
        closesttargetpos = closesttarget.position;

        if (closesttargetpos.x > (topleftpt.x + ((downrightpt.x - topleftpt.x) / 2)))
            targetpos.x = topleftpt.x + 0.2f;
        else
            targetpos.x = downrightpt.x - 0.2f;

        if (closesttargetpos.y > (downrightpt.y + ((topleftpt.y - downrightpt.y) / 2)))
            targetpos.y = downrightpt.y + 0.2f;
        else
            targetpos.y = topleftpt.y - 0.2f;
        
        foundcorner = true;
    }

    void MoveToCorner()
    {
        if (Vector2.Distance(curpos, targetpos) < 0.1f)
        {
            StartCoroutine("IdleForSecond_Charge", 1f);
        }
        else
        {
            targetspeed = (targetpos - curpos).normalized;
            checkdir(targetspeed); //function from parent script to filp

            targetangle = Mathf.Atan2(targetspeed.y, targetspeed.x) * Mathf.Rad2Deg; // use to rotate weapon
            targetrot = Quaternion.Euler(new Vector3(0f, 0f, targetangle * cursca.x + (cursca.x > 0 ? 0 : 180)));
            weapon.localRotation = Quaternion.Lerp(weapon.localRotation, targetrot, 0.2f * Time.deltaTime * 60);

            targetspeed = targetspeed * speed *1.2f * (10 / curweaponweight);
            curspeed = body.velocity;
            curspeed.x = Mathf.Lerp(curspeed.x, targetspeed.x, 0.2f);
            curspeed.y = Mathf.Lerp(curspeed.y, targetspeed.y, 0.2f);

            body.velocity = curspeed;
            speedforanim = Mathf.Max(Mathf.Abs(curspeed.x), Mathf.Abs(curspeed.y));
        }
    }

    void ChargeMove()
    {
        targetspeed = chargedir;
        checkdir(targetspeed); //function from parent script to filp

        targetangle = Mathf.Atan2(targetspeed.y, targetspeed.x) * Mathf.Rad2Deg; // use to rotate weapon
        targetrot = Quaternion.Euler(new Vector3(0f, 0f, targetangle * cursca.x + (cursca.x > 0 ? 0 : 180)));
        weapon.localRotation = Quaternion.Lerp(weapon.localRotation, targetrot, 0.2f * Time.deltaTime * 60);
        //Debug.Log(targetangle);

        targetspeed = targetspeed * speed * 1.5f * (10 / curweaponweight);
        //curspeed = body.velocity;
        //curspeed.x = Mathf.Lerp(curspeed.x, targetspeed.x, 0.05f);
        //curspeed.y = Mathf.Lerp(curspeed.y, targetspeed.y, 0.05f);

        body.velocity = targetspeed;
        speedforanim = Mathf.Max(Mathf.Abs(targetspeed.x), Mathf.Abs(targetspeed.y));
    }

    void ResetChargeState()
    {
        RushTrail.enabled = false;
        foundcorner = false;
        readytocharge = false;
        charge = false;
        charging = false;
        anim.SetBool("charging", false);
        priorityarr[3] = 0; //charge done
        lastjob = 3;
        StartCoroutine("IdleForSecond", 1f);
    }

    IEnumerator IdleForSecond_Charge(float time)
    {
        checkdir(((Vector2)closesttarget.position - curpos));
        priorityarr[0] = 5; //set idle as highest priorityjob
        yield return new WaitForSeconds(time);
        priorityarr[0] = 0;

        readytocharge = true;
        charging = true;
        anim.SetBool("charging", true);
        updateattackrange("chargerange");
    }

    void ThrowHit()
    {
        if (throwhiting)
        {
            if (throwhit)
            {
                weapon.localRotation = targetrot;
                anim.SetBool("throwhit", false);
            }
            else
            {
                Move();
                CheckTargetInRange();
                if (abletoattack)
                {
                    StopCoroutine("ThrowHitCounter");
                    throwhit = true;
                }
            }
        }
        else
        {
            throwhiting = true;
            anim.SetBool("throwhit", true);
            updateattackrange("throwstrung");
            StartCoroutine("ThrowHitCounter", 0.7f);
        }
        
    }

    public void ThrowHitCheck()
    {
        if (throwstrungscript.StrungCount() > 0)
        {
            anim.SetBool("throwgood", true);
            screenctrl.StopScreen(0.05f, 0.1f);
            // ResetThrowHitState() will do in anim
        }
        else
        {
            anim.SetBool("throwgood", false);
            ResetThrowHitState();
        }

    }

    IEnumerator ThrowHitCounter(float time)
    {
        yield return new WaitForSeconds(time);
        throwhit = true;
    }

    public void ThrowOut()
    {
        screenctrl.StopScreen(0.05f, 0.13f);
        throwstrungscript.ThrowOut(targetspeed*3f);
        ResetThrowHitState();
    }

    void ResetThrowHitState()
    {
        throwhit = false;
        anim.SetBool("throwhit", false);
        throwhiting = false;
        priorityarr[4] = 0;
        lastjob = 4;
        StartCoroutine("IdleForSecond", 1f);
    }

    /*IEnumerator BoolCounter(bool boolean, float time)
    {
        boolean = true;
        yield return new WaitForSeconds(time);
    }*/

    void FeatherAttack()
    {
        checkdir(((Vector2)closesttarget.position - curpos));
        if (feathercalled)
        {
            if (feathercalldone)
            {
                feathercalldone = false;
                StartCoroutine(FeatherFire(5, 0.2f));
            }
        }
        else
        {
            feathercalled = true;
            anim.SetBool("featherattacking", true);
            StartCoroutine(FeatherCall(5, 0.3f));
        }
    }

    IEnumerator FeatherCall(int num,float time)
    {
        for (int i = 0; i < num; i++)
        {
            featherscript[i] = Instantiate(feather,transform).GetComponent<feather>();
            waypoint[4] = featherwaypt.GetChild(i + 3).localPosition; //set feather's waypoint
            featherscript[i].Settle(waypoint, closesttarget);
            yield return new WaitForSeconds(time);
        }
        yield return new WaitForSeconds(2f);
        feathercalldone = true;
    }

    IEnumerator FeatherFire(int num, float time)
    {
        for (int i = 0; i < num; i++)
        {
            anim.SetTrigger("featherattack");
            featherscript[i].Fire();
            yield return new WaitForSeconds(time);
        }

        ResetFeatherAttackState();
    }

    void ResetFeatherAttackState()
    {
        feathercalled = false;
        anim.SetBool("featherattacking", false);
        priorityarr[6] = 0;
        lastjob = 6;
        StartCoroutine("IdleForSecond", 2f);
    }

    void HorseRush()
    {
        if (horserushing)
        {
            if (readytorush)
            {
                if (rushcount <= 8)
                {
                    ChargeMove();
                    rushcount++;
                }
                else
                {
                    ResetHorseRushState();
                }
            }
            else
            {
                Move();
                CheckTargetInRange();
                if (abletoattack)
                {
                    StopCoroutine("HorseRushCounter");
                    readytorush = true;
                    RushTrail.enabled = true;
                    chargedir = ((Vector2)closesttarget.position - curpos).normalized;
                }
            }
        }
        else
        {
            updateattackrange("chargerange");
            horserushing = true;
            anim.SetBool("horserush", true);
            StartCoroutine("HorseRushCounter", 1f);
        }
    }

    IEnumerator HorseRushCounter(float time)
    {
        yield return new WaitForSeconds(time);
        readytorush = true;
        RushTrail.enabled = true;
        chargedir = ((Vector2)closesttarget.position - curpos).normalized;
    }

    void ResetHorseRushState()
    {
        readytorush = false;
        horserushing = false;
        anim.SetBool("horserush", false);
        RushTrail.enabled = false;
        rushcount = 0;
        priorityarr[5] = 0;
        lastjob = 5;
        StartCoroutine("IdleForSecond", 1.5f);
    }

    void CheckTargetInRange()
    {
        if (closesttarget.gameObject.layer == LayerMask.NameToLayer("Interactable"))
        {
            abletoattack = false;
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
            anim.SetTrigger("attack");
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
        knifeRB.velocity = body.velocity;
    }

    public override void FilpVer()
    {
        base.FilpVer();
        if (Mathf.Sign(gettargetdir().y) == 1)
        {
            backwing.sortingOrder = 19;
            Debug.Log("do");
        }
        else
        {
            backwing.sortingOrder = 2;
        }
    }

    override public void FilpHor()
    {
        cursca = transform.root.localScale;
        cursca.x *= -1;
        transform.root.localScale = cursca;
        UI.localScale = cursca;

    }

    override public void OnCollisionEnter2D(Collision2D col)
    {
        if (charge)
        {
            colobject = col.transform;
            if (colobject.CompareTag("normal"))
            {
                colobject.GetComponent<normalai>().hit();
                return;
            }
            if (colobject.CompareTag("normal enemy"))
            {
                colobject.GetComponent<enemyai>().hit();
                return;
            }
            if (colobject.CompareTag("wall"))
            {
                if (lancescript.StrungCount() > 0)
                {
                    lancescript.ThrowOut(targetspeed * -3f);
                    screenctrl.StopScreen(0.05f, 0.1f);
                }
                else
                {
                    Stun(3f); //add stun time
                    screenctrl.ShakeScreen(0.03f);
                }
                Debug.Log(body.velocity);
                ResetChargeState();
                hit(((Vector2)transform.root.position - col.contacts[0].point).normalized * 0.5f);
                return;
            }
        }
        if (speedforanim > 0)
        {
            colobject = col.transform;
            if (colobject.CompareTag("normal"))
            {
                colobject.GetComponent<normalai>().hit();
                return;
            }
        }
    }
}
