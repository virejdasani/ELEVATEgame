using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullScript : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D col)
	{
		LevelControlScript.instance.youLose ();
	}
}
