using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class enemyai : MonoBehaviour, baseai, interactable {
    //use for getting information//
    public bool isdeath = false;
    public bool turnvel = true;
    [SerializeField]
    private int[] curdir = { 1, -1 };
    public Vector2 cursca, targetdir, zeroVector;
    private GameObject face;
    public Animator anim, curweaponanim;
    public float speedforanim;
    public Rigidbody2D body;
    public Transform healthbar, UI;
    public GameObject stunbar;
    public dropweapon dropweaponscript;
    public Collider2D[] cols;
    public int colcount;
    //***************************//

    //use for weapon issues//
    public float attackrange;
    public Transform weapon, hand;
    //*********************//

    //private int[] targetdir = { 1, -1 }; //use 1,-1 to simulate the x,y-coordinate (1 for +, -1 for -)

    //current state of this ai//
    public bool gethit = false, isstun = false, settled = false, picked = false, scared = false;
    public float curstuntime = 0, endstuntime = 0;
    public Transform colobject;
    //************************//

    //vision//
    public Transform closesttarget,hidetarget;
    //******//

    public void Awake()
    {
        zeroVector = Vector2.zero;
        face = transform.Find("face").gameObject;
        body = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        UI = transform.Find("UI");
        healthbar = UI.Find("healthbar");
        stunbar = UI.Find("stunbar").gameObject;
        weapon = transform.Find("weapon");
        dropweaponscript = GetComponent<dropweapon>();
        cols = GetComponents<Collider2D>();
        colcount = cols.Length;
        hand = transform.Find("hand");
        cursca = transform.localScale;
    }

    public int[] getcurdir()
    {
        return curdir;
    }

    public Vector2 gettargetdir()
    {
        return targetdir;
    }

    public void checkdir(Vector2 targetdir)
    {
        this.targetdir = targetdir;
        if (Mathf.Sign(targetdir.x) != curdir[0])
        {
            FilpHor();
            curdir[0] *= -1;
        }
        if (Mathf.Sign(targetdir.y) != curdir[1])
        {
            if (turnvel)
            {
                FilpVer();
            }
            curdir[1] *= -1;
        }
    }

    virtual public void FilpHor()
    {
        cursca = transform.localScale;
        cursca.x *= -1;
        transform.localScale = cursca;
        UI.localScale = cursca;
        //weapon.localScale = cursca;
    }

    virtual public void FilpVer()
    {
        face.SetActive(!face.activeSelf);
    }

    void dirrepair()
    {
        if (Mathf.Sign(transform.localScale.x) != curdir[0])
        {
            FilpHor();
        }
    }

    public void hit(Vector2 hitvel, Transform hitter)
    {
        gethit = true;
        anim.SetBool("scared", gethit);
        body.velocity = hitvel;
        closesttarget = hitter;
    }

    public void hit(Vector2 hitvel)
    {
        gethit = true;
        anim.SetBool("scared", gethit);
        body.velocity = hitvel;
    }

    public void hit()
    {
        gethit = true;
        anim.SetBool("scared", gethit);
    }

    public void updatedir(int [] newdir)
    {
        changedir(curdir, newdir);
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
    
    public bool InRange(float num, float min, float max)
    {
        if (num >= min && num <= max)
            return true;
        else
            return false;
    }

    public void updateattackrange(string weaponname)
    {
        attackrange = weapon.Find("attackrange").Find(weaponname).GetComponent<CircleCollider2D>().radius;
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
    
    abstract public void Interact(Transform player);

    abstract public void Scared(bool isscared);

    abstract public void CheckWillScared(float damage, Vector2 hitvel);

    abstract public void DropWeapon();

    virtual public void PickUp(Transform picker)
    {
        picked = true;

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
        if (!isdeath)
        {
            if (!scared)
            {
                gameObject.layer = LayerMask.NameToLayer("Enemy");
                //dropweaponscript.enabled = false;
            }
        }

        dirrepair();
    }

    public void Death()
    {
        isdeath = true;
        endstuntime = 0f;
        isstun = false;
        stunbar.SetActive(false);
        gameObject.layer = LayerMask.NameToLayer("Deadbody");
        //dropweaponscript.enabled = true;
        closesttarget = null;
        hidetarget = null;
        DropWeapon();
    }

    public void Rescued()
    {
        isdeath = false;
        Stun(2f);
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        //dropweaponscript.enabled = false;
        dirrepair();
    }

    public void Rescued(Vector2 hitvel)
    {
        isdeath = false;
        Stun(2f);
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        //dropweaponscript.enabled = false;
        hit(hitvel);
        dirrepair();
    }

    public bool IsScared()
    {
        return scared;
    }

    public bool IsHit()
    {
        return gethit;
    }

    virtual public void OnCollisionEnter2D(Collision2D col)
    {
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
                colobject.GetComponent<enemyai>().hit();
                return;
            }
            if (colobject.CompareTag("wall"))
            {
                Debug.Log(body.velocity);
                Stun(Mathf.Max(Mathf.Abs(body.velocity.x), Mathf.Abs(body.velocity.y)) / 2); //add stun time
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


/*
    void checkdir(Vector2 targetdir)
    {
        curdir = getcurdir();
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
        updatedir(curdir);
    }

    void FilpHor()
    {
        cursca = transform.localScale;
        cursca.x *= -1;
        transform.localScale = cursca;
        //weapon.localScale = cursca;
    }

    void FilpVer()
    {
        face.SetActive(!face.activeSelf);
    }

    */