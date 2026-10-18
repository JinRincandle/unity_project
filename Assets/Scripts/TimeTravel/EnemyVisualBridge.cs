using System.Collections;
using UnityEngine;
using Blocks.Character;

namespace TimeTravel
{
    [DisallowMultipleComponent]
    public class EnemyVisualBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BuildingBlocksCharacter character;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        [Header("Orientation")]
        [Tooltip("True if the sprite sheet natively faces Left (e.g. Bandits). False if it natively faces Right (e.g. Evil Wizard).")]
        [SerializeField] private bool nativeFacesLeft = true;

        [Header("Death Behavior")]
        [SerializeField] private float deathDestroyDelay = 1.6f;

        private static readonly int AnimStateHash = Animator.StringToHash("AnimState");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HurtHash = Animator.StringToHash("Hurt");
        private static readonly int DeathHash = Animator.StringToHash("Death");

        private bool isDead = false;
        private Color originalColor = Color.white;
        private Coroutine flashRoutine;

        private void Awake()
        {
            if (character == null) character = GetComponentInParent<BuildingBlocksCharacter>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

            if (spriteRenderer != null) originalColor = spriteRenderer.color;
        }

        private void OnEnable()
        {
            if (character == null) return;
            character.OnAttackPerformed += HandleAttack;
            character.OnDamaged += HandleDamaged;
            character.OnEliminated += HandleEliminated;
        }

        private void OnDisable()
        {
            if (character == null) return;
            character.OnAttackPerformed -= HandleAttack;
            character.OnDamaged -= HandleDamaged;
            character.OnEliminated -= HandleEliminated;
        }

        private void Update()
        {
            if (isDead || character == null || animator == null) return;

            // Flip sprite according to facing direction
            float dir = character.FacingDirection;
            if (spriteRenderer != null && Mathf.Abs(dir) > 0.05f)
            {
                spriteRenderer.flipX = nativeFacesLeft ? (dir > 0f) : (dir < 0f);
            }

            // Movement state: 0 = Idle, 1 = Combat Idle, 2 = Run
            float speed = character.CurrentSpeed;
            if (speed > 0.2f)
            {
                animator.SetInteger(AnimStateHash, 2);
            }
            else if (character.HasTarget)
            {
                animator.SetInteger(AnimStateHash, 1);
            }
            else
            {
                animator.SetInteger(AnimStateHash, 0);
            }

            animator.SetBool(GroundedHash, character.IsGrounded);
        }

        private void HandleAttack()
        {
            if (isDead || animator == null) return;
            animator.SetTrigger(AttackHash);
        }

        private void HandleDamaged(DamageInfo info)
        {
            if (isDead) return;

            if (animator != null)
            {
                animator.SetTrigger(HurtHash);
            }

            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(HurtFlashRoutine());
        }

        private IEnumerator HurtFlashRoutine()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(1.0f, 0.3f, 0.3f, 1.0f);
                yield return new WaitForSeconds(0.18f);
                if (!isDead && spriteRenderer != null)
                {
                    spriteRenderer.color = originalColor;
                }
            }
        }

        private void HandleEliminated()
        {
            if (isDead) return;
            isDead = true;

            if (animator != null)
            {
                animator.SetTrigger(DeathHash);
            }

            // Disable physics colliders and rigidbody so defeated enemy cannot push or damage player
            var cols = character.GetComponentsInChildren<Collider2D>();
            foreach (var col in cols)
            {
                if (col != null) col.enabled = false;
            }

            var rb = character.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }

            StartCoroutine(DeathFadeAndDestroyRoutine());
        }

        private IEnumerator DeathFadeAndDestroyRoutine()
        {
            yield return new WaitForSeconds(deathDestroyDelay);

            if (spriteRenderer != null)
            {
                float fadeTime = 0.6f;
                float elapsed = 0f;
                Color startCol = spriteRenderer.color;

                while (elapsed < fadeTime)
                {
                    elapsed += Time.deltaTime;
                    float alpha = Mathf.Lerp(startCol.a, 0f, elapsed / fadeTime);
                    spriteRenderer.color = new Color(startCol.r, startCol.g, startCol.b, alpha);
                    yield return null;
                }
            }

            if (character != null)
            {
                Destroy(character.gameObject);
            }
        }
    }
}
