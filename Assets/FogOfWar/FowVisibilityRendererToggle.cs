using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FowVisibilityReceiver))]
public class FowVisibilityRendererToggle : MonoBehaviour
{

    [SerializeField] private Renderer[] _renderers;

    private FowVisibilityReceiver _visibilityReceiver;

    private void Awake()
    {
        _visibilityReceiver = GetComponent<FowVisibilityReceiver>();
    }

    private void Start()
    {
        if (_visibilityReceiver.IsActuallyVisible)
        {
            OnVisionGained();
        }
        else
        {
            OnVisionLost();
        }
    }

    private void OnEnable()
    {
        _visibilityReceiver.VisionGained += OnVisionGained;
        _visibilityReceiver.VisionLost += OnVisionLost;
    }

    private void OnDisable()
    {
        _visibilityReceiver.VisionGained -= OnVisionGained;
        _visibilityReceiver.VisionLost -= OnVisionLost;
    }

    private void OnVisionLost()
    {
        foreach (var renderer in _renderers)
        {
            renderer.enabled = false;
        }
    }

    private void OnVisionGained()
    {
        foreach (var renderer in _renderers)
        {
            renderer.enabled = true;
        }
    }

}