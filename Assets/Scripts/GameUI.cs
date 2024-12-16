using TMPro;
using UnityEngine;

public enum CameraAngle {
    menu,
    whiteTeam,
    blackTeam
}

public class GameUI : MonoBehaviour {
    public static GameUI Instance {set; get;}

    public Server server;
    public Client client;

    [SerializeField] private Animator menuAnimator;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private GameObject[] cameraAngles;

    
    private void Awake() {
        Instance = this;
    }

    //Cameras
    public void ChangeCamera(CameraAngle index) {
        for(int i = 0; i < cameraAngles.Length; i++) {
            cameraAngles[i].SetActive(false);
        }
        cameraAngles[(int)index].SetActive(true);
    }

    //Buttons
    public void OnLocalGameButton() {
        menuAnimator.SetTrigger("NoMenu");
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
    }

    public void OnOnlineGameButton() {
        menuAnimator.SetTrigger("OnlineMenu");
    }

    public void OnOnlineHostButton() {
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
        menuAnimator.SetTrigger("HostMenu");
    }

    public void OnOnlineConnectButton() {
        if (server != null && server.isActive) {
            client.Init(addressInput.text, 8007);
        } else {
            Debug.LogError("Server is not active. Please start the server first.");
        }
    }

    public void OnOnlineBackButton() {
        menuAnimator.SetTrigger("StartMenu");
    }

    public void OnHostBackButton() {
        server.Shutdown();
        client.Shutdown();
        menuAnimator.SetTrigger("OnlineMenu");
    }

    private void InitializeServerClient(string ip, ushort port) {
        Debug.Log($"Initializing server and client on IP: {ip}, Port: {port}");
        server.Init(port);
        client.Init(ip, port);
    }
}
