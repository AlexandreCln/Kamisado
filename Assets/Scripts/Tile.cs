using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private GameObject _hoverHighlight;
    [SerializeField] private GameObject _legalHighlight;

    private bool _isHomeCell;
    private bool _targetable;
    private Vector2 _pos;

    private Color _color;

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
        EventManager.TriggerEvent("OnTileClicked", this);
    }

    public void Initialize(Vector2 initPos, Color initColor)
    {
        _pos = initPos;
        _color = initColor;
        _renderer.color = initColor;
        _isHomeCell = (_pos.y == 0) || (_pos.y == 7);
    }

    public Vector2 Pos => _pos;
    public Color Color => _color;
    public bool IsHomeCell => _isHomeCell;
    public Piece Piece { get; set; }
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
