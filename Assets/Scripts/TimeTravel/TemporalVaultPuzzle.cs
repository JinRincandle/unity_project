using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TimeTravel
{
    public class TemporalVaultPuzzle : MonoBehaviour
    {
        public static TemporalVaultPuzzle Instance { get; private set; }

        [Header("Vault Door")]
        [SerializeField] private Transform vaultDoor;
        [SerializeField] private Collider2D doorCollider;
        [SerializeField] private Vector3 openOffset = new Vector3(0, 5.0f, 0);
        [SerializeField] private float openSpeed = 3.0f;
        [SerializeField] private Light2D vaultLockLight;

        [Header("Correct Solution Sequence (Pillar IDs)")]
        [SerializeField] private int[] requiredSequence = new int[] { 3, 1, 2 }; // Abyss (3) -> Chronos (1) -> Sol (2)

        private List<int> currentInput = new List<int>();
        private bool isSolved = false;
        private Vector3 doorClosedPos;
        private Vector3 doorOpenPos;

        private void Awake()
        {
            Instance = this;
            RestoreVaultState();
        }

        private void Start()
        {
            RestoreVaultState();
        }

        public void RestoreVaultState()
        {
            if (vaultDoor != null && (doorClosedPos == Vector3.zero || doorOpenPos == Vector3.zero))
            {
                doorClosedPos = vaultDoor.position;
                doorOpenPos = doorClosedPos + openOffset;
            }

            if (TimeInventory.VaultDoorOpened)
            {
                isSolved = true;
                if (doorCollider != null) doorCollider.enabled = false;
                if (vaultDoor != null) vaultDoor.position = doorOpenPos;
                if (vaultLockLight != null)
                {
                    vaultLockLight.color = new Color(0.2f, 1f, 0.4f, 1f);
                    vaultLockLight.pointLightOuterRadius = 6.0f;
                    vaultLockLight.intensity = 3.0f;
                }
            }
        }

        private void Update()
        {
            if (isSolved && vaultDoor != null && vaultDoor.position != doorOpenPos)
            {
                vaultDoor.position = Vector3.MoveTowards(vaultDoor.position, doorOpenPos, openSpeed * Time.deltaTime);
            }
        }

        public void RegisterPillarActivated(TemporalRunePillar pillar)
        {
            if (isSolved) return;

            // Condition 1: Must have aligned conduits in the Past
            if (!TimeInventory.ConduitsAlignedInPast)
            {
                TemporalHUD.Instance?.ShowNotification("<color=#FF4444><b>POWER GRID OFFLINE!</b>\nThe foundation conduits were never connected. Travel to the <b>Past [1]</b> to align the 3 energy conduits during vault construction!</color>", 4.5f);
                ResetAllPillars();
                return;
            }

            currentInput.Add(pillar.PillarId);
            int stepIndex = currentInput.Count - 1;

            // Check if current step matches
            if (currentInput[stepIndex] == requiredSequence[stepIndex])
            {
                TemporalHUD.Instance?.ShowNotification($"<color=#00FF88>Socket {pillar.PillarName} Energized! [{currentInput.Count}/{requiredSequence.Length}]</color>", 2.5f);

                if (currentInput.Count == requiredSequence.Length)
                {
                    SolveVault();
                }
            }
            else
            {
                // Wrong sequence
                TemporalHUD.Instance?.ShowNotification("<color=#FF5555><b>TEMPORAL DISSONANCE!</b> Sequence incorrect — Sockets resetting...\n<i>Hint: Find the decryption sequence in the <b>Future [3]</b> memory terminal!</i></color>", 4.0f);
                ResetAllPillars();
            }
        }

        private void SolveVault()
        {
            isSolved = true;
            TimeInventory.VaultDoorOpened = true;
            if (doorCollider != null) doorCollider.enabled = false;

            if (vaultLockLight != null)
            {
                vaultLockLight.color = new Color(0.2f, 1f, 0.4f, 1f);
                vaultLockLight.pointLightOuterRadius = 6.0f;
                vaultLockLight.intensity = 3.0f;
            }

            TemporalHUD.Instance?.ShowNotification("<color=#00FFFF><b>CHRONO VAULT UNLOCKED!</b>\nBoth Past conduit alignment and Future cipher sequence satisfied! Claim the Master Timeline Core!</color>", 6.0f);
            TemporalHUD.Instance?.UpdateDisplay();
        }

        public void ResetAllPillars()
        {
            currentInput.Clear();
            var pillars = FindObjectsByType<TemporalRunePillar>(FindObjectsSortMode.None);
            foreach (var p in pillars)
            {
                p.ResetPillar();
            }
        }

        public bool IsSolved => isSolved;
    }
}
