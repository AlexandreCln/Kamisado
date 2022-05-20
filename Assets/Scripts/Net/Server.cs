using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using System;

public class Server : MonoBehaviour
{
    #region Singleton implementation
    public static Server Instance;

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public NetworkDriver driver;
    public Action connectionDropped;
    private NativeList<NetworkConnection> _connections;

    private bool _isActive = false;
    private const int _connectionCapacity = 2; // max connections on the server

    private const float _keepAliveTickRate = 20f;
    private float _lastKeepAlive;

    public void Init(ushort port)
    {
        NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = port;
        driver = NetworkDriver.Create();

        if (driver.Bind(endpoint) != 0)
        {
            // TODO: handle multiple games at a time
            Debug.LogWarning("Unable to bind on port " + endpoint.Port);
            return;
        }
        else
        {
            driver.Listen();
            Debug.Log("Listening on port " + endpoint.Port);
        }
        _connections = new NativeList<NetworkConnection>(_connectionCapacity, Allocator.Persistent);
        _isActive = true;
    }

    public void Shutdown()
    {
        if (_isActive)
        {
            driver.Dispose();
            _connections.Dispose();
            _isActive = false;
            Debug.Log("Server shutdown");
        }
    }

    #region Server specific
    public void SendToClient(NetworkConnection connection, NetMessage msg)
    {
        // a writer serialize data over the network
        DataStreamWriter writer;
        // start the connection, the DataStreamWriter get sending informations through a pipeline
        driver.BeginSend(connection, out writer);
        // TOTO: better to do an if (driver.BeginSend(connection, out writer) != 0) here ? and then EndSend only for this case
        // fill in the packet
        msg.Serialize(ref writer);
        // send
        driver.EndSend(writer);
    }

    public void Broadcast(NetMessage msg)
    {
        /* Send the same message to every clients */
        for (int i = 0; i < _connections.Length; i++)
        {
            if (_connections[i].IsCreated)
            {
                Debug.Log($"Sending {msg.Code} to : {_connections[i].InternalId}");
                SendToClient(_connections[i], msg);
            }
        }
    }
    #endregion

    private void Update()
    {
        if (!_isActive)
            return;

        _KeepAlive();

        // as NetworkDriver is part of the job system, empties up the queue of messages coming in 
        driver.ScheduleUpdate().Complete();

        _CleanupConnections();
        _AcceptNewConnections();
        _UpdateMessagePump();
    }

    private void OnDestroy()
    {
        Shutdown();
    }

    private void _KeepAlive()
    {
        /* Test the connection with he server at regular intervals */
        if (Time.time - _lastKeepAlive > _keepAliveTickRate)
        {
            _lastKeepAlive = Time.time;
            Broadcast(new NetKeepAlive());
        }
    }

    private void _CleanupConnections()
    {
        /* Remove connections references that are not connected to the server */
        for (int i = 0; i < _connections.Length; i++)
        {
            if (!_connections[i].IsCreated)
            {
                _connections.RemoveAtSwapBack(i);
                i--;
            }
        }
    }

    private void _AcceptNewConnections()
    {
        NetworkConnection connection;
        // The NetworkDriver's Accept() method check and returns pending clients connections. 
        // (I assume only those accepted by config ? Is this returns a NetworkConection for each one?)
        // Maybe Accept() accepts a waiting client one by one AND pop it to return the connection instance? 
        while ((connection = driver.Accept()) != default(NetworkConnection))
        {
            // accepts pending clients waiting to connect
            _connections.Add(connection);
        }
    }

    private void _UpdateMessagePump()
    {
        /* Check received messages, and if we have to reply or not */
        DataStreamReader stream;
        for (int i = 0; i < _connections.Length; i++)
        {
            NetworkEvent.Type cmd;
            // poll the driver for all the messages from all the connections
            while ((cmd = driver.PopEventForConnection(_connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    Debug.Log("Server pop a received message of type Data from the client");
                    NetUtility.OnData(stream, _connections[i], this);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.LogWarning("Client disconnected from server");
                    // reset the connection of the player 
                    _connections[i] = default(NetworkConnection);
                    // trigger non empty action
                    connectionDropped?.Invoke();
                    // shutdown the game on any deconnection, it's not common but it makes sense in this 2 player game
                    Shutdown();
                }
            }
        }
    }
}
