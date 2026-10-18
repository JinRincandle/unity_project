using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TimeTravel
{
    public class TemporalRunePillar : MonoBehaviour
    {
        [SerializeField] private int pillarId = 1;
        [SerializeField] private string pillarName = "Chronos (I)";
        [SerializeField] private Light2D runeLight;
        [SerializeField] private SpriteRenderer runeRenderer;
        [SerializeField] private Color dormantColor = new Color(0.3f, 0.3f, 0.4f, 0.7f);
        [SerializeField] private Color activeColor = new Color(0.2f, 0.9f, 1f, 1f);

        private bool isActivated = false;

        public int PillarId => pillarId;
        public string PillarName => pillarName;

        private void Awake()
        {
            if (runeLight == null) runeLight = GetComponentInChildren<Light2D>();
            if (runeRenderer == null) runeRenderer = GetComponent<SpriteRenderer>();

            ResetPillar();
        }

        public void Activate()
        {
            if (isActivated) return;
            isActivated = true;

            if (runeLight != null)
            {
                runeLight.color = activeColor;
                runeLight.intensity = 2.8f;
                runeLight.pointLightOuterRadius = 6.0f;
            }

            if (runeRenderer != null)
            {
                runeRenderer.color = activeColor;
            }

            TemporalVaultPuzzle.Instance?.RegisterPillarActivated(this);
        }

        public void ResetPillar()
        {
            isActivated = false;
            if (runeLight != null)
            {
                runeLight.color = dormantColor;
                runeLight.intensity = 1.0f;
                runeLight.pointLightOuterRadius = 3.5f;
            }

            if (runeRenderer != null)
            {
                runeRenderer.color = dormantColor;
            }
        }

        private bool playerInRange = false;

        private void Update()
        {
            if (!isActivated && playerInRange)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb != null && (kb.fKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
                {
                    Activate();
                }
            }
        }

        // Activates when player enters or interacts
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || 
                collision.GetComponentInParent<Blocks.Character.BuildingBlocksCharacter>() != null)
            {
                playerInRange = true;
                if (!isActivated)
                {
                    TemporalHUD.Instance?.ShowPrompt($"[F] Energize Socket: {pillarName}");
                }
            }
            else if (collision.GetComponent<Blocks.Attack.Projectile>() != null)
            {
                Activate();
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
