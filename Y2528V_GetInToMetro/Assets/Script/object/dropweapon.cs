using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dropweapon : MonoBehaviour {

    private Vector2 zeroVector;

    //**let player get the info of this weapon**//
    public string type, weaponname, weight, durability = "100";
    public int weaponnum; // seem useless rightnow
    private string[] weaponinfo;
    //******************************************//

    //**use for player to change weapon**//
    private bool canpick = false;
    private Transform nearplayer;
    private SpriteRenderer keypressimg;
    //***********************************//

    //**use for ctrl pick up issues**//
    private Collider2D[] cols;
    private int colcount;
    private Rigidbody2D body;
    //*******************************//

	// Use this for initialization
    void Awake()
    {
        zeroVector = Vector2.zero;
        cols = GetComponents<Collider2D>();
        colcount = cols.Length;
        body = GetComponent<Rigidbody2D>();
        keypressimg = transform.Find("UI/keyimg").GetComponent<SpriteRenderer>();
        weaponinfo = new string[4];
        weaponinfo[0] = type;
        weaponinfo[1] = weaponname;
        weaponinfo[2] = weight;
        weaponinfo[3] = durability;
    }

	void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        if (canpick)
        {
            if (nearplayer == null)
            {
                playerexit();
            }
            //Vector2.Distance(transform.position, nearplayer.position) > pickuprange
        }
    }

    public void playernear(Transform player)
    {
        canpick = true;
        nearplayer = player;
        keypressimg.enabled = canpick;
    } 

    public void playerexit()
    {
        canpick = false;
        nearplayer = null;
        keypressimg.enabled = canpick;
    }

    public string[] getweaponinfo()
    {
        return weaponinfo;
    }

    public float getweight()
    {
        return float.Parse(weight);
    }

    public void setdurability(float num)
    {
        durability = num.ToString();

        Debug.Log(durability);
    }

    public void PickUp(Transform picker)
    {
        if (CompareTag("normal") || CompareTag("normal enemy") || CompareTag("Player"))
        {
            GetComponent<baseai>().PickUp(picker);
        }

        for (int i = 0; i < colcount; i++)
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
        //************************************//
    }


    public void PutDown(Vector2 vel)
    {
        body.isKinematic = false;
        for (int i = 0; i < colcount; i++)
        {
            cols[i].enabled = true;
        }
        transform.parent = null;
        
        if (CompareTag("normal") || CompareTag("normal enemy") || CompareTag("Player"))
        {
            GetComponent<baseai>().PutDown(vel);
        }
        else
        {
            body.velocity = vel;
        }
    }

}
