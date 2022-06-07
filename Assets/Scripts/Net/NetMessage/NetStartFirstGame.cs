using Unity.Networking.Transport;

public class NetStartFirstGame : NetMessage
{
    public int AssignedTeam { set; get; }
    
    public NetStartFirstGame() /* Instantiated when creating a message */
    {
        Code = OperationCode.START_FIRST_GAME;
    }

    public NetStartFirstGame(DataStreamReader reader) /* Instantiated when receiving a message */
    {
        Code = OperationCode.START_FIRST_GAME;
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
        NetUtility.S_START_FIRST_GAME?.Invoke(this, cnn);
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_START_FIRST_GAME?.Invoke(this);
    }
}
