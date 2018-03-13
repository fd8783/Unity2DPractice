using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chargebounce : MonoBehaviour {

    public float inbounce, outbounce;

    private Rigidbody2D parentbody;
    private movementctrl parentscript;

	// Use this for initialization
	void Awake () {
        parentbody = transform.root.GetComponent<Rigidbody2D>();
        parentscript = transform.root.GetComponent<movementctrl>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void BounceIn()
    {
        parentbody.velocity = parentscript.getmousedir() * -1 * inbounce;
    }

    public void BounceOut()
    {
        parentbody.velocity = parentscript.getmousedir() * outbounce;
    }
}
