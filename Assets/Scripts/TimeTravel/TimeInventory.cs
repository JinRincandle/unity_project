using UnityEngine;

namespace TimeTravel
{
    public enum Era
    {
        Present,
        Future,
        Past
    }

    public enum TimelineSpawnPoint
    {
        DefaultStart,
        Present_FromFuturePortal,
        Present_FromPastPortal,
        Future_PortalArrival,
        Past_RiftArrival
    }

    public static class TimeInventory
    {
        // === Core Collectibles & Resources ===
        public static bool HasPresentOrb { get; set; } = false;
        public static bool HasFutureOrb { get; set; } = false;
        public static bool HasPastOrb { get; set; } = false;
        public static int PastOrbCount { get; set; } = 0;

        // === Persistent Player State Across Timelines ===
        public static float SavedPlayerHealth { get; set; } = -1f;
        public static TimelineSpawnPoint TargetSpawn { get; set; } = TimelineSpawnPoint.DefaultStart;

        // === Puzzle & World Progression Flags ===
        public static bool DiscoveredFutureCode { get; set; } = false;
        public static bool DiscoveredPastCode { get; set; } = false;
        public static bool ConduitsAlignedInPast { get; set; } = false;
        public static bool VaultDoorOpened { get; set; } = false;
        public static bool FutureBossDefeated { get; set; } = false;
        public static bool ChronoShieldOverloaded { get; set; } = false;
        public static int[] ConduitAngles { get; set; } = new int[] { 90, 180, 270 };
        public static Era CurrentEra { get; set; } = Era.Present;

        // === Timeline Travel & Distortion Mechanics ===
        public static int TimelineTravelCount { get; set; } = 0;
        public static bool JustReturnedToPresent { get; set; } = false;
        public static string DiscoveredSequenceText { get; set; } = "ABYSS (3) -> CHRONOS (1) -> SOL (2)";

        public static void ResetInventory()
        {
            HasPresentOrb = false;
            HasFutureOrb = false;
            HasPastOrb = false;
            PastOrbCount = 0;
            SavedPlayerHealth = -1f;
            TargetSpawn = TimelineSpawnPoint.DefaultStart;
            DiscoveredFutureCode = false;
            DiscoveredPastCode = false;
            ConduitsAlignedInPast = false;
            VaultDoorOpened = false;
            FutureBossDefeated = false;
            ChronoShieldOverloaded = false;
            ConduitAngles = new int[] { 90, 180, 270 };
            CurrentEra = Era.Present;
            TimelineTravelCount = 0;
            JustReturnedToPresent = false;
        }

        public static Vector3 GetSpawnCoordinates(Era era, TimelineSpawnPoint spawn)
        {
            if (era == Era.Present)
            {
                switch (spawn)
                {
                    case TimelineSpawnPoint.Present_FromFuturePortal:
                        return new Vector3(-2.50f, 15.20f, 0f); // Zone 3 Roof beside Future Portal
                    case TimelineSpawnPoint.Present_FromPastPortal:
                        return new Vector3(-25.00f, 5.70f, 0f); // Zone 1 Plateau beside Past Portal
                    default:
                        return Vector3.zero; // Use scene placed position (-32.0, 4.8)
                }
            }
            else if (era == Era.Future)
            {
                switch (spawn)
                {
                    case TimelineSpawnPoint.Future_PortalArrival:
                        return new Vector3(-6.00f, 1.70f, 0f); // Beside ChronoPortal_ToPresent
                    default:
                        return Vector3.zero;
                }
            }
            else // Past
            {
                switch (spawn)
                {
                    case TimelineSpawnPoint.Past_RiftArrival:
                        return new Vector3(-33.00f, 3.68f, 0f); // Stepping out of Start_TemporalRiftArch
                    default:
                        return Vector3.zero;
                }
            }
        }
    }
}
