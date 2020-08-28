using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveScript : MonoBehaviour {

	Rigidbody2D rb;
	float dirX;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {
		dirX = Input.GetAxis ("Horizontal");
	}

	void FixedUpdate()
	{
		rb.velocity = new Vector2 (dirX * 5, 0);
	}
}
