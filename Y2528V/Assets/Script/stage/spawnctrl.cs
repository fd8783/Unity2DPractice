using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spawnctrl : MonoBehaviour {

    public string spawnwho; //path to load from Resources
    public int totalspawn;
    public float[] timeslot;
    public int[] spawnnum;

    // calculate the spawn thing
    private float gamestarttime;
    public int curspawned;
    private float spawnintervel;
    private int numofslot, thisslotspawnnum;
    [SerializeField]
    private float[] spawntime;
    /////////////////////////

    private bool isarrive = false;
    private bool readyforspawn = true;
    private Vector2 respawnpt;

    private Transform tspawnpt, dspawnpt;
    private float tspawnpty, dspawnpty;
    private GameObject spawnman;
    private Transform train;

    // Use this for initialization
    void Awake()
    {
        Random.seed = System.Guid.NewGuid().GetHashCode();

        numofslot = spawnnum.Length;
        spawntime = new float[totalspawn];
        for (int i = 0; i < numofslot; i++)
        {
            spawnintervel = ((timeslot[i + 1] - timeslot[i])/ spawnnum[i]);
            thisslotspawnnum = curspawned + spawnnum[i];
            for (int j =curspawned; j < thisslotspawnnum; j++)
            {
                spawntime[j] = Mathf.Clamp(timeslot[i] + ((j - curspawned) *spawnintervel) + (spawnintervel * Random.Range(-1f, 1f)), timeslot[i], timeslot[i+1]);
            }
            curspawned += spawnnum[i]; //can't do ++ int j for loop because j for loop need to use (j - curspawned)
        }
        curspawned = 0;
    }

    void Start()
    {
        tspawnpt = transform.Find("tspawnpt");
        dspawnpt = transform.Find("dspawnpt");
        tspawnpty = tspawnpt.position.y;
        dspawnpty = dspawnpt.position.y;
        spawnman = Resources.Load(spawnwho) as GameObject;
        respawnpt.x = transform.position.x;
        gamestarttime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = curspawned; i < totalspawn; i++)
        {
            if (Time.time -gamestarttime > spawntime[curspawned])
            {
                Spawn();
            }
            else
            {
                break;
            }
        }
    }

    void Spawn()
    {
        respawnpt.y = Random.Range(tspawnpty, dspawnpty);
        Instantiate(spawnman, respawnpt, Quaternion.identity);
        curspawned++;
    }

    IEnumerator waitsec(float time)
    {
        yield return new WaitForSeconds(time);
        respawnpt.y = Random.Range(tspawnpty, dspawnpty);
        Instantiate(spawnman, respawnpt, Quaternion.identity);
        readyforspawn = true;
    }
}
