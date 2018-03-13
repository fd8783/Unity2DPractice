using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class carctrl : MonoBehaviour {
    public int carnum = 1;

    private SpriteRenderer[] carimg;
    private int carimgcount;
        
    private int targetfill;
    
    private int fillcount = 0;
    private float slowpercent = 0f;
    private Transform fillbar;
    private Image barinside;

    private bool isarrive = false;
    private Animator anim;
    private ableincar incarscript;

    //for bouncing out people when enemy get in car
    private LayerMask normallayer;
    public string spawnwho;
    private GameObject dooreffector, spawnman;
    private Vector2 effectorpos;
    private float effectorradius;
    private bool bouncing;
    private Collider2D[] targets;
    private int targetnum;
    private normalai normalscript;

    // Use this for initialization
    void Awake()
    {
        anim = GetComponent<Animator>();
        fillbar = transform.Find("fillbar");
        barinside = fillbar.Find("bar").GetComponent<Image>();
        carimg = GetComponentsInChildren<SpriteRenderer>();
        carimgcount = carimg.Length;
        for (int i = 0; i < carimgcount; i++)
        {
            carimg[i].sortingOrder -= (carnum - 1) * 2;
        }
        dooreffector = transform.Find("effector").gameObject;
        effectorradius = dooreffector.GetComponent<CircleCollider2D>().radius;
        spawnman = Resources.Load(spawnwho) as GameObject;
        normallayer = (1 << 9); // normal layer

    }

    void Start()
    {
        targetfill = stagectrl.carcanfill;
    }

    // Update is called once per frame
    void Update()
    {
        if (trainctrl.arrived)
        {
            if (isarrive == false)
            {
                effectorpos = dooreffector.transform.position; //settle
            }
            isarrive = true;
            anim.SetBool("dooropen", true);
        }
        else
        {
            isarrive = false;
            anim.SetBool("dooropen", false);
        }
        barupdate();
    }

    void barupdate()
    {
        barinside.fillAmount = ((float)fillcount/targetfill);
        if (barinside.fillAmount > 0.5)
        {
            if (barinside.fillAmount > 0.8)
            {
                if (barinside.fillAmount >= 1)
                {
                    slowpercent = 90;
                }
                else
                {
                    slowpercent = 75;
                }
            }
            else
            {
                slowpercent = 40;
            }
        }
    }

    public void getonein(Transform body, bool isdeath)
    {
        if (body.CompareTag("normal"))
        {
            if (isdeath)
            {

            }
            else
            {
                fillcount++;
            }
        }
        else if (body.CompareTag("normal enemy"))
        {
            if (isdeath)
            {

            }
            else
            {
                bouncing = true;
                StartCoroutine("BounceOut", 3);
                fillcount -= 3;
            }
        }
    }

    IEnumerator BounceOut(int num)
    {
        GetInCarOut();
        SpawnNormal(num);
        dooreffector.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        dooreffector.SetActive(false);
        bouncing = false;
    }

    void GetInCarOut()
    {
        targets = Physics2D.OverlapCircleAll(effectorpos, effectorradius, normallayer);
        targetnum = targets.Length;
        if (targetnum > 0)
        {
            for (int i = 0; i < targetnum; i++)
            {
                Debug.Log(targets[i]);
                targets[i].GetComponent<normalai>().getoutcar();
                targets[i].GetComponent<normalai>().hit(new Vector2(Random.Range(1.0f, 1.5f), Random.Range(-0.5f, 0.5f)));
            }
        }
    }

    void SpawnNormal(int spawnnum)
    {
        for (int i = 0; i < spawnnum; i++)
        {
            normalscript = Instantiate(spawnman, new Vector2(effectorpos.x + 0.1f, effectorpos.y + Random.Range(-0.1f, 0.1f)), Quaternion.identity).GetComponent<normalai>();
            normalscript.Scared(true);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!bouncing)
        {
            if (col.CompareTag("normal") || col.CompareTag("normal enemy"))
            {
                incarscript = col.GetComponent<ableincar>();
                if (incarscript != null)
                {
                    incarscript.getincar(transform.gameObject, slowpercent);
                }
            }
        }
    }

    public void setfill(int num)
    {
        fillcount = num;
    }
}
