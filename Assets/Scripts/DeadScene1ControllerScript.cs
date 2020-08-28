using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class DeadScene1ControllerScript : MonoBehaviour
{
    public void TryAgain()
    {
        SceneManager.LoadScene("Level01");
    }
    public void QuitGame()
    {
        Debug.Log("Application quit");
        Application.Quit();
    }
    public void BackToLevelSelect()
    {
        SceneManager.LoadScene("LevelSelect");
    }
}
