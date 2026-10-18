using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace TimeTravel
{
    public class ChronoDoor : MonoBehaviour
    {
        [Header("Door Requirements")]
        [SerializeField] private int requiredSwitches = 1;
        private int activatedSwitches = 0;

        [Header("Door Motion")]
        [SerializeField] private Vector3 openOffset = new Vector3(0, 4.5f, 0);
        [SerializeField] private float openSpeed = 3.0f;
        [SerializeField] private Collider2D doorCollider;
        [SerializeField] private Light2D lockLight;

        private bool isOpen = false;
        private Vector3 closedPos;
        private Vector3 openPos;

        private void Awake()
        {
            closedPos = transform.position;
            openPos = closedPos + openOffset;
            if (doorCollider == null) doorCollider = GetComponent<Collider2D>();
            if (lockLight == null) lockLight = GetComponentInChildren<Light2D>();

            if (lockLight != null) lockLight.color = new Color(1f, 0.2f, 0.2f, 1f);
        }

        private void Update()
        {
            if (isOpen)
            {
                transform.position = Vector3.MoveTowards(transform.position, openPos, openSpeed * Time.deltaTime);
            }
        }

        public void RegisterSwitchActivated()
        {
            activatedSwitches++;
            if (activatedSwitches >= requiredSwitches && !isOpen)
            {
                OpenDoor();
            }
            else
            {
                TemporalHUD.Instance?.ShowNotification("Switch Activated! [" + activatedSwitches + "/" + requiredSwitches + "] locks released.", 2.5f);
            }
        }

        private void OpenDoor()
        {
            isOpen = true;
            if (doorCollider != null) doorCollider.enabled = false;
            if (lockLight != null)
            {
                lockLight.color = new Color(0.2f, 1f, 0.4f, 1f);
                lockLight.pointLightOuterRadius = 5.0f;
            }
            TemporalHUD.Instance?.ShowNotification("<color=#00FF88>TEMPORAL BARRIER UNLOCKED!</color>", 3.0f);
        }
    }
}
