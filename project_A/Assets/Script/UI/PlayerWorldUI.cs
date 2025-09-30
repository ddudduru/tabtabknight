using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWorldUI : MonoBehaviour
{
    [Header("Owner")]
    [SerializeField] private Player_Control owner;  // optional; will auto-find if null
    [SerializeField] private bool followOwner = true;
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 2f, 0f);

    [Header("HP")]
    [SerializeField] private Image hpFill;

    [Header("Stamina")]
    [SerializeField] private Image staminaFill;

    [Header("Skill Gauge")]
    [SerializeField] private GameObject skillPanel;
    [SerializeField] private Image skillFill;
    [SerializeField] private float skillLerpSpeed = 8f;

    [Header("Dizzy")]
    [SerializeField] private GameObject dizzyPanel;
    [SerializeField] private TextMeshProUGUI dizzyText;

    // internal state
    private float targetSkillFill = 0f;
    private bool skillVisible = false;

    private void Awake()
    {
        if (owner == null)
        {
            owner = Player_Control.Instance;
        }

        if (skillPanel != null)
        {
            skillPanel.SetActive(false);
        }
        if (dizzyPanel != null)
        {
            dizzyPanel.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (owner == null)
        {
            return;
        }

        // World-space follow (optional)
        if (followOwner)
        {
            transform.position = owner.transform.position + followOffset;
        }

        // Dizzy UI
        if (dizzyPanel != null && dizzyText != null)
        {
            if (owner.IsDizzy)
            {
                if (!dizzyPanel.activeSelf)
                {
                    dizzyPanel.SetActive(true);
                }
                dizzyText.text = owner.DizzyAmount.ToString("0");
            }
            else
            {
                if (dizzyPanel.activeSelf)
                {
                    dizzyPanel.SetActive(false);
                }
            }
        }

        // HP UI
        if (hpFill != null)
        {
            float maxHp = Mathf.Max(0.0001f, owner.maxHP);
            float hp01 = Mathf.Clamp01(owner.CurrentHP / maxHp);
            hpFill.fillAmount = hp01;
        }

        // Stamina UI
        if (staminaFill != null)
        {
            float maxSt = Mathf.Max(0.0001f, owner.maxStamina);
            float st01 = Mathf.Clamp01(owner.currentStamina / maxSt);
            staminaFill.fillAmount = st01;
        }

        // Skill Gauge
        if (skillPanel != null && skillFill != null)
        {
            if (!skillVisible)
            {
                if (skillPanel.activeSelf)
                {
                    skillPanel.SetActive(false);
                }
                return;
            }

            skillFill.fillAmount = Mathf.Lerp(
                skillFill.fillAmount,
                targetSkillFill,
                Time.deltaTime * skillLerpSpeed
            );

            // auto-hide near zero
            if (targetSkillFill <= 0.001f && skillFill.fillAmount <= 0.01f)
            {
                skillVisible = false;
                skillPanel.SetActive(false);
            }
        }
    }

    // ----- Public API for external calls -----

    /// <summary>
    /// Called by skill logic per tick. currentTime <= maxTime.
    /// </summary>
    public void UpdateSkillTime(float currentTime, float maxTime)
    {
        if (skillPanel == null || skillFill == null)
        {
            return;
        }

        float denom = Mathf.Max(0.0001f, maxTime);
        targetSkillFill = Mathf.Clamp01(currentTime / denom);

        if (!skillVisible && targetSkillFill > 0f)
        {
            skillVisible = true;
            skillPanel.SetActive(true);
            skillFill.fillAmount = targetSkillFill;
        }
    }

    public void EndSkillTime()
    {
        targetSkillFill = 0f;
        skillVisible = false;
    }

    public void SetOwner(Player_Control newOwner)
    {
        owner = newOwner;
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
