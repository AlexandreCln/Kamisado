using UnityEngine;

public class GameUI : MonoBehaviour
{
    private Animator _menuAnimator;
    public static GameUI Instance { set; get; }
    void Awake()
    {
        Instance = this;
        _menuAnimator = GetComponent<Animator>();
    }

    // START MENU
    public void OnLocalGameButton()
    {
        Debug.Log("OnLocalGameButton");
        _menuAnimator.SetTrigger("InGameMenu");
    }

    public void OnOnlineGameButton()
    {
        Debug.Log("OnOnlineGameButton");
        _menuAnimator.SetTrigger("OnlineMenu");
    }

    // ONLINE MENU
    public void OnOnlineHostButton()
    {
        Debug.Log("OnOnlineHostButton");
        _menuAnimator.SetTrigger("HostMenu");
    }

    public void OnOnlineConnectButton()
    {
        Debug.Log("OnOnlineConnectButton"); // ??
    }

    public void OnOnlineBackButton()
    {
        Debug.Log("OnOnlineBackButton");
        _menuAnimator.SetTrigger("StartMenu");

    }

    // HOST MENU
    public void OnHostBackButton()
    {
        Debug.Log("OnHostBackButton");
        _menuAnimator.SetTrigger("OnlineMenu");
    }
}
