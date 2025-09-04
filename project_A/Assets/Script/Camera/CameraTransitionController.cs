using System.Collections;
using UnityEngine;

public class CameraTransitionController : MonoBehaviour
{
    [Header("Cameras")]
    public Camera cameraA;          // Home camera
    public Camera cameraB;          // In-game camera (disabled initially)

    [Header("Poses")]
    public Transform initPose;      // optional: where cameraA should start from 
    public Transform targetPose;    // where cameraA should smoothly move/rotate to

    [Header("Motion")]
    public float duration = 1.0f;   // seconds
    public AnimationCurve easing = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool slerpRotation = true;

    [Header("Options")]
    public bool setCameraAFromInitOnAwake = true;  // put cameraA at initPose on Awake
    public bool switchToBOnComplete = true;        // enable cameraB when done

    bool isRunning;
    public HomePlayer homePlayer;

    void Awake()
    {
        if (cameraA == null || cameraB == null || targetPose == null)
        {
            Debug.LogWarning("[CameraAtoBTransition] Missing references.");
            return;
        }
        
        if (setCameraAFromInitOnAwake && initPose != null)
        {
            cameraA.transform.position = initPose.position;
            cameraA.transform.rotation = initPose.rotation;
        }

        cameraA.gameObject.SetActive(true);
        cameraB.gameObject.SetActive(false);
    }

    public void Begin()
    {
        if (isRunning)
        {
            return;
        }
        if (cameraA == null || cameraB == null || targetPose == null)
        {
            Debug.LogWarning("[CameraAtoBTransition] Missing references.");
            return;
        }
        StartCoroutine(Co_MoveThenSwitch());
    }

    IEnumerator Co_MoveThenSwitch()
    {
        isRunning = true;
        if (homePlayer != null)
        {
            homePlayer.StartGame();
        }
        yield return new WaitForSeconds(1f);
        Transform cam = cameraA.transform;

        Vector3 p0 = cam.position;
        Quaternion r0 = cam.rotation;

        Vector3 p1 = targetPose.position;
        Quaternion r1 = targetPose.rotation;

        float t = 0f;
        float dur = Mathf.Max(0.0001f, duration);

        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            float k = easing != null ? easing.Evaluate(t) : t;

            cam.position = Vector3.LerpUnclamped(p0, p1, k);
            if (slerpRotation)
            {
                cam.rotation = Quaternion.SlerpUnclamped(r0, r1, k);
            }
            else
            {
                cam.rotation = Quaternion.LerpUnclamped(r0, r1, k);
            }

            yield return null;
        }

        // snap to exact target in case of float error
        cam.position = p1;
        cam.rotation = r1;
        yield return new WaitForSeconds(0.25f);
        if (switchToBOnComplete)
        {
            cameraA.gameObject.SetActive(false);
            cameraB.gameObject.SetActive(true);
            GameManager.instance.isStart = true;
        }

        isRunning = false;
    }
}