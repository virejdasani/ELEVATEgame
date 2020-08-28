﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour{
    public Transform player;
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    // Start is called before the first frame update
    void Start(){
        rb = this.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update(){
        if (player != null)
        {
            Vector3 direction = player.position - transform.position;
            // float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            // rb.rotation = angle;
            direction.Normalize();
            movement = direction;
        }
    
    }
    private void FixedUpdate() {
        if (player != null)
        {
            moveCharacter(movement);

        }
        
    
    }
    
    void moveCharacter(Vector2 direction){

        if (player != null){
            rb.MovePosition((Vector2)transform.position + (direction * moveSpeed * Time.deltaTime));
        }
    }
}
