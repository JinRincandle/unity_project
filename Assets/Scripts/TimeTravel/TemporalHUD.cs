using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TimeTravel
{
    public class TemporalHUD : MonoBehaviour
    {
        public static TemporalHUD Instance { get; private set; }

        [Header("UI Text References")]
        [SerializeField] private TextMeshProUGUI eraTitleText;
        [SerializeField] private TextMeshProUGUI objectiveText;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private TextMeshProUGUI presentOrbText;
        [SerializeField] private TextMeshProUGUI futureOrbText;

        [Header("Current Era")]
        [SerializeField] private Era sceneEra = Era.Present;

        private float notifTimer = 0f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            if (promptText != null) promptText.gameObject.SetActive(false);
            if (notificationText != null) notificationText.gameObject.SetActive(false);

            TimeInventory.CurrentEra = sceneEra;
            UpdateDisplay();
        }

        private void Update()
        {
            if (notifTimer > 0f)
            {
                notifTimer -= Time.deltaTime;
                if (notifTimer <= 0f && notificationText != null)
                {
                    notificationText.gameObject.SetActive(false);
                }
            }
        }

        public void ShowPrompt(string message)
        {
            if (promptText != null)
            {
                promptText.text = message;
                promptText.gameObject.SetActive(true);
            }
        }

        public void HidePrompt()
        {
            if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
        }

        public void ShowNotification(string message, float duration = 3.0f)
        {
            if (notificationText != null)
            {
                notificationText.text = message;
                notificationText.gameObject.SetActive(true);
                notifTimer = duration;
            }
        }

        [SerializeField] private TextMeshProUGUI pastOrbText;

        public void UpdateDisplay()
        {
            if (eraTitleText != null)
            {
                if (sceneEra == Era.Present)
                {
                    eraTitleText.text = "<b>ERA: PRESENT (2026 AD)</b>  |  [E] Attack  |  [R] Ranged  |  <b>[F] Interact</b>  |  <b>[1] Past  [2] Present  [3] Future</b>";
                }
                else if (sceneEra == Era.Past)
                {
                    eraTitleText.text = "<b>ERA: PAST (VOLCANIC CRUCIBLE)</b>  |  [E] Attack  |  [R] Ranged  |  <b>[F] Interact</b>  |  <b>[1] Past  [2] Present  [3] Future</b>";
                }
                else
                {
                    eraTitleText.text = "<b>ERA: FUTURE (ABYSSAL RUINS)</b>  |  [E] Attack  |  [R] Ranged  |  <b>[F] Interact</b>  |  <b>[1] Past  [2] Present  [3] Future</b>";
                }
            }

            if (presentOrbText != null)
            {
                presentOrbText.text = TimeInventory.HasPresentOrb 
                    ? "<color=#00FFFF>◆ PRESENT ORB: [1/1]</color>" 
                    : "<color=#556677>◇ PRESENT ORB: [0/1]</color>";
            }

            if (futureOrbText != null)
            {
                string pastStr = TimeInventory.HasPastOrb 
                    ? $"<color=#FFA500>◆ PAST ORB: [{TimeInventory.PastOrbCount}]</color>" 
                    : "<color=#556677>◇ PAST ORB: [0]</color>";
                
                string futStr = TimeInventory.HasFutureOrb 
                    ? "<color=#C080FF>◆ FUTURE ORB: [1/1]</color>" 
                    : "<color=#556677>◇ FUTURE ORB: [0/1]</color>";

                if (pastOrbText != null)
                {
                    futureOrbText.text = futStr;
                    pastOrbText.text = pastStr;
                }
                else
                {
                    futureOrbText.text = futStr + "  " + pastStr;
                }
            }

            UpdateObjectives();
        }

        public void UpdateObjectives()
        {
            if (objectiveText == null) return;

            if (sceneEra == Era.Present)
            {
                bool isSolved = TemporalVaultPuzzle.Instance != null && TemporalVaultPuzzle.Instance.IsSolved;

                string obj1 = TimeInventory.HasFutureOrb 
                    ? "<color=#00FF88>✓ West Wing Chrono Guardian Defeated -> Future Orb Claimed</color>"
                    : "<color=#FFFFFF>• Defeat the Chrono Guardian in West Wing to claim Future Orb</color>";

                string obj2;
                if (!TimeInventory.HasFutureOrb)
                {
                    obj2 = "<color=#888888>• Time Shift to Future [3] (Requires Future Orb)</color>";
                }
                else if (TimeInventory.DiscoveredFutureCode)
                {
                    obj2 = "<color=#00FF88>✓ Future Memory Archive Decrypted! Sequence: [3 -> 1 -> 2]</color>";
                }
                else
                {
                    obj2 = "<color=#C080FF>• Press [3] for Future Era -> Discover Memory Terminal & Catacomb Past Orb</color>";
                }

                string obj3;
                if (!TimeInventory.HasPastOrb)
                {
                    obj3 = "<color=#888888>• Time Shift to Past [1] (Requires Past Orb from Future Catacombs)</color>";
                }
                else if (TimeInventory.ConduitsAlignedInPast)
                {
                    obj3 = "<color=#00FF88>✓ Past Construction: Conduits Aligned! Power Flow Active</color>";
                }
                else
                {
                    obj3 = "<color=#FFA500>• Press [1] for Past Era -> Align 3 Conduits at the Construction Site</color>";
                }

                string obj4;
                if (isSolved)
                {
                    obj4 = "<color=#00FFFF>★ GREAT VAULT OPENED! Claim the Master Timeline Core!</color>";
                }
                else if (TimeInventory.ConduitsAlignedInPast && TimeInventory.DiscoveredFutureCode)
                {
                    obj4 = "<color=#FFFF00>★ READY! Energize Sockets in Sequence: [3: Abyss -> 1: Chronos -> 2: Sol]</color>";
                }
                else
                {
                    obj4 = "<color=#FF6666>• Chrono Vault Locked: Requires Past Conduit Alignment & Future Sequence</color>";
                }

                objectiveText.text = "<b>CHRONO VAULT MISSIONS:</b>\n" + obj1 + "\n" + obj2 + "\n" + obj3 + "\n" + obj4;
            }
            else if (sceneEra == Era.Past)
            {
                string obj1 = TimeInventory.ConduitsAlignedInPast
                    ? "<color=#00FF88>✓ All 3 Conduits Aligned! Foundation Power Flowing!</color>"
                    : "<color=#FFA500>• Rotate the 3 Levers to align Energy Conduits to (0° / Target)</color>";

                string obj2 = TimeInventory.ConduitsAlignedInPast
                    ? "<color=#00FFFF>• Conduits Locked! Press [2] or enter Portal to Return to Present</color>"
                    : "<color=#FFFFFF>• Beware high volcanic ambient heat hazard!</color>";

                objectiveText.text = "<b>PAST ERA MISSIONS:</b>\n" + obj1 + "\n" + obj2;
            }
            else
            {
                string obj1 = TimeInventory.DiscoveredFutureCode
                    ? "<color=#00FF88>✓ Memory Terminal Decrypted: Sequence [3 -> 1 -> 2]</color>"
                    : "<color=#FFFFFF>• Search Dark Ruins with Lantern to find the Memory Terminal</color>";

                string obj2 = TimeInventory.HasPastOrb
                    ? "<color=#00FF88>✓ Past Ignis Core Claimed from Catacombs!</color>"
                    : "<color=#FFA500>• Explore Lower Catacombs to find the Past Orb</color>";

                string obj3 = TimeInventory.HasPresentOrb
                    ? "<color=#00FFFF>• Press [2] or Enter Portal to Return to Present</color>"
                    : "<color=#FF6666>• Claim the Present Orb on Upper Vault Platform to return</color>";

                string obj4 = TimeInventory.FutureBossDefeated
                    ? "<color=#00FF88>✓ Void Sentinel Defeated! Future Timeline Purged!</color>"
                    : (TimeInventory.DiscoveredFutureCode || TimeInventory.ChronoShieldOverloaded)
                        ? "<color=#FFA500>★ Chrono Barrier Collapsed! Defeat Kaelen in the Chrono Citadel (East)!</color>"
                        : "<color=#FF6666>• Challenge Kaelen the Void Sentinel in East Arena (Breach Shield with Twin Switches!)</color>";

                objectiveText.text = "<b>FUTURE ERA MISSIONS:</b>\n" + obj1 + "\n" + obj2 + "\n" + obj3 + "\n" + obj4;
            }
        }
    }
}
