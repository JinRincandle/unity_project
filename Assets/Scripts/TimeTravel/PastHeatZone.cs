using UnityEngine;
using UnityEngine.Rendering.Universal;
using Blocks.Character;

namespace TimeTravel
{
    [RequireComponent(typeof(Collider2D))]
    public class PastHeatZone : MonoBehaviour
    {
        public static float GlobalPlayerHeat { get; private set; } = 0f;
        public static bool IsPlayerInHeatZone { get; private set; } = false;
        public static bool IsPlayerOnSafePlatform { get; private set; } = false;

        [Header("Heat Mechanics")]
        [Tooltip("Heat builds up per second while standing in this hazard zone.")]
        [SerializeField] private float heatRate = 10f; // Takes 10s of continuous standing to max out
        [Tooltip("Damage dealt per tick once heat hits 100.")]
        [SerializeField] private float heatDamagePerTick = 6f;
        [Tooltip("Seconds between damage ticks at max heat.")]
        [SerializeField] private float damageInterval = 1.25f;

        [Header("Visual Ambience")]
        [SerializeField] private Light2D heatZoneLight;
        [SerializeField] private Color warningHeatColor = new Color(1.0f, 0.45f, 0.1f, 1f);

        private BuildingBlocksCharacter playerChar;
        private float nextDamageTime = 0f;
        private bool warningShown50 = false;
        private bool warningShown80 = false;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            if (heatZoneLight == null) heatZoneLight = GetComponentInChildren<Light2D>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var character = collision.GetComponentInParent<BuildingBlocksCharacter>();
            if (character != null)
            {
                playerChar = character;
                IsPlayerInHeatZone = true;
            }
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            if (playerChar == null)
            {
                playerChar = collision.GetComponentInParent<BuildingBlocksCharacter>();
                if (playerChar == null) return;
            }

            IsPlayerInHeatZone = true;

            // If player is on a safe platform, safe zone shields them
            if (IsPlayerOnSafePlatform)
            {
                CoolDown(30f * Time.deltaTime);
                return;
            }

            // Accumulate heat
            GlobalPlayerHeat = Mathf.Clamp(GlobalPlayerHeat + heatRate * Time.deltaTime, 0f, 100f);

            // Shimmer heat light
            if (heatZoneLight != null)
            {
                heatZoneLight.intensity = 1.5f + Mathf.Sin(Time.time * 5f) * 0.4f;
            }

            // Warnings at thresholds
            if (GlobalPlayerHeat >= 50f && !warningShown50)
            {
                warningShown50 = true;
                TemporalHUD.Instance?.ShowNotification("<color=#FFA500><b>HEAT RISING (50%)!</b> Cross swiftly or find a Safe Platform!</color>", 2.5f);
            }

            if (GlobalPlayerHeat >= 80f && !warningShown80)
            {
                warningShown80 = true;
                TemporalHUD.Instance?.ShowNotification("<color=#FF3300><b>CRITICAL HEAT (80%)!</b> Magma radiation burning!</color>", 2.5f);
            }

            // Gradual damage ticks at max heat
            if (GlobalPlayerHeat >= 100f)
            {
                if (Time.time >= nextDamageTime)
                {
                    nextDamageTime = Time.time + damageInterval;
                    playerChar.TakeDamage(new DamageInfo(heatDamagePerTick, null, playerChar.transform.position, Vector2.zero, default));
                    TemporalHUD.Instance?.ShowNotification("<color=#FF0000><b>HEAT OVERLOAD!</b> Taking environmental damage!</color>", 1.5f);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (playerChar != null && (collision.transform == playerChar.transform || collision.transform.IsChildOf(playerChar.transform)))
            {
                IsPlayerInHeatZone = false;
                playerChar = null;
            }
        }

        private void Update()
        {
            // Naturally cool down when not in any heat zone
            if (!IsPlayerInHeatZone)
            {
                CoolDown(25f * Time.deltaTime);
            }
        }

        public static void CoolDown(float amount)
        {
            GlobalPlayerHeat = Mathf.Max(0f, GlobalPlayerHeat - amount);
        }

        public static void SetSafePlatformStatus(bool onSafePlatform)
        {
            IsPlayerOnSafePlatform = onSafePlatform;
        }
    }
}
