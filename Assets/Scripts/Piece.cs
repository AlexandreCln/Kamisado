using UnityEngine;

public class Piece : MonoBehaviour
{
    public Color Color { set; get; }
    public bool IsBlack { set; get; }
    public Vector2 _initPos;

    public void Initialize(Tile spawnTile)
    {
        if (spawnTile.IsBlackHomeCell)
        {
            IsBlack = true;
            GetComponent<SpriteRenderer>().color = Color.black;
        }
        transform.Find("InnerCircle").GetComponent<SpriteRenderer>().color = spawnTile.Color;
        Color = spawnTile.Color;
        _initPos = transform.position;
        spawnTile.InitPiece = this;
        spawnTile.Piece = this;
    }

    public void ResetPos()
    {
        transform.position = _initPos;
    }
}
