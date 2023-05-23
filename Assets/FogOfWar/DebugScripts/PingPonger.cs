using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPonger : MonoBehaviour
{

    public float Speed = 5f;

    private FowRevealer _fowRevealer;

    private Vector3 _originalPosition;
    private float _originalRange;

    private void Start()
    {
        _originalPosition = transform.position;
        _fowRevealer = GetComponent<FowRevealer>();
        _originalRange = _fowRevealer.Range;
    }

    private void Update()
    {
        transform.position = _originalPosition + Vector3.forward * Mathf.Sin(Time.time * Speed) * 10f;

        float newRange = _originalRange + Mathf.Sin(Time.time * 2f) * 3f;
        _fowRevealer.Range = Mathf.FloorToInt(newRange);
    }

}