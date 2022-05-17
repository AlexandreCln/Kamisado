using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public Server server;
    public Client client;
    [SerializeField] private TMP_InputField _addressInput;
    [SerializeField] private GameObject _victoryScreen;
    private Animator _menuAnimator;

    // TODO: remove singleton
    #region Singleton implementation 
    public static GameUI Instance { set; get; }
    void Awake()
    {
        Instance = this;
        _menuAnimator = GetComponent<Animator>();
        _RegisterEvents();
    }
    #endregion

    // START MENU
    public void OnLocalGameButton()
    {
        _menuAnimator.SetTrigger("InGameMenu");
        EventManager.TriggerEvent("StartLocalGame");
    }

    public void OnOnlineGameButton()
    {
        _menuAnimator.SetTrigger("OnlineMenu");
    }

    // ONLINE MENU
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

    // HOST MENU
    public void OnHostBackButton()
    {
        server.Shutdown();
        client.Shutdown();
        _menuAnimator.SetTrigger("OnlineMenu");
    }

    private void _RegisterEvents()
    {
        EventManager.AddListener("GameEnded", _OnGameEnded);
        
        NetUtility.C_START_GAME += _OnStartGameClient;
    }

    private void _OnStartGameClient(NetMessage msg)
    {
        _menuAnimator.SetTrigger("InGameMenu");
    }

    // Events
    private void _OnGameEnded(object winnerName)
    {
        _victoryScreen.SetActive(true);
        _victoryScreen.transform.Find("WinnerText").GetComponent<TMP_Text>().text = $"Player {(string)winnerName} win !";
    }
}
