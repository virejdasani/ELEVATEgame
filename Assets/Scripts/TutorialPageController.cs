using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialPageController : MonoBehaviour
{
    public void OKLevel0()
    {
        SceneManager.LoadScene("Level0");
    }
    public void BackToLevelSelect()
    {
        SceneManager.LoadScene("LevelSelect");
    }
    
}
