using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseTrigger : MonoBehaviour
{

    [SerializeField] private Game _game;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == _game.PlayerCharacter)
        {
            Debug.Log("Noise!");
            _game.MakeNoise(transform.position, 40f);
        }
    }

}