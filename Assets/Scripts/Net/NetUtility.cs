using System;
using Unity.Networking.Transport;
using UnityEngine;

public enum OperationCode
{
    KEEP_ALIVE = 1,
    WELCOME = 2,
    START_FIRST_GAME = 3,
    MAKE_MOVE = 4,
    REMATCH_DEMAND = 5,
    REMATCH = 6
}

public static class NetUtility
{
    public static void OnData(DataStreamReader stream, NetworkConnection cnn, Server server = null)
    {
        /* Handles a message (stream reader) of type NetworkEvent.Type.Data */
        NetMessage msg = null;
        // grab code information from it
        var opCode = (OperationCode)stream.ReadByte();
        switch (opCode)
        {
            case OperationCode.KEEP_ALIVE: msg = new NetKeepAlive(stream); break;
            case OperationCode.WELCOME: msg = new NetWelcome(stream); break;
            case OperationCode.START_FIRST_GAME: msg = new NetStartFirstGame(stream); break;
            case OperationCode.MAKE_MOVE: msg = new NetMakeMove(stream); break;
            case OperationCode.REMATCH_DEMAND: msg = new NetRematchDemand(stream); break;
            case OperationCode.REMATCH: msg = new NetRematch(stream); break;
            default:
                Debug.LogError("Message received had no OperationCode");
                break;
        }

        if (server != null)
            msg.ReceivedOnServer(cnn);
        else
            msg.ReceivedOnClient();
    }
    
    // Client Net Messages
    public static Action<NetMessage> C_KEEP_ALIVE;
    public static Action<NetMessage> C_WELCOME;
    public static Action<NetMessage> C_START_FIRST_GAME;
    public static Action<NetMessage> C_MAKE_MOVE;
    public static Action<NetMessage> C_REMATCH;
    
    // Server Net Messages
    public static Action<NetMessage, NetworkConnection> S_KEEP_ALIVE;
    public static Action<NetMessage, NetworkConnection> S_WELCOME;
    public static Action<NetMessage, NetworkConnection> S_START_FIRST_GAME;
    public static Action<NetMessage, NetworkConnection> S_MAKE_MOVE;
    public static Action<NetMessage, NetworkConnection> S_REMATCH_DEMAND;
}
