using UnityEngine;
using System.Collections.Generic;

public class Piece : MonoBehaviour
{
    [SerializeField] private Piece _blankPiecePrefab;
    private Color _color;

    private void OnEnable()
    {
        EventManager.AddListener("GenerateAllPieces", _GenerateAllPieces);
    }
    private void _GenerateAllPieces(object data)
    {
        var tiles = (Dictionary<Vector2, Tile>)data;
        foreach (KeyValuePair<Vector2, Tile> entry in tiles)
        {
            var tile = entry.Value;
            if (tile.IsHomeCell)
            {
                Piece newPiece = Instantiate(_blankPiecePrefab, new Vector3(tile.Pos.x, tile.Pos.y), Quaternion.identity);
                newPiece.transform.Find("InnerCircle").GetComponent<SpriteRenderer>().color = tile.Color;
                newPiece.transform.SetParent(transform);
                tile.Piece = newPiece;
            }
        }
    }

    public void Move(Vector2 pos)
    {
        transform.position = pos;
    }
}
