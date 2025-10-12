using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StageInUI : UIBase
{
    [Header("Refs")]
    [SerializeField] private LevelProgressManager3D progress;
    [SerializeField] private RectTransform trackRect;      // parent rect of markers (this rect)
    [SerializeField] private RectTransform markerStart;
    [SerializeField] private RectTransform markerDead;
    [SerializeField] private RectTransform markerPlayer;
    [SerializeField] private RectTransform markerGoal;

    [Header("Visual Options")]
    [SerializeField] private bool fadeHiddenGoal = true;
    [SerializeField] private float dangerDeadGap = 2.0f;
    [SerializeField] private Graphic markerDeadGraphic;
    [SerializeField] private float blinkSpeed = 6.0f;

    [Header("Vertical Mapping")]
    [Tooltip("If true, t=0 is at top (+Y), t=1 is at bottom (-Y). If false, t=0 bottom, t=1 top.")]
    [SerializeField] private bool invertTopToBottom = true;

    private float halfHeight;
    private bool initialized;

    protected override void Awake()
    {
        base.Awake();

        if (canvas != null)
        {
            canvas.overrideSorting = true;
        }
    }

    public override void OnPreload()
    {
        base.OnPreload();
        TryInit();
        PlaceStaticEnds();
        UpdateAllImmediate();
    }

    public override void Show(object param = null)
    {
        base.Show(param);
        TryInit();
        UpdateAllImmediate();
    }

    public override void Hide()
    {
        base.Hide();
    }

    private void Update()
    {
        if (!IsVisible)
        {
            return;
        }
        if (!initialized)
        {
            TryInit();
            if (!initialized)
            {
                return;
            }
        }
        if (trackRect == null || progress == null)
        {
            return;
        }

        float h = trackRect.rect.height * 0.5f;
        if (!Mathf.Approximately(halfHeight, h))
        {
            halfHeight = h;
        }

        float total = Mathf.Max(0.0001f, progress.TargetDistance);
        float traveled = Mathf.Clamp(progress.distanceTraveled, 0f, total);
        float tPlayer = traveled / total;

        float dzDist = Mathf.Max(0f, progress.distanceTraveled - progress.CurrentDeadGap);
        float tDead = Mathf.Clamp01(dzDist / total);

        float tAppear = Mathf.Clamp01(progress.GoalAppearAt);
        bool goalVisibleNow = (tPlayer >= tAppear) || !progress.HideGoalBeforeAppear;

        SetMarker01Vertical(markerPlayer, tPlayer);
        SetMarker01Vertical(markerDead, tDead);
        SetMarker01Vertical(markerStart, 0f);
        SetMarker01Vertical(markerGoal, 1f);

        if (markerGoal != null)
        {
            if (fadeHiddenGoal)
            {
                SetGraphicAlpha(markerGoal.GetComponent<Graphic>(), goalVisibleNow ? 1f : 0.2f);
            }
            else
            {
                markerGoal.gameObject.SetActive(goalVisibleNow);
            }
        }

        if (markerDeadGraphic != null)
        {
            float gap = progress.CurrentDeadGap;
            if (gap <= dangerDeadGap)
            {
                float a = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * blinkSpeed);
                SetGraphicAlpha(markerDeadGraphic, Mathf.Lerp(0.3f, 1f, a));
            }
            else
            {
                SetGraphicAlpha(markerDeadGraphic, 1f);
            }
        }
    }

    private void TryInit()
    {
        if (initialized)
        {
            return;
        }
        if (trackRect == null)
        {
            trackRect = GetComponent<RectTransform>();
        }
        if (trackRect == null)
        {
            return;
        }
        if (progress == null)
        {
            progress = LevelProgressManager3D.Instance;
        }
        if (progress == null)
        {
            return;
        }

        halfHeight = trackRect.rect.height * 0.5f;
        initialized = true;
    }

    private void PlaceStaticEnds()
    {
        if (!initialized)
        {
            return;
        }
        SetMarker01Vertical(markerStart, 0f);
        SetMarker01Vertical(markerGoal, 1f);
    }

    private void UpdateAllImmediate()
    {
        if (!initialized)
        {
            return;
        }

        float total = Mathf.Max(0.0001f, progress.TargetDistance);
        float traveled = Mathf.Clamp(progress.distanceTraveled, 0f, total);
        float tPlayer = traveled / total;

        float dzDist = Mathf.Max(0f, progress.distanceTraveled - progress.CurrentDeadGap);
        float tDead = Mathf.Clamp01(dzDist / total);

        SetMarker01Vertical(markerPlayer, tPlayer);
        SetMarker01Vertical(markerDead, tDead);
        SetMarker01Vertical(markerStart, 0f);
        SetMarker01Vertical(markerGoal, 1f);
    }

    private void SetMarker01Vertical(RectTransform rt, float t01)
    {
        if (rt == null)
        {
            return;
        }
        t01 = Mathf.Clamp01(t01);

        // Map t to Y: top(+halfHeight) -> bottom(-halfHeight) if invertTopToBottom == true.
        float y;
        if (invertTopToBottom)
        {
            y = Mathf.Lerp(halfHeight, -halfHeight, t01);
        }
        else
        {
            y = Mathf.Lerp(-halfHeight, halfHeight, t01);
        }

        Vector2 p = rt.anchoredPosition;
        p.y = y;
        rt.anchoredPosition = p;
    }

    private void SetGraphicAlpha(Graphic g, float a)
    {
        if (g == null)
        {
            return;
        }
        Color c = g.color;
        c.a = Mathf.Clamp01(a);
        g.color = c;
    }
}
