using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rightwall : MonoBehaviour {

    private Transform colobject;
    private enemyai enemyscript;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter2D(Collision2D col)
    {
        colobject = col.transform;
        if (colobject.CompareTag("normal enemy"))
        {
            enemyscript = colobject.GetComponent<enemyai>();
            if (enemyscript.IsScared() && !enemyscript.IsHit())
            {
                colobject.tag = "runningenemy";
                colobject.gameObject.layer = 18;
            }
        }
    }
}
