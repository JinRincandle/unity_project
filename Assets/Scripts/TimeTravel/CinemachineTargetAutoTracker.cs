using UnityEngine;
using Unity.Cinemachine;

namespace TimeTravel
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CinemachineCamera))]
    public class CinemachineTargetAutoTracker : MonoBehaviour
    {
        [Tooltip("The tag of the player object to automatically follow.")]
        [SerializeField] private string targetTag = "Player";

        private CinemachineCamera vcam;

        private void Awake()
        {
            vcam = GetComponent<CinemachineCamera>();
            EnsureTrackingTarget();
        }

        private void Start()
        {
            EnsureTrackingTarget();
        }

        private void Update()
        {
            if (vcam == null) vcam = GetComponent<CinemachineCamera>();

            if (vcam != null && vcam.Target.TrackingTarget == null)
            {
                EnsureTrackingTarget();
            }
        }

        public void EnsureTrackingTarget()
        {
            if (vcam == null) vcam = GetComponent<CinemachineCamera>();
            if (vcam == null) return;

            if (vcam.Target.TrackingTarget == null)
            {
                var player = GameObject.FindWithTag(targetTag);
                if (player == null)
                {
                    var bbc = Object.FindAnyObjectByType<Blocks.Character.BuildingBlocksCharacter>();
                    if (bbc != null) player = bbc.gameObject;
                }

                if (player != null)
                {
                    vcam.Target.TrackingTarget = player.transform;
                    vcam.Target.LookAtTarget = player.transform;
                    vcam.Follow = player.transform;
                }
            }
        }
    }
}
