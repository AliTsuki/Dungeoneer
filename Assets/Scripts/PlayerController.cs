﻿using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float MoveSpeed = 1f;
    public Vector2 Velocity = new Vector2();
    private Rigidbody2D rigidBody;
    private CapsuleCollider2D playerCollider;
    private PlayerInput input;

    // Start is called before the first frame update
    private void Start()
    {
        this.input = this.transform.GetComponent<PlayerInput>();
        this.rigidBody = this.transform.GetComponent<Rigidbody2D>();
        this.playerCollider = this.transform.GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        this.ProcessMovement();
    }

    //
    private void ProcessMovement()
    {
        this.Velocity = new Vector2(this.input.Horizontal, this.input.Vertical);
        if(this.Velocity.magnitude > 1f)
        {
            this.Velocity = this.Velocity.normalized; 
        }
        this.Velocity *=  this.MoveSpeed;
        this.rigidBody.velocity = this.Velocity;
    }
}
