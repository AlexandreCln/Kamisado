using TMPro;
using UnityEngine;
using System.Collections;

public class GameUI : MonoBehaviour
{
    public Server server;
    public Client client;
    [SerializeField] private TMP_InputField _addressInput;
    [SerializeField] private GameObject _menusBackground;
    [SerializeField] private GameObject _victoryLocalScreen;
    [SerializeField] private GameObject _victoryNetworkScreen;
    [SerializeField] private GameObject _waitingRematchScreen;
    [SerializeField] private GameObject _opponentDisconnectedScreen;
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

    public void OnOnlineBackStartMenuButton()
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

    // VICTORY LOCAL SCREEN BUTTONS
    public void OnLocalVictoryScreenMenuButton()
    {
        _victoryLocalScreen.SetActive(false);
        _menuAnimator.SetTrigger("StartMenu");
        EventManager.TriggerEvent("LocalEndGame");
    }
    
    public void OnRematchLocalButton()
    {
        _menusBackground.SetActive(false);
        _victoryLocalScreen.SetActive(false);
        EventManager.TriggerEvent("LocalEndGame");
    }

    // VICTORY NETWORK SCREEN BUTTONS
    public void OnRematchDemandButton()
    {
        _waitingRematchScreen.SetActive(true);
        _victoryNetworkScreen.SetActive(false);
        Client.Instance.SendToServer(new NetRematchDemand());
    }

    // COMMON BUTTONS
    public void OnNetworkEndedGameMenuButton()
    {
        _waitingRematchScreen.SetActive(false);
        _victoryNetworkScreen.SetActive(false);
        _menuAnimator.SetTrigger("StartMenu");
        EventManager.TriggerEvent("NetworkEndGame");
    }
    
    private void _RegisterEvents()
    {
        EventManager.AddListener("LocalGameEnded", _OnLocalGameEnded);
        EventManager.AddListener("NetworkGameEnded", _OnNetworkGameEnded);
        EventManager.AddListener("OpponentDisconnected", _OnOpponentDisconnected);
        
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
        _menusBackground.SetActive(true);
        _victoryNetworkScreen.SetActive(true);
        _victoryNetworkScreen.transform.Find("WinnerText").GetComponent<TMP_Text>().text = $"Player {(string)winnerName} win !";
    }

    private void _OnOpponentDisconnected()
    {
        StartCoroutine("_HandlesOpponentDisconnected");
    }

    private IEnumerator _HandlesOpponentDisconnected()
    {
        _menusBackground.SetActive(true);
        _opponentDisconnectedScreen.SetActive(true);
        yield return new WaitForSeconds(4);
        _opponentDisconnectedScreen.SetActive(false);
        _menuAnimator.SetTrigger("StartMenu");
    }

    // Network Events
    private void _OnStartGameClient(NetMessage msg)
    {
        _menuAnimator.SetTrigger("InGameMenu");
        _menusBackground.SetActive(false);
    }

    private void _OnRematch(NetMessage msg)
    {
        _menusBackground.SetActive(false);
        _waitingRematchScreen.SetActive(false);
    }
}
