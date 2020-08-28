using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Threading;

public class PlayerScript : MonoBehaviour
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
        // dirX = Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime;
        // transform.position = new Vector2(transform.position.x + dirX, transform.position.y);

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
            SceneManager.LoadScene("Dead0");
            
        }
        else if(collision.tag == "FinishLine")
        {
            SceneManager.LoadScene("Win1");
        }
        else if(collision.tag == "DeathPlatform")
        {
            Destroy(gameObject);
            dead();
            SceneManager.LoadScene("Dead0");
        }

    }
    
}
