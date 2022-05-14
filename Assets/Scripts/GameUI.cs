using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public Server server;
    public Client client;
    [SerializeField] private TMP_InputField _addressInput;
    private Animator _menuAnimator;
    
    #region Singleton implementation
    public static GameUI Instance { set; get; }
    void Awake()
    {
        Instance = this;
        _menuAnimator = GetComponent<Animator>();
    }
    #endregion

    // START MENU
    public void OnLocalGameButton()
    {
        _menuAnimator.SetTrigger("InGameMenu");
        server.Init(8007);
        client.Init("127.0.0.1", 8007);
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
}
