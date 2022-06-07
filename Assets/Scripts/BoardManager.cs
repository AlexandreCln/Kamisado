using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Networking.Transport;

public class BoardManager : MonoBehaviour
{    
    [SerializeField] private Tile _blankTilePrefab;
    [SerializeField] private Piece _whitePiecePrefab;
    [SerializeField] private Piece _blackPiecePrefab;
    [SerializeField] private GameObject _blackIndicator;
    [SerializeField] private GameObject _whiteIndicator;
    [SerializeField] private ParticleSystem _playerIndicatorEffect;
    [SerializeField] private Transform _cam;
    [SerializeField] private float _camDistance = 8f;
    [SerializeField] private float _pieceMovementDuration = 1f;
    [SerializeField] private float _screenVictoryDelaySeconds = 2f;

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
    private bool _isLocalGame = false;
    private bool _isFirstTurn = true;
    private bool _isBlackTurn = true;
    private int _readyForRematchPlayers = 0;

    #region Networking
    private int _totalPlayerConnected = -1;
    private int _teamId = -1;
    #endregion

    private void Awake()
    {
        _cam.transform.position = new Vector3(TILE_COUNT_X/2 - 0.5f, TILE_COUNT_Y/2 - 0.5f, -_camDistance);
        _RegisterEvents();
        _RegisterNetworkEvents();
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

    private void _SpawnAllPieces()
    {       
        _blackPieces = new Dictionary<Vector2, Piece>();
        foreach (Tile tile in _tiles.Values.Where(t => t.IsBlackHomeCell))
            _blackPieces[tile.Pos] = _SpawnPiece(tile, false);

        _whitePieces = new Dictionary<Vector2, Piece>();
        foreach (Tile tile in _tiles.Values.Where(t => t.IsWhiteHomeCell))
            _whitePieces[tile.Pos] = _SpawnPiece(tile);
    }

    private Piece _SpawnPiece(Tile spawnTile, bool white = true)
    {
        Vector2 piecePos = new Vector2(spawnTile.Pos.x, spawnTile.Pos.y);
        Piece spawnedPiece = Instantiate(white ? _whitePiecePrefab : _blackPiecePrefab, piecePos, Quaternion.identity);
        spawnedPiece.Initialize(spawnTile);
        spawnedPiece.transform.SetParent(transform);

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

    private Tile _GetTileAtPosition(Vector2 pos)
    {
        if (_tiles.TryGetValue(pos, out Tile tile))
            return _tiles[pos];

        return null;
    }

    private void _OnTileClicked(object data)
    {
        if (!_isLocalGame && !_IsMyturn())
            return;

        Tile targetTile = (Tile)data;

        if (targetTile.Targetable)
        {
            if (!_isLocalGame)
            {
                Debug.Log("send move to server");
                NetMakeMove mm = new NetMakeMove();
                mm.OriginX = (int)_activePiece.Pos.x;
                mm.OriginY = (int)_activePiece.Pos.y;
                mm.TargetY = (int)targetTile.Pos.y;
                mm.TargetX = (int)targetTile.Pos.x;
                mm.TeamId = _teamId;
                Client.Instance.SendToServer(mm);
            }

            Debug.Log($"move Piece locally from {_activePiece.Pos.x};{_activePiece.Pos.y} to {targetTile.Pos.x};{targetTile.Pos.y}");
            _isFirstTurn = false;
            _GetTileAtPosition(_activePiece.Pos).Piece = null;
            StartCoroutine(_MoveActivePiece(targetTile));
            _SwitchTurn();

            if (!targetTile.IsBlackHomeCell && !targetTile.IsWhiteHomeCell)
                _PrepareNextMove(targetTile);
            else
                StartCoroutine(_TriggerVictory());
        }   
        else if (
            _isFirstTurn && targetTile.Piece != null && 
            (targetTile.Piece.IsBlack && _isBlackTurn || !targetTile.Piece.IsBlack && !_isBlackTurn) &&
            (_isLocalGame || !_isLocalGame && _IsMyturn())
        ) {
            _activePiece = targetTile.Piece;
            _ActiveLegalTiles();
        }
    }

    private bool _IsMyturn()
    {
        return _teamId == 0 && _isBlackTurn || _teamId == 1 && !_isBlackTurn;
    }

    private IEnumerator _MoveActivePiece(Tile targetTile)
    {
        // swap Piece owners first then update pos
        targetTile.Piece = _activePiece;
        _activePiece.Pos = targetTile.Pos;

        // physically move the piece
        Piece pieceToMove = _activePiece;
        yield return new WaitUntil(() => !pieceToMove.IsMoving);

        float time = 0;
        Vector3 startPosition = pieceToMove.transform.position;
        pieceToMove.IsMoving = true;
        while (time < _pieceMovementDuration)
        {
            float t = time / _pieceMovementDuration;
            t = t*t*(3f-2f*t);
            pieceToMove.transform.position = Vector3.Lerp(startPosition, targetTile.Pos, t);
            time += Time.deltaTime;
            yield return null;
        }
        pieceToMove.IsMoving = false;
    }

    private IEnumerator _TriggerVictory()
    {
        _DisableAllTiles();
        yield return new WaitForSeconds(_screenVictoryDelaySeconds);
        EventManager.TriggerEvent(
            _isLocalGame ? "LocalGameEnded" : "NetworkGameEnded", 
            _isBlackTurn ? "white" : "black"
        );
        _ResetBoard();
    }
    
    private void _PrepareNextMove(Tile lastTargetTile)
    {
        var currentPlayerPieces = _isBlackTurn ? _blackPieces : _whitePieces;
        _activePiece = currentPlayerPieces.FirstOrDefault(x => x.Value.Color == lastTargetTile.Color).Value;
        bool canMove = _ActiveLegalTiles();

        if (!canMove)
        {
            Debug.Log("no available moves");
            _SwitchTurn();
            currentPlayerPieces = _isBlackTurn ? _blackPieces : _whitePieces;
            _activePiece = currentPlayerPieces.FirstOrDefault(x => x.Value.Color == lastTargetTile.Color).Value;
            canMove = _ActiveLegalTiles();

            if (!canMove)
                StartCoroutine(_TriggerVictory());
        }
    }

    private void _SwitchTurn()
    {
        _isBlackTurn = !_isBlackTurn;
        _RepositionTurnIndicatorEffect();
    }

    private void _RepositionTurnIndicatorEffect()
    {
        _playerIndicatorEffect.transform.position = Vector3.forward * 3 + 
            (_isBlackTurn ? 
                _blackIndicator.transform.position : 
                _whiteIndicator.transform.position);
    }

    private bool _ActiveLegalTiles()
    {
        _DisableAllTiles();
        bool canMove = false;
        Vector2 startPos = _activePiece.Pos;

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
                    Tile upLTile = _GetTileAtPosition(upL);

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
                    Tile upTile = _GetTileAtPosition(up);
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
                    Tile upRTile = _GetTileAtPosition(upR);
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
                    Tile downLTile = _GetTileAtPosition(downL);
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
                    Tile downTile = _GetTileAtPosition(down);
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
                    Tile downRTile = _GetTileAtPosition(downR);
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
        EventManager.AddListener("TileClicked", _OnTileClicked);
        EventManager.AddListener("StartLocalGame", _OnStartLocalGame);
        EventManager.AddListener("DisconnectHost", _OnDisconnectHost);
        EventManager.AddListener("NetworkEndGame", _OnNetworkEndGame);
        EventManager.AddListener("OpponentDisconnected", _OnOpponentDisconnected);
    }
    private void _RegisterNetworkEvents()
    {
        NetUtility.S_WELCOME += _OnWelcomeServer;
        NetUtility.S_MAKE_MOVE += _OnMakeMoveServer;
        NetUtility.S_REMATCH_DEMAND += _OnRematchDemand;

        NetUtility.C_WELCOME += _OnWelcomeClient;
        NetUtility.C_START_GAME += _OnStartGameClient;
        NetUtility.C_MAKE_MOVE += _OnMakeMoveClient;
        NetUtility.C_REMATCH += _OnRematch;
    }

    // Local
    private void _OnStartLocalGame()
    {
        _isLocalGame = true;
        _totalPlayerConnected = -1;
        _teamId = -1;

        if (_tiles == null)
        {
            _SpawnAllTiles();
            _SpawnAllPieces();
        }
        _RotateAndPositionGameElements();
    }

    // Server
    private void _OnWelcomeServer(NetMessage msg, NetworkConnection cnn)
    {
        /* When any client ask the driver to connect the configured server, on succes his connection receive a 
           NetworkEvent.Type.Connect, when the client open this message he send a NetWelcome to the server. */
        NetWelcome nw = msg as NetWelcome;
        // NetWelcome is like : Hey I'm connected and I send you a message, can you fill it with the team ID ?

        // fill the client's message with a team ID, based on the number of player
        nw.AssignedTeam = ++_totalPlayerConnected;
        Debug.Log("Handle a NetWelcome message from a client, then send it back with AssignedTeam ID : " + nw.AssignedTeam);

        // return the message back to the client
        Server.Instance.SendToClient(cnn, nw);

        // start the game if 2 players are connected
        if (_totalPlayerConnected == 1)
        {
            Server.Instance.Broadcast(new NetStartGame());
        }
    }

    private void _OnMakeMoveServer(NetMessage msg, NetworkConnection cnn)
    {
        NetMakeMove mm = msg as NetMakeMove;
        Server.Instance.Broadcast(mm);
    }

    private void _OnNetworkEndGame()
    {
        _readyForRematchPlayers = 0;
        _totalPlayerConnected = -1;
        _teamId = -1;
        _isBlackTurn = true;
        _RepositionTurnIndicatorEffect();
    }

    private void _OnDisconnectHost()
    {
        _totalPlayerConnected = -1;
        _teamId = -1;
    }

    private void _OnRematch(NetMessage ms)
    {
        _readyForRematchPlayers = 0;
    }

    private void _ResetBoard()
    {
        foreach (var tile in _tiles.Values)
            tile.Piece = tile.InitPiece;
        foreach (var piece in _whitePieces.Values)
            piece.ResetPos();
        foreach (var piece in _blackPieces.Values)
            piece.ResetPos();

        _DisableAllTiles();
        _activePiece = null;
        _isFirstTurn = true;
    }

    // Client
    private void _OnWelcomeClient(NetMessage msg)
    {
        if (_teamId != -1)
            return;

        NetWelcome nw = msg as NetWelcome;
        // New connected client assign a team itself, from the message deserialized earlier
        _teamId = nw.AssignedTeam;
        Debug.Log($"Assigned team : {nw.AssignedTeam}");
    }
    
    private void _OnOpponentDisconnected()
    {
        _RotateAndPositionGameElements();
        _OnDisconnectHost();
        _ResetBoard();
        _isBlackTurn = true;
        _RepositionTurnIndicatorEffect();
    }
    
    private void _OnStartGameClient(NetMessage msg)
    {
        _isLocalGame = false;
        _isBlackTurn = true;
        _RotateAndPositionGameElements();
        _RepositionTurnIndicatorEffect();

        if (_tiles == null)
        {
            _SpawnAllTiles();
            _SpawnAllPieces();
        }
    }

    private void _RotateAndPositionGameElements()
    {
        if (_isLocalGame || _teamId == 0)
        {
            Vector3 dir = new Vector3(0,0,0);
            _whiteIndicator.transform.eulerAngles = dir;
            _blackIndicator.transform.eulerAngles = dir;
            _cam.transform.eulerAngles = dir;
        }
        else
        {
            Vector3 indicatorsDir = new Vector3(0,180,180);
            _whiteIndicator.transform.eulerAngles = indicatorsDir;
            _blackIndicator.transform.eulerAngles = indicatorsDir;
            _cam.transform.eulerAngles = new Vector3(0,0,180);
        }
    }

    private void _OnMakeMoveClient(NetMessage msg)
    {
        _isFirstTurn = false;
        NetMakeMove mm = msg as NetMakeMove;

        if (mm.TeamId == _teamId)
            return;

        // handle first move
        if (_activePiece == null)
            _activePiece = _GetTileAtPosition(new Vector2(mm.OriginX, mm.OriginY)).Piece;

        Tile targetTile = _GetTileAtPosition(new Vector2(mm.TargetX, mm.TargetY));
        _GetTileAtPosition(_activePiece.Pos).Piece = null;
        StartCoroutine(_MoveActivePiece(targetTile));
        _SwitchTurn();
        
        if (!targetTile.IsBlackHomeCell && !targetTile.IsWhiteHomeCell)
            _PrepareNextMove(targetTile);
        else
            StartCoroutine(_TriggerVictory());
    }

    private void _OnRematchDemand(NetMessage msg, NetworkConnection conn)
    {
        if (++_readyForRematchPlayers == 2)
            Server.Instance.Broadcast(new NetRematch());
    }
    #endregion
}