using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public Server server;
    public Client client;
    [SerializeField] private TMP_InputField _addressInput;
    [SerializeField] private GameObject _menusBackground;
    [SerializeField] private GameObject _victoryLocalScreen;
    [SerializeField] private GameObject _victoryNetworkScreen;
    [SerializeField] private GameObject _waitingRematchScreen;
    private Animator _menuAnimator;

    void Awake()
    {
        _menuAnimator = GetComponent<Animator>();
        _RegisterEvents();
    }

    // START MENU BUTTONS
    public void OnLocalGameButton()
    {
        _menusBackground.SetActive(false);
        _menuAnimator.SetTrigger("InGameMenu");
        EventManager.TriggerEvent("StartLocalGame");
    }

    public void OnOnlineGameButton()
    {
        _menuAnimator.SetTrigger("OnlineMenu");
    }

    // ONLINE MENU BUTTONS
    public void OnOnlineHostButton()
    {
        _menuAnimator.SetTrigger("HostMenu");
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
    }

    public void OnOnlineConnectButton()
    {
        client.Init(_addressInput.text, 8007);
    }

    public void OnOnlineBackButton()
    {
        _menuAnimator.SetTrigger("StartMenu");
    }

    // HOST MENU BUTTONS
    public void OnHostBackButton()
    {
        server.Shutdown();
        client.Shutdown();
        EventManager.TriggerEvent("DisconnectHost");
        _menuAnimator.SetTrigger("OnlineMenu");
    }

    // VICTORY SCREEN BUTTONS
    public void OnRematchLocalButton()
    {
        _menusBackground.SetActive(false);
        _victoryLocalScreen.SetActive(false);
        EventManager.TriggerEvent("LocalRematch");
    }
    
    public void OnRematchDemandButton()
    {
        _victoryNetworkScreen.SetActive(false);
        _waitingRematchScreen.SetActive(true);
        Client.Instance.SendToServer(new NetRematchDemand());
    }

    private void _RegisterEvents()
    {
        EventManager.AddListener("LocalGameEnded", _OnLocalGameEnded);
        EventManager.AddListener("NetworkGameEnded", _OnNetworkGameEnded);
        
        NetUtility.C_START_GAME += _OnStartGameClient;
        NetUtility.C_REMATCH += _OnRematch;
    }

    // Local Events
    private void _OnLocalGameEnded(object winnerName)
    {
        _menusBackground.SetActive(true);
        _victoryLocalScreen.SetActive(true);
        _victoryLocalScreen.transform.Find("WinnerText").GetComponent<TMP_Text>().text = $"Player {(string)winnerName} win !";
    }

    private void _OnNetworkGameEnded(object winnerName)
    {
        _victoryNetworkScreen.SetActive(true);
        _victoryNetworkScreen.transform.Find("WinnerText").GetComponent<TMP_Text>().text = $"Player {(string)winnerName} win !";
    }

    // Network Events
    private void _OnStartGameClient(NetMessage msg)
    {
        _menuAnimator.SetTrigger("InGameMenu");
        _menusBackground.SetActive(false);
    }

    private void _OnRematch(NetMessage msg)
    {
        _waitingRematchScreen.SetActive(false);
    }
}
