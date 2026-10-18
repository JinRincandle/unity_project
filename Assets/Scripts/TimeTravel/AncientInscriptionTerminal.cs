using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace TimeTravel
{
    public class AncientInscriptionTerminal : MonoBehaviour
    {
        [Header("Terminal Settings")]
        [SerializeField] private string eraName = "Past";
        [SerializeField] private string cipherMessage = "ANCIENT CONDUIT INSCRIPTION DECIPHERED:\n<color=#00FFFF>Sequence: ABYSS (3) -> CHRONOS (1) -> SOL (2)</color>\n<i>Use this sequence on the Present Chrono Vault rune pillars!</i>";

        [Header("Visual Feedback")]
        [SerializeField] private Light2D terminalLight;
        [SerializeField] private Color unreadColor = new Color(1.0f, 0.4f, 0.1f, 1f);
        [SerializeField] private Color readColor = new Color(0.2f, 1.0f, 0.5f, 1f);

        private bool playerInRange = false;
        private bool isRead = false;

        private void Start()
        {
            if (terminalLight == null) terminalLight = GetComponentInChildren<Light2D>();
            UpdateLight();
        }

        private void Update()
        {
            if (playerInRange)
            {
                var kb = Keyboard.current;
                if (kb != null && (kb.fKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
                {
                    ReadInscription();
                }
            }
        }

        private void ReadInscription()
        {
            isRead = true;
            TimeInventory.DiscoveredPastCode = true;
            TimeInventory.ConduitsAlignedInPast = true;
            UpdateLight();

            TemporalHUD.Instance?.ShowNotification($"<color=#FFAA00><b>[{eraName.ToUpper()} INSCRIPTION TABLET]</b></color>\n{cipherMessage}", 6.0f);
            TemporalHUD.Instance?.UpdateDisplay();
        }

        private void UpdateLight()
        {
            if (terminalLight != null)
            {
                terminalLight.color = isRead ? readColor : unreadColor;
                terminalLight.intensity = isRead ? 3.0f : 1.8f;
                terminalLight.pointLightOuterRadius = 6.0f;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.GetComponentInParent<Blocks.Character.BuildingBlocksCharacter>() != null)
            {
                playerInRange = true;
                string prompt = isRead 
                    ? $"[F] Inspect Deciphered {eraName} Tablet" 
                    : $"<color=#FFAA00>[F] Decipher Ancient {eraName} Inscription</color>";
                TemporalHUD.Instance?.ShowNotification(prompt, 2.5f);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.GetComponentInParent<Blocks.Character.BuildingBlocksCharacter>() != null)
            {
                playerInRange = false;
            }
        }
    }
}
