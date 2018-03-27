using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponfacing : MonoBehaviour {  //this script mainly use to update the direction the weapon facing
                                               // now it may involved in some attack control too 
    public bool canmoveinheavy = false, rotateinheavy = false;
    public bool isdefault = false; //means infinite durability
    public float outammointervel;
    public float durability = 0;
    public bool havebullet;

    private Vector3 startpos, zeroVector;
    public string bulletpath;
    private GameObject bullet;
    private Rigidbody2D bulletRB;
    private Transform bulletpt;

    private Transform parent;
    private movementctrl parentscript;
    private SpriteRenderer[] sprite;
    private int spritecount;

    private int[] curdir = { 1, -1 }, targetdir = { 1, -1 };

    //for throw body//
    private Transform carryingbody, bodypos;
    //private Rigidbody2D bodyRB;
    //private int colcount;
    //private Collider2D[] bodycols;
    private float bodyweight;
    //**************//

    // Use this for initialization
    void Awake()
    {
        startpos = transform.localPosition;
        transform.localPosition = Vector3.zero;
        parent = transform.parent.parent.parent;
        parentscript = parent.GetComponent<movementctrl>();
        sprite = GetComponentsInChildren<SpriteRenderer>();
        spritecount = sprite.Length;
        zeroVector = Vector3.zero;
        bodypos = transform.Find("bodypos");
        if (havebullet)
        {
            bulletpt = transform.Find("bulletoutpt");
            bullet = Resources.Load(bulletpath) as GameObject;
        }
    }

	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        changedir(targetdir, parentscript.getcurdir());
        if (curdir[1] != targetdir[1])
        {
            FilpVer(targetdir[1]);
        }
        changedir(curdir, targetdir);
    }

    void FilpVer(int dir)
    {
        for (int i =0; i < spritecount; i++)
        {
            sprite[i].sortingOrder -= (dir * 15);
        }
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

    public void Hide()
    {
        for (int i = 0; i < spritecount; i++)
        {
            sprite[i].enabled = false;
        }
    }

    public void Show()
    {
        for (int i = 0; i < spritecount; i++)
        {
            sprite[i].enabled = true;
        }
    }

    public void HeavyAttack()
    {
        if (canmoveinheavy)
        {
            if (rotateinheavy)
            {

            }
            else
            {
                parentscript.RotateToDir(0, 0);
                parentscript.FixRot(true);
            }
        }
        else
        {
            parentscript.FixPose(true);
            if (rotateinheavy)
            {

            }
            else
            {
                parentscript.RotateToDir(0, 0);
                parentscript.FixRot(true);
            }
        }
    }

    public Vector3 getstartpos()
    {
        return startpos;
    }

    public void EndHeavyAttack()
    {
        parentscript.EndHeavyAttack();
    }

    public void EndLightAttack()
    {
        parentscript.EndLightAttack();
    }

    public void Shoot()
    {
        bullet = Instantiate(Resources.Load(bulletpath), bulletpt.position, transform.rotation) as GameObject;
        bullet.GetComponent<stabin>().Settle(parentscript.getmousedir() * (3f + 5f *(parentscript.getcharging() / 100)));
        durability -= 100;
        if (durability <= 0)
        {
            SecondWeaponOutOfAmmo();
        }
    }

    public void PickUpBody(Transform body) // only use for throw body
    {
        carryingbody = body;
        if (carryingbody.CompareTag("normal") || carryingbody.CompareTag("normal enemy") || carryingbody.CompareTag("bodyfragment"))
        {
            carryingbody.GetComponent<dropweapon>().PickUp(transform.Find("bodypos"));
            return;
        }
    }

    public void ThrowOut()
    {
        bodyweight = parentscript.getcurweaponweight();
        if (carryingbody.CompareTag("normal") || carryingbody.CompareTag("normal enemy") || carryingbody.CompareTag("bodyfragment"))
        {
            carryingbody.GetComponent<dropweapon>().PutDown(parentscript.getmousedir() * ((40 / bodyweight) + (parentscript.getcharging() / 100) * (40 / bodyweight)));

            durability -= 100;
            return;
        }
    }

    public void PutDown()
    {
        bodyweight = parentscript.getcurweaponweight();
        if (carryingbody.CompareTag("normal") || carryingbody.CompareTag("normal enemy") || carryingbody.CompareTag("bodyfragment"))
        {
            carryingbody.GetComponent<dropweapon>().PutDown(parentscript.getmousedir() * ((60 / bodyweight)));
            return;
        }

    }

    public void SecondWeaponOutOfAmmo()
    {
        parentscript.SecondWeaponOutOfAmmo();
    }

    public void ThrowWeaponOutOfAmmo()
    {
        if (durability <= 0)
        {
            parentscript.ThrowWeaponOutOfAmmo();
        }
        else
        {
            if (carryingbody.CompareTag("bodyfragment"))
            {
                ReloadNextFragment();
            }
        }
    }

    public void ReloadNextFragment()
    {
        carryingbody = bodypos.GetChild(bodypos.childCount - 1);
        parentscript.ReloadNextFragment();
    }

    public void SetDurability(float num)
    {
        durability = num;

        Debug.Log(durability);
    }

    public float GetDurability()
    {
        return durability;
    }
}



/*
    public void PickUpBody(Transform body) // only use for throw body
    {
        carryingbody = body;
        if (carryingbody.CompareTag("normal"))
        {
            carryingbody.GetComponent<normalai>().getoutcar();
        }
        bodyRB = carryingbody.GetComponent<Rigidbody2D>();
        bodycols = carryingbody.GetComponents<Collider2D>();
        colcount = bodycols.Length;

        for (int i = 0; i<colcount; i++)
        {
            bodycols[i].enabled = false;
        }
        
        carryingbody.parent = transform.Find("bodypos");
        bodyRB.isKinematic = true;

        // set pos rot to zero (include force)//
        bodyRB.velocity = zeroVector;
        carryingbody.localPosition = zeroVector;
        bodyRB.angularVelocity = 0f;
        carryingbody.localRotation = Quaternion.identity;
        ////////////////////////////////////////
    }

        public void Throwout()
    {
        bodyweight = parentscript.getcurweaponweight();
        bodyRB.isKinematic = false;
        for (int i = 0; i < colcount; i++)
        {
            bodycols[i].enabled = true;
        }
        carryingbody.parent = null;
        bodyRB.velocity = parentscript.getmousedir() * ((40 / bodyweight) + (parentscript.getcharging() / 100) * (40 / bodyweight));
    }

    public void PutDown()
    {
        bodyweight = parentscript.getcurweaponweight();
        bodyRB.isKinematic = false;
        for (int i = 0; i < colcount; i++)
        {
            bodycols[i].enabled = true;
        }
        carryingbody.parent = null;
        bodyRB.velocity = parentscript.getmousedir() * ((40 / bodyweight));
    }

        IEnumerator OutOfAmmo(float intervel)
    {
        yield return new WaitForSeconds(intervel);
        Debug.Log("dfs");
        SecondWeaponOutOfAmmo();
    }
*/
