using UnityEngine;
using UnityEngine.Rendering.Universal;
using Blocks.Character;

namespace TimeTravel
{
    [RequireComponent(typeof(Collider2D))]
    public class PastSafePlatform : MonoBehaviour
    {
        [Header("Cooling Sanctuary")]
        [SerializeField] private Light2D safeLight;
        [SerializeField] private Color safeCoolColor = new Color(0.2f, 0.85f, 1.0f, 0.8f);

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            if (safeLight == null) safeLight = GetComponentInChildren<Light2D>();
            if (safeLight != null)
            {
                safeLight.color = safeCoolColor;
                safeLight.intensity = 1.6f;
                safeLight.pointLightOuterRadius = 4.5f;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var character = collision.GetComponentInParent<BuildingBlocksCharacter>();
            if (character != null)
            {
                PastHeatZone.SetSafePlatformStatus(true);
                PastHeatZone.CoolDown(100f); // Rapid cooldown on touchdown
                TemporalHUD.Instance?.ShowNotification("<color=#00FFAA><b>SAFE PLATFORM:</b> Ambient heat dissipated.</color>", 2.0f);
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            var character = collision.GetComponentInParent<BuildingBlocksCharacter>();
            if (character != null)
            {
                PastHeatZone.SetSafePlatformStatus(true);
                PastHeatZone.CoolDown(50f * Time.deltaTime);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            var character = collision.GetComponentInParent<BuildingBlocksCharacter>();
            if (character != null)
            {
                PastHeatZone.SetSafePlatformStatus(false);
            }
        }
    }
}
