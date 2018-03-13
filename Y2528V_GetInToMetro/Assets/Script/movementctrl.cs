using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movementctrl : MonoBehaviour, baseai {

    //**upgrade**//
    public bool rollautoattack = false;
    //***********//


    //** getting normal info **//
    [SerializeField]
    private Animator anim, curweaponanim;
    private Rigidbody2D body;
    private BoxCollider2D col;
    private GameObject face;
    private Transform man;
    //private CharacterController CC;
    //private CircleCollider2D punchhitbox;
    //private GameObject rhand;
    //*************************//

    //** getting audio info **//
    private AudioSource[] SoundList, footstep;
    private AudioSource rollsound; 
    private bool footstepplayed;
    private int footstepnum = 4, choosefootstepnum;
    //*************************//

    //** getting particle info **//
    private Transform particleeffect;
    private ParticleSystem walkdust, rolldust;
    //*************************//

    //** current status **//
    public bool gethit = false;
    private bool isdeath = false;
    private bool rolled = false;
    private bool picked = false;
    private bool scared = false;
    public bool exhaust = false;
    //********************//

    //** use for movement and facing**//
    public bool speedmax;
    private int rollinglayer = 13, defaultlayer = 8;

    private Vector2 curspeed, norcurspeed, cursca, zerovetor;
    private Vector2 mousepos;
    private Vector3 anglecross;
    private Quaternion playerrot, startrot;
    [SerializeField]
    private float speed = 1.2f, curweaponweight, speedforanim, countroll;
    private float mouseangle;
    private int[] curdir = { 1, -1 };
    [SerializeField]
    private int[] targetdir = { 1, -1 }; //use 1,-1 to simulate the x,y-coordinate (1 for +, -1 for -)
    //********************************//

    //** use for attacking **//
    private bool charge = false;
    private float charging;
    private bool fixpose = false, fixrot = false; //in now's setting, player can't move when doing sp(some heavyattack)
    [SerializeField]
    private bool heavyattacking;
    //***********************//

    //** use for checking pickable weapon **//
    private Vector3 curpos;
    private LayerMask weaponlayer = (1 << 14) | (1 << 11) | (1 << 17) | (1 << 16);
    private Collider2D[] pickableweapon;
    private Transform closestweapon, preclosestweapon;
    private int pickableweaponcount, closestweaponnum;
    private float closestweapondis, newdis;
    private dropweapon dropweaponscript;
    //--------------------------------------//
    //******* use for changing weapon ******//
    private string[] weaponinfo = new string[4];
    private Transform weapon, weaponinback, bodyfragment;
    private string[] emptyweaponinfo = { "empty", "empty", "empty", "empty" };
    //public string[] defaultweapon = { "melee", "stopbrand", "10", "empty" }, secondweapon = { "empty", "empty", "empty", "empty" }, throwweapon = { "empty", "empty", "empty", "empty" };
    public string[,] weaponorder = new string[3,4] { { "melee", "stopbrand", "10", "empty" }, { "empty", "empty", "empty", "empty" }, { "empty", "empty", "empty", "empty" } };
    /////////////////////////////////////////////////    default                             , second                                , throw
    private int curweaponorder = 0;     //                 ^^ 0                                   ^^1                                    ^^2
    private float cursecweadurability, fragmentcount; // current second weapon durability
    private weaponlist weaponlistscript;
    private weaponinbacklist weaponinbacklistscript;
    private GameObject weapondropped;
    private Transform droppt;
    private weaponfacing curweaponfacing;
    //**************************************//

    // Use this for initialization
    void Start()
    {
        //CC = GetComponent<CharacterController>();
        man = transform.Find("man");
        cursca = transform.localScale;
        face = man.Find("face").gameObject;
        weapon = transform.Find("weapon");
        weaponinback = man.Find("body/backweaponctrl");
        weaponlistscript = weapon.GetComponent<weaponlist>();
        weaponinbacklistscript = weaponinback.GetComponent<weaponinbacklist>();
        zerovetor = Vector2.zero;
        anim = man.GetComponent<Animator>();
        col = GetComponent<BoxCollider2D>();
        body = GetComponent<Rigidbody2D>();
        mousepos = zerovetor;
        curweaponweight = int.Parse(weaponorder[0,2]);
        //rhand = transform.Find("rhand").gameObject;
        //punchhitbox = rhand.GetComponent<CircleCollider2D>();
        curweaponanim = weaponlistscript.changeweapon(curweaponanim, "melee", "stopbrand",100);
        curweaponfacing = curweaponanim.GetComponent<weaponfacing>();
        droppt = man.Find("droppt");
        bodyfragment = weapon.Find("throw/bodyfragment/bodypos");
        startrot = transform.rotation;

        SoundList = man.GetComponents<AudioSource>();
        footstep = new AudioSource[footstepnum];
        for (int i = 0; i < footstepnum; i++)
        {
            footstep[i] = SoundList[i];
        }
        rollsound = SoundList[footstepnum]; //4, which is the 5th

        particleeffect = transform.Find("effect");
        walkdust = particleeffect.Find("walkdust").GetComponent<ParticleSystem>();
        rolldust = particleeffect.Find("rolldust").GetComponent<ParticleSystem>();
    }

    void FixedUpdate()
    {
        curpos = transform.position;
        speedforanim = 0;
        
        if (!fixpose && !picked && !exhaust )
        {
            Move();
        }
        //Debug.Log("FIXEDUP"+Time.deltaTime);

        anim.SetFloat("speed", speedforanim);

        if (speedforanim > 0.01f) 
        {
            if (!footstepplayed)
            {
                StartCoroutine("PlayFootStep");
            }
        }
        else
        {
            StopCoroutine("PlayFootStep");
            footstepplayed = false;
        }

        if (Vector2.Distance(body.velocity, Vector2.zero) > 0.05f)
        {
            if (walkdust.isStopped)
                walkdust.Play();
        }
        else
        {
            walkdust.Stop();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!picked && !exhaust)
        {
            // should stick in this order together//
            SearchPickableWeapon();  //this
            PickUpWeapon();      // and this
            //************************************//
            if (!fixpose)
            {
                ChangeWeaponCheck();
                Roll();
            }
            if (!fixrot)
            {
                Rotate();
            }
            /*if (gethit)
            {
                if (InRange(body.velocity.x, -0.15f, 0.15f) && InRange(body.velocity.y, -0.15f, 0.15f))
                {
                    gethit = false;
                    anim.SetBool("scared", false);
                }
            }*/

            HeavyAttack();
            Attack();
            //Debug.Log("UP"+Time.deltaTime);
        }
        else
        {
            if (!picked)
            {
                RotateBody();
            }
        }
        //if (gethit)
        //  Debug.Log(body.velocity);
    }

    void Move()
    {
        curspeed = body.velocity;
        norcurspeed = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        curspeed.x = Mathf.Lerp(curspeed.x, norcurspeed.x * speed , (norcurspeed.x <= 0.1f ? 0.6f : 0.2f));
        curspeed.y = Mathf.Lerp(curspeed.y, norcurspeed.y * speed , (norcurspeed.y <= 0.1f ? 0.6f : 0.2f));
        //curspeed.x = norcurspeed.x * speed * (10 / curweaponweight); // 10 is the standard weight for weapon
        //curspeed.y = norcurspeed.y * speed * (10 / curweaponweight);
        speedmax = (Mathf.Abs(curspeed.x) > (speed * 0.9 * (10 / curweaponweight)) || Mathf.Abs(curspeed.y) > (speed * 0.9 * (10/ curweaponweight))) ? true : false;

        /*curspeed.x = Input.GetAxis("Horizontal") * speed;
        curspeed.y = Input.GetAxis("Vertical") * speed;*/
        body.velocity = curspeed;
        speedforanim = Mathf.Max(Mathf.Abs(curspeed.x), Mathf.Abs(curspeed.y));
        //axis.x = Input.GetAxis("Horizontal");
        //axis.y = Input.GetAxis("Vertical");
    }

    /*void CCMove()
    {
        norcurspeed = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).normalized;
        curspeed.x = norcurspeed.x * speed * (sprint ? 1.8f : 1f);
        curspeed.y = norcurspeed.y * speed * (sprint ? 1.8f : 1f);
        CC.Move(norcurspeed/20);
    }*/

    IEnumerator PlayFootStep()
    {
        footstepplayed = true;
        while (footstepplayed)
        {
            choosefootstepnum = Random.Range(0, footstepnum);
            footstep[choosefootstepnum].pitch = 1 + Random.Range(0.8f, 1.1f);
            footstep[choosefootstepnum].Play();
            yield return new WaitForSeconds(0.24f);
        }
    }

    void RotateBody()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, startrot, 0.2f * Time.deltaTime * 60);
    }

    void Rotate()
    {
        mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - curpos;
        mousepos = mousepos.normalized;
        /*mouseangle = Vector2.Angle(new Vector2(0.01f, 0.01f), mousepos);
        anglecross = Vector3.Cross(new Vector2(0.01f, 0.01f), mousepos);
        //mouseangle = Vector2.Angle(transform.position, mousepos);
        //anglecross = Vector3.Cross(transform.position, mousepos);

        mouseangle *= Mathf.Sign(anglecross.z);
        mouseangle += 45;
        if (mouseangle < -180)
        {
            mouseangle += 360;
        }*/
                                                                                            
        mouseangle = Mathf.Atan2(mousepos.y, mousepos.x) * Mathf.Rad2Deg;    //                 90
                                                                             //                 |
        if (mouseangle > 90)                                                 //                 |
        {                                                                    //    180/-180-------------0
            changedir(targetdir, -1, 1);                                     //                 |
        }                                                                    //                 |
        else                                                                 //                -90
        {
            if (mouseangle > 0)
            {
                changedir(targetdir, 1, 1);
            }
            else
            {
                if (mouseangle > -90)
                {
                    changedir(targetdir, 1, -1);
                }
                else
                {
                    changedir(targetdir, -1, -1);
                }
            }
        }

        if (curdir[0] != targetdir[0])
        {
            FilpHor();
        }
        if (curdir[1] != targetdir[1])
        {
            FilpVer();
        }
        changedir(curdir, targetdir);

        //Debug.Log(mouseangle);
        playerrot = Quaternion.Euler(new Vector3(0f, 0f, mouseangle*cursca.x+ (cursca.x > 0 ? 0 : 180)));
        weapon.localRotation = Quaternion.Lerp(weapon.localRotation, playerrot, 0.2f * Time.deltaTime * 60);
        //mousepos.x = 0;
        //mousepos.y = 0;
        //Debug.Log(mouseangle+ " " + anglecross.z);
        //transform.Rotate(mousepos);

    }

    public void RotateToDir(float x, float y) //need improve
    {
        mouseangle = Mathf.Atan2(y, x) * Mathf.Rad2Deg;                      //                 90
                                                                             //                 |
        if (mouseangle > 90)                                                 //                 |
        {                                                                    //    180/-180-------------0
            changedir(targetdir, -1, 1);                                     //                 |
        }                                                                    //                 |
        else                                                                 //                -90
        {
            if (mouseangle > 0)
            {
                changedir(targetdir, 1, 1);
            }
            else
            {
                if (mouseangle > -90)
                {
                    changedir(targetdir, 1, -1);
                }
                else
                {
                    changedir(targetdir, -1, -1);
                }
            }
        }

        if (curdir[0] != targetdir[0])
        {
            FilpHor();
        }
        if (curdir[1] != targetdir[1])
        {
            FilpVer();
        }
        changedir(curdir, targetdir);

        //Debug.Log(mouseangle);
        playerrot = Quaternion.Euler(new Vector3(0f, 0f, mouseangle * cursca.x + (cursca.x > 0 ? 0 : 180)));
        weapon.localRotation = playerrot;
    }

    void Roll()
    {
        if (Input.GetButtonDown("Roll"))
        {
            if (Time.time > countroll)
            {
                countroll = Time.time + 1.25f;

                rolled = true;
                anim.SetTrigger("roll");
                StartCoroutine("RollShake");
                hideweapon(); //do end roll staff here
                FixPose(true);
                FixRot(true);

                if (norcurspeed.x == 0 && norcurspeed.y == 0)
                {
                    body.velocity = new Vector2(targetdir[0], targetdir[1]).normalized * 2.5f;
                }
                else
                {
                    RotateToDir(norcurspeed.x, norcurspeed.y);
                    body.velocity = norcurspeed * 2.5f;
                }

                if (rollautoattack && ((curweaponorder == 0 && weaponorder[0, 0] == "melee") || curweaponorder == 1 && weaponorder[1, 0] == "melee"))
                {
                    curweaponanim.SetBool("lightattack", false);
                }
                rolldust.Play();
                rollsound.pitch = 0.8f + Random.Range(-0.1f, 0.1f);
                rollsound.Play();
            }
        }
    }

    IEnumerator RollShake()
    {
        yield return new WaitForSeconds(0.18f);
        screenctrl.ShakeScreen(0.01f);
        yield return new WaitForSeconds(0.18f);
        screenctrl.ShakeScreen(0.005f);
    }

    void Attack()
    {
        if (Input.GetButtonDown("Attack"))
        {
            charging = 0f;

        }
        if (!fixpose)
        {
            if (Input.GetButton("Attack"))
            {
                curweaponanim.SetBool("lightattack", true);
                charging = Mathf.Clamp(charging + (1f * Time.deltaTime * 60), 0, 100);
                //Debug.Log(charging);
            }

        }
        if (Input.GetButtonUp("Attack"))
        {
            if (fixpose && !heavyattacking) 
            {
                curweaponanim.SetTrigger("staterecover");
                charging = 0;
            }
            curweaponanim.SetBool("lightattack", false);
        }
    }

    public void EndLightAttack()
    {
        charging = 0;
    }

    void HeavyAttack()
    {
        if (Input.GetButtonDown("HeavyAttack"))
        {
            charging = 0f;

            if (weaponorder[0,1] == curweaponanim.name)
            {
                curweaponanim.SetBool("special", true);
            }
            curweaponanim.SetBool("heavyattack", true);
            heavyattacking = true;
        }
        if (Input.GetButton("HeavyAttack"))
        {
            curweaponanim.SetBool("heavyattack", true);
            charging = Mathf.Clamp(charging + (1f * Time.deltaTime * 60), 0, 100);
            //Debug.Log(charging);
        }
        if (Input.GetButtonUp("HeavyAttack"))
        {
            curweaponanim.SetBool("heavyattack", false);
        }
    }

    public void EndHeavyAttack()
    {
        curweaponanim.SetBool("special", false);
        curweaponanim.SetBool("heavyattack", false);
        FixPose(false);
        FixRot(false);
        heavyattacking = false;
    }

    public void FixPose(bool isfix) //use for fix player pose when doing sp attack
    {
        if (isfix)
        {
            body.velocity = body.velocity /5;
        }
        else
        {
            if (rolled)
            {
                rolldust.Stop();
                rolled = false;
            }
        }
        fixpose = isfix;
    }

    public void FixRot(bool isrot)
    {
        if (isrot)
        {
           
        }
        fixrot = isrot;
    }

    void SearchPickableWeapon()
    {
        pickableweapon = Physics2D.OverlapCircleAll(transform.position, 0.2f, weaponlayer);
        pickableweaponcount = pickableweapon.Length;

        if (pickableweaponcount > 0)
        {
            closestweapondis = Vector2.Distance(curpos, pickableweapon[0].transform.position);
            closestweaponnum = 0;
            closestweapon = pickableweapon[0].transform;
            for (int i = 1; i < pickableweaponcount; i++)
            {
                newdis = Vector2.Distance(curpos, pickableweapon[i].transform.position);
                if (newdis < closestweapondis)
                {
                    closestweapondis = newdis;
                    closestweaponnum = i;
                }
            }
            closestweapon = pickableweapon[closestweaponnum].transform;
            dropweaponscript = closestweapon.GetComponent<dropweapon>();
            if (preclosestweapon == null)
            {
                preclosestweapon = closestweapon;
            }
            else
            {
                if (preclosestweapon != closestweapon)
                {
                    preclosestweapon.GetComponent<dropweapon>().playerexit();
                    preclosestweapon = closestweapon;
                }
            }

            dropweaponscript.playernear(transform);
        }
        else
        {
            if (preclosestweapon != null)
            {
                preclosestweapon.GetComponent<dropweapon>().playerexit();
                preclosestweapon = null;
            }
        }
    }

    void PickUpWeapon()
    {
        if (Input.GetButtonDown("PickUp"))
        {
            if (pickableweaponcount > 0)
            {
                if (closestweapon.gameObject.layer == 17 || closestweapon.CompareTag("chest")) //17 is Interactable Layer
                {
                    closestweapon.GetComponent<interactable>().Interact(transform);
                    return;
                }

                weaponinfo = dropweaponscript.getweaponinfo();
                
                if (curweaponorder == 2 && weaponorder[2,1] == "normalbody") // if holding a body
                {
                    HoldingBodyDrop();
                }

                if (weaponinfo[0] == "throw")
                {
                    if (weaponinfo[1] == "normalbody")
                    {
                        curweaponanim = weaponlistscript.changeweapon(closestweapon, curweaponanim, weaponinfo[0], weaponinfo[1], 100); // [0][1][2] stand for type, weaponname, weight ,  [3] weight is not need in weaponlist script
                        UpdateThrowWeapon(weaponinfo[0], weaponinfo[1], weaponinfo[2], "100"); // [2] weight is not need in weaponlist script, but used in here
                    }
                    else
                    {
                        if (weaponorder[2, 1] != "empty")
                        {
                            if (weaponinfo[1] == weaponorder[2, 1]) //check is player already have that weapon
                            {
                                fragmentcount += float.Parse(weaponinfo[3]);
                            }
                            else //useless now, because there is only one type = bodyframent
                            {
                                weapondropped = Instantiate(Resources.Load("dropweapon/" + weaponorder[2, 1]), droppt.position, Quaternion.identity) as GameObject;
                                weapondropped.GetComponent<dropweapon>().setdurability(cursecweadurability);
                            }
                        }
                        else
                        {
                            fragmentcount = float.Parse(weaponinfo[3]);
                        }
                        curweaponanim = weaponlistscript.changeweapon(closestweapon, curweaponanim, weaponinfo[0], weaponinfo[1], fragmentcount); // [0][1][2] stand for type, weaponname, weight ,  [3] weight is not need in weaponlist script
                        UpdateThrowWeapon(weaponinfo[0], weaponinfo[1], weaponinfo[2], fragmentcount.ToString()); // [2] weight is not need in weaponlist script, but used in here
                    }
                    
                    anim.SetTrigger("weaponchanged");  //****use to interrupt the attack animation to prevent player attack and changed weapon at the same time******//

                    weaponinbacklistscript.ShowWeaponInBack(weaponorder[curweaponorder, 1]);
                    curweaponorder = 2;
                }
                else //other types, which means that will be places as second weapon
                {
                    /*if (weaponorder[1,1] != "empty")
                    {
                        if (weaponinfo[1] == weaponorder[1, 1]) //check is player already have that weapon
                        {
                            cursecweadurability += float.Parse(weaponinfo[3]);
                        }
                        else
                        {
                            weapondropped = Instantiate(Resources.Load("dropweapon/" + weaponorder[1, 1]), droppt.position, Quaternion.identity) as GameObject;
                            weapondropped.GetComponent<dropweapon>().setdurability(cursecweadurability);
                        }
                    }
                    else
                    {
                        cursecweadurability = float.Parse(weaponinfo[3]);
                    }

                    //curweaponanim = weaponlistscript.changeweapon(closestweapon, curweaponanim, weaponinfo[0], weaponinfo[1], cursecweadurability); // [0][1][2] stand for type, weaponname, weight ,  [3] weight is not need in weaponlist script
                    //anim.SetTrigger("weaponchanged");  //use to interrupt the attack animation to prevent player attack and changed weapon at the same time//

                    weaponinbacklistscript.HideWeaponInBack(weaponorder[1, 1]); //this order can make sure the same name weapon can show
                    UpdateSecondWeapon(weaponinfo[0], weaponinfo[1], weaponinfo[2], cursecweadurability.ToString()); // [2] weight is not need in weaponlist script, but used in here

                    curweaponanim = weaponlistscript.changeweapon(closestweapon, curweaponanim, weaponinfo[0], weaponinfo[1], cursecweadurability);

                    if (curweaponorder == 0) // if player using default/throw weapon, then change to second
                    {
                        weaponinbacklistscript.ShowWeaponInBack(weaponorder[0, 1]);
                    }
                    curweaponorder = 1;*/

                    if (weaponorder[1, 1] == "empty")
                    {
                        if (curweaponorder == 0)
                        {
                            weaponinbacklistscript.ShowWeaponInBack(weaponorder[0, 1]);
                        }
                        cursecweadurability = float.Parse(weaponinfo[3]);
                    }
                    else
                    {
                        if (curweaponorder == 0)
                        {
                            weaponinbacklistscript.HideWeaponInBack(weaponorder[1, 1]);
                            weaponinbacklistscript.ShowWeaponInBack(weaponorder[0, 1]);
                        }
                        else if (curweaponorder == 2)
                        {
                            weaponinbacklistscript.HideWeaponInBack(weaponorder[1, 1]);
                        }

                        if (weaponinfo[1] == weaponorder[1, 1]) //check is player already have that weapon
                        {
                            cursecweadurability += float.Parse(weaponinfo[3]);
                        }
                        else
                        {
                            weapondropped = Instantiate(Resources.Load("dropweapon/" + weaponorder[1, 1]), droppt.position, Quaternion.identity) as GameObject;
                            weapondropped.GetComponent<dropweapon>().setdurability(cursecweadurability);
                        }
                    }
                    UpdateSecondWeapon(weaponinfo[0], weaponinfo[1], weaponinfo[2], cursecweadurability.ToString()); // [2] weight is not need in weaponlist script, but used in here

                    curweaponanim = weaponlistscript.changeweapon(closestweapon, curweaponanim, weaponinfo[0], weaponinfo[1], cursecweadurability);
                    curweaponorder = 1;
                }

                curweaponweight = int.Parse(weaponorder[curweaponorder, 2]);
                curweaponfacing = curweaponanim.GetComponent<weaponfacing>();
            }
        }
    }

    void HoldingBodyDrop()
    {
        curweaponfacing.PutDown();
        if (bodyfragment.childCount > 0)
        {
            string[] tempinfo = bodyfragment.GetChild(0).GetComponent<dropweapon>().getweaponinfo();
            UpdateThrowWeapon(tempinfo[0], tempinfo[1], tempinfo[2], tempinfo[3]);
            Debug.Log(tempinfo);
        }
        else
        {
            UpdateThrowWeapon("empty", "empty", "empty", "empty");
        }
    }

    public void ReloadNextFragment()
    {
        string[] fragmentinfo = bodyfragment.GetChild(bodyfragment.childCount - 1).GetComponent<dropweapon>().getweaponinfo();
        UpdateThrowWeapon(fragmentinfo[0], fragmentinfo[1], fragmentinfo[2], fragmentinfo[3]);
        curweaponweight = float.Parse(fragmentinfo[2]);
    }

    void ChangeWeaponCheck()
    {
        if (Input.GetButtonDown("PreviousWeapon"))
        {
            PreviousWeapon();
        }
        if (Input.GetButtonDown("PosteriorWeapon"))
        {
            PosteriorWeapon();
        }
    }

    void PreviousWeapon()
    {
        bool changed = false;

        int tempordernum = curweaponorder;
        for (int i = 0; i < 2; i++)
        {
            tempordernum--;
            if (tempordernum == -1)
                tempordernum = 2;

            if (weaponorder[tempordernum,0] != "empty")
            {
                //change weapon
                ChangeWeapon(tempordernum);
                changed = true;
                break;
            }
        }

        if (!changed)
            Debug.Log("u have no weapon to change are u moron??????");
    }

    void PosteriorWeapon()
    {
        bool changed = false;
        int tempordernum = curweaponorder;
        for (int i = 0; i < 2; i++)
        {
            tempordernum++;
            if (tempordernum == 3)
                tempordernum = 0;

            if (weaponorder[tempordernum, 0] != "empty")
            {
                //change weapon
                ChangeWeapon(tempordernum);
                changed = true;
                break;
            }
        }

        if (!changed)
            Debug.Log("u have no weapon to change are u moron??????");
    }

    void ChangeWeapon(int weaponnum) //assumpt when this function call, the [weaponnum] weapon is able to be change(i.e. not empty , not 0 durability
    {
        if (weaponnum == 0) //want to change to default weapon
        {
            if (curweaponorder == 1) //is holding second weapon
            {
                weaponinbacklistscript.HideWeaponInBack(weaponorder[0, 1]); //if player holding second, there must be a default in back
                weaponinbacklistscript.ShowWeaponInBack(weaponorder[1, 1]); //put the holding (second) weapon in back
            }
            else if (curweaponorder == 2) //if player holding throw weapon, there is 2 possible state : get two weapon in back / get default weapon in back  == default must in back
            {
                weaponinbacklistscript.HideWeaponInBack(weaponorder[0, 1]);
                if (weaponorder[2,1] == "normalbody")
                {
                    //do sth
                    HoldingBodyDrop();//maybe drop the body and if there are fragments? change to fragments : change to empty
                }
            }
            curweaponorder = 0;
            curweaponanim = weaponlistscript.changeweapon(curweaponanim, weaponorder[0, 0], weaponorder[0, 1], 100); //100 is meaningless, durability is no use for default weapon
            curweaponweight = int.Parse(weaponorder[0, 2]);
        }
        else if (weaponnum == 1)  // want to change to second weapon : here assume second weapon is not empty(in back) 
        {
            if (curweaponorder == 0) //is holding default weapon
            {
                weaponinbacklistscript.ShowWeaponInBack(weaponorder[0, 1]);
                weaponinbacklistscript.HideWeaponInBack(weaponorder[1, 1]);
            }
            else if (curweaponorder == 2) //is holding throw weapon, there must be two weapon in back
            {
                weaponinbacklistscript.HideWeaponInBack(weaponorder[1, 1]);
                if (weaponorder[2, 1] == "normalbody")
                {
                    //do sth
                    HoldingBodyDrop();//maybe drop the body and if there are fragments? change to fragments : change to empty
                }
            }
            curweaponorder = 1;
            curweaponanim = weaponlistscript.changeweapon(curweaponanim, weaponorder[1, 0], weaponorder[1, 1],cursecweadurability);
            curweaponweight = int.Parse(weaponorder[1, 2]);
        }
        else //weaponnum == 2 == want to change to throweapon > body cannot be hide > must be bodyfragment(s)
        {
            weaponinbacklistscript.ShowWeaponInBack(weaponorder[curweaponorder, 1]);

            curweaponorder = 2;
            curweaponanim = weaponlistscript.changeweapon(curweaponanim, weaponorder[2, 0], weaponorder[2, 1],fragmentcount);
            curweaponweight = int.Parse(weaponorder[2, 2]);
        }

        anim.SetTrigger("weaponchanged");
        curweaponfacing = curweaponanim.GetComponent<weaponfacing>();
        curweaponfacing.Show();
    }

    /*void ChangeWeapon()
    {
        if (curweaponorder == 0) //using default weapon , want to switch to second
        {
            if (weaponorder[1,0] != "empty")
            {
                curweaponorder = 1;
                curweaponanim = weaponlistscript.changeweapon(curweaponanim, weaponorder[1,0], weaponorder[1,1]);
                weaponinbacklistscript.ChangeWeaponInBack(weaponorder[0,1]); //send name to that script, only need name now
                curweaponweight = int.Parse(weaponorder[1,2]);
                anim.SetTrigger("weaponchanged");
            }
            else
            {
                Debug.Log("don't have second weapon");
            }
        }
        else  //using second weapon , want to switch to default
        {
            if (weaponorder[1,0] == "throw") //hardcode so9sadddddddddddddddddddddddddddd/////////
            {
                UpdateSecondWeapon("empty", "empty", "empty", "empty");
                Transform throwobject = curweaponanim.transform;
                curweaponfacing.PutDown();
            }
            ////////////////////////////////////////////////////////////////////////////////////////////
            curweaponorder = 0;
            curweaponanim = weaponlistscript.changeweapon(curweaponanim, weaponorder[0,0], weaponorder[0,1]);
            weaponinbacklistscript.ChangeWeaponInBack(weaponorder[1,1]);
            curweaponweight = int.Parse(weaponorder[0,2]);
            anim.SetTrigger("weaponchanged");
        }
        curweaponfacing = curweaponanim.GetComponent<weaponfacing>();
        curweaponfacing.Show();
    }*/

    public void Death()
    {

    }

    public void Exhaust(bool isexhaust)
    {
        curweaponanim.SetTrigger("staterecover");
        charging = 0;
        exhaust = isexhaust;
        if (exhaust)
            gameObject.layer = 17; //interactable layer
        else
            gameObject.layer = 8; //player layer
    }

    public void Scared(bool isscared)
    {

    }

    public bool IsHit()
    {
        return gethit;
    }

    public bool IsScared()
    {
        return scared;
    }

    public void Stun(float time)
    {

    }

    public void PickUp(Transform picker)
    {
        curweaponanim.SetTrigger("staterecover");
        charging = 0;
        picked = true;
    }

    public void PutDown(Vector2 vel)
    {
        picked = false;
        hit(vel);
    }

    IEnumerator ProtectionTime(float time)
    {
        gameObject.layer = 17; //interactable layer
        yield return new WaitForSeconds(time);
        gameObject.layer = 8; //player layer
    }

    public void UpdateDefaultWeapon(string type, string weaponname, string weight)
    {
        weaponorder[0,0] = type;
        weaponorder[0,1] = weaponname;
        weaponorder[0,2] = weight;
    }

    public void UpdateSecondWeapon(string type, string weaponname, string weight, string durability)
    {
        weaponorder[1,0] = type;
        weaponorder[1,1] = weaponname;
        weaponorder[1,2] = weight;
        weaponorder[1,3] = durability;
    }

    public void UpdateThrowWeapon(string type, string weaponname, string weight, string durability)
    {
        weaponorder[2,0] = type;
        weaponorder[2,1] = weaponname;
        weaponorder[2,2] = weight;
        weaponorder[2,3] = durability;
    }

    public void SecondWeaponOutOfAmmo() //later may need to do simialr thing with ThrowWeaponOutOfAmmo()
    {
        UpdateSecondWeapon("empty", "empty", "empty", "empty");
        ChangeWeapon(0);
    }

    public void ThrowWeaponOutOfAmmo()
    {
        if (bodyfragment.childCount > 0)
        {
            string[] tempinfo = bodyfragment.GetChild(0).GetComponent<dropweapon>().getweaponinfo();
            UpdateThrowWeapon(tempinfo[0], tempinfo[1], tempinfo[2], tempinfo[3]);
            ChangeWeapon(2);
        }
        else
        {
            UpdateThrowWeapon("empty", "empty", "empty", "empty");
            ChangeWeapon(0);
        }
    }

    public void hit(Vector2 hitvel)
    {
        gethit = true;
        anim.SetBool("scared", gethit);
        body.velocity += hitvel;
        //body.AddForce(hitvel * 1000f);
    }

    public void hit()
    {
        gethit = true;
        anim.SetBool("scared", gethit);
    }

    void FilpHor()
    {
        cursca = transform.localScale;
        cursca.x *= -1;
        transform.localScale = cursca;
        //weaponinback.localScale = cursca;
        //weapon.localScale = cursca;
    }

    void FilpVer()
    {
        face.SetActive(!face.activeSelf);
    }

    void changedir(int[] dirarr, int firstnum, int secondnum)
    {
        dirarr[0] = firstnum;
        dirarr[1] = secondnum;
    }

    void changedir(int[] dirarr, int[] targetdir)
    {
        dirarr[0] = targetdir[0];
        dirarr[1] = targetdir[1];
    }

    public int[] getcurdir()
    {
        return curdir;
    }

    public Vector2 getmousedir()
    {
        return mousepos;
    }

    public float getcharging()
    {
        return charging;
    }

    public float getcurweaponweight()
    {
        return curweaponweight;
    }

    bool InRange(float num, float min, float max)
    {
        if (num >= min && num <= max)
            return true;
        else
            return false;
    }

    void hideweapon()
    {
        curweaponfacing.Hide();
        gameObject.layer = rollinglayer;
        StartCoroutine("showweapon", 0.55f);
    }

    IEnumerator showweapon(float time)
    {
        yield return new WaitForSeconds(time);

        curweaponfacing.Show();
        gameObject.layer = defaultlayer;

        FixPose(false);
        FixRot(false);
    }

    IEnumerator StopRoll(float time)
    {
        curweaponfacing.Show();
        gameObject.layer = defaultlayer;
        StopCoroutine("RollShake");
        rollsound.Stop();
        yield return new WaitForSeconds(time);
        FixPose(false);
        FixRot(false);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (speedmax)
        {
            if (col.transform.CompareTag("normal"))
            {
                col.gameObject.GetComponent<normalai>().hit();
            }
        }
        if (rolled)
        {
            //if (col.transform.CompareTag("wall"))
            screenctrl.ShakeScreen(0.05f);
            StartCoroutine("StopRoll", 0.5f);
            hit(((Vector2)curpos - col.contacts[0].point).normalized);
        }
    }
}
