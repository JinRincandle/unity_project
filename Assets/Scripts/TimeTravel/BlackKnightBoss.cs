using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Blocks.Character;

namespace TimeTravel
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class BlackKnightBoss : MonoBehaviour, IDamageable
    {
        public static BlackKnightBoss Instance { get; private set; }

        [Header("Boss Identity")]
        [SerializeField] private string bossName = "Kaelen, the Void Sentinel";
        [SerializeField] private float bossMaxHealth = 280f;
        [SerializeField] private float currentHealth = 280f;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => bossMaxHealth;
        public bool IsShieldActive => isShieldActive;
        public bool IsDefeated => isDefeated;

        [Header("Arena Bounds & Sealing")]
        [SerializeField] private float arenaTriggerX = 38.0f;
        [SerializeField] private GameObject arenaBarrier;
        [SerializeField] private GameObject exitGateway;
        [SerializeField] private GameObject droppedOrbPrefab;

        [Header("Puzzle Requirement (Chrono Shield)")]
        [SerializeField] private ChronoSwitch switchAlpha;
        [SerializeField] private ChronoSwitch switchBeta;
        [SerializeField] private GameObject chronoShieldVisual;
        [SerializeField] private Light2D shieldLight;
        private bool isShieldActive = true;

        [Header("Combat Settings")]
        [SerializeField] private float moveSpeed = 3.2f;
        [SerializeField] private float meleeRange = 3.0f;
        [SerializeField] private float meleeDamage = 18f;
        [SerializeField] private float meleeCooldown = 2.2f;
        [SerializeField] private float rangedCooldown = 4.0f;
        [SerializeField] private float rollCooldown = 6.0f;
        [SerializeField] private LayerMask obstacleMask;

        [Header("Visual & Audio")]
        [SerializeField] private Sprite projectileSprite;
        [SerializeField] private AudioClip bossMusic;
        [SerializeField] private AudioClip swordSwingClip;
        [SerializeField] private AudioClip rangedCastClip;
        [SerializeField] private AudioClip shieldDeflectClip;
        [SerializeField] private AudioClip hitGruntClip;
        [SerializeField] private AudioClip victoryMusic;

        private Rigidbody2D rb;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private AudioSource audioSource;
        private AudioSource musicSource;
        private Transform playerTransform;

        private bool isEngaged = false;
        private bool isDefeated = false;
        private bool isActionLocked = false;
        private bool isRolling = false;
        private float meleeTimer = 1.0f;
        private float rangedTimer = 2.5f;
        private float rollTimer = 4.0f;

        // IDamageable implementation
        public Transform Transform => transform;
        public bool IsDamageable => !isDefeated && !isRolling;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MeleeHash = Animator.StringToHash("MeleeAttack");
        private static readonly int HeavyHash = Animator.StringToHash("HeavyAttack");
        private static readonly int RangedHash = Animator.StringToHash("RangedAttack");
        private static readonly int RollHash = Animator.StringToHash("Roll");
        private static readonly int HurtHash = Animator.StringToHash("Hurt");
        private static readonly int DeathHash = Animator.StringToHash("Death");

        private void EnsureComponents()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (animator == null) animator = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void Awake()
        {
            Instance = this;
            EnsureComponents();

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = 0.7f;
            if (bossMusic != null) musicSource.clip = bossMusic;

            currentHealth = bossMaxHealth;
            obstacleMask = LayerMask.GetMask("Default", "Ground");

            // Guarantee exact collider alignment with sprite feet (boots at local y = -0.310f)
            var col = GetComponent<CapsuleCollider2D>();
            if (col != null)
            {
                col.offset = new Vector2(-0.075f, -0.160f);
                col.size = new Vector2(0.24f, 0.30f);
            }

            if (projectileSprite == null)
            {
#if UNITY_EDITOR
                projectileSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/[BuildingBlocks]/Art/VFX/Textures/Sprite_Slash.png");
#endif
            }

            // Check persistent completion flag
            if (TimeInventory.FutureBossDefeated)
            {
                isDefeated = true;
                if (arenaBarrier != null) arenaBarrier.SetActive(false);
                if (exitGateway != null) exitGateway.SetActive(true);
                gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            FindPlayer();

            if (switchAlpha == null)
            {
                var sa = GameObject.Find("ChronoSwitch_Alpha");
                if (sa != null) switchAlpha = sa.GetComponent<ChronoSwitch>();
            }
            if (switchBeta == null)
            {
                var sb = GameObject.Find("ChronoSwitch_Beta");
                if (sb != null) switchBeta = sb.GetComponent<ChronoSwitch>();
            }

            if (arenaBarrier != null) arenaBarrier.SetActive(false);
            if (exitGateway != null) exitGateway.SetActive(false);

            CheckPuzzleShieldStatus(silent: true);
        }

        private void FindPlayer()
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        private void Update()
        {
            if (isDefeated) return;

            if (playerTransform == null)
            {
                FindPlayer();
                if (playerTransform == null) return;
            }

            // 1. Detect player entering boss arena
            if (!isEngaged)
            {
                if (playerTransform.position.x >= arenaTriggerX)
                {
                    EngageBoss();
                }
                return;
            }

            // 2. Continually verify puzzle shield state
            CheckPuzzleShieldStatus(silent: false);

            if (isActionLocked) return;

            // 3. Combat Facing
            float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            float dirX = playerTransform.position.x - transform.position.x;
            if (Mathf.Abs(dirX) > 0.2f && !isRolling)
            {
                // Sprite flip facing
                transform.localScale = new Vector3(Mathf.Sign(dirX) * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            // 4. Cooldown Timers
            float dt = Time.deltaTime;
            meleeTimer -= dt;
            rangedTimer -= dt;
            rollTimer -= dt;

            // 5. Tactical AI Decisions
            // Close range: Melee Sword Attack
            if (distToPlayer <= meleeRange && meleeTimer <= 0f)
            {
                StartCoroutine(PerformMeleeAttack());
                return;
            }

            // Mid to Long range: Shadow Blade Wave (Ranged)
            if (distToPlayer > meleeRange + 1.0f && distToPlayer <= 16.0f && rangedTimer <= 0f)
            {
                StartCoroutine(PerformRangedAttack());
                return;
            }

            // Evade / Roll when player gets aggressively close or during ranged cooldown
            if (distToPlayer < 2.0f && rollTimer <= 0f && Random.value < 0.4f)
            {
                StartCoroutine(PerformRoll(Mathf.Sign(-dirX)));
                return;
            }

            // 6. Movement towards player within arena boundaries
            float minArenaX = 37.0f;
            float maxArenaX = 73.5f;

            if (distToPlayer > meleeRange * 0.85f && !isRolling)
            {
                float moveDir = Mathf.Sign(dirX);
                if ((moveDir < 0 && transform.position.x <= minArenaX) || (moveDir > 0 && transform.position.x >= maxArenaX))
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                    if (animator != null) animator.SetFloat(SpeedHash, 0f);
                }
                else
                {
                    rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
                    if (animator != null) animator.SetFloat(SpeedHash, 1.0f);
                }
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                if (animator != null) animator.SetFloat(SpeedHash, 0f);
            }

            // 7. Clamp X inside arena
            float clampedX = Mathf.Clamp(transform.position.x, minArenaX, maxArenaX);
            if (clampedX != transform.position.x)
            {
                transform.position = new Vector3(clampedX, transform.position.y, transform.position.z);
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }

            // 8. Anti-Sink Ground Level Check (Floor top is Y = 0.0, feet are at -0.31 in local space)
            float scaleY = Mathf.Abs(transform.localScale.y);
            float expectedStandY = scaleY * 0.310f; // Exact height where feet touch Y = 0.0
            if (transform.position.y < expectedStandY - 0.2f)
            {
                transform.position = new Vector3(transform.position.x, expectedStandY, transform.position.z);
                if (rb.linearVelocity.y < 0f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                }
            }
        }

        public void CheckPuzzleShieldStatus(bool silent = false)
        {
            // The shield collapses if the player discovered the future cipher archive or energized the twin switches
            bool switchesActive = (switchAlpha != null && switchAlpha.IsActivated && switchBeta != null && switchBeta.IsActivated);
            bool vaultOpen = TimeInventory.VaultDoorOpened || TimeInventory.DiscoveredFutureCode;
            bool puzzleSolved = switchesActive || vaultOpen || TimeInventory.ChronoShieldOverloaded;

            if (puzzleSolved && isShieldActive)
            {
                // Shield Collapse!
                isShieldActive = false;
                TimeInventory.ChronoShieldOverloaded = true;

                if (chronoShieldVisual != null) chronoShieldVisual.SetActive(false);
                if (shieldLight != null) shieldLight.enabled = false;

                if (!silent)
                {
                    if (shieldDeflectClip != null && audioSource != null)
                    {
                        audioSource.PlayOneShot(shieldDeflectClip, 1.5f);
                    }
                    TemporalHUD.Instance?.ShowNotification(
                        "<color=#00FFFF><b>CHRONO BARRIER BREACHED!</b></color>\n<size=18>The Quantum Grid has overloaded the Black Knight's Temporal Aegis! The boss is vulnerable!</size>",
                        5.0f
                    );
                }
            }
            else if (!puzzleSolved && !isShieldActive)
            {
                isShieldActive = true;
                if (chronoShieldVisual != null) chronoShieldVisual.SetActive(true);
                if (shieldLight != null) shieldLight.enabled = true;
            }
        }

        public void EngageBoss()
        {
            if (isEngaged) return;
            isEngaged = true;

            // Seal the arena barrier
            if (arenaBarrier != null) arenaBarrier.SetActive(true);

            // Play boss theme
            if (musicSource != null && musicSource.clip != null)
            {
                musicSource.Play();
            }

            string shieldStatus = isShieldActive 
                ? "<color=#FF4444>[CHRONO SHIELD ACTIVE]</color>" 
                : "<color=#00FF88>[SHIELD OFFLINE]</color>";

            TemporalHUD.Instance?.ShowNotification(
                $"<color=#C080FF><b>BOSS ENCOUNTER:</b></color> <b>{bossName.ToUpper()}</b>\n{shieldStatus}",
                4.5f
            );
        }

        // =========================================================
        // ATTACK PATTERN 1: MELEE SWORD SLASH COMBO
        // =========================================================
        private IEnumerator PerformMeleeAttack()
        {
            isActionLocked = true;
            meleeTimer = meleeCooldown;
            rb.linearVelocity = Vector2.zero;
            if (animator != null) animator.SetFloat(SpeedHash, 0f);

            bool useHeavy = (Random.value < 0.35f);
            if (animator != null)
            {
                animator.SetTrigger(useHeavy ? HeavyHash : MeleeHash);
            }

            if (swordSwingClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(swordSwingClip, 1.2f);
            }

            // Telegraph delay
            yield return new WaitForSeconds(useHeavy ? 0.45f : 0.25f);

            if (isDefeated || playerTransform == null)
            {
                isActionLocked = false;
                yield break;
            }

            // Line-of-sight check to ensure attacks cannot unfairly damage through solid walls
            float scaleY = Mathf.Abs(transform.localScale.y);
            Vector2 origin = (Vector2)transform.position + new Vector2(0f, -0.160f * scaleY);
            Vector2 playerTarget = playerTransform.position + Vector3.up * 0.8f;
            RaycastHit2D wallCheck = Physics2D.Linecast(origin, playerTarget, obstacleMask);

            float currentDist = Vector2.Distance(origin, playerTarget);
            if (currentDist <= meleeRange + 0.5f && wallCheck.collider == null)
            {
                // Land melee strike on player
                var playerDamageable = playerTransform.GetComponent<IDamageable>() ?? playerTransform.GetComponentInParent<IDamageable>();
                if (playerDamageable != null && playerDamageable.IsDamageable)
                {
                    Vector2 hitDir = (playerTarget - origin).normalized;
                    float dmg = useHeavy ? meleeDamage * 1.4f : meleeDamage;
                    DamageInfo damageInfo = new DamageInfo(dmg, null, playerTarget, hitDir, HitReaction.PushBack(12f, 0.3f));
                    playerDamageable.TakeDamage(damageInfo);
                }
            }

            yield return new WaitForSeconds(0.4f);
            isActionLocked = false;
        }

        // =========================================================
        // ATTACK PATTERN 2: LONG-RANGE SHADOW BLADE WAVE
        // =========================================================
        private IEnumerator PerformRangedAttack()
        {
            isActionLocked = true;
            rangedTimer = rangedCooldown;
            rb.linearVelocity = Vector2.zero;
            if (animator != null) animator.SetFloat(SpeedHash, 0f);

            if (animator != null) animator.SetTrigger(RangedHash);

            if (rangedCastClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(rangedCastClip, 1.3f);
            }

            // Charge windup telegraph
            yield return new WaitForSeconds(0.55f);

            if (isDefeated || playerTransform == null)
            {
                isActionLocked = false;
                yield break;
            }

            // Spawn projectile
            float facing = Mathf.Sign(transform.localScale.x);
            float currentScaleY = Mathf.Abs(transform.localScale.y);
            Vector3 spawnPos = transform.position + new Vector3(facing * (0.22f * currentScaleY), -0.160f * currentScaleY, 0f);
            Vector2 launchDir = (playerTransform.position + Vector3.up * 0.5f - spawnPos).normalized;

            GameObject waveObj = new GameObject("ShadowBladeWave");
            waveObj.transform.position = spawnPos;

            var sr = waveObj.AddComponent<SpriteRenderer>();
            sr.sprite = projectileSprite;
            sr.color = new Color(0.75f, 0.2f, 1f, 1f); // Abyssal violet void blade
            sr.sortingLayerName = "Foreground";
            sr.sortingOrder = 15;
            waveObj.transform.localScale = new Vector3(facing * 1.8f, 1.8f, 1f);

            var proj = waveObj.AddComponent<ShadowBladeProjectile>();
            proj.Initialize(launchDir * 11.5f, 16f, transform, obstacleMask);

            yield return new WaitForSeconds(0.45f);
            isActionLocked = false;
        }

        // =========================================================
        // ATTACK PATTERN 3: TACTICAL PHASE ROLL
        // =========================================================
        private IEnumerator PerformRoll(float rollDir)
        {
            isActionLocked = true;
            isRolling = true;
            rollTimer = rollCooldown;

            if (animator != null) animator.SetTrigger(RollHash);

            float rollDuration = 0.55f;
            float elapsed = 0f;
            float rollSpeed = moveSpeed * 2.2f;

            while (elapsed < rollDuration)
            {
                if (isDefeated) break;
                elapsed += Time.deltaTime;
                rb.linearVelocity = new Vector2(rollDir * rollSpeed, rb.linearVelocity.y);
                yield return null;
            }

            isRolling = false;
            isActionLocked = false;
        }

        // =========================================================
        // IDAMAGEABLE IMPLEMENTATION & DAMAGE HANDLING
        // =========================================================
        public void TakeDamage(DamageInfo damage)
        {
            if (isDefeated || isRolling) return;

            // 1. Check Temporal Aegis (Puzzle requirement)
            if (isShieldActive)
            {
                // Attack deflected!
                if (shieldDeflectClip != null && audioSource != null)
                {
                    audioSource.PlayOneShot(shieldDeflectClip, 1.4f);
                }

                if (shieldLight != null)
                {
                    StartCoroutine(ShieldFlashRoutine());
                }

                TemporalHUD.Instance?.ShowNotification(
                    "<color=#FF4444><b>IMMUNE!</b></color> The Black Knight is protected by the Chrono Barrier!\n<i>Activate the 2 Quantum Switches in the Terraces to breach its defenses!</i>",
                    3.5f
                );
                return;
            }

            // 2. Take legitimate damage
            currentHealth -= damage.Amount;
            if (hitGruntClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitGruntClip, 1.0f);
            }

            StartCoroutine(HurtFlashRoutine());

            if (damage.Amount > 25f && animator != null)
            {
                animator.SetTrigger(HurtHash);
            }

            if (currentHealth <= 0f)
            {
                currentHealth = 0f;
                HandleDefeat();
            }
        }

        private IEnumerator ShieldFlashRoutine()
        {
            if (shieldLight == null) yield break;
            float origIntensity = shieldLight.intensity;
            shieldLight.intensity = origIntensity * 3.0f;
            shieldLight.color = Color.white;
            yield return new WaitForSeconds(0.12f);
            shieldLight.intensity = origIntensity;
            shieldLight.color = new Color(0.2f, 0.9f, 1f, 1f);
        }

        private IEnumerator HurtFlashRoutine()
        {
            if (spriteRenderer == null) yield break;
            Color origColor = spriteRenderer.color;
            spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 1f);
            yield return new WaitForSeconds(0.14f);
            if (spriteRenderer != null) spriteRenderer.color = origColor;
        }

        // =========================================================
        // DEFEAT BEHAVIOR & REWARDS
        // =========================================================
        private void HandleDefeat()
        {
            if (isDefeated) return;
            isDefeated = true;
            isActionLocked = true;
            EnsureComponents();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            // Trigger death animation
            if (animator != null) animator.SetTrigger(DeathHash);

            // Audio transition
            if (musicSource != null) musicSource.Stop();
            if (victoryMusic != null && audioSource != null)
            {
                audioSource.PlayOneShot(victoryMusic, 1.5f);
            }

            // Persistent progress
            TimeInventory.FutureBossDefeated = true;

            // Open Arena barriers
            if (arenaBarrier != null) arenaBarrier.SetActive(false);
            if (exitGateway != null) exitGateway.SetActive(true);

            // Drop Reward Orb
            var drop = GetComponent<EnemyOrbDrop>();
            if (drop != null)
            {
                drop.SpawnPhysicalOrb();
            }
            else
            {
                // Fallback: spawn Past Orb right in front of boss
                SpawnVictoryOrb();
            }

            TemporalHUD.Instance?.ShowNotification(
                "<color=#00FFFF><b>CHRONO SENTINEL DEFEATED!</b></color>\n<size=18>The Future Timeline has been purged of corruption! Temporal Gateway opened!</size>",
                7.0f
            );
            TemporalHUD.Instance?.UpdateDisplay();
        }

        private void SpawnVictoryOrb()
        {
            Vector3 spawnPos = transform.position + new Vector3(0f, -0.160f * Mathf.Abs(transform.localScale.y), 0f);
            if (droppedOrbPrefab != null)
            {
                Instantiate(droppedOrbPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                GameObject orbGo = new GameObject("DefeatedBoss_FutureOrbDrop");
                orbGo.transform.position = spawnPos;
                var orb = orbGo.AddComponent<TemporalOrb>();
                orb.SetOrbKind(TemporalOrb.OrbKind.Future, isDynamicDrop: false);
            }
        }

        // =========================================================
        // HIGH-FIDELITY BOSS HEALTH BAR (OnGUI)
        // =========================================================
        private void OnGUI()
        {
            if (!isEngaged || isDefeated) return;

            float curHp = Mathf.Max(0, currentHealth);
            float maxHp = Mathf.Max(1, bossMaxHealth);
            float pct = Mathf.Clamp01(curHp / maxHp);

            float barWidth = Mathf.Min(Screen.width * 0.55f, 540f);
            float barHeight = 24f;
            float startX = (Screen.width - barWidth) * 0.5f;
            float startY = 32f;

            // Background frame
            GUI.color = new Color(0.06f, 0.06f, 0.1f, 0.9f);
            GUI.DrawTexture(new Rect(startX - 4, startY - 26, barWidth + 8, barHeight + 36), Texture2D.whiteTexture);

            // Title & Status
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 13
            };

            if (isShieldActive)
            {
                GUI.color = new Color(0.3f, 0.9f, 1f, 1f);
                GUI.Label(new Rect(startX, startY - 24, barWidth, 18), $"◆  {bossName.ToUpper()}  [TEMPORAL AEGIS: ACTIVE]  ◆", titleStyle);
            }
            else
            {
                GUI.color = new Color(1f, 0.4f, 0.4f, 1f);
                GUI.Label(new Rect(startX, startY - 24, barWidth, 18), $"◆  {bossName.ToUpper()}  [VULNERABLE]  ◆", titleStyle);
            }

            // Bar background slot
            GUI.color = new Color(0.15f, 0.05f, 0.15f, 0.95f);
            GUI.DrawTexture(new Rect(startX, startY, barWidth, barHeight), Texture2D.whiteTexture);

            // Fill color
            Color fillColor = isShieldActive
                ? new Color(0.2f, 0.7f, 1f, 0.9f) // Cyan shield glow
                : (pct > 0.5f) 
                    ? Color.Lerp(new Color(1f, 0.5f, 0.1f), new Color(0.85f, 0.2f, 0.2f), (1f - pct) * 2f)
                    : Color.Lerp(new Color(0.85f, 0.2f, 0.2f), new Color(0.5f, 0.05f, 0.05f), (0.5f - pct) * 2f);

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
    /// Shadow Blade projectile launched by the Black Knight Boss.
    /// Fully respects collision geometry and dissipates on walls/platforms.
    /// </summary>
    public class ShadowBladeProjectile : MonoBehaviour
    {
        private Vector2 velocity;
        private float damage;
        private Transform owner;
        private LayerMask obstacleMask;
        private float lifetime = 3.5f;
        private bool hasHit = false;

        public void Initialize(Vector2 vel, float dmg, Transform caster, LayerMask obstacles)
        {
            velocity = vel;
            damage = dmg;
            owner = caster;
            obstacleMask = obstacles;

            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = velocity;

            var col = gameObject.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.2f, 0.6f);
            col.isTrigger = true;

            // Point light for void projectile
            var lGo = new GameObject("VoidWave_Light");
            lGo.transform.SetParent(transform);
            lGo.transform.localPosition = Vector3.zero;
            var l = lGo.AddComponent<Light2D>();
            l.lightType = Light2D.LightType.Point;
            l.color = new Color(0.8f, 0.25f, 1f, 1f);
            l.pointLightOuterRadius = 3.2f;
            l.intensity = 2.4f;

            // Rotate sprite along velocity vector
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
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

            // Don't hit owner or other projectiles/triggers
            if (owner != null && (other.transform == owner || other.transform.IsChildOf(owner))) return;
            if (other.isTrigger && !other.CompareTag("Player")) return;

            // Check if player hit
            var playerDamageable = other.GetComponentInParent<IDamageable>();
            if (playerDamageable != null && playerDamageable.IsDamageable && (other.CompareTag("Player") || other.GetComponentInParent<BuildingBlocksCharacter>() != null))
            {
                hasHit = true;
                Vector2 hitPoint = other.ClosestPoint(transform.position);
                Vector2 pushDir = velocity.normalized;
                DamageInfo dmg = new DamageInfo(damage, null, hitPoint, pushDir, HitReaction.PushBack(14f, 0.35f));
                playerDamageable.TakeDamage(dmg);
                Destroy(gameObject);
                return;
            }

            // Hit environment wall or platform: immediately detonate without passing through
            if (other.GetComponentInParent<IDamageable>() == null)
            {
                hasHit = true;
                Destroy(gameObject);
            }
        }
    }
}
