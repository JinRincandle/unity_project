using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace TimeTravel
{
    public class ChronoSwitch : MonoBehaviour
    {
        [SerializeField] private string switchName = "Chrono Switch";
        [SerializeField] private ChronoDoor targetDoor;
        [SerializeField] private Light2D switchLight;
        [SerializeField] private SpriteRenderer crystalRenderer;
        [SerializeField] private Color dormantColor = new Color(0.8f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color activeColor = new Color(0.2f, 1f, 0.6f, 1f);

        private bool isActivated = false;
        public bool IsActivated => isActivated;
        private bool playerInRange = false;

        private void Awake()
        {
            if (switchLight == null) switchLight = GetComponentInChildren<Light2D>();
            if (crystalRenderer == null) crystalRenderer = GetComponent<SpriteRenderer>();

            if (switchLight != null) switchLight.color = dormantColor;
            if (crystalRenderer != null) crystalRenderer.color = dormantColor;
        }

        private void Update()
        {
            if (!isActivated && playerInRange)
            {
                var kb = Keyboard.current;
                if (kb != null && (kb.fKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
                {
                    Activate();
                }
            }
        }

        public void Activate()
        {
            if (isActivated) return;
            isActivated = true;

            if (switchLight != null)
            {
                switchLight.color = activeColor;
                switchLight.pointLightOuterRadius = 6.0f;
                switchLight.intensity = 2.5f;
            }

            if (crystalRenderer != null)
            {
                crystalRenderer.color = activeColor;
            }

            if (targetDoor != null)
            {
                targetDoor.RegisterSwitchActivated();
            }

            TemporalHUD.Instance?.ShowPrompt(switchName + " (ACTIVE)");
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.GetComponentInParent<Blocks.Character.BuildingBlocksCharacter>() != null)
            {
                playerInRange = true;
                if (!isActivated)
                {
                    TemporalHUD.Instance?.ShowPrompt("[F] Activate " + switchName);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.GetComponentInParent<Blocks.Character.BuildingBlocksCharacter>() != null)
            {
                playerInRange = false;
                TemporalHUD.Instance?.HidePrompt();
            }
        }
    }
}
