using System;
using UnityEngine;

public class FowVisibilityReceiver : MonoBehaviour
{

    public event Action VisionGained;
    public event Action VisionLost;

    [SerializeField] private FogOfWar _fogOfWar;
    [SerializeField] private float _radius;

    private float _lastSeenTime;
    public bool IsActuallyVisible { get; private set; } // TODO: Rename

    private void Update()
    {
        bool wasActuallyVisible = IsActuallyVisible;
        bool isVisible = _fogOfWar.SampleVisibility(transform.position, _radius);

        if (isVisible == true)
        {
            _lastSeenTime = Time.time;

            if (wasActuallyVisible == false)
            {
                IsActuallyVisible = true;
                VisionGained?.Invoke();
            }
        }
        else
        {
            if (Time.time > _lastSeenTime + 0.4f)
            {
                if (wasActuallyVisible == true)
                {
                    IsActuallyVisible = false;
                    VisionLost?.Invoke();
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        int intRadius = Mathf.CeilToInt(_radius);
        var centerTile = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.z));
        var boundsOriginTile = centerTile - Vector2Int.one * intRadius;

        for (int x = 0; x < intRadius * 2 + 1; x++)
        {
            for (int y = 0; y < intRadius * 2 + 1; y++)
            {
                if (IsPointInsideCircle(intRadius, intRadius, x, y, _radius) == true)
                {
                    var tilePosition = boundsOriginTile + new Vector2Int(x, y);
                    var isVisible = _fogOfWar.SampleVisibility(tilePosition);

                    Gizmos.color = isVisible ? Color.green : Color.red;
                    Vector3 worldPosition = new Vector3(tilePosition.x + 0.5f, 0.0f, tilePosition.y + 0.5f);
                    Gizmos.DrawCube(worldPosition, Vector3.one);
                }
            }
        }

        bool IsPointInsideCircle(int centerX, int centerY, int x, int y, float radius)
        {
            float dx = centerX - x;
            float dy = centerY - y;
            float distance_squared = dx * dx + dy * dy;
            return distance_squared <= radius * radius;
        }
    }

}