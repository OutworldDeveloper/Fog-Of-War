using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FogOfWar))]
public sealed class FowManager : MonoBehaviour
{

    private static FogOfWar _activeInstance;

    public static bool HasActiveInstance()
    {
        return _activeInstance != null;
    }

    public static IRevealSource AddSource(Vector3 position, int radius)
    {
        return _activeInstance.AddSource(position, radius);
    }

    public static bool SampleVisibility(Vector3 position, float radius)
    {
        return _activeInstance.SampleVisibility(position, radius);
    }

    public static bool SampleVisibility(Vector3 position)
    {
        return _activeInstance.SampleVisibility(position);
    }

    public static bool SampleVisibility(Vector2Int tile)
    {
        return _activeInstance.SampleVisibility(tile);
    }

    private void Awake()
    {
        if (_activeInstance != null)
        {
            Debug.LogError("There should only be one FowManager in a scene.", _activeInstance);
        }
        else
        {
            _activeInstance = GetComponent<FogOfWar>();
        }
    }

}