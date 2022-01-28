using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    public float speed = 2; 
    void FixedUpdate() 
    {
        Vector3 moveVector = new Vector3();
        if (Input.GetKey(KeyCode.A))
        {
            moveVector += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveVector += new Vector3(1, 0, 0);
        }
        if (Input.GetKey(KeyCode.W))
        {
            moveVector += new Vector3(0, 1, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveVector += new Vector3(0, -1, 0);
        }   
        transform.position += moveVector.normalized * speed * Time.deltaTime;
    }
}
