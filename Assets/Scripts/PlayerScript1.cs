using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading;
using System;


public class PlayerScript1 : MonoBehaviour
{
    public GameObject playerHurt;

    Rigidbody2D rb;
    // float dirX;

    [SerializeField]
    float moveSpeed = 5f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     void dead()
    {
        Instantiate(playerHurt, transform.position, Quaternion.identity);


    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "DeathStar")
        {
            Destroy(gameObject);
            dead();  
            // Thread.Sleep(1000);
            SceneManager.LoadScene("Dead1");
            
        }
        else if(collision.tag == "FinishFlag")
        {
            SceneManager.LoadScene("Win0");
        }

        

    }
}
