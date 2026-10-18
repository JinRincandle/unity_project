using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace TimeTravel
{
    public class ChronoControlPanel : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private Light2D panelLight;
        [SerializeField] private Color dormantColor = new Color(0.9f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color readyColor = new Color(0.2f, 0.9f, 1f, 1f);
        [SerializeField] private Color solvedColor = new Color(0.2f, 1f, 0.4f, 1f);

        private bool playerInRange = false;

        private void Awake()
        {
            if (panelLight == null) panelLight = GetComponentInChildren<Light2D>();
            UpdateVisuals();
        }

        private void Update()
        {
            if (playerInRange)
            {
                var kb = Keyboard.current;
                if (kb != null && (kb.fKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
                {
                    InspectPanel();
                }
            }
        }

        private void UpdateVisuals()
        {
            if (panelLight == null) return;

            if (TimeInventory.VaultDoorOpened)
            {
                panelLight.color = solvedColor;
                panelLight.intensity = 2.5f;
            }
            else if (TimeInventory.ConduitsAlignedInPast)
            {
                panelLight.color = readyColor;
                panelLight.intensity = 2.0f;
            }
            else
            {
                panelLight.color = dormantColor;
                panelLight.intensity = 1.0f;
            }
        }

        public void InspectPanel()
        {
            UpdateVisuals();

            if (TimeInventory.VaultDoorOpened)
            {
                TemporalHUD.Instance?.ShowNotification(
                    "<color=#00FF88><b>[CONTROL PANEL: HARMONIZED]</b>\nChrono Vault lock disengaged. Timeline core accessible.</color>", 
                    4.0f
                );
            }
            else if (!TimeInventory.ConduitsAlignedInPast)
            {
                TemporalHUD.Instance?.ShowNotification(
                    "<color=#FFA500><b>[DAMAGED CONTROL PANEL - INSCRIPTION]</b>\nFaded symbols visible: <b>[Abyss (III) - Chronos (I) - Sol (II)]</b>\n<color=#FF5555>STATUS: OFFLINE.</color> Vault power conduits were never connected during foundation construction in the <b>Past [1]</b>!</color>", 
                    6.0f
                );
            }
            else if (!TimeInventory.DiscoveredFutureCode)
            {
                TemporalHUD.Instance?.ShowNotification(
                    "<color=#00FFFF><b>[CONTROL PANEL: ENERGIZED]</b>\nFoundation conduits connected from the Past! The 3 sockets are charged, but historical activation order is missing.\n<i>Investigate the <b>Future [3]</b> memory archives to discover the sequence!</i></color>", 
                    6.0f
                );
            }
            else
            {
                TemporalHUD.Instance?.ShowNotification(
                    "<color=#00FF88><b>[CONTROL PANEL: READY FOR SEQUENCE]</b>\nConduits aligned & Future sequence known!\nEnergize sockets: <b>3 (Abyss) -> 1 (Chronos) -> 2 (Sol)</b></color>", 
                    5.0f
                );
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || 
                collision.GetComponentInParent<Blocks.Character.BuildingBlocksCharacter>() != null)
            {
                playerInRange = true;
                UpdateVisuals();
                TemporalHUD.Instance?.ShowPrompt("[F] Inspect Damaged Control Panel & Inscription");
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || 
                collision.GetComponentInParent<Blocks.Character.BuildingBlocksCharacter>() != null)
            {
                playerInRange = false;
                TemporalHUD.Instance?.HidePrompt();
            }
        }
    }
}
