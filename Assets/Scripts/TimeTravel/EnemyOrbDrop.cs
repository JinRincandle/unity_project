using UnityEngine;
using UnityEngine.Rendering.Universal;
using Blocks.Character;

namespace TimeTravel
{
    [RequireComponent(typeof(BuildingBlocksCharacter))]
    [DisallowMultipleComponent]
    public class EnemyOrbDrop : MonoBehaviour
    {
        [Header("Drop Settings")]
        public TemporalOrb.OrbKind orbToDrop = TemporalOrb.OrbKind.Present;
        [SerializeField] private GameObject orbPickupPrefab;
        [Range(0f, 1f)]
        public float dropChance = 1.0f;
        public string dropMessage = "<color=#00FFFF><b>ORB DROPPED!</b> A defeated enemy dropped a Chronos Core!</color>";

        private BuildingBlocksCharacter character;
        private bool hasDropped = false;

        private void Awake()
        {
            character = GetComponent<BuildingBlocksCharacter>();
        }

        private void OnEnable()
        {
            if (character != null)
            {
                character.OnEliminated += HandleCharacterEliminated;
            }
        }

        private void OnDisable()
        {
            if (character != null)
            {
                character.OnEliminated -= HandleCharacterEliminated;
            }
        }

        private void HandleCharacterEliminated()
        {
            if (hasDropped) return;
            hasDropped = true;

            if (Random.value > dropChance) return;

            SpawnPhysicalOrb();
        }

        public GameObject SpawnPhysicalOrb()
        {
            Vector3 spawnPos = CalculateSafeSpawnPosition();
            GameObject orbGo = null;

            if (orbPickupPrefab != null)
            {
                orbGo = Instantiate(orbPickupPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                // Check if an official prefab exists in Assets/Prefabs/TimeTravel/
                string prefabPath = $"Assets/Prefabs/TimeTravel/DroppedOrb_{orbToDrop}.prefab";
#if UNITY_EDITOR
                var loadedPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (loadedPrefab != null)
                {
                    orbGo = Instantiate(loadedPrefab, spawnPos, Quaternion.identity);
                }
#endif
                if (orbGo == null)
                {
                    orbGo = CreateDynamicOrbObject(spawnPos, orbToDrop);
                }
            }

            // Configure TemporalOrb
            var orbScript = orbGo.GetComponent<TemporalOrb>();
            if (orbScript == null) orbScript = orbGo.AddComponent<TemporalOrb>();
            orbScript.SetOrbKind(orbToDrop);

            // Configure Physics2D
            var rb = orbGo.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = orbGo.AddComponent<Rigidbody2D>();
            }
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1.4f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Upward pop with slight horizontal spread
            rb.linearVelocity = new Vector2(Random.Range(-1.0f, 1.0f), 3.5f);

            // Ignore collision with player on all non-trigger colliders so player doesn't push/kick orb
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                var pCols = player.GetComponentsInChildren<Collider2D>();
                var oCols = orbGo.GetComponentsInChildren<Collider2D>();
                foreach (var oc in oCols)
                {
                    if (!oc.isTrigger)
                    {
                        foreach (var pc in pCols)
                        {
                            Physics2D.IgnoreCollision(oc, pc, true);
                        }
                    }
                }
            }

            TemporalHUD.Instance?.ShowNotification(dropMessage, 3.5f);
            return orbGo;
        }

        private Vector3 CalculateSafeSpawnPosition()
        {
            Vector3 origin = transform.position + Vector3.up * 0.6f;

            // Check if origin overlaps solid geometry (excluding triggers and player)
            var overlap = Physics2D.OverlapCircle(origin, 0.25f);
            if (overlap != null && !overlap.isTrigger && !overlap.CompareTag("Player"))
            {
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    Vector3 awayFromWall = (player.transform.position - transform.position).normalized;
                    origin += awayFromWall * 0.5f + Vector3.up * 0.4f;
                }
                else
                {
                    origin += Vector3.up * 0.8f;
                }
            }

            return origin;
        }

        public static GameObject CreateDynamicOrbObject(Vector3 pos, TemporalOrb.OrbKind kind)
        {
            GameObject orb = new GameObject($"Dropped_{kind}_Orb");
            orb.transform.position = pos;

            // Visual Sprite
            var sr = orb.AddComponent<SpriteRenderer>();
            sr.sprite = ResolveOrbSprite(kind);
            sr.sortingLayerName = "Foreground";
            sr.sortingOrder = 10;
            sr.color = TemporalOrb.GetOrbColor(kind);
            orb.transform.localScale = new Vector3(1.2f, 1.2f, 1.0f);

            // Point Light2D
            var l2d = orb.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Point;
            l2d.color = TemporalOrb.GetOrbColor(kind);
            l2d.pointLightOuterRadius = 4.2f;
            l2d.pointLightInnerRadius = 0.6f;
            l2d.intensity = 2.5f;

            // Ground Landing Solid Collider (small radius so it rests neatly on ground)
            var groundCol = orb.AddComponent<CircleCollider2D>();
            groundCol.radius = 0.2f;
            groundCol.isTrigger = false;

            // Direct Pickup Trigger Collider (generous radius for reliable player detection)
            var pickupCol = orb.AddComponent<CircleCollider2D>();
            pickupCol.radius = 0.85f;
            pickupCol.isTrigger = true;

            // TemporalOrb script directly on root
            var pickup = orb.AddComponent<TemporalOrb>();
            pickup.SetOrbKind(kind);

            return orb;
        }

        private static Sprite ResolveOrbSprite(TemporalOrb.OrbKind kind)
        {
#if UNITY_EDITOR
            string spritePath = (kind == TemporalOrb.OrbKind.Past)
                ? "Assets/Brackeys/2D Mega Pack/Items & Icons/Pixel Art/Fire.png"
                : (kind == TemporalOrb.OrbKind.Future)
                    ? "Assets/Brackeys/2D Mega Pack/Items & Icons/Pixel Art/MagicEssence.png"
                    : "Assets/Brackeys/2D Mega Pack/Items & Icons/Pixel Art/Diamond.png";

            var sp = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sp != null) return sp;
#endif
            return CreateFallbackCircleSprite();
        }

        private static Sprite CreateFallbackCircleSprite()
        {
            int res = 32;
            Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2(res * 0.5f, res * 0.5f);
            float r = res * 0.45f;

            for (int y = 0; y < res; y++)
            {
                for (int x = 0; x < res; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist <= r)
                    {
                        float alpha = Mathf.Clamp01(1f - (dist / r) * 0.2f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 16f);
        }
    }
}
