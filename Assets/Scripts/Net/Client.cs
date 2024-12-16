using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour {
    #region Singleton implementation
    public static Client Instance {set; get; }
    private void Awake() {
        Instance = this;
    }
    #endregion
    public NetworkDriver driver;
    private NetworkConnection connection;
    private bool isActive = false;
    public Action connectionDropped;

    //Methods
    public void Init(string ip, ushort port) {
        driver = NetworkDriver.Create();

        if(string.IsNullOrEmpty(ip) || port <= 0) {
            Debug.Log("Invalid IP or port");
            return;
        }

        NetworkEndpoint endpoint = NetworkEndpoint.Parse(ip, port);
        endpoint.Port = port;

        connection = driver.Connect(endpoint);

        Debug.Log($"Attempting to connect to Server on {endpoint.Address}:{endpoint.Port}");

        isActive = true;

        RegisterToEvent();
    }

    public void Shutdown() {
        if(isActive) {
            UnregisterToEvent();
            driver.Dispose();
            isActive = false;
            connection = default(NetworkConnection);
        }
    }

    public void OnDestroy() {
        Shutdown();
    }

    public void Update() {
        if(!isActive) {
            return;
        }

        driver.ScheduleUpdate().Complete();
        CheckAlive();

        UpdateMessagePump();
    }

    private void CheckAlive() {
        if (!connection.IsCreated && isActive) {
            Debug.Log("Lost connection to server");
            connectionDropped?.Invoke();
            Shutdown();
        }
    }

    private void UpdateMessagePump() {
        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty) {
            if (cmd == NetworkEvent.Type.Connect) {
                SendToServer(new NetWelcome());
                Debug.Log("Connected!");
            } else if(cmd == NetworkEvent.Type.Data) {
                NetUtility.OnData(stream, default(NetworkConnection));
            } else if(cmd == NetworkEvent.Type.Disconnect) {
                Debug.Log("Client got disconnected from server");
                connection = default(NetworkConnection);
                connectionDropped?.Invoke();
            }
        }
    }

    public void SendToServer(NetMessage msg) {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }


    //Event Parsing
    private void RegisterToEvent() {
        NetUtility.C_KEEP_ALIVE += OnKeepAlive;
    }

    private void UnregisterToEvent() {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    }

    private void OnKeepAlive(NetMessage nm) {
        //Send back to server
        SendToServer(nm);
    }

}
