using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _piecePrefab;
    [SerializeField] private GameObject _hoverHighlight;
    [SerializeField] private GameObject _legalHighlight;

    private bool _isHomeCell;
    private Color _color;
    private bool _targetable;

    public GameObject piece = null;
    public Vector2 pos;

    private void OnMouseEnter()
    {
        _hoverHighlight.SetActive(true);
    }

    private void OnMouseExit()
    {
        _hoverHighlight.SetActive(false);
    }

    private void OnMouseDown()
    {
        EventManager.TriggerTypedEvent("OnTileClicked", new CustomEventData(this));
    }

    public void Initialize(Vector2 initPos, Color color)
    {
        pos = initPos;
        _color = color;
        _renderer.color = _color;

        _isHomeCell = (0 == pos.y) || (7 == pos.y);
        if (_isHomeCell)
            GeneratePieceOnTop(_color);
    }

    public void GeneratePieceOnTop(Color pieceColor)
    {
        GameObject newPiece = Instantiate(_piecePrefab, new Vector3(pos.x, pos.y), Quaternion.identity);
        newPiece.transform.Find("InnerCircle").GetComponent<SpriteRenderer>().color = pieceColor;
        piece = newPiece;
    }
    public void MovePiece(Tile targetTile)
    {
        Color pieceColor = piece.transform.Find("InnerCircle").GetComponent<SpriteRenderer>().color;
        targetTile.GeneratePieceOnTop(pieceColor);
        Destroy(piece);
        piece = null;
    }

    public bool IsHomeCell { get => _isHomeCell; }
    public bool IsOccupied { get => null != piece; }
    public bool Targetable
    {
        get =>_targetable;
        set
        {
             _legalHighlight.SetActive(value);
            _targetable = value; 
        }
    }
}
