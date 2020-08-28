using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainManuControlScript : MonoBehaviour {

	public Button level0Button, level01Button, level02Button;
	int levelPassed;

	// Use this for initialization
	void Start () {

		// To make the level avalaible
		levelPassed = PlayerPrefs.GetInt ("LevelPassed");
		level0Button.interactable = true;
		level01Button.interactable = false;
		level02Button.interactable = false;

		switch (levelPassed) {
		case 1:
			level01Button.interactable = true;
			break;
		case 2:
			level01Button.interactable = true;
			level02Button.interactable = true;
			break;
			


		}
		
	}

	public void Level0Select()
		{
			SceneManager.LoadScene("Tutorial");
		}

	public void BackToMainMenu()
	{
		SceneManager.LoadScene("MainMenu");
	}	
	
	
	// public void levelToLoad (int level)
	// {
	// 	SceneManager.LoadScene (level);
	// }


}
