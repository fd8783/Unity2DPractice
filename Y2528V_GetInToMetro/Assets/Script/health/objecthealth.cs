using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objecthealth : MonoBehaviour {

    public float maxhp = 3f;
    private float hp;

    private bool destroyed;
	// Use this for initialization
    void Awake()
    {
        hp = maxhp;
    }

	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        if (hp <= 0f)
        {
            if (!destroyed)
            {
                Crush();
            }
        }
    }

    public void Hurt(float damage)
    {
        hp = Mathf.Max(0f, hp - damage);
        if (CompareTag("train"))
        {
            GetComponent<trainctrl>().Hurt();
        }
    }

    public void Crush()
    {
        destroyed = true;
        if (CompareTag("train"))
        {
            Debug.Log("train crush");
            return;
        }
    }
}
