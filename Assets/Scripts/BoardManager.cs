using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BoardManager : MonoBehaviour
{    
    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Transform _cam;


    [Header("Color Theme")]
    [SerializeField] private Color _orange;
    [SerializeField] private Color _blue;
    [SerializeField] private Color _purple;
    [SerializeField] private Color _pink;
    [SerializeField] private Color _yellow;
    [SerializeField] private Color _red;
    [SerializeField] private Color _green;
    [SerializeField] private Color _brown;

    private Dictionary<Vector2, Tile> _tiles;
    private Tile _selectedTileWithPiece = null;

    void Start()
    {
        _cam.transform.position = new Vector3(4f - 0.5f, 4f - 0.5f, -13);
        _GenerateTiles();
    }
    
    private void OnEnable()
    {
        EventManager.AddTypedListener("OnTileClicked", _OnTileClicked);
        // EventManager.AddTypedListener("SelectTileWithPiece", _SelectTileWithPiece);
        // EventManager.AddListener("UnselectTileWithPiece", _UnselectTileWithPiece);
    }

    void _GenerateTiles()
    {
        _tiles = new Dictionary<Vector2, Tile>();
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                Tile spawnedTile = Instantiate(_tilePrefab, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

                Vector2 pos = new Vector2(x, y);
                spawnedTile.Initialize(pos, _DetermineTileColor(pos));
                _tiles[pos] = spawnedTile;
            }   
        }
    }

    Color _DetermineTileColor(Vector2 pos)
    {
        if (pos.y == pos.x)
            return _brown;
        if (pos.y == (1 + 3 * pos.x) % 8)
            return _purple;
        if (pos.y == (2 + 5 * pos.x) % 8)
            return _blue;
        if (pos.y == (3 + 7 * pos.x) % 8)
            return _yellow;
        if (pos.y == (4 + 1 * pos.x) % 8)
            return _pink;
        if (pos.y == (5 + 3 * pos.x) % 8)
            return _green;
        if (pos.y == (6 + 5 * pos.x) % 8)
            return _red;
        if (pos.y == (7 + 7 * pos.x) % 8)
            return _orange;

        return Color.white;
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out Tile tile))
            return _tiles[pos];

        return null;
    }

    private void _OnTileClicked(CustomEventData data)
    {
        Tile tile = data.tile;

        if (tile.piece != null)
        {
            _selectedTileWithPiece = tile;
            _SetTargetableTiles(_selectedTileWithPiece.pos);
        }
        else
        {
            Debug.Log(tile.Targetable);
            if (_selectedTileWithPiece != null && _selectedTileWithPiece != tile && tile.Targetable) 
                _selectedTileWithPiece.MovePiece(tile);

            _selectedTileWithPiece = null;
            _UnsetTargetableTiles();
        }
    }

    private void _SetTargetableTiles(Vector2 pos)
    {
        _UnsetTargetableTiles();
        
        // left diagonal
        Vector2 ld = new Vector2(pos.x - 1, pos.y + 1);
        // up direction
        Vector2 up = new Vector2(pos.x, pos.y + 1);
        // right diagonal
        Vector2 rd = new Vector2(pos.x + 1, pos.y + 1);

        bool testLd = true;
        bool testUp = true;
        bool testRd = true;

        for (int i = 0; i < 8; i++)
        {
            if (testLd)
            {
                Tile ldTile = GetTileAtPosition(ld);
                if (ldTile == null || ldTile.IsOccupied)
                {
                    testLd = false;
                }
                else
                {
                    ldTile.Targetable = true;
                    ld.x--;
                    ld.y++;
                }
            }
            if (testUp)
            {
                Tile upTile = GetTileAtPosition(up);
                if (upTile == null || upTile.IsOccupied)
                {
                    testUp = false;
                }
                else
                {
                    upTile.Targetable = true;
                    up.y++;
                }
            }
            if (testRd)
            {
                Tile rdTile = GetTileAtPosition(rd);
                if (rdTile == null || rdTile.IsOccupied)
                {
                    testRd = false;
                }
                else
                {
                    rdTile.Targetable = true;
                    rd.x++;
                    rd.y++;
                }
            }
        }
    }

    private void _UnsetTargetableTiles()
    {
        foreach (var tile in _tiles.Where(kvp => kvp.Value.Targetable).ToList())
            tile.Value.Targetable = false;
    }
}