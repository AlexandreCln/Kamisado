using Unity.Networking.Transport;

public class NetStartGame : NetMessage
{
    public int AssignedTeam { set; get; }
    
    public NetStartGame() /* Instantiated when creating a message */
    {
        Code = OperationCode.START_GAME;
    }

    public NetStartGame(DataStreamReader reader) /* Instantiated when receiving a message */
    {
        Code = OperationCode.START_GAME;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(AssignedTeam);
    }

    /// <summary>Parse the data in the same order as it was packaged</summary>
    public override void Deserialize(DataStreamReader reader)
    {
        AssignedTeam = reader.ReadInt();
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_START_GAME?.Invoke(this, cnn);
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_START_GAME?.Invoke(this);
    }
}
