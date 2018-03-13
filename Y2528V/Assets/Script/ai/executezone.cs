using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class executezone : MonoBehaviour {

    public int maxprisoners = 1;
    public float executetime = 0.35f, reloadtime = 0.6f;
    private bool ablefornext = true;

    private Vector3 zeroVector;
    private Animator anim;

    private bool isfull = false;
    private int curprisoner = 0;
    private Vector2 randomhitvector;

    private List<Transform> deadprisoners = new List<Transform>();
    [SerializeField]
    private float time;
    [SerializeField]
    private List<float> endexecutetime = new List<float>();

    //for checking the deathbody entered
    private List<Transform> DeadBodyList = new List<Transform>();
    private Transform DeadBodyGot;
    private int DeadBodyLayer, EnemyLayer;

    //for pickup and down//
    private Rigidbody2D bodyRB;
    private Transform carryingbody;
    private Collider2D[] bodycols;
    private float colcount;

    // Use this for initialization
    void Awake()
    {
        Random.seed = System.Guid.NewGuid().GetHashCode();
        DeadBodyLayer = LayerMask.NameToLayer("Deadbody");
        EnemyLayer = LayerMask.NameToLayer("Enemy");
        zeroVector = Vector3.zero;
        anim = transform.root.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckExecute();
        time = Time.time;
    }

    void CheckExecute()
    {
        if (ablefornext)
        {
            while (curprisoner < maxprisoners && DeadBodyList.Count > 0) // still can get more patients
            {
                AddPrisoner(DeadBodyList[0]);
            }

            for (int i = 0; i < curprisoner; i++)
            {
                if (Time.time > endexecutetime[i])
                {
                    PrisonerExecuted(deadprisoners[i]);
                    ablefornext = false;
                    StartCoroutine("reloading", reloadtime);
                    break; //execute one at a time
                }
            }
        }
    }

    IEnumerator reloading(float time)
    {
        yield return new WaitForSeconds(time);
        ablefornext = true;
    }

    

    public void AddPrisoner(Transform prisoner)
    {
        deadprisoners.Add(prisoner);
        endexecutetime.Add(Time.time + executetime);

        curprisoner++;
        if (curprisoner == maxprisoners)
        {
            isfull = true;
        }
        DeadBodyList.Remove(prisoner);
        PickUpBody(prisoner);
        anim.SetTrigger("execute");
    }

    public void PrisonerExecuted(Transform prisoner)
    {

        endexecutetime.RemoveAt(deadprisoners.IndexOf(prisoner));
        deadprisoners.Remove(prisoner);
        curprisoner--;
        PutDown(prisoner);
    }

    public void PickUpBody(Transform body) // only use for throw body
    {
        carryingbody = body;
        if (carryingbody.CompareTag("normal") || carryingbody.CompareTag("normal enemy"))
        {
            carryingbody.GetComponent<dropweapon>().PickUp(transform.Find("laypos"));
            return;
        }
    }

    public void PutDown(Transform body)
    {
        carryingbody = body;
        randomhitvector = new Vector2(Random.Range(-0.7f, 0.7f), Random.Range(0.2f, 1f)).normalized;
        randomhitvector *= 6f;

        if (carryingbody.CompareTag("normal") || carryingbody.CompareTag("normal enemy"))
        {
            carryingbody.GetComponent<dropweapon>().PutDown(randomhitvector);
        }

        carryingbody.GetComponent<health>().Crushed(Random.Range(2f, 4f));
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == DeadBodyLayer)
        {
            DeadBodyGot = col.transform;
            DeadBodyList.Add(DeadBodyGot);
        }
        else if (col.CompareTag("normal enemy"))
        {
            DeadBodyGot = col.transform;
            DeadBodyList.Add(DeadBodyGot);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer == DeadBodyLayer || col.CompareTag("normal enemy"))
        {
            DeadBodyGot = col.transform;
            if (DeadBodyList.Contains(DeadBodyGot))
            {
                DeadBodyList.Remove(DeadBodyGot);
            }
        }
    }
}
