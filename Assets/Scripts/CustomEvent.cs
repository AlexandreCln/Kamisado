using UnityEngine.Events;

// easily store the data of our events
// similarly to C unions: we define several fields but we use only one at a time
public class CustomEventData
{
    public Tile tile;

    public CustomEventData(Tile tile)
    {
        this.tile = tile;
    }
}

// custom type of event that contain data
[System.Serializable]
public class CustomEvent : UnityEvent<CustomEventData> {}