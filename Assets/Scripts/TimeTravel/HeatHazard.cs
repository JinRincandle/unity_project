using UnityEngine;
using Blocks.Character;

namespace TimeTravel
{
    public class HeatHazard : MonoBehaviour
    {
        [Header("Heat Settings")]
        [SerializeField] private float heatRate = 5.0f; // Percent heat increase per second
        [SerializeField] private float damageInterval = 2.0f;
        [SerializeField] private float heatDamage = 8.0f;

        private float currentHeat = 0f;
        private float nextDamageTime = 0f;
        private bool isPlayerInSafeZone = false;

        public float CurrentHeatPercent => currentHeat;

        private void Update()
        {
            if (isPlayerInSafeZone)
            {
                currentHeat = Mathf.MoveTowards(currentHeat, 0f, heatRate * 2.0f * Time.deltaTime);
                return;
            }

            currentHeat += heatRate * Time.deltaTime;
            currentHeat = Mathf.Clamp(currentHeat, 0f, 100f);

            if (currentHeat >= 80f)
            {
                if (Time.time >= nextDamageTime)
                {
                    nextDamageTime = Time.time + damageInterval;
                    var player = GameObject.FindWithTag("Player");
                    if (player != null)
                    {
                        var charComp = player.GetComponent<BuildingBlocksCharacter>();
                        if (charComp != null)
                        {
                            charComp.TakeDamage(new DamageInfo(heatDamage, null, charComp.transform.position, Vector2.zero, default));
                        }
                    }
                    TemporalHUD.Instance?.ShowNotification("<color=#FF3300><b>HEAT OVERLOAD (90°C)!</b> High ambient temperature is burning you! Return to Present to cool down!</color>", 2.0f);
                }
            }
        }

        public void SetSafeZone(bool safe)
        {
            isPlayerInSafeZone = safe;
        }
    }
}
