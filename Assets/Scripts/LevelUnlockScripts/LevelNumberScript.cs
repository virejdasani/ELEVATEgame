using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelNumberScript : MonoBehaviour {

	Text text;
	int sceneIndex;

	// Use this for initialization
	void Start () {
		text = GetComponent<Text> ();
		sceneIndex = SceneManager.GetActiveScene ().buildIndex;
	}
	
	// Update is called once per frame
	void Update () {
		text.text = "Level " + sceneIndex;
	}
}
