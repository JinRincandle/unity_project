using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace TimeTravel
{
    public class ConduitLever : MonoBehaviour
    {
        [SerializeField] private int conduitIndex = 0;
        [SerializeField] private string leverLabel = "Conduit Lever 1";
        [SerializeField] private Light2D leverLight;
        [SerializeField] private SpriteRenderer leverRenderer;
        [SerializeField] private Color normalColor = new Color(1f, 0.6f, 0.2f, 1f);
        [SerializeField] private Color interactColor = new Color(0.2f, 1f, 0.8f, 1f);

        private bool playerInRange = false;

        private void Awake()
        {
            if (leverLight == null) leverLight = GetComponentInChildren<Light2D>();
            if (leverRenderer == null) leverRenderer = GetComponent<SpriteRenderer>();

            if (leverLight != null) leverLight.color = normalColor;
            if (leverRenderer != null) leverRenderer.color = normalColor;
        }

        private void Update()
        {
            if (playerInRange)
            {
                var kb = Keyboard.current;
                if (kb != null && (kb.fKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame))
                {
                    PullLever();
                }
            }
        }

        public void PullLever()
        {
            if (leverLight != null)
            {
                leverLight.color = interactColor;
                Invoke(nameof(ResetLight), 0.35f);
            }

            if (ChronoConduitGrid.Instance != null)
            {
                ChronoConduitGrid.Instance.RotateConduit(conduitIndex);
            }
        }

        private void ResetLight()
        {
            if (leverLight != null) leverLight.color = normalColor;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || 
                collision.GetComponentInParent<Blocks.Character.BuildingBlocksCharacter>() != null)
            {
                playerInRange = true;
                TemporalHUD.Instance?.ShowPrompt($"[F] Pull {leverLabel} (Rotate 90°)");
            }
            else if (collision.GetComponent<Blocks.Attack.Projectile>() != null)
            {
                PullLever();
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
