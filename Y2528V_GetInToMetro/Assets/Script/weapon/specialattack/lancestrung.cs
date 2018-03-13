using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lancestrung : MonoBehaviour {

    private Transform colobject, strungpos;
    private List<Transform> strunglist = new List<Transform>();
    private int strungcount;

	// Use this for initialization
	void Awake () {
        strungpos = transform.parent.Find("lance/bodypos");
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ThrowOut(Vector2 vel)
    {
        strungcount = strunglist.Count;
        if (strungcount > 0)
        {
            for (int i = strungcount - 1; i >= 0; i--)
            {
                strunglist[i].GetComponent<health>().Hurt(1000f);
                strunglist[i].GetComponent<dropweapon>().PutDown(vel);
                strunglist.RemoveAt(i);
            }
        }
    }

    public int StrungCount()
    {
        return strunglist.Count;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        colobject = col.transform;
        if (colobject.gameObject.layer == 8) //is player
        {
            strunglist.Add(colobject);
            colobject.GetComponent<dropweapon>().PickUp(strungpos);
        }
    }
}
