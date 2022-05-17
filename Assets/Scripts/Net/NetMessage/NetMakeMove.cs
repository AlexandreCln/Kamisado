using Unity.Networking.Transport;

public class NetMakeMove : NetMessage
{
    public int OriginX { set; get; }
    public int OriginY { set; get; }
    public int TargetX { set; get; }
    public int TargetY { set; get; }
    public int TeamId { set; get; }
    
    public NetMakeMove() /* Instantiated when creating a message */
    {
        Code = OperationCode.MAKE_MOVE;
    }

    public NetMakeMove(DataStreamReader reader) /* Instantiated when receiving a message */
    {
        Code = OperationCode.MAKE_MOVE;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(OriginX);
        writer.WriteInt(OriginY);
        writer.WriteInt(TargetX);
        writer.WriteInt(TargetY);
        writer.WriteInt(TeamId);
    }

    /// <summary>Parse the data in the same order as it was packaged</summary>
    public override void Deserialize(DataStreamReader reader)
    {
        OriginX = reader.ReadInt();
        OriginY = reader.ReadInt();
        TargetX = reader.ReadInt();
        TargetY = reader.ReadInt();
        TeamId = reader.ReadInt();
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_MAKE_MOVE?.Invoke(this, cnn);
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_MAKE_MOVE?.Invoke(this);
    }
}
