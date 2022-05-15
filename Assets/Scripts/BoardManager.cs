using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Networking.Transport;

public class BoardManager : MonoBehaviour
{    
    [SerializeField] private Tile _blankTilePrefab;
    [SerializeField] private Piece _blankPiecePrefab;

    [SerializeField] private Transform _cam;
    [SerializeField] private float _camDistance = 13f;

    [Header("Color Theme")]
    [SerializeField] private Color _orange;
    [SerializeField] private Color _blue;
    [SerializeField] private Color _purple;
    [SerializeField] private Color _pink;
    [SerializeField] private Color _yellow;
    [SerializeField] private Color _red;
    [SerializeField] private Color _green;
    [SerializeField] private Color _brown;

    private const int TILE_COUNT_X = 8;
    private const int TILE_COUNT_Y = 8;
    private Dictionary<Vector2, Tile> _tiles;
    private Dictionary<Vector2, Piece> _blackPieces;
    private Dictionary<Vector2, Piece> _whitePieces;
    private Piece _activePiece;
    private bool _isFirstTurn = true;
    private bool _isBlackTurn = true;

    #region Networking
    private int _playerCount = -1;
    private int _currentTeam = -1;
    #endregion

    private void Awake()
    {
        _cam.transform.position = new Vector3(TILE_COUNT_X/2 - 0.5f, TILE_COUNT_Y/2 - 0.5f, -_camDistance);
        _RegisterEvents();
    }

    private void _SpawnAllTiles()
    {
        _tiles = new Dictionary<Vector2, Tile>();
        for (int y = 0; y < TILE_COUNT_Y; y++)
        {
            for (int x = 0; x < TILE_COUNT_X; x++)
            {
                Vector2 pos = new Vector2(x, y);
                _tiles[pos] = _SpawnTile(pos);
            }   
        }
    }

    private Tile _SpawnTile(Vector2 pos)
    {
        Tile spawnedTile = Instantiate(_blankTilePrefab, new Vector3(pos.x, pos.y), Quaternion.identity);
        spawnedTile.name = $"Tile {pos.x} {pos.y}";
        spawnedTile.Initialize(pos, _DetermineTileColor(pos));
        spawnedTile.transform.SetParent(transform);

        return spawnedTile;
    }

    private void _GenerateAllPieces()
    {       
        _blackPieces = new Dictionary<Vector2, Piece>();
        foreach (Tile tile in _tiles.Values.Where(t => t.IsBlackHomeCell))
            _blackPieces[tile.Pos] = _SpawnPiece(tile);

        _whitePieces = new Dictionary<Vector2, Piece>();
        foreach (Tile tile in _tiles.Values.Where(t => t.IsWhiteHomeCell))
            _whitePieces[tile.Pos] = _SpawnPiece(tile);
    }

    private Piece _SpawnPiece(Tile spawnTile)
    {
        Vector2 piecePos = new Vector2(spawnTile.Pos.x, spawnTile.Pos.y);
        Piece spawnedPiece = Instantiate(_blankPiecePrefab, piecePos, Quaternion.identity);
        spawnedPiece.Initialize(spawnTile);

        return spawnedPiece;
    }

    Color _DetermineTileColor(Vector2 pos)
    {
        if (pos.y == pos.x) return _brown;
        if (pos.y == (1 + 3 * pos.x) % TILE_COUNT_X) return _purple;
        if (pos.y == (2 + 5 * pos.x) % TILE_COUNT_X) return _blue;
        if (pos.y == (3 + 7 * pos.x) % TILE_COUNT_X) return _yellow;
        if (pos.y == (4 + 1 * pos.x) % TILE_COUNT_X) return _pink;
        if (pos.y == (5 + 3 * pos.x) % TILE_COUNT_X) return _green;
        if (pos.y == (6 + 5 * pos.x) % TILE_COUNT_X) return _red;
        if (pos.y == (7 + 7 * pos.x) % TILE_COUNT_X) return _orange;
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
        Tile targetTile = (Tile)data;

        if (targetTile.Targetable)
        {
            _isFirstTurn = false;
            _MovePiece(targetTile);
            _NextMove(targetTile);
        }   
        else if (_isFirstTurn && targetTile.Piece != null)
        {
            _activePiece = targetTile.Piece;
            _ActiveLegalTiles();
        }
    }

    private void _MovePiece(Tile targetTile)
    {
        // swap Piece owners first
        GetTileAtPosition(_activePiece.transform.position).Piece = null;
        targetTile.Piece = _activePiece;
        // then move the piece
        _activePiece.transform.position = targetTile.Pos;

        if (targetTile.IsBlackHomeCell || targetTile.IsWhiteHomeCell)
            Debug.Log("Player " + ( _isBlackTurn ? "black":"white") + " win !");
    }
    
    private void _NextMove(Tile lastTargetTile)
    {
        _isBlackTurn = !_isBlackTurn;
        var currentPlayerPieces = _isBlackTurn ? _blackPieces : _whitePieces;
        _activePiece = currentPlayerPieces.FirstOrDefault(x => x.Value.Color == lastTargetTile.Color).Value;
        bool canMove = _ActiveLegalTiles();
        
        if (!canMove)
        {
            Debug.Log("no available moves");
            _isBlackTurn = !_isBlackTurn;
            currentPlayerPieces = _isBlackTurn ? _blackPieces : _whitePieces;
            _activePiece = currentPlayerPieces.FirstOrDefault(x => x.Value.Color == lastTargetTile.Color).Value;
            canMove = _ActiveLegalTiles();

            if (!canMove)
                Debug.Log("Game ended");
        }
    }

    private bool _ActiveLegalTiles()
    {
        _DisableAllTiles();

        bool canMove = false;
        Vector2 startPos = _activePiece.transform.position;

        if (_isBlackTurn && _activePiece.IsBlack)
        {
            var upL = new Vector2(startPos.x - 1, startPos.y + 1);
            var up = new Vector2(startPos.x, startPos.y + 1);
            var upR = new Vector2(startPos.x + 1, startPos.y + 1);
            var testUpL = true;
            var testUp = true;
            var testUpR = true;

            for (int i = 0; i < TILE_COUNT_Y; i++)
            {
                if (testUpL)
                {
                    Tile upLTile = GetTileAtPosition(upL);

                    if (upLTile == null || upLTile.Piece != null)
                    {
                        testUpL = false;
                    }
                    else
                    {
                        canMove = true;
                        upLTile.Targetable = true;
                        upL.x--;
                        upL.y++;
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
                        canMove = true;
                        upTile.Targetable = true;
                        up.y++;
                    }
                }
                if (testUpR)
                {
                    Tile upRTile = GetTileAtPosition(upR);
                    if (upRTile == null || upRTile.Piece != null)
                    {
                        testUpR = false;
                    }
                    else
                    {
                        canMove = true;
                        upRTile.Targetable = true;
                        upR.x++;
                        upR.y++;
                    }
                }
            }
        }
        else if (!_isBlackTurn && !_activePiece.IsBlack)
        {
            var downL = new Vector2(startPos.x - 1, startPos.y - 1);
            var down = new Vector2(startPos.x, startPos.y - 1);
            var downR = new Vector2(startPos.x + 1, startPos.y - 1);
            var testDownL = true;
            var testDown = true;
            var testDownR = true;

            for (int i = TILE_COUNT_Y; i > 0; i--)
            {
                if (testDownL)
                {
                    Tile downLTile = GetTileAtPosition(downL);
                    if (downLTile == null || downLTile.Piece != null)
                    {
                        testDownL = false;
                    }
                    else
                    {
                        canMove = true;
                        downLTile.Targetable = true;
                        downL.x--;
                        downL.y--;
                    }
                }
                if (testDown)
                {
                    Tile downTile = GetTileAtPosition(down);
                    if (downTile == null || downTile.Piece != null)
                    {
                        testDown = false;
                    }
                    else
                    {
                        canMove = true;
                        downTile.Targetable = true;
                        down.y--;
                    }
                }
                if (testDownR)
                {
                    Tile downRTile = GetTileAtPosition(downR);
                    if (downRTile == null || downRTile.Piece != null)
                    {
                        testDownR = false;
                    }
                    else
                    {
                        canMove = true;
                        downRTile.Targetable = true;
                        downR.x++;
                        downR.y--;
                    }
                }
            }
        }
        return canMove;
    }

    private void _DisableAllTiles()
    {
        foreach (var tile in _tiles.Values.Where(kvp => kvp.Targetable))
            tile.Targetable = false;
    }

     #region Events
    private void _RegisterEvents()
    {
        // Local events
        // if (_isLocalGame)
        EventManager.AddListener("TileClicked", _OnTileClicked);
        EventManager.AddListener("StartLocalGame", _OnStartLocalGame);
        
        // Network events
        NetUtility.S_WELCOME += _OnWelcomeServer;

        NetUtility.C_WELCOME += _OnWelcomeClient;
        NetUtility.C_START_GAME += _OnStartGameClient;
    }

    private void _UnregisterEvents()
    {
        EventManager.RemoveListener("TileClicked", _OnTileClicked);
        EventManager.RemoveListener("StartLocalGame", _OnStartLocalGame);

        NetUtility.S_WELCOME -= _OnWelcomeServer;

        NetUtility.C_WELCOME -= _OnWelcomeClient;
        NetUtility.C_START_GAME -= _OnStartGameClient;
    }

    // Local
    private void _OnStartLocalGame()
    {
        _SpawnAllTiles();
        _GenerateAllPieces();
    }

    // Server
    private void _OnWelcomeServer(NetMessage msg, NetworkConnection cnn)
    {
        /* When any client ask the driver to connect the configured server, on succes his connection receive a 
           NetworkEvent.Type.Connect, when he open this message he send a NetWelcome to the server. */
        NetWelcome nw = msg as NetWelcome;
        // NetWelcome is like : Hey I'm connected and I send you a box, can you populated it with the team ID ?

        // populate the client's message with a team ID, based on the number of player
        nw.AssignedTeam = ++_playerCount;
        Debug.Log("Handle a NetWelcome message from a client, then send it back with AssignedTeam ID : " + nw.AssignedTeam);

        // return the message back to the client
        Server.Instance.SendToClient(cnn, nw);

        // start the game if 2 players are connected
        // if (_playerCount == 1)
        if (true)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }

    // Client
    private void _OnWelcomeClient(NetMessage msg)
    {
        NetWelcome nw = msg as NetWelcome;
        // New connected client assign a team itself, from the message deserialized earlier
        _currentTeam = nw.AssignedTeam;

        Debug.Log($"Assigned team : {nw.AssignedTeam}");
    }
    
    private void _OnStartGameClient(NetMessage msg)
    {
        _SpawnAllTiles();
        _GenerateAllPieces();
    }
    #endregion
}