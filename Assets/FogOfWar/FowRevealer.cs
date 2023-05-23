using UnityEngine;

public sealed class FowRevealer : MonoBehaviour
{

    [SerializeField] private FogOfWar _fow;

    public int Range = 10;
    private IRevealSource _source;

    private void OnEnable()
    {
        _source?.Enable();
    }

    private void Start()
    {
        _source = _fow.AddSource(transform.position, Range);
    }

    private void Update()
    {
        _source.Move(transform.position);
        _source.UpdateRadius(Range);
    }

    private void OnDestroy()
    {
        _source.Kill();
    }

    private void OnDisable()
    {
        _source.Disable();
    }

}