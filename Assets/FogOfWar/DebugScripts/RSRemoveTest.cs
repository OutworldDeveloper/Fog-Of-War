using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RSRemoveTest : MonoBehaviour
{

    [SerializeField] private FogOfWar _fow;

    private IRevealSource _revealSource;

    private void Start()
    {
        _revealSource = _fow.AddSource(transform.position, 10);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J) == true)
        {
            _revealSource.Kill();
        }
    }

}