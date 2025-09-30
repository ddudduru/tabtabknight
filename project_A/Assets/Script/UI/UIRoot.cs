using UnityEngine;
using UnityEngine.EventSystems;

public class UIRoot : MonoBehaviour
{
    public static UIRoot Instance { get; private set; }

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

    private void Start()
    {
        UIManager.Instance.InitializeUI();
    }
}
