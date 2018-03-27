using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyweaponlist : MonoBehaviour {

    //** getting some normal info**//
    private Vector3 startpos, curpos, zerovector;
    private Animator anim;
    //*****************************//

    //** showing current weapon info **//
    public GameObject curweapon;
    //public Animator curweaponanim;
    //*********************************//

    //** use for melee weapon **//
    private Transform melee;
    private int meleecount;
    //**************************//

    //** use for changing weapon **//
    private int foundmeleenum;
    private Transform weapontochange;
    private string[] meleeattackmode = { "swing", "stab" };
    //*****************************//

    // Use this for initialization
    void Awake()
    {
        anim = transform.root.GetComponent<Animator>();
        zerovector = Vector3.zero;
        startpos = transform.localPosition;
        curpos = startpos;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Animator changeweapon(Transform dropweapon, Animator curweaponanim, string type, string weaponname) //use to pick up drop weapon
    {
        Debug.Log(type + " " + weaponname);

        weapontochange = transform.Find(type).Find(weaponname);
        if (weapontochange == null)
        {
            Debug.Log("this character can't equip this weapon");
            return curweaponanim; //return the original animator if fail to change weapon
        }
        else
        {
            if (curweapon != null)
            {
                curweapon.SetActive(false); //set the old weapon object disable first
            }
            curweapon = weapontochange.gameObject;
            curweapon.SetActive(true);
            curpos = startpos + curweapon.GetComponent<enemyweaponfacing>().getstartpos();
        }

        transform.localPosition = curpos;
        return weapontochange.GetComponent<Animator>();
        //Destroy(dropweapon.root.gameObject);
    }

    public Animator changeweapon(Animator curweaponanim, string type, string weaponname)
    {
        Debug.Log(type + " " + weaponname);

        weapontochange = transform.Find(type).Find(weaponname);
        if (weapontochange == null)
        {
            Debug.Log("this character can't equip this weapon");
            return curweaponanim; //return the original animator if fail to change weapon
        }
        else
        {
            if (curweapon != null)
            {
                curweapon.SetActive(false);
            }
            curweapon = weapontochange.gameObject;
            curweapon.SetActive(true);
            curpos = startpos + curweapon.GetComponent<enemyweaponfacing>().getstartpos();

            Debug.Log(curweapon.GetComponent<enemyweaponfacing>().getstartpos().x);
        }

        transform.localPosition = curpos;
        return weapontochange.GetComponent<Animator>();
    }

}