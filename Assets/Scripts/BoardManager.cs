using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BoardManager : MonoBehaviour
{    
    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    [SerializeField] private int _width, _height;
    [SerializeField] private Tile _blankTilePrefab;
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
    private Tile _selectedTileWithPiece;

    void Start()
    {
        _cam.transform.position = new Vector3(4f - 0.5f, 4f - 0.5f, -13);
        _GenerateAllTiles();
        EventManager.TriggerEvent("GenerateAllPieces", _tiles);
    }
    
    private void OnEnable()
    {
        EventManager.AddListener("OnTileClicked", _OnTileClicked);
    }

    void _GenerateAllTiles()
    {
        _tiles = new Dictionary<Vector2, Tile>();
        for (int y = 0; y < TILE_COUNT_Y; y++)
        {
            for (int x = 0; x < TILE_COUNT_X; x++)
            {
                Vector2 pos = new Vector2(x, y);
                _tiles[pos] = _GenerateTile(pos);
            }   
        }
    }

    private Tile _GenerateTile(Vector2 pos)
    {
        Tile spawnedTile = Instantiate(_blankTilePrefab, new Vector3(pos.x, pos.y), Quaternion.identity);
        spawnedTile.name = $"Tile {pos.x} {pos.y}";
        spawnedTile.Initialize(pos, _DetermineTileColor(pos));
        spawnedTile.transform.SetParent(transform);

        return spawnedTile;
    }

    Color _DetermineTileColor(Vector2 pos)
    {
        if (pos.y == pos.x)
            return _brown;
        if (pos.y == (1 + 3 * pos.x) % TILE_COUNT_X)
            return _purple;
        if (pos.y == (2 + 5 * pos.x) % TILE_COUNT_X)
            return _blue;
        if (pos.y == (3 + 7 * pos.x) % TILE_COUNT_X)
            return _yellow;
        if (pos.y == (4 + 1 * pos.x) % TILE_COUNT_X)
            return _pink;
        if (pos.y == (5 + 3 * pos.x) % TILE_COUNT_X)
            return _green;
        if (pos.y == (6 + 5 * pos.x) % TILE_COUNT_X)
            return _red;
        if (pos.y == (7 + 7 * pos.x) % TILE_COUNT_X)
            return _orange;

        return Color.white;
    }

    public Tile GetTileAtPosition(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out Tile tile))
            return _tiles[pos];

        return null;
    }

    private void _OnTileClicked(object data)
    {
        Tile clickedTile = (Tile)data;

        if (clickedTile.Piece != null)
        {
            _selectedTileWithPiece = clickedTile;
            _ShowLegalMoves(clickedTile.Pos);
        }
        else
        {
            if (_selectedTileWithPiece != null && clickedTile.Targetable)
            {
                MovePiece(_selectedTileWithPiece, clickedTile);
            }

            _selectedTileWithPiece = null;
            _HideLegalMoves();
        }
    }

    private void MovePiece(Tile startTile, Tile targetTile)
    {
        _selectedTileWithPiece.Piece.Move(targetTile.Pos);
        // swap piece owners
        targetTile.Piece = _selectedTileWithPiece.Piece;
        startTile.Piece = null;
    }

    private void _ShowLegalMoves(Vector2 pos)
    {
        _HideLegalMoves();
        
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
                if (ldTile == null || ldTile.Piece != null)
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
                if (upTile == null || upTile.Piece != null)
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
                if (rdTile == null || rdTile.Piece != null)
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

    private void _HideLegalMoves()
    {
        foreach (var tile in _tiles.Where(kvp => kvp.Value.Targetable).ToList())
            tile.Value.Targetable = false;
    }
}