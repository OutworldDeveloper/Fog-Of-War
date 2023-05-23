using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacter : MonoBehaviour
{

    private CharacterController _characterController;
    private Vector3 _velocity;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        var input = GatherInput();
        var desiredVelocity = new Vector3(input.x, 0f, input.y) * 5f;

        _velocity = Vector3.MoveTowards(_velocity, desiredVelocity, 10f * Time.deltaTime);

        _characterController.Move((_velocity + Vector3.down * 9.8f) * Time.deltaTime);
    }

    private Vector2 GatherInput()
    {
        var input = Vector2.zero;

        if (Input.GetKey(KeyCode.W) == true)
        {
            input.y += 1;
        }

        if (Input.GetKey(KeyCode.S) == true)
        {
            input.y -= 1;
        }

        if (Input.GetKey(KeyCode.A) == true)
        {
            input.x -= 1;
        }

        if (Input.GetKey(KeyCode.D) == true)
        {
            input.x += 1;
        }

        return input;
    }

}