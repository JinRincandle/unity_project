using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TimeTravel
{
    public class TimelineDistortionManager : MonoBehaviour
    {
        public static TimelineDistortionManager Instance { get; private set; }

        [Header("Crack Visual Assets")]
        [SerializeField] private Sprite crackSprite;
        [SerializeField] private Material crackMaterial;

        [Header("Distortion Intensity")]
        [SerializeField] private float baseShakeDuration = 3.5f;
        [SerializeField] private float baseShakeMagnitude = 0.35f;

        private List<GameObject> activeCracks = new List<GameObject>();
        private Camera mainCamera;
        private Vector3 initialCamOffset;
        private bool isShaking = false;

        private static readonly Vector3[] CrackLocations = new Vector3[]
        {
            new Vector3(-34.2f, 5.5f, 0),   // Zone 1 cliff
            new Vector3(-10.5f, 7.2f, 0),   // Zone 2 temple wall
            new Vector3(1.2f, 5.0f, 0),     // Zone 4 gorge wall
            new Vector3(16.5f, 11.0f, 0),   // Zone 6 tower
            new Vector3(11.0f, -1.5f, 0),   // Zone 5 underground ceiling
            new Vector3(26.0f, 2.0f, 0)     // Zone 7 vault exterior
        };

        private void Awake()
        {
            Instance = this;
            mainCamera = Camera.main;

            if (crackSprite == null)
            {
#if UNITY_EDITOR
                crackSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/[BuildingBlocks]/Art/VFX/Textures/Sprite_Cracks.png");
#endif
            }
        }

        private void Start()
        {
            ApplyTimelineCracks();

            if (TimeInventory.JustReturnedToPresent || TimeInventory.TimelineTravelCount > 0)
            {
                TriggerTimelineEarthquake();
                TimeInventory.JustReturnedToPresent = false;
            }
        }

        public void ApplyTimelineCracks()
        {
            // Clear existing
            foreach (var c in activeCracks)
            {
                if (c != null) Destroy(c);
            }
            activeCracks.Clear();

            int travelCount = TimeInventory.TimelineTravelCount;
            if (travelCount <= 0) return;

            // Number of cracks scales with timeline travel count
            int crackCount = Mathf.Clamp(travelCount * 2, 2, CrackLocations.Length);

            for (int i = 0; i < crackCount; i++)
            {
                Vector3 loc = CrackLocations[i];
                GameObject crackGo = new GameObject($"TimeCrack_{i}");
                crackGo.transform.SetParent(transform);
                crackGo.transform.position = loc;
                crackGo.transform.localScale = new Vector3(1.4f, 1.4f, 1f);

                var sr = crackGo.AddComponent<SpriteRenderer>();
                sr.sprite = crackSprite;
                sr.sortingLayerName = "Foreground";
                sr.sortingOrder = 10;
                // Eerie glowing violet/cyan temporal fracture
                sr.color = (i % 2 == 0) ? new Color(0.8f, 0.3f, 1f, 0.9f) : new Color(0.2f, 0.9f, 1f, 0.9f);

                // Add pulsing light to each crack
                var lightGo = new GameObject("CrackLight");
                lightGo.transform.SetParent(crackGo.transform);
                lightGo.transform.localPosition = Vector3.zero;

                var l2d = lightGo.AddComponent<Light2D>();
                l2d.lightType = Light2D.LightType.Point;
                l2d.color = sr.color;
                l2d.pointLightOuterRadius = 3.5f;
                l2d.intensity = 1.8f;

                activeCracks.Add(crackGo);
            }
        }

        public void TriggerTimelineEarthquake()
        {
            if (isShaking) return;
            StartCoroutine(EarthquakeRoutine());
        }

        private IEnumerator EarthquakeRoutine()
        {
            isShaking = true;
            int count = Mathf.Max(1, TimeInventory.TimelineTravelCount);
            float duration = baseShakeDuration + (count * 0.5f);
            float magnitude = baseShakeMagnitude * Mathf.Clamp(1.0f + count * 0.25f, 1.0f, 2.5f);

            if (TemporalHUD.Instance != null)
            {
                TemporalHUD.Instance.ShowNotification(
                    $"<color=#FF3333><b>[EARTHQUAKE: TIME FRACTURE LEVEL {count}]</b></color>\n<size=18>Traveling through time has destabilized the Present! Cracks appear in reality!</size>",
                    4.0f
                );
            }

            float elapsed = 0f;
            Vector3 camOriginalPos = (mainCamera != null) ? mainCamera.transform.position : Vector3.zero;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float currentMag = magnitude * (1f - progress);

                float offsetX = (Mathf.PerlinNoise(Time.time * 25f, 0f) * 2f - 1f) * currentMag;
                float offsetY = (Mathf.PerlinNoise(0f, Time.time * 25f) * 2f - 1f) * currentMag;

                if (mainCamera != null)
                {
                    mainCamera.transform.position = new Vector3(
                        mainCamera.transform.position.x + offsetX,
                        mainCamera.transform.position.y + offsetY,
                        mainCamera.transform.position.z
                    );
                }

                yield return null;
            }

            isShaking = false;
        }

        private void Update()
        {
            // Subtle pulse on active cracks
            if (activeCracks.Count > 0)
            {
                float pulse = 0.8f + Mathf.PingPong(Time.time * 2f, 0.4f);
                foreach (var c in activeCracks)
                {
                    if (c != null)
                    {
                        var l = c.GetComponentInChildren<Light2D>();
                        if (l != null) l.intensity = 1.5f * pulse;
                    }
                }
            }
        }
    }
}
