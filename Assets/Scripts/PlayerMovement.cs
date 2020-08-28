using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public CharacterController2D controller;

    public Animator animator;

    public float runSpeed = 40f;

    float horizontalMove = 0f;

    bool jump = false;

    bool isDead = false;


    


    // // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // Update is called once per frame
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;

        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        // Check if player pressed jump
        if(Input.GetButtonDown("Jump"))
        {
            jump = true;
            animator.SetBool("isJumping", true);
        }
    
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Coin"))
        {
            Destroy(other.gameObject);
        }
    }


    // When player touches ground, lands
    public void OnLanding()
    {
        animator.SetBool("isJumping", false);
    }

    void FixedUpdate()
    {
        //Move Player

        controller.Move(horizontalMove * Time.fixedDeltaTime, false, jump);
        jump = false;

    }
}
