using UnityEngine;
using UnityEngine.Rendering.Universal;
using Blocks.Character;

namespace TimeTravel
{
    public class TimelineStabilizerCore : MonoBehaviour
    {
        [SerializeField] private Light2D coreLight;
        [SerializeField] private float hoverAmplitude = 0.25f;
        [SerializeField] private float hoverFrequency = 2.5f;

        private Vector3 startPos;
        private bool isClaimed = false;

        private void Awake()
        {
            startPos = transform.position;
            if (coreLight == null) coreLight = GetComponentInChildren<Light2D>();
        }

        private void Update()
        {
            if (isClaimed) return;
            transform.position = startPos + new Vector3(0, Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude, 0);

            if (coreLight != null)
            {
                coreLight.pointLightOuterRadius = 6.0f + Mathf.Sin(Time.time * 4f) * 0.8f;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (isClaimed) return;

            if (collision.CompareTag("Player") || collision.GetComponentInParent<Blocks.Character.BuildingBlocksCharacter>() != null)
            {
                Claim();
            }
        }

        public void Claim()
        {
            isClaimed = true;
            if (coreLight != null)
            {
                coreLight.color = new Color(0.2f, 1f, 0.8f, 1f);
                coreLight.pointLightOuterRadius = 16.0f;
                coreLight.intensity = 5.0f;
            }

            // Play Victory Stinger
            var vicClip = Resources.Load<AudioClip>("Stinger_Victory");
#if UNITY_EDITOR
            if (vicClip == null) vicClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/[BuildingBlocks]/Audio/SFX/Stinger_Victory.wav");
#endif
            if (vicClip != null)
            {
                AudioSource.PlayClipAtPoint(vicClip, transform.position, 1.5f);
            }

            // Heal time cracks
            TimeInventory.TimelineTravelCount = 0;
            if (TimelineDistortionManager.Instance != null)
            {
                TimelineDistortionManager.Instance.ApplyTimelineCracks();
            }

            // Notify player and player character victory
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                var charComp = player.GetComponent<BuildingBlocksCharacter>();
                if (charComp != null) charComp.NotifyVictory();
            }

            TemporalHUD.Instance?.ShowNotification(
                "<color=#FFD700><b>★ FRACTURES OF TIME: TIMELINE RESTORED! ★</b></color>\n<size=20>Past, Present, and Future are reunited in temporal harmony.\nYou have conquered the paradox and saved reality!</size>",
                12.0f
            );
            TemporalHUD.Instance?.UpdateDisplay();
        }
    }
}
