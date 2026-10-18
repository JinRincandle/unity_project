using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TimeTravel
{
    public class PastHeatHazard : MonoBehaviour
    {
        [Header("Heat Settings")]
        [SerializeField] private float maxSafeTime = 90.0f; // Seconds before critical heat
        [SerializeField] private float heatDissipationRate = 15.0f;
        [SerializeField] private Light2D ambientHeatLight;
        [SerializeField] private Color normalHeatColor = new Color(1f, 0.4f, 0.1f, 1f);
        [SerializeField] private Color criticalHeatColor = new Color(1f, 0.1f, 0.05f, 1f);

        private float currentHeatTimer = 0f;
        private bool isWarningShown50 = false;
        private bool isWarningShown80 = false;
        private Vector3 checkpointPos;
        private Transform playerTransform;

        private void Awake()
        {
            var player = FindFirstObjectByType<Blocks.Character.BuildingBlocksCharacter>();
            if (player != null)
            {
                playerTransform = player.transform;
                checkpointPos = player.transform.position;
            }
        }

        private void Update()
        {
            currentHeatTimer += Time.deltaTime;
            float heatPercent = Mathf.Clamp01(currentHeatTimer / maxSafeTime);

            if (ambientHeatLight != null)
            {
                ambientHeatLight.color = Color.Lerp(normalHeatColor, criticalHeatColor, heatPercent);
                ambientHeatLight.intensity = 1.0f + heatPercent * 1.5f;
            }

            if (heatPercent >= 0.5f && !isWarningShown50)
            {
                isWarningShown50 = true;
                TemporalHUD.Instance?.ShowNotification("<color=#FFA500>HEAT ALERT (50%): Ambient volcanic temperature rising! Work swiftly!</color>", 3.0f);
            }

            if (heatPercent >= 0.8f && !isWarningShown80)
            {
                isWarningShown80 = true;
                TemporalHUD.Instance?.ShowNotification("<color=#FF3333>CRITICAL HEAT (80%): Magma radiation near lethal threshold!</color>", 3.5f);
            }

            if (heatPercent >= 1.0f)
            {
                TriggerHeatDisplacement();
            }
        }

        public void CoolDown(float amount)
        {
            currentHeatTimer = Mathf.Max(0, currentHeatTimer - amount);
            if (currentHeatTimer < maxSafeTime * 0.8f) isWarningShown80 = false;
            if (currentHeatTimer < maxSafeTime * 0.5f) isWarningShown50 = false;
        }

        private void TriggerHeatDisplacement()
        {
            currentHeatTimer = 0f;
            isWarningShown50 = false;
            isWarningShown80 = false;

            if (playerTransform != null)
            {
                playerTransform.position = checkpointPos;
                var rb = playerTransform.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }

            TemporalHUD.Instance?.ShowNotification("<color=#FF0000><b>HEAT OVERLOAD!</b> Temporal failsafe pulled you back to the checkpoint.</color>", 4.0f);
        }
    }
}
