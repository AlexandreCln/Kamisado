using UnityEngine;

public class Piece : MonoBehaviour
{
    public Color Color { set; get; }
    public bool IsBlack { set; get; }

    public void Initialize(Tile spawnTile)
    {
        if (spawnTile.IsBlackHomeCell)
        {
            IsBlack = true;
            GetComponent<SpriteRenderer>().color = Color.black;
        }
        transform.Find("InnerCircle").GetComponent<SpriteRenderer>().color = spawnTile.Color;
        Color = spawnTile.Color;
        transform.SetParent(transform);
        spawnTile.Piece = this;
    }

    public void Move(Vector2 pos)
    {
        transform.position = pos;
    }
}
