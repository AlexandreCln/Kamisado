using UnityEngine;
using Unity.Networking.Transport;
using System;

public class Client : MonoBehaviour
{
     #region Singleton implementation
    public static Client Instance;

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public NetworkDriver driver;
    public Action connectionDropped;
    private NetworkConnection _connection;
    private bool _isActive = false;
    private const float _keepAliveTickRate = 10f;
    private float _lastKeepAlive;

    public void Init(string ip, ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip, port);
        _connection = driver.Connect(endpoint);
        Debug.Log("Client attempting to connect to Server on " + endpoint.Address);
        _isActive = true;

        _RegisterToEvent();
    }

    public void Shutdown()
    {
        /* Shutdown connection so that client can reconnect to a server later */
        if (_isActive)
        {
            _UnregisterToEvent();
            driver.Dispose();
            _connection = default(NetworkConnection); // not sure it is mandatory but let's be safe
            _isActive = false;
            Debug.Log("Client shutdown");
        }
    }

    private void Update()
    {
        if (!_isActive)
            return;

        // as NetworkDriver is part of the job system, empties up the queue of messages coming in 
        driver.ScheduleUpdate().Complete();

        _CheckAlive();
        _UpdateMessagePump();
    }
    
    private void OnDestroy()
    {
        Shutdown();
    }

    private void _CheckAlive()
    {
        /* Check the connection with the server */
        if (!_connection.IsCreated && _isActive)
        {
            Debug.LogWarning("Lost connection with the server.");
            connectionDropped?.Invoke();
            Shutdown();
        }
    }

    public void SendToServer(NetMessage msg)
    {
        // a writer can serialize data over the network
        DataStreamWriter writer;
        // start the connection, the DataStreamWriter get sending informations through a pipeline
        driver.BeginSend(_connection, out writer);
        // fill in the packet
        msg.Serialize(ref writer);
        // send
        driver.EndSend(writer);
    }

    private void _UpdateMessagePump()
    {
        /* Check received messages, and if we have to reply or not */

        // a reader can deserialize data serialized by a writer
        DataStreamReader stream;
        NetworkEvent.Type cmd;
        // poll the client connection's messages
        while ((cmd = _connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                SendToServer(new NetWelcome());
                Debug.Log("Success connect event : Send to server a NetWelcome to ask the AssignedTeam ID");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                NetUtility.OnData(stream, default(NetworkConnection));
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.LogWarning("Client disconnected from server");
                // reset the client connection 
                _connection = default(NetworkConnection);
                // trigger non empty action
                connectionDropped?.Invoke();
                Shutdown();
            }
        }
    }

    #region Event parsing
    private void _RegisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE += _OnKeepAlive;
    }
    
    private void _UnregisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE -= _OnKeepAlive;
    }

    private void _OnKeepAlive(NetMessage nm)
    {
        /* Keep track of the keep alive messages send by the server and send it back, to keep both side alive */
        SendToServer(nm);
    }
    #endregion
}
