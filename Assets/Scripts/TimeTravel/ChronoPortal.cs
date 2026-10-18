using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using Blocks.Character;

namespace TimeTravel
{
    [DisallowMultipleComponent]
    public class ChronoPortal : MonoBehaviour
    {
        [Header("Destination Scene")]
        [SerializeField] private string targetSceneName = "FutureEra";
        [SerializeField] private Era requiredEraOrb = Era.Future;

        public string TargetSceneName => targetSceneName;
        public Era RequiredEraOrb => requiredEraOrb;

        [Header("Portal Visuals")]
        [SerializeField] private Light2D portalLight;
        [SerializeField] private SpriteRenderer vortexRenderer;
        [SerializeField] private Color dormantColor = new Color(0.3f, 0.3f, 0.5f, 0.5f);
        [SerializeField] private Color activeColor = new Color(0.3f, 0.8f, 1f, 1f);

        private bool playerInRange = false;

        private void Awake()
        {
            if (portalLight == null) portalLight = GetComponentInChildren<Light2D>();
            if (vortexRenderer == null) vortexRenderer = GetComponent<SpriteRenderer>();

            UpdatePortalState();
        }

        private void Start()
        {
            UpdatePortalState();
        }

        private void Update()
        {
            if (vortexRenderer != null)
            {
                float rotSpeed = CanTravel() ? 120f : 30f;
                vortexRenderer.transform.Rotate(0, 0, rotSpeed * Time.deltaTime);
            }

            if (playerInRange)
            {
                var kb = Keyboard.current;
                var gp = Gamepad.current;
                bool interactPressed = (kb != null && (kb.fKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame)) ||
                                       (gp != null && gp.buttonWest.wasPressedThisFrame);

                if (interactPressed)
                {
                    TryTravel();
                }
            }
        }

        [SerializeField] private bool requireOrb = true;

        public bool CanTravel()
        {
            if (!requireOrb) return true;
            // Returning to Present from Past is always permitted
            if (TimeInventory.CurrentEra == Era.Past && TimeTravelController.NormalizeSceneName(targetSceneName) == "SampleScene")
            {
                return true;
            }
            if (requiredEraOrb == Era.Future) return TimeInventory.HasFutureOrb;
            if (requiredEraOrb == Era.Present) return TimeInventory.HasPresentOrb;
            if (requiredEraOrb == Era.Past) return TimeInventory.HasPastOrb;
            return true;
        }

        public void UpdatePortalState()
        {
            bool ready = CanTravel();
            if (portalLight != null)
            {
                portalLight.color = ready ? activeColor : dormantColor;
                portalLight.pointLightOuterRadius = ready ? 8.0f : 4.0f;
                portalLight.intensity = ready ? 2.5f : 1.0f;
            }

            if (vortexRenderer != null)
            {
                vortexRenderer.color = ready ? activeColor : dormantColor;
            }
        }

        public void TryTravel()
        {
            if (CanTravel())
            {
                string normScene = TimeTravelController.NormalizeSceneName(targetSceneName);

                // Save Player Health
                var player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    var charComp = player.GetComponent<BuildingBlocksCharacter>();
                    if (charComp != null)
                    {
                        TimeInventory.SavedPlayerHealth = charComp.CurrentHealth;
                    }
                }

                // Determine Intended Arrival Spawn Point
                if (normScene == "FutureEra")
                {
                    TimeInventory.TargetSpawn = TimelineSpawnPoint.Future_PortalArrival;
                }
                else if (normScene == "past")
                {
                    TimeInventory.TargetSpawn = TimelineSpawnPoint.Past_RiftArrival;
                }
                else // Returning to SampleScene
                {
                    string thisScene = gameObject.scene.name.ToLower();
                    if (thisScene.Contains("future") || TimeInventory.CurrentEra == Era.Future)
                    {
                        TimeInventory.TargetSpawn = TimelineSpawnPoint.Present_FromFuturePortal;
                    }
                    else
                    {
                        TimeInventory.TargetSpawn = TimelineSpawnPoint.Present_FromPastPortal;
                    }
                    TimeInventory.JustReturnedToPresent = true;
                }

                TimeInventory.TimelineTravelCount++;

                TemporalHUD.Instance?.HidePrompt();
                TemporalHUD.Instance?.ShowNotification("<color=#00FFFF><b>WARPING THROUGH TIMELINE RIFT...</b></color>", 2.5f);

                if (Application.isPlaying)
                {
                    SceneManager.LoadScene(normScene);
                }
#if UNITY_EDITOR
                else
                {
                    string path = (normScene == "past") ? "Assets/Scenes/past.unity" :
                                  (normScene == "FutureEra") ? "Assets/Scenes/FutureEra.unity" :
                                  "Assets/Scenes/SampleScene.unity";
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Single);
                }
#endif
            }
            else
            {
                string orbNeeded = (requiredEraOrb == Era.Future) ? "Future Orb (Aether Core)" : 
                                   (requiredEraOrb == Era.Past) ? "Past Orb (Ignis Core)" : "Present Orb (Chronos Core)";
                TemporalHUD.Instance?.ShowNotification("<color=#FF6666><b>Portal Offline!</b> Requires " + orbNeeded + "!</color>", 3.5f);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.GetComponentInParent<BuildingBlocksCharacter>() != null)
            {
                playerInRange = true;
                UpdatePortalState();

                if (CanTravel())
                {
                    string targetEra = (targetSceneName.ToLower().Contains("future")) ? "Future" : 
                                       (targetSceneName.ToLower().Contains("past")) ? "Past" : "Present";
                    TemporalHUD.Instance?.ShowPrompt("[F] Enter Time Portal -> Warp to " + targetEra);
                }
                else
                {
                    string orbNeeded = (requiredEraOrb == Era.Future) ? "Future Orb" : 
                                       (requiredEraOrb == Era.Past) ? "Past Orb" : "Present Orb";
                    TemporalHUD.Instance?.ShowPrompt("<color=#FF6666>Chrono Portal Inactive (Requires " + orbNeeded + ")</color>");
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") || collision.GetComponentInParent<BuildingBlocksCharacter>() != null)
            {
                playerInRange = false;
                TemporalHUD.Instance?.HidePrompt();
            }
        }
    }
}
