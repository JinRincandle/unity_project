using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Blocks.Character;

namespace TimeTravel
{
    [DisallowMultipleComponent]
    public class TemporalOrb : MonoBehaviour
    {
        public enum OrbKind
        {
            Present,
            Future,
            Past
        }

        [Header("Orb Configuration")]
        [SerializeField] private OrbKind orbKind = OrbKind.Future;
        [SerializeField] private string orbName = "Aether Future Core";
        [SerializeField] private Light2D orbLight;
        [SerializeField] private float hoverAmplitude = 0.25f;
        [SerializeField] private float hoverFrequency = 2.5f;
        [SerializeField] private AudioClip pickupSound;
        [SerializeField] private bool persistAcrossScenes = true;

        private Vector3 startPos;
        private Vector3 settledPos;
        private bool isCollected = false;
        private Rigidbody2D rb;
        private bool isGroundedSettled = false;
        private float restTimer = 0f;

        public bool IsCollected => isCollected;
        public OrbKind CurrentOrbKind => orbKind;

        public void SetOrbKind(OrbKind kind, bool isDynamicDrop = true)
        {
            orbKind = kind;
            if (kind == OrbKind.Present) orbName = "Chronos Present Core";
            else if (kind == OrbKind.Future) orbName = "Aether Future Core";
            else orbName = "Ignis Past Core";
            if (isDynamicDrop) persistAcrossScenes = false;
            ApplyVisualColors();
        }

        private void Awake()
        {
            startPos = transform.position;
            settledPos = transform.position;
            rb = GetComponent<Rigidbody2D>();
            if (orbLight == null) orbLight = GetComponentInChildren<Light2D>();

            ApplyVisualColors();

            // Ignore solid collision with player so the player never stumbles on or kicks the orb
            IgnorePlayerCollisions();

            // Static scene orbs auto-deactivate if already collected in current save
            if (persistAcrossScenes)
            {
                if (orbKind == OrbKind.Present && TimeInventory.HasPresentOrb) gameObject.SetActive(false);
                if (orbKind == OrbKind.Future && TimeInventory.HasFutureOrb) gameObject.SetActive(false);
                // For Past, if 3 or more have been acquired, scene orbs can rest
                if (orbKind == OrbKind.Past && TimeInventory.PastOrbCount >= 3) gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            // Re-check player collision ignore in Start once all scene objects are instantiated
            IgnorePlayerCollisions();
        }

        private void ApplyVisualColors()
        {
            Color orbColor = GetOrbColor(orbKind);
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = orbColor;

            if (orbLight != null)
            {
                orbLight.color = orbColor;
            }
        }

        public static Color GetOrbColor(OrbKind kind)
        {
            switch (kind)
            {
                case OrbKind.Present:
                    return new Color(0.0f, 0.95f, 1.0f, 1.0f); // Bright Cyan
                case OrbKind.Future:
                    return new Color(0.75f, 0.45f, 1.0f, 1.0f); // Radiant Purple
                case OrbKind.Past:
                default:
                    return new Color(1.0f, 0.55f, 0.1f, 1.0f); // Molten Orange
            }
        }

        private void IgnorePlayerCollisions()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;

            var playerCols = player.GetComponentsInChildren<Collider2D>();
            var myCols = GetComponentsInChildren<Collider2D>();

            for (int i = 0; i < myCols.Length; i++)
            {
                var myCol = myCols[i];
                if (!myCol.isTrigger)
                {
                    for (int j = 0; j < playerCols.Length; j++)
                    {
                        if (playerCols[j] != null)
                        {
                            Physics2D.IgnoreCollision(myCol, playerCols[j], true);
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (isCollected) return;

            if (rb == null)
            {
                // Static placed orb: gentle stationary floating
                transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude, 0);
            }
            else
            {
                // Dynamic dropped orb: wait for it to land, then settle smoothly
                if (!isGroundedSettled)
                {
                    if (rb.linearVelocity.sqrMagnitude < 0.05f)
                    {
                        restTimer += Time.deltaTime;
                        if (restTimer > 0.4f)
                        {
                            isGroundedSettled = true;
                            settledPos = transform.position;
                            rb.bodyType = RigidbodyType2D.Kinematic;
                            rb.linearVelocity = Vector2.zero;
                        }
                    }
                    else
                    {
                        restTimer = 0f;
                    }
                }
                else
                {
                    // Once settled, gentle hover bobbing
                    transform.position = settledPos + new Vector3(0, Mathf.Sin(Time.time * hoverFrequency) * (hoverAmplitude * 0.4f), 0);
                }

                // Anti-void safety: if dropped below level bounds, safely recover
                if (transform.position.y < -35f)
                {
                    var player = GameObject.FindWithTag("Player");
                    if (player != null)
                    {
                        transform.position = player.transform.position + Vector3.up * 1.5f;
                    }
                    else
                    {
                        transform.position = startPos;
                    }
                    settledPos = transform.position;
                    rb.linearVelocity = Vector2.zero;
                }
            }

            // Light pulse
            if (orbLight != null)
            {
                orbLight.pointLightOuterRadius = 4.2f + Mathf.Sin(Time.time * 3.5f) * 0.6f;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isCollected) return;

            if (collision.CompareTag("Player") || collision.GetComponentInParent<BuildingBlocksCharacter>()?.CompareTag("Player") == true)
            {
                var character = collision.GetComponentInParent<BuildingBlocksCharacter>();
                Collect(character);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isCollected) return;

            if (collision.collider.CompareTag("Player") || collision.collider.GetComponentInParent<BuildingBlocksCharacter>()?.CompareTag("Player") == true)
            {
                var character = collision.collider.GetComponentInParent<BuildingBlocksCharacter>();
                Collect(character);
            }
        }

        public void Collect(BuildingBlocksCharacter character = null)
        {
            if (isCollected) return;
            isCollected = true;

            // 1. Instantly disable all colliders to guarantee single-collection
            var colliders = GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].enabled = false;
            }

            // 2. Play Audio Feedback
            AudioClip clipToPlay = pickupSound;
            if (clipToPlay == null)
            {
                clipToPlay = Resources.Load<AudioClip>("Bonus");
            }
            if (clipToPlay != null)
            {
                AudioSource.PlayClipAtPoint(clipToPlay, transform.position, 1.0f);
            }

            // 3. Spawn Radiant Pickup VFX
            SpawnPickupEffect();

            // 4. Update TimeInventory & Character Health
            if (orbKind == OrbKind.Present)
            {
                TimeInventory.HasPresentOrb = true;
                TemporalHUD.Instance?.ShowNotification("<color=#00FFFF><b>ACQUIRED: Present Chronos Core!</b></color>", 4.0f);
            }
            else if (orbKind == OrbKind.Future)
            {
                TimeInventory.HasFutureOrb = true;
                TemporalHUD.Instance?.ShowNotification("<color=#C080FF><b>ACQUIRED: Future Aether Core!</b> Portal resonance active!</color>", 4.5f);
            }
            else
            {
                TimeInventory.HasPastOrb = true;
                TimeInventory.PastOrbCount++;
                TemporalHUD.Instance?.ShowNotification($"<color=#FFA500><b>ACQUIRED: Past Ignis Core!</b> ({TimeInventory.PastOrbCount} Collected)</color>", 4.0f);
            }

            // Small health restoration for player
            if (character != null)
            {
                character.Apply(StatType.Health, 25f);
            }

            // 5. Update HUD Immediately
            TemporalHUD.Instance?.UpdateDisplay();

            // 6. Instantly hide visuals so there is zero delay
            var sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
            if (orbLight != null) orbLight.enabled = false;

            // 7. Clean Removal
            gameObject.SetActive(false);
            if (rb != null || !persistAcrossScenes)
            {
                // Dynamic drop or transient orb
                if (Application.isPlaying)
                {
                    Destroy(gameObject);
                }
                else
                {
                    DestroyImmediate(gameObject);
                }
            }
        }

        private void SpawnPickupEffect()
        {
            GameObject fx = new GameObject("Orb_Pickup_Effect");
            fx.transform.position = transform.position;

            var l2d = fx.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Point;
            l2d.color = GetOrbColor(orbKind);
            l2d.pointLightOuterRadius = 6.0f;
            l2d.intensity = 4.0f;

            var sr = fx.AddComponent<SpriteRenderer>();
            var mySr = GetComponentInChildren<SpriteRenderer>();
            if (mySr != null && mySr.sprite != null)
            {
                sr.sprite = mySr.sprite;
            }
            sr.color = l2d.color;
            sr.sortingLayerName = "Foreground";
            sr.sortingOrder = 15;

            fx.AddComponent<PickupEffectFader>();
        }
    }

    public class PickupEffectFader : MonoBehaviour
    {
        private Light2D l2d;
        private SpriteRenderer sr;
        private float timer = 0f;
        private const float Duration = 0.45f;

        private void Awake()
        {
            l2d = GetComponent<Light2D>();
            sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float t = timer / Duration;

            if (l2d != null)
            {
                l2d.intensity = Mathf.Lerp(4.0f, 0f, t);
                l2d.pointLightOuterRadius = Mathf.Lerp(6.0f, 10.0f, t);
            }

            if (sr != null)
            {
                transform.localScale = Vector3.Lerp(Vector3.one * 1.0f, Vector3.one * 2.8f, t);
                Color c = sr.color;
                c.a = Mathf.Lerp(1.0f, 0f, t);
                sr.color = c;
            }

            if (timer >= Duration)
            {
                Destroy(gameObject);
            }
        }
    }
}
