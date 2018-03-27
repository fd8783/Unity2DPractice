using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class catapultfire : MonoBehaviour {

    private Transform shadow, aim;
    private Rigidbody2D stoneRB;
    private Animator anim, aimanim;

    private bool fired, aiming, aimdone;
    private Vector2 shadowpos = Vector2.zero, aimcurpos;
    private float trainposx;

    // Use this for initialization
    void Awake () {
        shadow = transform.parent.Find("shadow");
        aim = transform.parent.Find("aim");
        aimanim = aim.GetComponent<Animator>();
        stoneRB = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        trainposx = GameObject.Find("train").transform.position.x + 0.1f;
	}

    void Start()
    {
        
    }
	
	// Update is called once per frame
	void Update () {
        aimcurpos = aim.position;

        if (!aimdone)
        {
            AimMove();
        }
        else
        {
            if (!fired)
            {
                Fire();
            }
            ShadowMove();
        }

	}

    void ShadowMove()
    {
        shadowpos.x = transform.localPosition.x;
        shadow.localPosition = shadowpos;
    }

    void AimMove()
    {
        if (aim.position.x > trainposx)
        {
            aimcurpos.x -= 0.05f;

            aim.position = aimcurpos;
                
        }
        else
        {
            if (!aiming)
            {
                aimanim.SetBool("aiming", true);
                aiming = true;
                StartCoroutine("AimForSecond", 2f);
            }
        }
    }

    IEnumerator AimForSecond(float time)
    {
        yield return new WaitForSeconds(time);
        aimanim.SetBool("aiming", false);
        aimanim.SetBool("aimdone", true);

        yield return new WaitForSeconds(1f);
        aimdone = true;
        aiming = false;
    }

    void Fire()
    {
        stoneRB.isKinematic = false;
        stoneRB.velocity = new Vector2(-10f, 0f);
        fired = true;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("train"))
        {
            anim.SetTrigger("crush");
            shadow.GetComponent<SpriteRenderer>().enabled = false;
            col.transform.root.GetComponent<objecthealth>().Hurt(1f);
            screenctrl.StopScreen(0.06f, 0.15f);
            Destroy(transform.parent.gameObject, 1f);
        }
    }

}
