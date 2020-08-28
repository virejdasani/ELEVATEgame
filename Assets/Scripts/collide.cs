using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class collide : MonoBehaviour
{
    public GameObject hurt;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Hit");
        GameObject h = Instantiate(hurt) as GameObject;
        h.transform.position = transform.position;


    }
}
