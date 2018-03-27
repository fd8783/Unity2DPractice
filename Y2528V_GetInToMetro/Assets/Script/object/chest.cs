using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class chest : MonoBehaviour, interactable {

    public GameObject[] droplist;
    [Range(0f, 1f)]
    public float[] probability; // use accumulate method, all of them need to be sum up as 1

    private int arrlength, choosenum;
    private float RandomNumber;
    
    // Use this for initialization
    void Awake () {
        arrlength = droplist.Length;
	}
	
	// Update is called once per frame
	void Update () {
    }

    public void Interact(Transform interacter)
    {
        RandomNumber = Random.Range(0f, 1f);
        if (arrlength > 0)
        {
            for (int i = 0; i < arrlength; i++)
            {
                if (RandomNumber < probability[i])
                {
                    choosenum = i;
                    break;
                }
                else
                {
                    RandomNumber -= probability[i];
                }
            }
            Instantiate(droplist[choosenum], transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("fuck u u forget to put dropable item into this chest u fking idiot");
        }

    }
}
