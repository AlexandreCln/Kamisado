using Unity.Networking.Transport;

public class NetWelcome : NetMessage
{
    public int AssignedTeam { set; get; }
    
    public NetWelcome() /* Instantiated when creating a message */
    {
        Code = OperationCode.WELCOME;
    }

    public NetWelcome(DataStreamReader reader) /* Instantiated when receiving a message */
    {
        Code = OperationCode.WELCOME;
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
        NetUtility.S_WELCOME?.Invoke(this, cnn);
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_WELCOME?.Invoke(this);
    }
}
