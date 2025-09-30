using UnityEngine;
using UnityEngine.UI;

public class HomeUI : UIBase
{
    [Header("Refs")]
    [SerializeField] private Button startButton;

    // cached from OpenParam
    private GameStateMachine machine;
    private bool isReturn;

    public struct OpenParam
    {
        public GameStateMachine Machine; // required: who to notify when start is pressed
        public bool IsReturn;            // optional: which intro sequence to play
    }

    public override void OnPreload()
    {
        // keep as-is (no-op)
    }

    protected override void Awake()
    {
        base.Awake();

        if (startButton == null)
        {
            foreach (var b in GetComponentsInChildren<Button>(true))
            {
                if (b.name.ToLower().Contains("start"))
                {
                    startButton = b;
                    break;
                }
            }
        }

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(() => Debug.Log("Start Click!"));
            startButton.onClick.AddListener(OnClickStart);
        }
    }

    public override void Show(object param = null)
    {
        base.Show(param);

        // defaults
        machine = null;
        isReturn = false;

        if (param is OpenParam p)
        {
            machine = p.Machine;
            isReturn = p.IsReturn;
        }

        // play intro sequence
        if (isReturn == false)
            StartSequenceController.Instance.Sit();
        else
            StartSequenceController.Instance.Return();
    }

    public override void Hide()
    {
        base.Hide();
        // nothing else to clean
    }

    private void OnClickStart()
    {
        SoundManager.instance.Play_SoundEffect(SoundManager.SoundType.Effect_Button_Click);

        if (machine != null)
        {
            // delegate to game state machine (±«¿Â)
            machine.StartGame();
            StartSequenceController.Instance.Begin();
        }
        else
        {
            // fallback if machine not passed
            LoadScene_Control.LoadScene("GameScene");
        }
    }
}
