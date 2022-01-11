using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// https://www.youtube.com/watch?v=BC3AKOQUx04&t=499s
public class UiDamageIndicator : MonoBehaviour
{
    public float maxTimer = 1.0f;
    private float timer = 1.0f;

    public float fadeSpeed = 4.0f;

    [SerializeField] private RectTransform rect;
    [SerializeField] private Image image;

    // The direction of indicator should refer to camera transform 
    private Transform cameraControllerTransform;
    private Transform target;

    private Quaternion targetRotation = Quaternion.identity;
    private Vector3 targetPosition = Vector3.zero;

    private IEnumerator countdownEnumerator;

    public void Register(Transform cameraControllerTransform, Transform target)
    {
        this.cameraControllerTransform = cameraControllerTransform;
        this.target = target;

        StartCoroutine(RotateToTarget());
        StartCountdown();
    }

    public void StartCountdown()
    {
        if (countdownEnumerator != null)
            StopCoroutine(countdownEnumerator);

        countdownEnumerator = Countdown();
        StartCoroutine(countdownEnumerator);
    }

    public void Restart()
    {
        timer = maxTimer;
        StartCountdown();
    }

    IEnumerator RotateToTarget()
    {
        while (enabled)
        {
            if (target)
            {
                targetPosition = target.position;
                
            }
            // https://forum.unity.com/threads/help-with-damage-indicator-script.560920/
            var direction = cameraControllerTransform.position - targetPosition;
            direction.Normalize();

            var angle = GetHitAngle(cameraControllerTransform, direction);
            rect.rotation = Quaternion.Euler(0, 0, -angle);

            yield return null;
        }
    }

    public float GetHitAngle(Transform cameraTransform, Vector3 incomingDir)
    {
        // Flatten to plane
        // The hit indicator is shown on a 2D canvas so the y direction should be ignored.
        var otherDir = new Vector3(-incomingDir.x, 0f, -incomingDir.z);
        var cameraFwd = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);

        // Direction between player fwd and incoming object
        var angle = Vector3.SignedAngle(cameraFwd, otherDir, Vector3.up);

        return angle;
    }

    IEnumerator Countdown()
    {
        // First , fade in the indicator
        while (image.color.a < 1.0f)
        {
            Color tempColor = image.color;
            tempColor.a += fadeSpeed * Time.deltaTime;

            image.color = tempColor;
            yield return null;
        }
        // Then , stay for time specified
        while (timer > 0)
        {
            timer--;
            yield return new WaitForSeconds(1);
        }
        // Last , fade out the indicator
        while (image.color.a > 0f)
        {
            Color tempColor = image.color;
            tempColor.a -= fadeSpeed * Time.deltaTime;

            image.color = tempColor;
            yield return null;
        }

        // Optional to destroy at end
        // Destroy(gameObject);
    }
    
}
