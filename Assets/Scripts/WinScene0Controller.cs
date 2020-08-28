using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;



public class WinScene0Controller : MonoBehaviour
{
    

    public void NextLevel1()
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
