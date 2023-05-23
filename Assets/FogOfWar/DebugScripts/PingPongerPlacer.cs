using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPongerPlacer : MonoBehaviour
{

    [SerializeField] private PingPonger _original;
    [SerializeField] private int _count = 1;
    [SerializeField] private float _spacing = 4;

    private void Start()
    {
        for (int i = 0; i < _count; i++)
        {
            var pingPonger = Instantiate(_original, new Vector3(-i * _spacing, 0f, 0f), Quaternion.identity, transform);
            pingPonger.Speed = Random.Range(4, 6);
        }

        _original.gameObject.SetActive(false);
    }

}