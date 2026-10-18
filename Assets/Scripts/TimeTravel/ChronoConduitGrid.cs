using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TimeTravel
{
    public class ChronoConduitGrid : MonoBehaviour
    {
        public static ChronoConduitGrid Instance { get; private set; }

        [System.Serializable]
        public class ConduitNode
        {
            public string conduitName = "Conduit Alpha";
            public Transform conduitTransform;
            public Light2D conduitLight;
            public SpriteRenderer conduitRenderer;
            public int targetAngle = 0; // Target degrees for alignment (e.g. 0)
            [HideInInspector] public int currentAngle = 90;
        }

        [Header("Conduits")]
        [SerializeField] private ConduitNode[] conduits = new ConduitNode[3];

        [Header("Visual Feedback")]
        [SerializeField] private Color misalignedColor = new Color(0.9f, 0.4f, 0.1f, 1f); // Hot orange / unaligned
        [SerializeField] private Color alignedColor = new Color(0.1f, 0.9f, 1f, 1f);      // Vibrant cyan / power flowing
        [SerializeField] private Light2D masterGridLight;

        [Header("Sound")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip rotateClip;
        [SerializeField] private AudioClip alignedClip;

        private bool isFullyAligned = false;

        public bool IsFullyAligned => isFullyAligned;

        private void Awake()
        {
            Instance = this;
            if (audioSource == null) audioSource = GetComponent<AudioSource>();
            RestoreGridState();
        }

        private void Start()
        {
            RestoreGridState();
        }

        public void RestoreGridState()
        {
            // Restore from TimeInventory
            if (TimeInventory.ConduitAngles != null && TimeInventory.ConduitAngles.Length == conduits.Length)
            {
                for (int i = 0; i < conduits.Length; i++)
                {
                    conduits[i].currentAngle = TimeInventory.ConduitAngles[i];
                    if (conduits[i].conduitTransform != null)
                    {
                        conduits[i].conduitTransform.localEulerAngles = new Vector3(0, 0, conduits[i].currentAngle);
                    }
                }
            }

            CheckAlignment(silent: true);
        }

        public void RotateConduit(int index)
        {
            if (index < 0 || index >= conduits.Length) return;

            var node = conduits[index];
            node.currentAngle = (node.currentAngle + 90) % 360;

            if (node.conduitTransform != null)
            {
                node.conduitTransform.localEulerAngles = new Vector3(0, 0, node.currentAngle);
            }

            // Save to TimeInventory
            if (TimeInventory.ConduitAngles == null || TimeInventory.ConduitAngles.Length != conduits.Length)
            {
                TimeInventory.ConduitAngles = new int[conduits.Length];
            }
            TimeInventory.ConduitAngles[index] = node.currentAngle;

            if (audioSource != null && rotateClip != null)
            {
                audioSource.PlayOneShot(rotateClip);
            }

            CheckAlignment(silent: false);
        }

        public void CheckAlignment(bool silent = false)
        {
            bool allMatched = true;
            for (int i = 0; i < conduits.Length; i++)
            {
                var node = conduits[i];
                bool nodeAligned = (node.currentAngle == node.targetAngle);
                Color c = nodeAligned ? alignedColor : misalignedColor;

                if (node.conduitRenderer != null) node.conduitRenderer.color = c;
                if (node.conduitLight != null)
                {
                    node.conduitLight.color = c;
                    node.conduitLight.intensity = nodeAligned ? 2.5f : 1.2f;
                }

                if (!nodeAligned) allMatched = false;
            }

            isFullyAligned = allMatched;
            TimeInventory.ConduitsAlignedInPast = allMatched;

            if (masterGridLight != null)
            {
                masterGridLight.color = allMatched ? alignedColor : misalignedColor;
                masterGridLight.intensity = allMatched ? 3.5f : 1.5f;
            }

            if (!silent)
            {
                if (allMatched)
                {
                    if (audioSource != null && alignedClip != null) audioSource.PlayOneShot(alignedClip);
                    TemporalHUD.Instance?.ShowNotification(
                        "<color=#00FFFF><b>ENERGY CONDUIT GRID FULLY ALIGNED!</b>\nPower grid connected to the Vault foundation! This change echoes permanently into the Present.</color>",
                        5.0f
                    );
                }
                else
                {
                    int alignedCount = 0;
                    for (int i = 0; i < conduits.Length; i++)
                    {
                        if (conduits[i].currentAngle == conduits[i].targetAngle) alignedCount++;
                    }
                    TemporalHUD.Instance?.ShowNotification(
                        $"Conduit adjusted. [{alignedCount}/{conduits.Length}] Conduits connected to grid.",
                        2.0f
                    );
                }

                TemporalHUD.Instance?.UpdateDisplay();
            }
        }
    }
}
