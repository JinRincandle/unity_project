using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using Blocks.Character;
using Unity.Cinemachine;

namespace TimeTravel
{
    [DisallowMultipleComponent]
    public class TimeTravelController : MonoBehaviour
    {
        public static TimeTravelController Instance { get; private set; }

        [SerializeField] private Era currentSceneEra = Era.Present;
        private bool isTransitioning = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            TimeInventory.CurrentEra = currentSceneEra;
        }

        private void Start()
        {
            // Restore persistent player state and arrival spawn position upon entering scene
            InitializePlayerInScene();
        }

        public void InitializePlayerInScene()
        {
            var player = GameObject.FindWithTag("Player");
            if (player == null) return;

            var charComp = player.GetComponent<BuildingBlocksCharacter>();
            var rb = player.GetComponent<Rigidbody2D>();

            // 1. Reposition to Intended Timeline Arrival Point
            if (TimeInventory.TargetSpawn != TimelineSpawnPoint.DefaultStart)
            {
                Vector3 arrival = TimeInventory.GetSpawnCoordinates(currentSceneEra, TimeInventory.TargetSpawn);
                if (arrival != Vector3.zero)
                {
                    player.transform.position = arrival;
                    if (rb != null)
                    {
                        rb.position = (Vector2)arrival;
                        rb.linearVelocity = Vector2.zero;
                    }
                }
                // Reset so standard respawns stay within the level
                TimeInventory.TargetSpawn = TimelineSpawnPoint.DefaultStart;
            }

            // 2. Restore Persistent Health
            if (charComp != null && TimeInventory.SavedPlayerHealth > 0f)
            {
                float delta = TimeInventory.SavedPlayerHealth - charComp.CurrentHealth;
                if (Mathf.Abs(delta) > 0.01f)
                {
                    charComp.Apply(StatType.Health, delta);
                }
            }

            // 3. Instant Snap Camera to Avoid Jarring Pans Across Map
            SnapCameraToPlayer(player.transform);

            // 4. Sync CurrentEra & Puzzle States
            TimeInventory.CurrentEra = currentSceneEra;
            Object.FindAnyObjectByType<TemporalVaultPuzzle>()?.RestoreVaultState();
            Object.FindAnyObjectByType<ChronoConduitGrid>()?.RestoreGridState();

            // 5. Update HUD
            TemporalHUD.Instance?.UpdateDisplay();
        }

        private void SnapCameraToPlayer(Transform playerTransform)
        {
            // Cinemachine Camera Support
            var vcam = Object.FindAnyObjectByType<CinemachineCamera>();
            if (vcam != null)
            {
                vcam.Target.TrackingTarget = playerTransform;
                vcam.Follow = playerTransform;
                vcam.transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, vcam.transform.position.z);
            }

            // Main Camera Support
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(playerTransform.position.x, playerTransform.position.y, mainCam.transform.position.z);
            }
        }

        private void Update()
        {
            if (isTransitioning) return;

            var kb = Keyboard.current;
            if (kb == null) return;

            // 1 = Past
            if (kb.digit1Key.wasPressedThisFrame || kb.numpad1Key.wasPressedThisFrame)
            {
                AttemptTravel(Era.Past);
            }
            // 2 = Present
            else if (kb.digit2Key.wasPressedThisFrame || kb.numpad2Key.wasPressedThisFrame)
            {
                AttemptTravel(Era.Present);
            }
            // 3 = Future
            else if (kb.digit3Key.wasPressedThisFrame || kb.numpad3Key.wasPressedThisFrame)
            {
                AttemptTravel(Era.Future);
            }
        }

        public void AttemptTravel(Era targetEra)
        {
            if (targetEra == currentSceneEra)
            {
                TemporalHUD.Instance?.ShowNotification($"<color=#AAAAAA>Already in the {currentSceneEra} Era.</color>", 2.0f);
                return;
            }

            if (targetEra == Era.Past)
            {
                if (TimeInventory.HasPastOrb)
                {
                    PreparePlayerStateForTravel(TimelineSpawnPoint.Past_RiftArrival);
                    StartTransition("past", "<color=#FFA500>TEMPORAL RIFT TO PAST (WAR-TORN VOLCANIC CRUCIBLE)...</color>");
                }
                else
                {
                    TemporalHUD.Instance?.ShowNotification("<color=#FF6666>Cannot shift to Past: Missing Past Orb (Ignis Core)! Claim it from the Future Catacombs.</color>", 4.0f);
                }
                return;
            }

            if (targetEra == Era.Present)
            {
                if (currentSceneEra == Era.Future)
                {
                    if (TimeInventory.HasPresentOrb)
                    {
                        PreparePlayerStateForTravel(TimelineSpawnPoint.Present_FromFuturePortal);
                        StartTransition("SampleScene", "<color=#00FFFF>SHIFTING TO PRESENT (2026 AD)...</color>");
                    }
                    else
                    {
                        TemporalHUD.Instance?.ShowNotification("<color=#FF6666>Cannot shift to Present: Missing Present Orb! Unlock the Future Vault platform to claim it.</color>", 3.5f);
                    }
                }
                else
                {
                    PreparePlayerStateForTravel(TimelineSpawnPoint.Present_FromPastPortal);
                    StartTransition("SampleScene", "<color=#00FFFF>RETURNING TO PRESENT...</color>");
                }
                return;
            }

            if (targetEra == Era.Future)
            {
                if (TimeInventory.HasFutureOrb)
                {
                    PreparePlayerStateForTravel(TimelineSpawnPoint.Future_PortalArrival);
                    StartTransition("FutureEra", "<color=#C080FF>TEMPORAL LEAP TO FUTURE (3042 AD)...</color>");
                }
                else
                {
                    TemporalHUD.Instance?.ShowNotification("<color=#FF6666>Cannot shift to Future: Missing Future Orb! Defeat the Chrono Guardian in the West Wing to claim it.</color>", 4.0f);
                }
                return;
            }
        }

        public void PreparePlayerStateForTravel(TimelineSpawnPoint nextSpawn)
        {
            TimeInventory.TargetSpawn = nextSpawn;

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
        }

        private void StartTransition(string sceneName, string notification)
        {
            isTransitioning = true;
            TimeInventory.TimelineTravelCount++;
            string norm = NormalizeSceneName(sceneName);
            if (norm == "SampleScene")
            {
                TimeInventory.JustReturnedToPresent = true;
            }
            TemporalHUD.Instance?.ShowNotification(notification, 2.5f);
            pendingSceneName = norm;
            Invoke(nameof(ExecuteSceneLoad), 0.75f);
        }

        public static string NormalizeSceneName(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "SampleScene";
            string lower = raw.ToLower();
            if (lower.Contains("past")) return "past";
            if (lower.Contains("future")) return "FutureEra";
            if (lower.Contains("sample") || lower.Contains("present")) return "SampleScene";
            return raw;
        }

        private string pendingSceneName;
        private void ExecuteSceneLoad()
        {
            if (Application.isPlaying)
            {
                SceneManager.LoadScene(pendingSceneName);
            }
#if UNITY_EDITOR
            else
            {
                string path = (pendingSceneName == "past") ? "Assets/Scenes/past.unity" :
                              (pendingSceneName == "FutureEra") ? "Assets/Scenes/FutureEra.unity" :
                              "Assets/Scenes/SampleScene.unity";
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path, UnityEditor.SceneManagement.OpenSceneMode.Single);
            }
#endif
        }
    }
}
