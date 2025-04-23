using UnityEngine;

public class UserInfoManager : MonoBehaviour
{
    public static UserInfoManager Instance { get; private set; }

    public string Username { get; set; } = "Guest";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetUsername(string username)
    {
        Username = username;
    }
}
