using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class randomsuit : MonoBehaviour {

    public Sprite[] hats;
    public Sprite[] clothings;

    private Transform hat, clothing;
    private SpriteRenderer hatimg, clothingimg;

	// Use this for initialization
	void Awake () {
        hat = transform.Find("hat");
        clothing = transform.Find("clothing/cloth");
        hatimg = hat.GetComponent<SpriteRenderer>();
        clothingimg = clothing.GetComponent<SpriteRenderer>();

        hatimg.sprite = hats[Random.Range(0, hats.Length)];
        clothingimg.sprite = clothings[Random.Range(0, clothings.Length)];
    }
	
	// Update is called once per frame
	void Update () {
	}
}
