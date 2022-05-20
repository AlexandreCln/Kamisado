using Unity.Networking.Transport;

public class NetRematchDemand : NetMessage
{
    public NetRematchDemand() /* Instantiated when creating a message */
    {
        Code = OperationCode.REMATCH_DEMAND;
    }

    public NetRematchDemand(DataStreamReader reader) /* Instantiated when receiving a message */
    {
        Code = OperationCode.REMATCH_DEMAND;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }

    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_REMATCH_DEMAND?.Invoke(this, cnn);
    }
}
