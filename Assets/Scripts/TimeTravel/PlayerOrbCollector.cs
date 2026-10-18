using UnityEngine;
using Blocks.Character;

namespace TimeTravel
{
    /// <summary>
    /// Dedicated Player component that detects and collects Temporal Orbs.
    /// Uses dual-redundant trigger/collision callbacks, proactive overlap sweeping,
    /// and gentle magnetic attraction so orbs never get missed or stuck out of reach.
    /// </summary>
    [RequireComponent(typeof(BuildingBlocksCharacter))]
    [DisallowMultipleComponent]
    public class PlayerOrbCollector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [Tooltip("Direct pickup trigger radius for overlap sweep.")]
        [SerializeField] private float pickupRadius = 1.2f;

        [Header("Magnetic Attraction")]
        [Tooltip("If enabled, dropped orbs glide smoothly towards the player when in range.")]
        [SerializeField] private bool enableMagnet = true;
        [SerializeField] private float magnetRadius = 3.5f;
        [SerializeField] private float magnetSpeed = 8.0f;

        private BuildingBlocksCharacter character;

        private void Awake()
        {
            character = GetComponent<BuildingBlocksCharacter>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryCollectOrbFromCollider(other);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            TryCollectOrbFromCollider(collision.collider);
        }

        private ContactFilter2D contactFilter = new ContactFilter2D();
        private System.Collections.Generic.List<Collider2D> overlapList = new System.Collections.Generic.List<Collider2D>();

        private void Update()
        {
            // 1. Proactive Sweep for Instant Pickup
            overlapList.Clear();
            int count = Physics2D.OverlapCircle(transform.position, pickupRadius, contactFilter.NoFilter(), overlapList);
            for (int i = 0; i < count; i++)
            {
                var col = overlapList[i];
                if (col != null && col.gameObject != gameObject)
                {
                    TryCollectOrbFromCollider(col);
                }
            }

            // 2. Magnetic Attraction for Dropped Orbs
            if (enableMagnet)
            {
                ApplyOrbMagnetism();
            }
        }

        private void TryCollectOrbFromCollider(Collider2D col)
        {
            if (col == null) return;

            // Check directly on object or in parent hierarchy
            var orb = col.GetComponent<TemporalOrb>();
            if (orb == null)
            {
                orb = col.GetComponentInParent<TemporalOrb>();
            }

            if (orb != null && !orb.IsCollected)
            {
                orb.Collect(character);
            }
        }

        private void ApplyOrbMagnetism()
        {
            overlapList.Clear();
            int magnetHits = Physics2D.OverlapCircle(transform.position, magnetRadius, contactFilter.NoFilter(), overlapList);
            for (int i = 0; i < magnetHits; i++)
            {
                var col = overlapList[i];
                if (col == null || col.gameObject == gameObject) continue;

                var orb = col.GetComponent<TemporalOrb>();
                if (orb == null) orb = col.GetComponentInParent<TemporalOrb>();

                if (orb != null && !orb.IsCollected)
                {
                    // Only attract dynamic dropped orbs that have physics or marked for attraction
                    var rb = orb.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 targetPos = (Vector2)transform.position + Vector2.up * 0.5f;
                        Vector2 toPlayer = targetPos - (Vector2)orb.transform.position;
                        float dist = toPlayer.magnitude;

                        if (dist > 0.05f)
                        {
                            // If very close, instant collect
                            if (dist <= pickupRadius * 0.9f)
                            {
                                orb.Collect(character);
                            }
                            else
                            {
                                // Smooth pull towards player
                                Vector2 pullDir = toPlayer.normalized;
                                float pullSpeed = Mathf.Lerp(magnetSpeed * 1.5f, magnetSpeed * 0.5f, dist / magnetRadius);
                                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, pullDir * pullSpeed, Time.deltaTime * 10f);
                            }
                        }
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, pickupRadius);

            if (enableMagnet)
            {
                Gizmos.color = new Color(0.8f, 0.4f, 1f, 0.2f);
                Gizmos.DrawWireSphere(transform.position, magnetRadius);
            }
        }
    }
}
