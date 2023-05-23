using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public sealed class GenericEffect : MonoBehaviour
{

    [SerializeField] private Shader _shader;
    [SerializeField] private Texture2D _fogTexture;

    private Material _material;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (_material == null)
        {
            _material = new Material(_shader);
            _material.SetTexture("_FogTexture", _fogTexture);
        }

        Graphics.Blit(source, destination, _material);
    }


}