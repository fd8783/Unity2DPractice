using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIimgfollowing : MonoBehaviour {

    //**use to let UI follow the img**//
    public Transform img;
    private Vector3 imgstartpos, UIstartpos;
    //********************************//

    // Use this for initialization
    void Start () {
        imgstartpos = img.localPosition;
        UIstartpos = transform.localPosition;
    }
	
	// Update is called once per frame
	void Update () {
        Follow();
    }

    void Follow()
    {
        transform.localPosition = UIstartpos + (img.localPosition - imgstartpos);
        //Debug.Log(transform.position+" "+ transform.localPosition+" "+weaponimg.position+" "+weaponimg.localPosition);
    }
}
