using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
[DisallowMultipleComponent]
public abstract class UIBase : MonoBehaviour
{
    [Header("UI Identity")]
    [SerializeField] private string uiId = "UniqueId";      // set in prefab
    [SerializeField] private UILayer layer = UILayer.Screen; // screen / popup / overlay
    [SerializeField] private bool isPopup = false;           // popup stack target

    [Header("Cached")]
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected CanvasGroup canvasGroup;

    public string Id => uiId;
    public UILayer Layer => layer;
    public bool IsPopup => isPopup;
    public bool IsVisible { get; private set; }

    protected virtual void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
        }
        canvas.overrideSorting = true; // sorting control by manager
    }

    // called once right after instantiate
    public virtual void OnPreload()
    {
        // override if needed
    }

    public virtual void Show(object param = null)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        gameObject.SetActive(true);
        IsVisible = true;
    }

    public virtual void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        // keep active for instant re-show (optional to SetActive(false))
        this.gameObject.SetActive(false);
        IsVisible = false;
    }

    public void HideInstant()
    {
        Hide();
    }

    public void SetInteractable(bool enable)
    {
        if (canvasGroup == null) return;
        canvasGroup.blocksRaycasts = enable;
        canvasGroup.interactable = enable;
    }
}
