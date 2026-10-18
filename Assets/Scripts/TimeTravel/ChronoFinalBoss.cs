using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Blocks.Character;
using Blocks.Attack;

namespace TimeTravel
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BuildingBlocksCharacter))]
    public class ChronoFinalBoss : MonoBehaviour
    {
        [Header("Boss Identity")]
        [SerializeField] private string bossName = "Chronos, Avatar of the Fractured Paradox";
        [SerializeField] private float bossMaxHealth = 300f;

        [Header("Sanctum Arena")]
        [SerializeField] private float engageThresholdX = 32.5f;
        [SerializeField] private GameObject arenaBarrier;
        [SerializeField] private GameObject timelineCoreShield;
        [SerializeField] private TimelineStabilizerCore timelineCore;

        [Header("Boss Attacks")]
        [SerializeField] private float meleeDamage = 22f;
        [SerializeField] private float meleeRange = 2.4f;
        [SerializeField] private float attackCooldown = 1.4f;
        [SerializeField] private GameObject shockwaveVfxPrefab;

        [Header("Audio")]
        [SerializeField] private AudioClip bossMusic;
        [SerializeField] private AudioClip strikeSound;
        [SerializeField] private AudioClip shockwaveSound;
        [SerializeField] private AudioClip victorySound;

        [Header("Teleport Points (Inside Sanctum)")]
        [SerializeField] private Transform[] teleportNodes;

        private BuildingBlocksCharacter character;
        private Transform playerTransform;
        private AudioSource audioSource;
        private bool isEngaged = false;
        private bool isDefeated = false;
        private float nextAttackTime = 0f;
        private float nextTeleportTime = 0f;
        private bool isPhase2 = false;

        private void Awake()
        {
            character = GetComponent<BuildingBlocksCharacter>();
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            if (arenaBarrier != null) arenaBarrier.SetActive(false);
            if (timelineCoreShield != null) timelineCoreShield.SetActive(true);
        }

        private void Start()
        {
            var playerGo = GameObject.FindWithTag("Player");
            if (playerGo != null) playerTransform = playerGo.transform;

            // Initialize health
            if (character != null && character.MaxHealth > 0f)
            {
                bossMaxHealth = character.MaxHealth;
            }
        }

        private void OnEnable()
        {
            if (character != null)
            {
                character.OnDamaged += HandleDamaged;
                character.OnEliminated += HandleDefeated;
            }
        }

        private void OnDisable()
        {
            if (character != null)
            {
                character.OnDamaged -= HandleDamaged;
                character.OnEliminated -= HandleDefeated;
            }
        }

        private void Update()
        {
            if (isDefeated) return;

            if (!isEngaged)
            {
                if (playerTransform != null && playerTransform.position.x >= engageThresholdX && playerTransform.position.y >= 10.0f)
                {
                    EngageBoss();
                }
                return;
            }

            if (playerTransform == null) return;

            // Combat AI Loop
            float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            float dirX = playerTransform.position.x - transform.position.x;

            if (Mathf.Abs(dirX) > 0.2f)
            {
                character.FacePosition(playerTransform.position);
            }

            // Phase 2 check (under 50% HP)
            if (!isPhase2 && character.CurrentHealth <= bossMaxHealth * 0.5f)
            {
                isPhase2 = true;
                TemporalHUD.Instance?.ShowNotification("<color=#FF0055><b>TEMPORAL SURGE!</b> Chronos accelerates time — attacks become rapid!</color>", 3.0f);
            }

            // Phase 2 Teleportation
            if (isPhase2 && Time.time >= nextTeleportTime && distToPlayer < 2.0f)
            {
                TeleportToFarthestNode();
                nextTeleportTime = Time.time + Random.Range(4.0f, 6.5f);
                return;
            }

            // Melee Attack when in range
            if (distToPlayer <= meleeRange && Time.time >= nextAttackTime)
            {
                ExecuteMeleeAttack();
                nextAttackTime = Time.time + (isPhase2 ? attackCooldown * 0.7f : attackCooldown);
            }
        }

        private void EngageBoss()
        {
            isEngaged = true;

            if (arenaBarrier != null) arenaBarrier.SetActive(true);

            if (bossMusic != null && audioSource != null)
            {
                audioSource.clip = bossMusic;
                audioSource.loop = true;
                audioSource.volume = 0.85f;
                audioSource.Play();
            }

            TemporalHUD.Instance?.ShowNotification(
                $"<color=#FFD700><b>FINAL TRIAL: {bossName.ToUpper()}!</b></color>\n<size=18>Defeat the avatar of the broken timeline to unlock the Master Core!</size>",
                5.0f
            );
        }

        private void ExecuteMeleeAttack()
        {
            character.FacePosition(playerTransform.position);

            if (strikeSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(strikeSound, 1.2f);
            }

            // Deal damage via character hitbox
            character.HitBox(
                new Vector2(1.2f, 0f),
                new Vector2(2.4f, 2.0f),
                meleeDamage,
                HitReaction.PushBack(18f, 0.35f)
            );
        }

        private void TeleportToFarthestNode()
        {
            if (teleportNodes == null || teleportNodes.Length == 0) return;

            Transform bestNode = null;
            float maxDist = -1f;

            foreach (var node in teleportNodes)
            {
                if (node == null) continue;
                float d = Vector2.Distance(node.position, playerTransform.position);
                if (d > maxDist)
                {
                    maxDist = d;
                    bestNode = node;
                }
            }

            if (bestNode != null)
            {
                transform.position = bestNode.position;
                var rb = GetComponent<Rigidbody2D>();
                if (rb != null) rb.position = bestNode.position;

                if (shockwaveSound != null && audioSource != null)
                {
                    audioSource.PlayOneShot(shockwaveSound, 1.0f);
                }
            }
        }

        private void HandleDamaged(DamageInfo info)
        {
            if (!isEngaged) EngageBoss();
        }

        private void HandleDefeated()
        {
            if (isDefeated) return;
            isDefeated = true;

            if (audioSource != null) audioSource.Stop();
            if (victorySound != null && audioSource != null)
            {
                audioSource.PlayOneShot(victorySound, 1.5f);
            }

            // Lower arena barrier
            if (arenaBarrier != null) arenaBarrier.SetActive(false);

            // Lower shield on the master core
            if (timelineCoreShield != null) timelineCoreShield.SetActive(false);

            // Notify player
            TemporalHUD.Instance?.ShowNotification(
                "<color=#00FFFF><b>PARADOX OVERLORD DEFEATED!</b></color>\n<size=20>The temporal anomaly collapses! Claim the <b>Master Timeline Core</b> to heal reality!</size>",
                8.0f
            );

            // Drop guaranteed victory orb
            var drop = GetComponent<EnemyOrbDrop>();
            if (drop != null)
            {
                drop.SpawnPhysicalOrb();
            }
        }

        private void OnGUI()
        {
            if (!isEngaged || isDefeated || character == null) return;

            float screenW = Screen.width;
            float barW = Mathf.Min(480f, screenW * 0.6f);
            float barH = 24f;
            float barX = (screenW - barW) * 0.5f;
            float barY = 55f;

            float curHp = Mathf.Max(0f, character.CurrentHealth);
            float hpPct = Mathf.Clamp01(curHp / bossMaxHealth);

            // Boss Title
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 17,
                fontStyle = FontStyle.Bold
            };
            titleStyle.normal.textColor = new Color(1.0f, 0.85f, 0.3f, 1f);
            GUI.Label(new Rect(barX, barY - 26f, barW, 24f), bossName, titleStyle);

            // Background
            GUI.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            GUI.DrawTexture(new Rect(barX - 2, barY - 2, barW + 4, barH + 4), Texture2D.whiteTexture);

            // HP Fill (Cyan to Gold Gradient)
            Color fillColor = Color.Lerp(new Color(0.1f, 0.9f, 1.0f), new Color(1f, 0.75f, 0.1f), 1f - hpPct);
            GUI.color = fillColor;
            GUI.DrawTexture(new Rect(barX, barY, barW * hpPct, barH), Texture2D.whiteTexture);

            // HP Text
            GUIStyle hpTextStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 13,
                fontStyle = FontStyle.Bold
            };
            hpTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(barX, barY, barW, barH), $"{curHp:F0} / {bossMaxHealth:F0}", hpTextStyle);

            GUI.color = Color.white;
        }
    }
}
