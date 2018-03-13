using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weaponinbacklist : MonoBehaviour {     //this script mainly use to update the weapon facing in back

    public int weaponcanhold = 2;

    private Transform[] weaponinback;

    private string[] curbackweaponname;
    private Transform weapontochange;


    //private GameObject[] childs;
    //private string[] childsname;
    //private int childcount;

    // Use this for initialization
    void Awake()
    {
        /*childcount = transform.childCount;
        childs = new GameObject[childcount];
        childsname = new string[childcount];
        for (int i =0; i < childcount; i++)
        {
            childs[i] = transform.GetChild(i).gameObject;
            childsname[i] = transform.GetChild(i).name;
        }*/

        weaponinback = new Transform[weaponcanhold];
        for (int i =0; i < weaponcanhold; i++)
        {
            weaponinback[i] = transform.GetChild(i);
        }

        curbackweaponname = new string[weaponcanhold];
        for (int i =0; i <weaponcanhold; i++)
        {
            curbackweaponname[i] = "empty";
        }


    }

    // Update is called once per frame
    void Update()
    {


    }

    public void ShowWeaponInBack(string name)
    {
        /*bool changed = false;
        for (int i = 0; i < childcount; i++)
        {
            if (!changed)
            {
                if (childsname[i] == name)
                {
                    childs[i].gameObject.SetActive(true);
                    changed = true;
                }
                else
                {
                    childs[i].gameObject.SetActive(false);
                }
            }
            else
            {
                childs[i].gameObject.SetActive(false);
            }
        }
        if (!changed)
        {
            Debug.Log("changeweaponinback failed!");
        }*/

        bool showed = false;

        for (int i =0; i < weaponcanhold; i++)
        {
            if (curbackweaponname[i] == "empty")
            {
                weapontochange = weaponinback[i].Find(name);
                if (weapontochange == null)
                {
                    Debug.Log("changeweaponinback failed!");
                }
                else
                {
                    weapontochange.gameObject.SetActive(true);
                    weaponinback[i].Find(curbackweaponname[i]).gameObject.SetActive(false);
                    curbackweaponname[i] = name;
                    showed = true;
                }
                break;
            }
        }
        if (!showed)
        {
            Debug.Log("show weapon in back fail");
        }
    }

    public void HideWeaponInBack(string name)
    {
        bool hided = false;

        for (int i = weaponcanhold-1; i >= 0; i--) //if there are two same weapon, hide the last one first(if 2, then hide the 2nd)
        {
            if (curbackweaponname[i] == name)
            {
                weapontochange = weaponinback[i].Find(name);
                if (weapontochange == null) //may don't need to check this in this case
                {
                    Debug.Log("changeweaponinback failed!");
                }
                else
                {
                    weapontochange.gameObject.SetActive(false);
                    weaponinback[i].Find("empty").gameObject.SetActive(true);
                    curbackweaponname[i] = "empty";
                    hided = true;
                }
                break;
            }
            Debug.Log(curbackweaponname[i]);
        }
        if (!hided)
        {
            Debug.Log("hide weapon in back fail");
        }
    }

    public void HideAllWeaponInBack()
    {
        for (int i = 0; i < weaponcanhold; i++) //if there are two same weapon, hide the last one first(if 2, then hide the 2nd)
        {
            weapontochange = weaponinback[i].Find(curbackweaponname[i]);
            if (weapontochange == null) //may don't need to check this in this case
            {
                Debug.Log("changeweaponinback failed!");
            }
            else
            {
                weapontochange.gameObject.SetActive(false);
                weaponinback[i].Find("empty").gameObject.SetActive(true);
                curbackweaponname[i] = "empty";
            }
        }
    }

    public string[] CurBackWeapon()
    {
        return curbackweaponname;
    }

    public string CurBackWeapon(int num)
    {
        return curbackweaponname[num];
    }
}
