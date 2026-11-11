using UnityEngine;
using UnityEngine.Rendering.Universal;

public sealed class LanternPickupPulse : MonoBehaviour
{
    [SerializeField] private Light2D targetLight;
    [SerializeField] private float baseIntensity = 0.7f;
    [SerializeField] private float pulseAmount = 0.08f;
    [SerializeField] private float pulseSpeed = 1.1f;

    private void Update()
    {
        if (targetLight)
            targetLight.intensity = baseIntensity + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
    }
}
