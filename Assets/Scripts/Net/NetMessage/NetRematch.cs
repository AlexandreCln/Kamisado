using Unity.Networking.Transport;

public class NetRematch : NetMessage
{
    public NetRematch() /* Instantiated when creating a message */
    {
        Code = OperationCode.REMATCH;
    }

    public NetRematch(DataStreamReader reader) /* Instantiated when receiving a message */
    {
        Code = OperationCode.REMATCH;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }

    public override void ReceivedOnClient()
    {
        NetUtility.C_REMATCH?.Invoke(this);
    }
}
