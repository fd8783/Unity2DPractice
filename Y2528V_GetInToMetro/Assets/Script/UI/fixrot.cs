using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fixrot : MonoBehaviour {

    private Vector3 startsca; // just fix rot now, fix sca in ai script's filp function
    [SerializeField]
    private Quaternion startrot = Quaternion.Euler(0,0,0);

	// Use this for initialization
	void Start () {
        //startrot = transform.rotation;
        startrot.w = 1;
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.rotation.z != 0)
        {
            transform.rotation = startrot;
        }
	}
}
