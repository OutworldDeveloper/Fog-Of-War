using UnityEngine;

public sealed class FowRevealer : MonoBehaviour
{

    public int Range = 10;
    private IRevealSource _source;

    private void OnEnable()
    {
        _source?.Enable();
    }

    private void Start()
    {
        if (FowManager.HasActiveInstance() == false)
        {
            Debug.LogError("There's no active FowManager in the scene.");
            Debug.DebugBreak();
        }

        _source = FowManager.AddSource(transform.position, Range);
    }

    private void Update()
    {
        _source.Move(transform.position);
        _source.UpdateRadius(Range);
    }

    private void OnDestroy()
    {
        _source?.Kill();
    }

    private void OnDisable()
    {
        _source?.Disable();
    }

}