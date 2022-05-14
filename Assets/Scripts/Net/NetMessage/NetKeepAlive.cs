using Unity.Networking.Transport;

public class NetKeepAlive : NetMessage
{
    public NetKeepAlive() /* Instantiated when creating a message */
    {
        Code = OperationCode.KEEP_ALIVE;
    }

    public NetKeepAlive(DataStreamReader reader) /* Instantiated when receiving a message */
    {
        Code = OperationCode.KEEP_ALIVE;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }

    /// <summary>Parse the data in the same order as it was packaged</summary>
    public override void Deserialize(DataStreamReader reader)
    {
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_KEEP_ALIVE?.Invoke(this, cnn);
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_KEEP_ALIVE?.Invoke(this);
    }
}
