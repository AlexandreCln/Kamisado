using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    [SerializeField] private RawImage _img;
    [SerializeField] private float _speedX, _speedY;

    void Update()
    {
        var newPos = _img.uvRect.position + new Vector2(_speedX, _speedY) * Time.deltaTime;
        _img.uvRect = new Rect(newPos, _img.uvRect.size);        
    }
}
