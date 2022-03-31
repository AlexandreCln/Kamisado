using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _piecePrefab;
    [SerializeField] private GameObject _highlight;

    private bool _isHomeCell;
    private Color _color;
    private Vector2 _pos;
    private GameObject _piece = null;
    private bool _isShowingLegalMoves;

    void OnMouseEnter()
    {
        _highlight.SetActive(true);
    }

    void OnMouseExit()
    {
        _highlight.SetActive(false);
    }

    void OnMouseDown()
    {
        Debug.Log("OnMouseDown");

        // trigger event "ShowLegalMoves" with data targetTilePos
    }

    public void Initialize(Vector2 pos, Color color)
    {
        _pos = pos;
        _color = color;
        _renderer.color = _color;

        _isHomeCell = (0 == pos.y) || (7 == pos.y);
        if (_isHomeCell)
            GeneratePiece();
    }

    public void GeneratePiece()
    {
        GameObject newPiece = Instantiate(_piecePrefab, new Vector3(_pos.x, _pos.y), Quaternion.identity);
        newPiece.transform.Find("InnerCircle").GetComponent<SpriteRenderer>().color = _color;
        _piece = newPiece;
    }

    public bool IsHomeCell { get => _isHomeCell; }
}
