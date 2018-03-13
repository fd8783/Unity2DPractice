using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class stagectrl : MonoBehaviour {

    public int levelfilltarget, curfill, totalpassengers;
    public float leveltime = 180f; //default 3mins

    public static int carcanfill = 40; //size of cars
    public int carcanfillshowed = 40;

    [Range(0,1)]
    public float[] carstartfillpercent; //use 0.x to present

    private int carcount;

    private GameObject train;

	// Use this for initialization
	void Awake () {
        carcanfill = carcanfillshowed;

        train = GameObject.Find("train");
        carcount = carstartfillpercent.Length;
        for (int i = 0; i < carcount; i++)
        {
            train.transform.Find("carctrl").GetChild(i).GetComponent<carctrl>().setfill((int)(carcanfill * carstartfillpercent[i]));
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
