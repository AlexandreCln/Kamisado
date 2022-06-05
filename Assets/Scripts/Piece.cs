using UnityEngine;

public class Piece : MonoBehaviour
{
    public Color Color { set; get; }
    public bool IsBlack { set; get; }
    public Vector2 Pos { set; get; }
    public bool IsMoving { set; get; }
    public Vector2 _initPos;

    public void Initialize(Tile spawnTile)
    {
        IsBlack = spawnTile.IsBlackHomeCell;
        transform.Find("PieceInterior").GetComponent<SpriteRenderer>().color = spawnTile.Color;
        Color = spawnTile.Color;
        _initPos = transform.position;
        Pos = transform.position;
        spawnTile.InitPiece = this;
        spawnTile.Piece = this;
    }

    public void ResetPos()
    {
        transform.position = _initPos;
        Pos = _initPos;
    }
}
