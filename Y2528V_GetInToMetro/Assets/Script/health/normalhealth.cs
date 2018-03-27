using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class normalhealth : MonoBehaviour,health {

    //*****get normal info******//
    private Animator anim;
    private Transform hat, head, face, clothing, body, leg; // use for crushing body
    private LayerMask BodyFragmentLayer;
    private MonoBehaviour[] allscript;
    private int scriptcount;
    private Rigidbody2D headRB, bodyRB;
    private Vector2 randomvel;
    //**************************//

    public float maxhp = 100;

    [SerializeField]
    private float hp; //curhp

    [SerializeField]
    private bool isdeath = false, isexhaust = false;
    
    private GameObject healthbar;
    private Image bar;
    private float howmuchgreen = 110;

    private normalai normalscript;
    private enemyai enemyscript;

	// Use this for initialization
    void Awake()
    {
        BodyFragmentLayer = LayerMask.NameToLayer("Bodyfragment");
        healthbar = transform.Find("UI/healthbar").gameObject;
        bar = healthbar.transform.Find("bar").GetComponent<Image>();
        bar.color = Color.HSVToRGB((howmuchgreen / 359), 1, (float)200 / 255);
        anim = GetComponent<Animator>();
        hat = transform.Find("hat");
        head = transform.Find("head");
        face = transform.Find("face");
        clothing = transform.Find("clothing");
        body = transform.Find("body");
        leg = transform.Find("leg");
        headRB = head.GetComponent<Rigidbody2D>();
        bodyRB = body.GetComponent<Rigidbody2D>();
    }

	void Start () {
        recover();
        healthbarupdate();
        if (CompareTag("normal"))
        {
            normalscript = transform.GetComponent<normalai>();
        }
        else if (CompareTag("normal enemy"))
        {
            enemyscript = transform.GetComponent<enemyai>();
        }
    }
	
	// Update is called once per frame
	void Update () {
		if (!isdeath)
        {
            if (hp <= 0)
            {
                Death();
            }
        }
        anim.SetBool("isdeath", isdeath); //now, if disable/re-enable a object reset it's anim's parameters, so i do it at update
    }

    public void Hurt(float damage)
    {
        if (!isdeath)
        {
            hp = Mathf.Max(0f, hp - damage);
            healthbarupdate();
            if (CompareTag("normal"))
            {
                if (damage >= 40)
                {
                    normalscript.Scared(true);
                }
            }
        }
    }

    void healthbarupdate()
    {
        if (hp < maxhp)
        {
            if (hp == 0)
            {
                healthbar.SetActive(false);
            }
            else
            {
                healthbar.SetActive(true);
            }
        }
        else
        {
            healthbar.SetActive(false);
        }
        bar.fillAmount = hp / maxhp;
        bar.color = Color.HSVToRGB((howmuchgreen *(hp/ maxhp) / 359), 1, (float)200 / 255);
    }
    
    void Death()
    {
        if (!isdeath)
        {
            isdeath = true;
            if (CompareTag("normal"))
            {
                normalscript.Death();
            }
            else if (CompareTag("normal enemy"))
            {
                enemyscript.Death();
            }
        }
    }

    public void Rescued()
    {
        isdeath = false;
        recover();
        if (CompareTag("normal"))
        {
            normalscript.Rescued();
        }
        else if (CompareTag("normal enemy"))
        {
            enemyscript.Rescued();
        }
    }

    public void Rescued(Vector2 hitvel)
    {
        isdeath = false;
        recover();
        if (CompareTag("normal"))
        {
            normalscript.Rescued(hitvel);
        }
        else if (CompareTag("normal enemy"))
        {
            enemyscript.Rescued(hitvel);
        }
    }

    public bool IsDeath()
    {
        return isdeath;
    }

    public void recover()
    {
        hp = maxhp;
    }

    public void Crushed(float coefficient)
    {
        //seprate to head and body two parts
        //head & hat & face
        hat.parent = head;
        face.parent = head;
        head.gameObject.layer = BodyFragmentLayer;
        head.tag = "bodyfragment";
        head.GetComponent<CircleCollider2D>().enabled = true;
        headRB.isKinematic = false;
        head.parent = null;

        //body & leg & clothing
        clothing.parent = body;
        leg.parent = body;
        body.gameObject.layer = BodyFragmentLayer;
        body.tag = "bodyfragment";
        body.GetComponent<CircleCollider2D>().enabled = true;
        bodyRB.isKinematic = false;
        body.parent = null;

        //disable all script
        allscript = gameObject.GetComponents<MonoBehaviour>();
        scriptcount = allscript.Length;
        for (int i = 0; i < scriptcount; i++)
        {
            allscript[i].enabled = false;
        }

        //if needed, bounce them out
        if (coefficient > 0)
        {
            randomvel = Random.insideUnitCircle * coefficient;
            headRB.velocity = randomvel;
            randomvel = Random.insideUnitCircle * coefficient;
            bodyRB.velocity = randomvel;
        }
        Destroy(gameObject);
    }
}
