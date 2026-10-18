using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Blocks.Character;
using Blocks.Audio;

namespace TimeTravel
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BuildingBlocksCharacter))]
    public class EvilWizardBoss : MonoBehaviour
    {
        [Header("Boss Identity")]
        [SerializeField] private string bossName = "Ignis, the Corrupted Archmage";
        [SerializeField] private float bossMaxHealth = 220f;

        [Header("Arena Bounds & Sealing")]
        [SerializeField] private float arenaTriggerX = 82f;
        [SerializeField] private GameObject arenaBarrier;
        [SerializeField] private Transform[] teleportNodes;

        [Header("Combat Spells")]
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private Transform staffFirePoint;
        [SerializeField] private float fireballSpeed = 9.5f;
        [SerializeField] private float fireballDamage = 16f;
        [SerializeField] private float spellCooldown = 2.4f;
        [SerializeField] private float meleeRange = 2.4f;

        [Header("Teleport & Evasion")]
        [SerializeField] private GameObject smokeVfxPrefab;
        [SerializeField] private float teleportCooldown = 5.0f;

        [Header("Audio")]
        [SerializeField] private AudioClip bossMusic;
        [SerializeField] private AudioClip castSound;
        [SerializeField] private AudioClip teleportSound;
        [SerializeField] private AudioClip victorySound;
        [SerializeField] private AudioSource audioSource;

        [Header("Rewards & Gate")]
        [SerializeField] private GameObject exitGateway;

        private BuildingBlocksCharacter character;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Transform playerTransform;
        private AudioSource musicSource;

        private bool isEngaged = false;
        private bool isDefeated = false;
        private float spellTimer = 1.0f;
        private float teleportTimer = 3.0f;
        private int currentTeleportIndex = 1;
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HurtHash = Animator.StringToHash("Hurt");
        private static readonly int DeathHash = Animator.StringToHash("Death");

        private void Awake()
        {
            character = GetComponent<BuildingBlocksCharacter>();
            animator = GetComponentInChildren<Animator>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();

            // Setup music audio source
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = 0.75f;
            if (bossMusic != null) musicSource.clip = bossMusic;
        }

        private void Start()
        {
            // Find player
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo == null) playerGo = GameObject.Find("Player");
            if (playerGo != null) playerTransform = playerGo.transform;

            // Make sure arena barrier starts inactive until player steps into the arena
            if (arenaBarrier != null) arenaBarrier.SetActive(false);

            // Configure boss health
            if (character != null)
            {
                if (character.MaxHealth > 0f) bossMaxHealth = character.MaxHealth;
                character.OnDamaged += HandleBossDamaged;
                character.OnEliminated += HandleBossEliminated;
            }
        }

        private void OnDestroy()
        {
            if (character != null)
            {
                character.OnDamaged -= HandleBossDamaged;
                character.OnEliminated -= HandleBossEliminated;
            }
        }

        private void Update()
        {
            if (isDefeated) return;

            // 1. Detect player entering the Boss Arena
            if (!isEngaged)
            {
                if (playerTransform == null)
                {
                    var p = GameObject.FindWithTag("Player");
                    if (p != null) playerTransform = p.transform;
                }

                if (playerTransform != null && playerTransform.position.x >= arenaTriggerX)
                {
                    EngageBoss();
                }
                return;
            }

            if (playerTransform == null) return;

            // 2. Combat facing direction
            float dir = playerTransform.position.x - transform.position.x;
            if (character != null && Mathf.Abs(dir) > 0.1f)
            {
                character.FacePosition(playerTransform.position);
            }

            // 3. Spell and ability timers
            float dt = Time.deltaTime;
            spellTimer -= dt;
            teleportTimer -= dt;

            float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // 4. Emergency Teleportation if player gets too close and hits the boss
            if (distToPlayer < meleeRange && teleportTimer <= 0f)
            {
                TeleportToNewNode();
                return;
            }

            // 5. Cast spells when cooldown is ready
            float effectiveCooldown = (character != null && character.CurrentHealth < bossMaxHealth * 0.5f)
                ? spellCooldown * 0.65f // Enraged phase casts faster!
                : spellCooldown;

            if (spellTimer <= 0f)
            {
                spellTimer = effectiveCooldown;
                StartCoroutine(CastFireballRoutine());
            }
        }

        public void EngageBoss()
        {
            if (isEngaged) return;
            isEngaged = true;

            // Seal the arena barrier
            if (arenaBarrier != null) arenaBarrier.SetActive(true);

            // Start boss music
            if (musicSource != null && musicSource.clip != null)
            {
                musicSource.Play();
            }

            // Flash aura and announce
            TemporalHUD.Instance?.ShowNotification($"<color=#FF3311><b>WARNING:</b></color> {bossName.ToUpper()} AWAKENS!", 4.0f);
        }

        private IEnumerator CastFireballRoutine()
        {
            if (isDefeated || playerTransform == null) yield break;

            // Trigger cast animation
            if (animator != null) animator.SetTrigger(AttackHash);

            if (castSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(castSound, 1.2f);
            }

            // Windup telegraph
            yield return new WaitForSeconds(0.4f);

            if (isDefeated || playerTransform == null) yield break;

            // Determine fire point
            Vector3 firePos = staffFirePoint != null 
                ? staffFirePoint.position 
                : transform.position + new Vector3(character != null ? character.FacingDirection * 0.8f : 0.8f, 1.0f, 0);

            // Launch fireball towards player chest
            Vector3 targetPos = playerTransform.position + Vector3.up * 0.6f;
            Vector2 direction = (targetPos - firePos).normalized;

            SpawnFireball(firePos, direction);

            // In enraged phase, occasionally fire a spread pair!
            if (character != null && character.CurrentHealth < bossMaxHealth * 0.5f)
            {
                yield return new WaitForSeconds(0.2f);
                if (!isDefeated && playerTransform != null)
                {
                    Vector2 angledDir = Quaternion.Euler(0, 0, 15f) * direction;
                    SpawnFireball(firePos, angledDir);
                }
            }
        }

        private void SpawnFireball(Vector3 pos, Vector2 dir)
        {
            GameObject fb = null;
            if (fireballPrefab != null)
            {
                fb = Instantiate(fireballPrefab, pos, Quaternion.identity);
            }
            else
            {
                // Fallback procedural fireball projectile
                fb = new GameObject("EvilWizard_Fireball");
                fb.transform.position = pos;
                var sr = fb.AddComponent<SpriteRenderer>();
                sr.color = new Color(1f, 0.45f, 0.1f, 1f);
                sr.sortingLayerName = "Main";
                sr.sortingOrder = 5;
                fb.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
            }

            // Configure projectile component
            var proj = fb.GetComponent<BossFireballProjectile>();
            if (proj == null) proj = fb.AddComponent<BossFireballProjectile>();
            proj.Initialize(dir * fireballSpeed, fireballDamage, character);
        }

        public void TeleportToNewNode()
        {
            if (teleportNodes == null || teleportNodes.Length == 0) return;

            teleportTimer = (character != null && character.CurrentHealth < bossMaxHealth * 0.5f) ? 3.5f : 5.0f;

            // Pick a different node from current
            int nextIndex = (currentTeleportIndex + 1) % teleportNodes.Length;
            currentTeleportIndex = nextIndex;
            Vector3 destination = teleportNodes[nextIndex].position;

            StartCoroutine(TeleportRoutine(destination));
        }

        private IEnumerator TeleportRoutine(Vector3 dest)
        {
            // Spawn smoke at departure
            if (smokeVfxPrefab != null)
            {
                Instantiate(smokeVfxPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            }

            if (teleportSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(teleportSound, 1.2f);
            }

            // Briefly vanish
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            if (character != null) character.enabled = false;

            yield return new WaitForSeconds(0.25f);

            // Reappear at new location
            transform.position = dest;
            if (smokeVfxPrefab != null)
            {
                Instantiate(smokeVfxPrefab, dest + Vector3.up * 0.5f, Quaternion.identity);
            }

            if (spriteRenderer != null) spriteRenderer.enabled = true;
            if (character != null) character.enabled = true;

            // Immediately ready to cast!
            spellTimer = 0.6f;
        }

        private void HandleBossDamaged(DamageInfo info)
        {
            if (isDefeated) return;

            // Chance to auto-evade when taking damage
            if (Random.value < 0.35f && teleportTimer <= 0.5f)
            {
                TeleportToNewNode();
            }
        }

        private void HandleBossEliminated()
        {
            if (isDefeated) return;
            isDefeated = true;

            // Stop boss music & play victory fanfare
            if (musicSource != null) musicSource.Stop();
            if (victorySound != null && audioSource != null)
            {
                audioSource.PlayOneShot(victorySound, 1.5f);
            }

            // Drop arena barrier
            if (arenaBarrier != null) arenaBarrier.SetActive(false);

            // Spawn Past Chronos Core Orb reward at altar center
            SpawnBossDefeatOrb();

            // Unlock exit gateway
            if (exitGateway != null)
            {
                exitGateway.SetActive(true);
                var l2d = exitGateway.GetComponentInChildren<Light2D>();
                if (l2d != null) l2d.intensity = 4.0f;
            }

            TemporalHUD.Instance?.ShowNotification(
                $"<color=#FFD700><b>BOSS VANQUISHED!</b></color> {bossName} has fallen! The Sacred Past Chronos Core is yours!", 6.0f);
        }

        private void SpawnBossDefeatOrb()
        {
            var drop = GetComponent<EnemyOrbDrop>();
            if (drop != null)
            {
                drop.SpawnPhysicalOrb();
            }
        }

        // --- Custom Grand Boss Health Bar UI ---
        private void OnGUI()
        {
            if (!isEngaged || isDefeated || character == null) return;

            float curHp = Mathf.Max(0, character.CurrentHealth);
            float maxHp = Mathf.Max(1, character.MaxHealth);
            float pct = Mathf.Clamp01(curHp / maxHp);

            // Frame dimensions centered at the top of the screen
            float barWidth = Mathf.Min(Screen.width * 0.55f, 520f);
            float barHeight = 24f;
            float startX = (Screen.width - barWidth) * 0.5f;
            float startY = 32f;

            // Background frame
            GUI.color = new Color(0.08f, 0.08f, 0.1f, 0.88f);
            GUI.DrawTexture(new Rect(startX - 4, startY - 24, barWidth + 8, barHeight + 32), Texture2D.whiteTexture);

            // Boss Title
            GUI.color = new Color(1f, 0.85f, 0.35f, 1f);
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 14
            };
            GUI.Label(new Rect(startX, startY - 22, barWidth, 18), $"◆  {bossName.ToUpper()}  ◆", titleStyle);

            // Bar background slot
            GUI.color = new Color(0.2f, 0.08f, 0.08f, 0.95f);
            GUI.DrawTexture(new Rect(startX, startY, barWidth, barHeight), Texture2D.whiteTexture);

            // Health bar fill (Fiery gradient effect)
            Color fillColor = (pct > 0.5f) 
                ? Color.Lerp(new Color(1f, 0.45f, 0.1f), new Color(0.9f, 0.15f, 0.15f), (1f - pct) * 2f)
                : Color.Lerp(new Color(0.9f, 0.15f, 0.15f), new Color(0.55f, 0.05f, 0.05f), (0.5f - pct) * 2f);
            GUI.color = fillColor;
            GUI.DrawTexture(new Rect(startX, startY, barWidth * pct, barHeight), Texture2D.whiteTexture);

            // Text overlay inside bar
            GUI.color = Color.white;
            var textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 11
            };
            GUI.Label(new Rect(startX, startY, barWidth, barHeight), $"{curHp:F0} / {maxHp:F0} HP", textStyle);
            GUI.color = Color.white;
        }
    }

    /// <summary>
    /// Fireball spell projectile launched by Evil Wizard Boss.
    /// </summary>
    public class BossFireballProjectile : MonoBehaviour
    {
        private Vector2 velocity;
        private float damage;
        private BuildingBlocksCharacter owner;
        private float lifetime = 4.0f;
        private bool hasHit = false;

        public void Initialize(Vector2 vel, float dmg, BuildingBlocksCharacter caster)
        {
            velocity = vel;
            damage = dmg;
            owner = caster;

            var rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = velocity;

            var col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;
            col.isTrigger = true;

            // Add warm point light
            var l = GetComponentInChildren<Light2D>();
            if (l == null)
            {
                var lGo = new GameObject("Fireball_Light");
                lGo.transform.SetParent(transform);
                lGo.transform.localPosition = Vector3.zero;
                l = lGo.AddComponent<Light2D>();
                l.lightType = Light2D.LightType.Point;
                l.color = new Color(1f, 0.45f, 0.1f, 1f);
                l.pointLightOuterRadius = 3.5f;
                l.intensity = 2.5f;
            }
        }

        private void Update()
        {
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasHit) return;

            // Don't hit owner or triggers
            if (owner != null && (other.gameObject == owner.gameObject || other.transform.IsChildOf(owner.transform))) return;
            if (other.isTrigger) return;

            // Check if player or damageable hit
            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.IsDamageable)
            {
                hasHit = true;
                Vector2 hitPoint = other.ClosestPoint(transform.position);
                Vector2 pushDir = velocity.normalized;
                DamageInfo dmg = new DamageInfo(damage, owner, hitPoint, pushDir, HitReaction.PushBack(14f, 0.3f));
                damageable.TakeDamage(dmg);
                Destroy(gameObject);
                return;
            }

            // Hit environment wall / floor
            if (other.GetComponentInParent<IDamageable>() == null)
            {
                hasHit = true;
                Destroy(gameObject);
            }
        }
    }
}
