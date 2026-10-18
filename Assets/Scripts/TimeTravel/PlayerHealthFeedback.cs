using System.Collections;
using UnityEngine;
using Blocks.Character;

namespace TimeTravel
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BuildingBlocksCharacter))]
    public class PlayerHealthFeedback : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioClip hurtSound;
        [SerializeField] private AudioSource audioSource;

        [Header("Visual Flash")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color hurtFlashColor = new Color(1f, 0.25f, 0.25f, 1f);

        private BuildingBlocksCharacter character;
        private Color originalColor = Color.white;
        private Coroutine flashRoutine;

        private void Awake()
        {
            character = GetComponent<BuildingBlocksCharacter>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

            if (spriteRenderer != null) originalColor = spriteRenderer.color;
        }

        private void OnEnable()
        {
            if (character == null) return;
            character.OnDamaged += HandleDamaged;
            character.OnEliminated += HandleEliminated;
        }

        private void OnDisable()
        {
            if (character == null) return;
            character.OnDamaged -= HandleDamaged;
            character.OnEliminated -= HandleEliminated;
        }

        private void HandleDamaged(DamageInfo info)
        {
            // 1. Play hurt audio
            if (hurtSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hurtSound, 1.2f);
            }

            // 2. Visual sprite flash
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(HurtFlashRoutine());

            // 3. HUD Feedback
            float cur = character.CurrentHealth;
            float max = character.MaxHealth;
            if (cur > 0f)
            {
                if (cur <= max * 0.3f)
                {
                    TemporalHUD.Instance?.ShowNotification($"<color=#FF0000><b>CRITICAL HEALTH! ({cur:F0}/{max:F0} HP)</b></color>", 2.0f);
                }
                else
                {
                    TemporalHUD.Instance?.ShowNotification($"<color=#FF5544><b>DAMAGE TAKEN (-{info.Amount:F0} HP)! Health: {cur:F0}/{max:F0}</b></color>", 1.5f);
                }
            }
        }

        private void HandleEliminated()
        {
            TemporalHUD.Instance?.ShowNotification("<color=#FF0000><b>DEFEATED! Respawning at checkpoint...</b></color>", 3.0f);
        }

        private IEnumerator HurtFlashRoutine()
        {
            if (spriteRenderer != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    spriteRenderer.color = hurtFlashColor;
                    yield return new WaitForSeconds(0.08f);
                    spriteRenderer.color = originalColor;
                    yield return new WaitForSeconds(0.08f);
                }
            }
        }
    }
}
