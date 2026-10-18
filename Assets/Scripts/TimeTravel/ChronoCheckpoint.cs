using UnityEngine;
using UnityEngine.Rendering.Universal;
using Blocks.Character;

namespace TimeTravel
{
    [RequireComponent(typeof(Collider2D))]
    public class ChronoCheckpoint : MonoBehaviour
    {
        [Header("Checkpoint Settings")]
        [SerializeField] private Transform respawnPoint;
        [SerializeField] private Light2D checkpointLight;
        [SerializeField] private SpriteRenderer runeRenderer;
        [SerializeField] private Color inactiveColor = new Color(0.9f, 0.5f, 0.15f, 0.8f);
        [SerializeField] private Color activeColor = new Color(0.15f, 0.9f, 1.0f, 1.0f);

        [Header("Audio & Feedback")]
        [SerializeField] private AudioClip activateSound;

        private bool isActivated = false;

        public void SetRespawnPoint(Transform t)
        {
            respawnPoint = t;
        }

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            if (checkpointLight == null) checkpointLight = GetComponentInChildren<Light2D>();
            if (runeRenderer == null) runeRenderer = GetComponent<SpriteRenderer>();

            UpdateVisuals();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isActivated) return;

            var character = collision.GetComponentInParent<BuildingBlocksCharacter>();
            if (character != null)
            {
                ActivateCheckpoint(character);
            }
        }

        public void ActivateCheckpoint(BuildingBlocksCharacter character)
        {
            if (isActivated) return;
            isActivated = true;

            Vector3 spawnPos = respawnPoint != null ? respawnPoint.position : transform.position;
            character.SetRespawnPosition(spawnPos);

            UpdateVisuals();

            // Audio feedback
            if (activateSound != null)
            {
                AudioSource.PlayClipAtPoint(activateSound, transform.position, 1.0f);
            }

            TemporalHUD.Instance?.ShowNotification("<color=#00FFFF><b>CHRONO CHECKPOINT ANCHORED!</b> Timeline resonance stored.</color>", 3.5f);
            Debug.Log($"<color=#00FFFF><b>[Checkpoint]</b> Activated at {spawnPos}! Player will respawn here if eliminated.</color>");
        }

        private void UpdateVisuals()
        {
            Color targetCol = isActivated ? activeColor : inactiveColor;

            if (runeRenderer != null)
            {
                runeRenderer.color = targetCol;
            }

            if (checkpointLight != null)
            {
                checkpointLight.color = targetCol;
                checkpointLight.intensity = isActivated ? 3.0f : 1.5f;
                checkpointLight.pointLightOuterRadius = isActivated ? 8.0f : 5.0f;
            }
        }
    }
}
