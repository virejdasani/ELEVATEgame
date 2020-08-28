using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CupScript : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D col)
	{
		LevelControlScript.instance.youWin ();
	}
}
