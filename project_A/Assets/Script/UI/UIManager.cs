using System.Collections.Generic;
using TMPro;
using UnityEditor.SceneManagement;
using System;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public enum UILayer
{
    Screen = 0,
    Popup = 1,
    Overlay = 2
}

[DisallowMultipleComponent]
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Preload Prefabs (drag & drop)")]
    [SerializeField] private List<GameObject> preloadPrefabs = new List<GameObject>();

    [Header("Behavior")]
    [SerializeField] private bool hideOtherScreensOnShow = true;

    // registries
    private readonly Dictionary<string, UIBase> _byId = new Dictionary<string, UIBase>(128);
    private readonly Dictionary<Type, UIBase> _byType = new Dictionary<Type, UIBase>(128);

    // popup stack
    private readonly Stack<UIBase> _popupStack = new Stack<UIBase>(32);

    // sorting
    private const int ScreenBaseOrder = 0;
    private int _nextSortingOrder = 1;

    // top-most visible UI (by sorting order)
    public UIBase TopMost { get; private set; }

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

    public void InitializeUI()
    {
        foreach (var prefab in preloadPrefabs)
        {
            if (prefab == null) continue;

            GameObject go = Instantiate(prefab, transform);
            go.name = prefab.name;

            var ui = go.GetComponent<UIBase>();
            if (ui == null)
            {
                Debug.LogError($"UI prefab {prefab.name} missing UIBase-derived component.");
                Destroy(go);
                continue;
            }
            if (string.IsNullOrWhiteSpace(ui.Id))
            {
                Debug.LogError($"UI prefab {prefab.name} has empty Id. Set a unique Id.");
                Destroy(go);
                continue;
            }
            if (_byId.ContainsKey(ui.Id))
            {
                Debug.LogError($"Duplicate UI Id: {ui.Id}");
                Destroy(go);
                continue;
            }

            // default sorting setup
            var c = ui.GetComponent<Canvas>();
            if (ui.Layer == UILayer.Screen)
            {
                c.sortingOrder = ScreenBaseOrder;
            }
            else
            {
                c.sortingOrder = _nextSortingOrder++;
            }

            ui.OnPreload();
            ui.HideInstant();

            _byId.Add(ui.Id, ui);
            _byType.Add(ui.GetType(), ui);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_popupStack.Count > 0)
            {
                // close only the top popup
                InternalHide(_popupStack.Peek());
            }
        }
    }

    // ---------- Public API (Id) ----------

    public UIBase Show(string id, object param = null)
    {
        if (!_byId.TryGetValue(id, out var ui))
        {
            Debug.LogError($"Show failed. Unknown UI Id: {id}");
            return null;
        }
        InternalShow(ui, param);
        return ui;
    }

    public void Hide(string id)
    {
        if (_byId.TryGetValue(id, out var ui))
        {
            InternalHide(ui);
        }
    }

    public bool TryGet(string id, out UIBase ui) => _byId.TryGetValue(id, out ui);

    // ---------- Public API (Type) ----------

    public T Show<T>(object param = null) where T : UIBase
    {
        if (!_byType.TryGetValue(typeof(T), out var ui))
        {
            Debug.LogError($"Show<{typeof(T).Name}> failed. Not registered.");
            return null;
        }
        InternalShow(ui, param);
        return ui as T;
    }

    public void Hide<T>() where T : UIBase
    {
        if (_byType.TryGetValue(typeof(T), out var ui))
        {
            InternalHide(ui);
        }
    }

    public bool TryGet<T>(out T ui) where T : UIBase
    {
        if (_byType.TryGetValue(typeof(T), out var u))
        {
            ui = u as T;
            return true;
        }
        ui = null;
        return false;
    }

    // ---------- Internals ----------

    private void InternalShow(UIBase ui, object param)
    {
        var c = ui.GetComponent<Canvas>();

        // screen policy: hide other screens (optional)
        if (!ui.IsPopup && hideOtherScreensOnShow)
        {
            foreach (var kv in _byId)
            {
                var other = kv.Value;
                if (other == ui) continue;
                if (!other.IsPopup && other.IsVisible)
                {
                    other.Hide();
                }
            }
            c.sortingOrder = ScreenBaseOrder;
        }

        // popup stacking
        if (ui.IsPopup)
        {
            if (_popupStack.Count > 0)
            {
                _popupStack.Peek().SetInteractable(false);
            }
            _popupStack.Push(ui);
            c.sortingOrder = _nextSortingOrder++;
        }
        else
        {
            // non-popup also brought to front among screens
            c.sortingOrder = ScreenBaseOrder;
        }

        ui.Show(param);
        ui.transform.SetAsLastSibling();
        RefreshTopMost();
    }

    private void InternalHide(UIBase ui)
    {
        if (!ui.IsVisible) return;

        if (ui.IsPopup)
        {
            // keep LIFO discipline: only top can be closed
            if (_popupStack.Count == 0 || _popupStack.Peek() != ui)
            {
                return;
            }
            _popupStack.Pop();
            ui.Hide();

            if (_popupStack.Count > 0)
            {
                _popupStack.Peek().SetInteractable(true);
            }
        }
        else
        {
            ui.Hide();
        }

        RefreshTopMost();
    }

    private void RefreshTopMost()
    {
        UIBase top = null;
        int topOrder = int.MinValue;

        foreach (var kv in _byId)
        {
            var ui = kv.Value;
            if (!ui.IsVisible) continue;

            var c = ui.GetComponent<Canvas>();
            int order = c != null ? c.sortingOrder : 0;

            if (order >= topOrder)
            {
                topOrder = order;
                top = ui;
            }
        }

        TopMost = top;
    }
}
