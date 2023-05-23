using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class FogOfWar : MonoBehaviour
{

    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private bool _shouldBlur;
    [SerializeField] private float _blendSpeed = 0.1f;
    [SerializeField] private ComputeShader _fowBlenderShader;
    [SerializeField] private ComputeShader _fowCalculatorShader;
    [SerializeField] private ComputeShader _fowCombinerShader;

    [SerializeField] private int _gridSize = 1024;
    [SerializeField] private int _revealSourceMaxRange = 20;

    private List<RevealSource> _revealSources = new List<RevealSource>();
    private RenderTexture _currentFowRT;
    private RenderTexture _bluredFowRT;
    private Color[] _wallGrid;
    private Texture2D _wallsTexture;
    private float _lastFowUpdateTime;

    private Vector2Int FowTextureResolution => new Vector2Int(_gridSize, _gridSize);

    private void Start()
    {
        _wallsTexture = new Texture2D(_gridSize, _gridSize, TextureFormat.R8, false, true);
        _wallsTexture.filterMode = FilterMode.Point;

        _wallGrid = new Color[_gridSize * _gridSize];

        // Bake walls into texture

        for (int x = 0; x < _gridSize; x++)
        {
            for (int y = 0; y < _gridSize; y++)
            {
                var tile = new Vector2Int(x - _gridSize / 2, y - _gridSize / 2);
                var samplePosition = new Vector3(tile.x + 0.5f, 0.5f, tile.y + 0.5f);

                bool isBlocker = Physics.CheckBox(samplePosition, Vector3.one * 0.5f, Quaternion.identity, _layerMask);

                if (isBlocker == true)
                {
                    var gridIndex = ConvertTileToGridIndex(tile.x, tile.y);
                    _wallGrid[gridIndex].r = 1;
                }
            }
        }

        _wallsTexture.SetPixels(_wallGrid);
        _wallsTexture.Apply(false, true);

        //

        _fowCalculatorShader.SetTexture(0, "Walls", _wallsTexture);

        Vector2Int origin = Vector2Int.zero - FowTextureResolution / 2;
        Shader.SetGlobalVector("_TestOrigin", new Vector2(origin.x, origin.y));

        _bluredFowRT = new RenderTexture(FowTextureResolution.x, FowTextureResolution.y, 0);

        _currentFowRT = new RenderTexture(FowTextureResolution.x, FowTextureResolution.y, 0);
        _currentFowRT.autoGenerateMips = false;
        _currentFowRT.enableRandomWrite = true;
        _currentFowRT.format = RenderTextureFormat.R8;
        _currentFowRT.filterMode = FilterMode.Point;
        _currentFowRT.Create();

        _fowBlenderShader.SetVector("Resolution", (Vector2)FowTextureResolution);
        _fowBlenderShader.SetTexture(0, "Result", _currentFowRT);
        _fowBlenderShader.SetFloat("BlendSpeed", _blendSpeed);

        _fowCombinerShader.SetInt("MaxRevealRadius", _revealSourceMaxRange);
        _fowCalculatorShader.SetInt("MaxRevealRadius", _revealSourceMaxRange);

        Shader.SetGlobalTexture("_TestFogText", _shouldBlur ? _bluredFowRT : _currentFowRT);
        Shader.SetGlobalVector("_FowTextureRes", (Vector2)FowTextureResolution);
    }

    private void Update()
    {
        if (Time.time > _lastFowUpdateTime + 0.1f)
        {
            _lastFowUpdateTime = Time.time;

            // Remove dead reveal sources
            for (int i = 0; i < _revealSources.Count; i++)
            {
                var index = _revealSources.Count - 1 - i;
                var source = _revealSources[index];

                if (source.IsDead == true)
                {
                    _revealSources.RemoveAt(index);
                }
            }

            for (int i = 0; i < _revealSources.Count; i++)
            {
                var source = _revealSources[i];

                if (source.IsEnabled == false)
                {
                    continue;
                }

                if (source.IsDirty == true)
                {
                    CalculateFow(source);
                    source.IsDirty = false;
                }
            }
        }

        UpdateCurrentFowRT(); // TODO: limit fps of this
    }

    public IRevealSource AddSource(Vector3 position, int radius)
    {
        var visibilityTexture = new RenderTexture(_revealSourceMaxRange * 2, _revealSourceMaxRange * 2, 0);
        visibilityTexture.autoGenerateMips = false;
        visibilityTexture.enableRandomWrite = true;
        visibilityTexture.format = RenderTextureFormat.R8;
        visibilityTexture.filterMode = FilterMode.Point;
        visibilityTexture.Create();

        var source = new RevealSource(position, radius, visibilityTexture);
        _revealSources.Add(source);
        return source;
    }

    public bool SampleVisibility(Vector3 position, float radius)
    {
        int intRadius = Mathf.CeilToInt(radius);
        var centerTile = GetTile(position);
        var boundsOriginTile = centerTile - Vector2Int.one * intRadius;

        for (int x = 0; x < intRadius * 2 + 1; x++)
        {
            for (int y = 0; y < intRadius * 2 + 1; y++)
            {
                if (IsPointInsideCircle(intRadius, intRadius, x, y, radius) == true)
                {
                    var isVisible = SampleVisibility(boundsOriginTile + new Vector2Int(x, y));

                    if (isVisible == true)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool SampleVisibility(Vector3 position)
    {
        var tile = GetTile(position);
        return SampleVisibility(tile);
    }

    public bool SampleVisibility(Vector2Int tile)
    {
        for (int i = 0; i < _revealSources.Count; i++)
        {
            var source = _revealSources[i];

            if (source.IsEnabled == false)
            {
                continue;
            }

            var isInRange = IsPointInsideCircle(source.Tile.x, source.Tile.y, tile.x, tile.y, source.Radius);

            if (isInRange == true)
            {
                if (Raycast(source.Tile, tile) == false)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsPointInsideCircle(int centerX, int centerY, int x, int y, float radius)
    {
        float dx = centerX - x;
        float dy = centerY - y;
        float distance_squared = dx * dx + dy * dy;
        return distance_squared <= radius * radius;
    }

    private bool SampleWall(Vector2Int tile)
    {
        var gridIndex = ConvertTileToGridIndex(tile.x, tile.y);
        return _wallGrid[gridIndex].r > 0;
    }

    private void UpdateCurrentFowRT()
    {
        // First we combine all reveal source's textures into one
        var tempRT = RenderTexture.GetTemporary(FowTextureResolution.x, FowTextureResolution.y, 0,
            UnityEngine.Experimental.Rendering.GraphicsFormat.R32_SInt,
            1);
        tempRT.enableRandomWrite = true;
        tempRT.Create();

        _fowCombinerShader.SetTexture(0, "Result", tempRT);

        for (int i = 0; i < _revealSources.Count; i++)
        {
            var source = _revealSources[i];

            if (source.IsEnabled == false)
            {
                continue;
            }

            _fowCombinerShader.SetTexture(0, "AddedTexture", source.VisibilityTexture);
            var gridCoords = ConvertTileToGridCoords(source.LastCalculationTile);
            _fowCombinerShader.SetInts("SourcePosition", gridCoords.x, gridCoords.y);

            _fowCombinerShader.Dispatch(0, _revealSourceMaxRange * 2 / 8, _revealSourceMaxRange * 2 / 8, 1);
        }

        // Then we smoothly move from current fow texture into the new one
        _fowBlenderShader.SetTexture(0, "DesiredFow", tempRT);

        _fowBlenderShader.SetFloat("DeltaTime", Time.deltaTime);
        _fowBlenderShader.Dispatch(0, FowTextureResolution.x / 8, FowTextureResolution.y / 8, 1);

        RenderTexture.ReleaseTemporary(tempRT);

        // And finaly we blur the result if needed
        if (_shouldBlur == true)
        {
            SimpleBlur.Blur(_currentFowRT, _bluredFowRT, 1, true);
        }
    }

    private int ConvertTileToGridIndex(int x, int y)
    {
        x += _gridSize / 2;
        y += _gridSize / 2;
        return y * _gridSize + x;
    }

    public Vector2Int ConvertTileToGridCoords(Vector2Int tile) // Not sure?
    {
        var result = new Vector2Int();

        result.x = tile.x + _gridSize / 2;
        result.y = tile.y + _gridSize / 2;

        return result;
    }

    private void CalculateFow(RevealSource source)
    {
        source.LastCalculationTile = source.Tile;

        _fowCalculatorShader.SetTexture(0, "Result", source.VisibilityTexture);

        var gridCoords = ConvertTileToGridCoords(source.Tile);
        _fowCalculatorShader.SetVector("SourcePosition", (Vector2)gridCoords);
        _fowCalculatorShader.SetFloat("SourceRadius", source.Radius);

        _fowCalculatorShader.Dispatch(0, _revealSourceMaxRange * 2 / 8, _revealSourceMaxRange * 2 / 8, 1);
    }

    private bool Raycast(Vector2Int originTile, Vector2Int destinationTile)
    {
        Vector2 rayOrigin = originTile + Vector2.one * 0.5f;
        Vector2 rayDirection = destinationTile + Vector2.one * 0.5f - rayOrigin;
        rayDirection = rayDirection.normalized;

        int safety = 0;
        float currentDistance = 0;
        Vector2Int currentTile = originTile;

        while (currentTile != destinationTile)
        {
            currentTile = Vector2Int.FloorToInt(rayOrigin + rayDirection * currentDistance);
            currentDistance += 0.05f;

            bool isWall = SampleWall(currentTile);

            if (isWall)
            {
                return true;
            }

            safety++;

            if (safety > 500)
            {
                break;
            }
        }

        return false;
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();

        if (false)
        {
            GUILayout.Label("Walls texture:");
            GUILayout.Label(_wallsTexture, GUILayout.Width(512), GUILayout.Height(512));
        }

        GUILayout.Label("Displayed fow:");
        GUILayout.Label(_currentFowRT, GUILayout.Width(512), GUILayout.Height(512));

        if (true)
        {
            int maxDisplayed = Mathf.Min(4, _revealSources.Count);
            for (int i = 0; i < maxDisplayed; i++)
            {
                var source = _revealSources[i];

                GUILayout.Label($"Source #{i}");
                GUILayout.Label(source.VisibilityTexture);
            }
        }

        GUILayout.EndVertical();
    }

    private static Vector2Int GetTile(Vector3 worldPosition)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPosition.x), Mathf.FloorToInt(worldPosition.z));
    }

    private class RevealSource : IRevealSource
    {
        public RevealSource(Vector3 position, int radius, RenderTexture visibilityTexture)
        {
            Tile = GetTile(position);
            Radius = radius;
            IsEnabled = true;
            IsDead = false;
            IsDirty = true;
            VisibilityTexture = visibilityTexture;
        }

        public Vector2Int Tile { get; private set; }
        public Vector2Int LastCalculationTile { get; set; }
        public int Radius { get; private set; }
        public bool IsEnabled { get; private set; }
        public bool IsDead { get; private set; }
        public bool IsDirty { get; set; }
        public RenderTexture VisibilityTexture { get; }

        public void Move(Vector3 position)
        {
            var newTile = GetTile(position);

            if (Tile != newTile)
            {
                Tile = newTile;
                IsDirty = true;
            }
        }

        public void UpdateRadius(int radius)
        {
            if (radius != Radius)
            {
                Radius = radius;
                IsDirty = true;
            }
        }

        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
        }

        public void Kill()
        {
            IsDead = true;
        }

    }

}

public interface IRevealSource
{
    public void Move(Vector3 position);
    public void Enable();
    public void Disable();
    public void Kill();
    public void UpdateRadius(int radius);

}