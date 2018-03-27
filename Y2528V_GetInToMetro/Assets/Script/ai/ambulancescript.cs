using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ambulancescript : MonoBehaviour {
    public int maxpatient = 1;
    public float rescuetime = 3f;

    private Vector3 zeroVector;

    private bool isfull = false;
    [SerializeField]
    private int curpatient = 0;
    private Vector2 randomhitvector;

    private List<Transform> patients = new List<Transform>();
    [SerializeField]
    private float time;
    [SerializeField]
    private List<float> endrescuetime = new List<float>();

    //for checking the deathbody entered
    private List<Transform> DeadBodyList = new List<Transform>();
    private Transform DeadBodyGot;
    private int DeadBodyLayer;

    //for pickup and down//
    private Transform carryingbody;

    // Use this for initialization
    void Awake()
    {
        Random.seed = System.Guid.NewGuid().GetHashCode();
        DeadBodyLayer = LayerMask.NameToLayer("Deadbody");
        zeroVector = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        CheckRescue();
        time = Time.time;
    }

    void CheckRescue()
    {
        while (curpatient < maxpatient && DeadBodyList.Count > 0) // still can get more patients
        {
            AddPatients(DeadBodyList[0]);
        }
        
        for (int i = 0; i < curpatient; i++)
        {
            if (Time.time > endrescuetime[i])
            {
                PatientRescued(patients[i]);
                i--; //we use list, size reduce when we remove item from that list
            }
        }
    }

    public void AddPatients(Transform patient)
    {
        patients.Add(patient);
        endrescuetime.Add(Time.time + rescuetime);

        curpatient++;

        isfull = curpatient == maxpatient ? true : false;

        DeadBodyList.Remove(patient);
        PickUpBody(patient);
    }

    public void PatientRescued(Transform patient)
    {
        endrescuetime.RemoveAt(patients.IndexOf(patient));
        patients.Remove(patient);
        curpatient--;

        isfull = curpatient == maxpatient ? true : false;

        PutDown(patient);

        patient.GetComponent<health>().Rescued(); // put it at last to do the Scared(false) 
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
            return;
        }
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == DeadBodyLayer)
        {
            DeadBodyGot = col.transform;
            DeadBodyList.Add(DeadBodyGot);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer == DeadBodyLayer)
        {
            DeadBodyGot = col.transform;
            if (DeadBodyList.Contains(DeadBodyGot))
            {
                DeadBodyList.Remove(DeadBodyGot);
            }
        }
    }
}
