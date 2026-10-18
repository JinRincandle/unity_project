using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TimeTravel
{
    public class FutureCipherArchive : MonoBehaviour
    {
        [SerializeField] private Light2D terminalLight;
        [SerializeField] private string decodedCipherMessage = "<color=#00FFFF><b>[FUTURE ARCHIVE DECRYPTED]</b>\nHistorical Record: The Present Vault was unsealed with sequence:\n<b>3 -> 1 -> 2 (Abyss -> Chronos -> Sol)</b>\nPress <b>[2]</b> to return to the Present and enter the sequence!</color>";

        private bool hasTriggered = false;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.GetComponentInParent<Blocks.Character.BuildingBlocksCharacter>() != null)
            {
                TimeInventory.DiscoveredFutureCode = true;

                if (terminalLight != null)
                {
                    terminalLight.color = new Color(0.2f, 1f, 0.9f, 1f);
                    terminalLight.intensity = 3.0f;
                    terminalLight.pointLightOuterRadius = 10.0f;
                }

                TemporalHUD.Instance?.ShowNotification(decodedCipherMessage, 6.0f);
                TemporalHUD.Instance?.UpdateDisplay();
            }
        }
    }
}
