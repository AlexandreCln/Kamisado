using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _cam;

    private Dictionary<Vector2, Tile> _tiles;

    [Header("Color Theme")]
    public Color orange;
    public Color blue;
    public Color purple;
    public Color pink;
    public Color yellow;
    public Color red;
    public Color green;
    public Color brown;

    void Start()
    {
        _cam.transform.position = new Vector3(4f - 0.5f, 4f - 0.5f, -13);
        GenerateTiles();

        // TODO: subscribe to event "ShowLegalMoves"
        // ShowLegalMoves();
    }

    void GenerateTiles()
    {
        _tiles = new Dictionary<Vector2, Tile>();
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                Tile spawnedTile = Instantiate(_tilePrefab, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

                Vector2 pos = new Vector2(x, y);
                spawnedTile.Initialize(pos, DetermineTileColor(pos));
                _tiles[pos] = spawnedTile;
            }   
        }
    }

    Color DetermineTileColor(Vector2 pos)
    {
        if (pos.y == pos.x)
            return brown;
        if (pos.y == (1 + 3 * pos.x) % 8)
            return purple;
        if (pos.y == (2 + 5 * pos.x) % 8)
            return blue;
        if (pos.y == (3 + 7 * pos.x) % 8)
            return yellow;
        if (pos.y == (4 + 1 * pos.x) % 8)
            return pink;
        if (pos.y == (5 + 3 * pos.x) % 8)
            return green;
        if (pos.y == (6 + 5 * pos.x) % 8)
            return red;
        if (pos.y == (7 + 7 * pos.x) % 8)
            return orange;

        return Color.white;
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out Tile tile))
            return _tiles[pos];

        return null;
    }

    // TODO: subscribe to event
    void ShowLegalMoves()
    {
        Debug.Log("ShowLegalMoves");
    }
}