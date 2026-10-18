using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TimeTravel;

namespace FracturesOfTime.Editor
{
    public static class BuildExpandedPresentMap
    {
        private const string RootName = "[5X_EXPANDED_WORLD]";

        [MenuItem("FracturesOfTime/Build 5x Present Map (Photo Styled)")]
        public static void BuildMap()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            Debug.Log($"<color=#00FFAA><b>Beginning 5x Present Map Construction in: {activeScene.name}</b></color>");

            // 1. Load Sprite Assets
            Sprite sprSolidGround = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/solid-ground.png");
            Sprite sprColumn = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/column.png");
            Sprite sprRockBig = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/rockblock-big.png");
            Sprite sprRockBridge = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/rockblock-bridge.png");
            Sprite sprRockSlim = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/rockblock-slim.png");
            Sprite sprStairs = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/stairs-1.png");
            Sprite sprLadder = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/ladder.png");
            Sprite sprBgBrick = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/background/background_brick.png");
            Sprite sprBgColumn = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/background/background_column.png");
            Sprite sprAlienArch = LoadSprite("Assets/[BuildingBlocks]/Art/Environment/Textures/AlienSculpture01.png");
            Sprite sprAlienStatue4 = LoadSprite("Assets/[BuildingBlocks]/Art/Environment/Textures/AlienStatue04.png");
            Sprite sprAlienStatue3 = LoadSprite("Assets/[BuildingBlocks]/Art/Environment/Textures/AlienStatue03.png");
            Sprite sprMovingPlatform = LoadSprite("Assets/[BuildingBlocks]/Art/Environment/Textures/MovingPlatform.png");
            Sprite sprMist = LoadSprite("Assets/[BuildingBlocks]/Art/Environment/Textures/Mist.png");
            Sprite sprRoots = LoadSprite("Assets/[BuildingBlocks]/Art/Environment/Textures/UndergroundRoots.png");

            // Load Prefabs
            GameObject torchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GrottoEscape2/GrottoEscape2Artwork/Prefabs/AnimatedTorch.prefab");
            GameObject waterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GrottoEscape2/GrottoEscape2Artwork/Prefabs/AnimatedWater.prefab");
            GameObject meleeEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/[BuildingBlocks]/Prefabs/MeleeEnemy.prefab");
            GameObject grenadeEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/[BuildingBlocks]/Prefabs/GrenadeEnemy.prefab");

            // 2. Prepare or Create the Root Container
            GameObject existingRoot = GameObject.Find(RootName);
            if (existingRoot != null)
            {
                Undo.DestroyObjectImmediate(existingRoot);
            }

            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create 5x Expanded World");

            // Move EliminationZone down to Y = -35 so exploration down to -25 is safe
            GameObject elimZone = GameObject.Find("EliminationZone");
            if (elimZone != null)
            {
                Undo.RecordObject(elimZone.transform, "Adjust Elimination Zone");
                elimZone.transform.position = new Vector3(0, -35f, 0);
                var box = elimZone.GetComponent<BoxCollider2D>();
                if (box != null)
                {
                    Undo.RecordObject(box, "Adjust Elimination Box");
                    box.size = new Vector2(400f, 10f);
                    box.isTrigger = true;
                }
            }

            // -------------------------------------------------------------
            // ZONE 1: GRAND CENTRAL CHRONO PLAZA (Photo 1 Style)
            // Center: X in [-26, +26], Y in [-2, 14]
            // -------------------------------------------------------------
            GameObject zone1 = new GameObject("Zone1_GrandChronoPlaza_Photo1");
            zone1.transform.SetParent(root.transform);

            // Grand Floor
            CreateTiledPlatform(zone1, "Plaza_Floor", new Vector3(0, -0.75f, 0), new Vector2(54f, 1.5f), sprSolidGround, sprRockBig);

            // Grand Cathedral Stone Arches (Photo 1 Ribs)
            CreateSpriteObject(zone1, "Plaza_Arch_Left", new Vector3(-16f, 5.5f, 0), new Vector3(2.6f, 3.2f, 1f), sprAlienArch, "Background", -1, new Color(0.85f, 0.9f, 1f, 0.9f));
            CreateSpriteObject(zone1, "Plaza_Arch_Right", new Vector3(16f, 5.5f, 0), new Vector3(-2.6f, 3.2f, 1f), sprAlienArch, "Background", -1, new Color(0.85f, 0.9f, 1f, 0.9f));

            // Classical Columns flanking the plaza
            for (float x = -22f; x <= 22f; x += 11f)
            {
                if (Mathf.Abs(x) < 2f) continue;
                CreateSpriteObject(zone1, $"Plaza_Column_{x}", new Vector3(x, 4f, 0), new Vector3(2f, 3.5f, 1f), sprColumn, "Background", -2, new Color(0.7f, 0.75f, 0.85f, 0.8f));
                if (torchPrefab != null)
                {
                    var torch = (GameObject)PrefabUtility.InstantiatePrefab(torchPrefab, zone1.transform);
                    torch.transform.position = new Vector3(x + (x < 0 ? 0.6f : -0.6f), 3.5f, 0);
                }
            }

            // Suspended Chrono Sphere (Photo 1 Centerpiece)
            CreateSuspendedSphere(zone1, new Vector3(0, 8.5f, 0));

            // Drifting Mist under the plateau overhang and across the plaza (as seen in reference image)
            CreateSpriteObject(zone1, "Overhang_Drifting_Mist", new Vector3(-8.89f, -0.5f, 0), new Vector3(10f, 2.2f, 1f), sprMist, "Foreground", 1, new Color(0.75f, 0.9f, 1f, 0.55f));
            CreateSpriteObject(zone1, "Plaza_Mist_Right", new Vector3(24f, -1.5f, 0), new Vector3(12f, 2.2f, 1f), sprMist, "Foreground", 1, new Color(0.75f, 0.9f, 1f, 0.55f));

            // Ancient Rubble and Statues on Spawn Plateau (matching reference image)
            Sprite sprRubble = LoadSprite("Assets/[BuildingBlocks]/Art/Environment/Textures/Rubble.png");
            if (sprRubble != null)
            {
                CreateSpriteObject(zone1, "Spawn_Ancient_Rubble", new Vector3(-18f, 1.2f, 0), new Vector3(1.8f, 1.8f, 1f), sprRubble, "Main", 1, Color.white);
            }
            CreateSpriteObject(zone1, "Spawn_Shrine_Statue_Left", new Vector3(-12f, 1.8f, 0), new Vector3(1.6f, 1.6f, 1f), sprAlienStatue4, "Main", 1, new Color(0.3f, 1f, 0.8f));
            CreateSpriteObject(zone1, "Spawn_Shrine_Statue_Right", new Vector3(12f, 1.8f, 0), new Vector3(1.6f, 1.6f, 1f), sprAlienStatue3, "Main", 1, new Color(1f, 0.85f, 0.3f));

            // Re-anchor Portals & Player in Plaza
            AnchorObject("ChronoPortal_ToPast", new Vector3(-15f, 0.5f, 0));
            AnchorObject("ChronoPortal_ToFuture", new Vector3(15f, 0.5f, 0));
            AnchorObject("Player", new Vector3(0, 1.2f, 0));

            // -------------------------------------------------------------
            // ZONE 2: WEST UPPER WING - RUNE SANCTUARY (Photo 3 Style)
            // Multi-tier terraces: X in [-115, -28], Y in [6, 24]
            // -------------------------------------------------------------
            GameObject zone2 = new GameObject("Zone2_RuneSanctuary_Photo3");
            zone2.transform.SetParent(root.transform);

            // Terrace Level 1 (Y = 6.5)
            CreateTiledPlatform(zone2, "Terrace_L1", new Vector3(-55f, 6.5f, 0), new Vector2(50f, 1.2f), sprSolidGround, sprRockBridge);
            // Terrace Level 2 (Y = 12.5)
            CreateTiledPlatform(zone2, "Terrace_L2", new Vector3(-75f, 12.5f, 0), new Vector2(45f, 1.2f), sprSolidGround, sprRockBridge);
            // Terrace Level 3 / High Overlook (Y = 18.5)
            CreateTiledPlatform(zone2, "Terrace_L3", new Vector3(-95f, 18.5f, 0), new Vector2(35f, 1.2f), sprSolidGround, sprRockBridge);

            // Connectors: Stairs & Ladders
            CreateSpriteObject(zone2, "Stairs_Plaza_To_L1", new Vector3(-28f, 3.5f, 0), new Vector3(3f, 3f, 1f), sprStairs, "Main", 0, Color.white);
            CreateTiledPlatform(zone2, "Incline_Step_1", new Vector3(-30f, 3.5f, 0), new Vector2(6f, 1f), sprSolidGround, sprRockBig);
            CreateSpriteObject(zone2, "Ladder_L1_To_L2", new Vector3(-58f, 9.5f, 0), new Vector3(1.5f, 5f, 1f), sprLadder, "Main", 1, Color.white);
            CreateSpriteObject(zone2, "Ladder_L2_To_L3", new Vector3(-78f, 15.5f, 0), new Vector3(1.5f, 5f, 1f), sprLadder, "Main", 1, Color.white);

            // Background ancient brick wall across Sanctuary
            for (float bx = -110f; bx <= -32f; bx += 8f)
            {
                CreateSpriteObject(zone2, $"Sanctuary_BgBrick_{bx}", new Vector3(bx, 14f, 0), new Vector3(5f, 14f, 1f), sprBgBrick, "Background", -4, new Color(0.45f, 0.48f, 0.55f, 0.6f));
            }

            // 3 Massive Etched Monolithic Rune Columns (Photo 3)
            CreateEtchedMonolith(zone2, "RuneMonolith_I_Chronos", new Vector3(-88f, 14.5f, 0), "I : CHRONOS", new Color(0.2f, 0.9f, 1f), sprColumn, sprAlienStatue4);
            CreateEtchedMonolith(zone2, "RuneMonolith_II_Sol", new Vector3(-68f, 14.5f, 0), "II : SOL", new Color(1f, 0.75f, 0.2f), sprColumn, sprAlienStatue3);
            CreateEtchedMonolith(zone2, "RuneMonolith_III_Abyss", new Vector3(-48f, 8.5f, 0), "III : ABYSS", new Color(0.85f, 0.3f, 1f), sprColumn, sprAlienStatue4);

            // Add Torches along the terraces
            if (torchPrefab != null)
            {
                InstantiatePrefab(torchPrefab, zone2.transform, new Vector3(-40f, 8f, 0));
                InstantiatePrefab(torchPrefab, zone2.transform, new Vector3(-60f, 14f, 0));
                InstantiatePrefab(torchPrefab, zone2.transform, new Vector3(-80f, 20f, 0));
                InstantiatePrefab(torchPrefab, zone2.transform, new Vector3(-105f, 20f, 0));
            }

            // Move the old Rune Clue Wall into this sanctuary as reference
            GameObject oldClueWall = GameObject.Find("[Rune_Clue_Wall]");
            if (oldClueWall != null)
            {
                Undo.RecordObject(oldClueWall.transform, "Relocate Rune Clue Wall");
                oldClueWall.transform.position = new Vector3(-68f, 14f, 0);
            }

            // -------------------------------------------------------------
            // ZONE 3: WEST LOWER CATACOMBS & GUARDIAN LAIR (Inspiration: Under-Spawn Dungeon)
            // Carved directly under the plateau: X in [-115, -28], Y in [-16, 2]
            // -------------------------------------------------------------
            GameObject zone3 = new GameObject("Zone3_WestCatacombs_And_BossLair");
            zone3.transform.SetParent(root.transform);

            // Cavern Floor
            CreateTiledPlatform(zone3, "Catacombs_Floor", new Vector3(-70f, -13f, 0), new Vector2(85f, 2f), sprRockBig, sprRockBig);
            // Intermediate Ledges & Stepping Stones
            CreateTiledPlatform(zone3, "Catacombs_Ledge_A", new Vector3(-38f, -6f, 0), new Vector2(14f, 1.2f), sprRockSlim, sprRockBridge);
            CreateTiledPlatform(zone3, "Catacombs_Ledge_B", new Vector3(-58f, -9f, 0), new Vector2(16f, 1.2f), sprRockSlim, sprRockBridge);

            // Two Glowing Purple Shrines in the Under-Spawn Cavern (from reference image)
            var shrine1 = CreateSpriteObject(zone3, "Cavern_Purple_Shrine_1", new Vector3(-75f, -11.5f, 0), new Vector3(2f, 2f, 1f), sprAlienStatue3, "Main", 0, new Color(0.9f, 0.2f, 1f));
            CreatePointLight(shrine1, "Shrine1_Light", new Vector3(-75f, -10.5f, 0), new Color(0.9f, 0.2f, 1f), 8f, 2.5f);

            var shrine2 = CreateSpriteObject(zone3, "Cavern_Purple_Shrine_2", new Vector3(-60f, -11.5f, 0), new Vector3(2f, 2f, 1f), sprAlienStatue3, "Main", 0, new Color(0.9f, 0.2f, 1f));
            CreatePointLight(shrine2, "Shrine2_Light", new Vector3(-60f, -10.5f, 0), new Color(0.9f, 0.2f, 1f), 8f, 2.5f);

            // Sunken Ancient Temple Structure embedded in subterranean rock (from reference image)
            CreateSpriteObject(zone3, "Sunken_Temple_Relic", new Vector3(-68f, -18f, 0), new Vector3(3f, 3.5f, 1f), sprAlienArch, "Background", -2, new Color(0.75f, 0.7f, 0.65f, 0.9f));

            // Deep Subterranean Twisted Roots (from reference image)
            CreateSpriteObject(zone3, "Deep_Cavern_Roots_1", new Vector3(-85f, -17f, 0), new Vector3(4f, 4f, 1f), sprRoots, "Background", -1, new Color(0.6f, 0.55f, 0.5f, 0.9f));
            CreateSpriteObject(zone3, "Deep_Cavern_Roots_2", new Vector3(-50f, -17f, 0), new Vector3(4f, 4f, 1f), sprRoots, "Background", -1, new Color(0.6f, 0.55f, 0.5f, 0.9f));

            // Rocky Stalactites & Ruins
            for (float rx = -110f; rx <= -35f; rx += 14f)
            {
                CreateSpriteObject(zone3, $"Catacomb_Ruins_{rx}", new Vector3(rx, -10f, 0), new Vector3(2.5f, 3.5f, 1f), sprRockBig, "Background", -1, new Color(0.6f, 0.6f, 0.7f, 0.85f));
            }

            // Re-anchor WolfBossEnemy and FutureOrb_Pickup in the Boss Lair
            AnchorObject("WolfBossEnemy", new Vector3(-98f, -11.5f, 0));
            AnchorObject("FutureOrb_Pickup", new Vector3(-108f, -11.5f, 0));

            // Hanging roots
            CreateSpriteObject(zone3, "Cavern_Roots_1", new Vector3(-85f, -7f, 0), new Vector3(3f, 3f, 1f), sprRoots, "Background", 0, new Color(0.7f, 0.65f, 0.6f, 0.9f));
            CreateSpriteObject(zone3, "Cavern_Roots_2", new Vector3(-102f, -7f, 0), new Vector3(3f, 3f, 1f), sprRoots, "Background", 0, new Color(0.7f, 0.65f, 0.6f, 0.9f));

            // -------------------------------------------------------------
            // ZONE 4: EAST UPPER WING - CONDUIT MACHINE FACILITY (Photo 2 Style)
            // Massive looped energy pipes, reactor core, catwalks: X in [30, 115], Y in [6, 24]
            // -------------------------------------------------------------
            GameObject zone4 = new GameObject("Zone4_ConduitMachineFacility_Photo2");
            zone4.transform.SetParent(root.transform);

            // Machine Catwalks
            CreateTiledPlatform(zone4, "Machine_Deck_L1", new Vector3(55f, 6.5f, 0), new Vector2(45f, 1.2f), sprSolidGround, sprRockSlim);
            CreateTiledPlatform(zone4, "Machine_Deck_L2", new Vector3(78f, 12.5f, 0), new Vector2(42f, 1.2f), sprSolidGround, sprRockSlim);
            CreateTiledPlatform(zone4, "Machine_Deck_L3_Reactor", new Vector3(98f, 18.5f, 0), new Vector2(32f, 1.2f), sprSolidGround, sprRockSlim);

            // Moving Platforms bridging gaps
            CreateSpriteObject(zone4, "MovingPlatform_Machine1", new Vector3(42f, 9.5f, 0), new Vector3(1.2f, 1f, 1f), sprMovingPlatform, "Main", 0, Color.cyan);
            CreateSpriteObject(zone4, "MovingPlatform_Machine2", new Vector3(68f, 15.5f, 0), new Vector3(1.2f, 1f, 1f), sprMovingPlatform, "Main", 0, Color.cyan);

            // Massive Curved Industrial Pipework & Fluid Conduits (Photo 2)
            CreateConduitPipingSystem(zone4, new Vector3(75f, 15f, 0));

            // Wall Bricks for High-Tech Facility Background
            for (float bx = 35f; bx <= 110f; bx += 10f)
            {
                CreateSpriteObject(zone4, $"Facility_Bg_{bx}", new Vector3(bx, 14f, 0), new Vector3(6f, 14f, 1f), sprBgBrick, "Background", -4, new Color(0.35f, 0.42f, 0.5f, 0.65f));
            }

            // -------------------------------------------------------------
            // ZONE 5: EAST LOWER WING - CHRONO VAULT & INNER SANCTUM (Photo 4 Style)
            // X in [26, 125], Y in [-2, 12]
            // -------------------------------------------------------------
            GameObject zone5 = new GameObject("Zone5_VaultCourtyard_And_Sanctum_Photo4");
            zone5.transform.SetParent(root.transform);

            // Grand Vault Avenue Floor connecting from Central Plaza to the Vault Door
            CreateTiledPlatform(zone5, "Vault_Avenue_Floor", new Vector3(56f, -0.75f, 0), new Vector2(58f, 1.5f), sprSolidGround, sprRockBig);

            // Inner Sanctum Floor (Behind Vault Door: X in [84, 122])
            CreateTiledPlatform(zone5, "InnerSanctum_Floor", new Vector3(103f, -0.75f, 0), new Vector2(38f, 1.5f), sprSolidGround, sprRockBig);

            // Vault Corridor Decorative Columns and Torches
            for (float x = 32f; x <= 80f; x += 12f)
            {
                CreateSpriteObject(zone5, $"Avenue_Column_{x}", new Vector3(x, 4f, 0), new Vector3(2f, 3.5f, 1f), sprColumn, "Background", -2, new Color(0.8f, 0.82f, 0.9f, 0.8f));
                if (torchPrefab != null)
                {
                    InstantiatePrefab(torchPrefab, zone5.transform, new Vector3(x + 0.5f, 3.5f, 0));
                }
            }

            // Re-anchor Chrono Vault Puzzle Objects in Zone 5
            AnchorObject("ChronoControlPanel", new Vector3(64f, 0.5f, 0));
            AnchorObject("RunePillar_1_Chronos (I)", new Vector3(70f, 0.5f, 0));
            AnchorObject("RunePillar_2_Sol (II)", new Vector3(74f, 0.5f, 0));
            AnchorObject("RunePillar_3_Abyss (III)", new Vector3(78f, 0.5f, 0));
            AnchorObject("ChronoVaultDoor", new Vector3(84f, 2.5f, 0));
            AnchorObject("MasterTimelineCore", new Vector3(105f, 2.5f, 0));

            // Inner Sanctum Enclosure Walls (Back wall & Ceiling)
            CreateTiledPlatform(zone5, "Sanctum_BackWall", new Vector3(122f, 5f, 0), new Vector2(2f, 12f), sprSolidGround, sprRockBig);
            CreateTiledPlatform(zone5, "Sanctum_Ceiling", new Vector3(103f, 10f, 0), new Vector2(40f, 1.5f), sprSolidGround, sprRockBig);

            // Inner Sanctum Statues & Grandeur (Flanking MasterTimelineCore)
            CreateSpriteObject(zone5, "Sanctum_Statue_Left", new Vector3(95f, 3.5f, 0), new Vector3(2.5f, 2.5f, 1f), sprAlienStatue4, "Background", -1, Color.white);
            CreateSpriteObject(zone5, "Sanctum_Statue_Right", new Vector3(115f, 3.5f, 0), new Vector3(-2.5f, 2.5f, 1f), sprAlienStatue3, "Background", -1, Color.white);

            // Sanctum Golden Glow
            CreatePointLight(zone5, "Sanctum_Divine_Light", new Vector3(105f, 4f, 0), new Color(1f, 0.9f, 0.5f), 10f, 2.8f);

            // -------------------------------------------------------------
            // ZONE 6: SUBTERRANEAN AQUEDUCT & DRAINAGE (Connecting Depths)
            // X in [-70, 70], Y in [-24, -8]
            // -------------------------------------------------------------
            GameObject zone6 = new GameObject("Zone6_SubterraneanAqueduct");
            zone6.transform.SetParent(root.transform);

            // Tilted Ancient Monument half-buried in the soil (inspired by reference image)
            var buriedRelic = CreateSpriteObject(zone4, "Tilted_Buried_Monument", new Vector3(45f, 2.5f, 0), new Vector3(2.5f, 3.5f, 1f), sprAlienArch, "Main", 0, new Color(0.9f, 0.85f, 0.8f));
            buriedRelic.transform.rotation = Quaternion.Euler(0, 0, -32f);

            // Swirling mist across the buried relic (from reference image)
            CreateSpriteObject(zone4, "Mound_Swirling_Mist", new Vector3(45f, 2.0f, 0), new Vector3(10f, 2.5f, 1f), sprMist, "Foreground", 1, new Color(0.75f, 0.9f, 1f, 0.5f));

            // Deep Bedrock Floor
            CreateTiledPlatform(zone6, "Aqueduct_Bedrock", new Vector3(25f, -22f, 0), new Vector2(180f, 2f), sprRockBig, sprRockBig);

            // Animated Water channels & deep sea basin across the floor (matching reference image)
            if (waterPrefab != null)
            {
                for (float wx = -50f; wx <= 115f; wx += 18f)
                {
                    var w = InstantiatePrefab(waterPrefab, zone6.transform, new Vector3(wx, -20.5f, 0));
                    w.transform.localScale = new Vector3(4.5f, 1.2f, 1f);
                }
            }

            // Stepping Stone Platforms across the water
            for (float px = -40f; px <= 40f; px += 16f)
            {
                CreateTiledPlatform(zone6, $"Water_SteppingStone_{px}", new Vector3(px, -17f, 0), new Vector2(6f, 1f), sprRockSlim, sprRockBridge);
                if (torchPrefab != null)
                {
                    InstantiatePrefab(torchPrefab, zone6.transform, new Vector3(px, -15.8f, 0));
                }
            }

            // -------------------------------------------------------------
            // 2D SMALL ENEMIES PATROL SQUADS
            // -------------------------------------------------------------
            GameObject enemiesRoot = new GameObject("Small_Enemies_Patrols");
            enemiesRoot.transform.SetParent(root.transform);

            if (meleeEnemyPrefab != null)
            {
                // Rune Sanctuary Patrols
                SpawnPatrolEnemy(enemiesRoot, meleeEnemyPrefab, new Vector3(-45f, 7.8f, 0), "Rune_Terrace_Melee_1");
                SpawnPatrolEnemy(enemiesRoot, meleeEnemyPrefab, new Vector3(-72f, 13.8f, 0), "Rune_Terrace_Melee_2");
                // West Catacombs Patrols
                SpawnPatrolEnemy(enemiesRoot, meleeEnemyPrefab, new Vector3(-48f, -11.8f, 0), "Catacomb_Melee_1");
                SpawnPatrolEnemy(enemiesRoot, meleeEnemyPrefab, new Vector3(-80f, -11.8f, 0), "Catacomb_Melee_2");
                // Conduit Machine Facility Patrols
                SpawnPatrolEnemy(enemiesRoot, meleeEnemyPrefab, new Vector3(50f, 7.8f, 0), "Facility_Melee_1");
                SpawnPatrolEnemy(enemiesRoot, meleeEnemyPrefab, new Vector3(82f, 13.8f, 0), "Facility_Melee_2");
                // Vault Approach Guard
                SpawnPatrolEnemy(enemiesRoot, meleeEnemyPrefab, new Vector3(45f, 0.8f, 0), "Vault_Guard_Melee");
            }

            if (grenadeEnemyPrefab != null)
            {
                // Rune Sanctuary High Balcony
                SpawnPatrolEnemy(enemiesRoot, grenadeEnemyPrefab, new Vector3(-92f, 19.8f, 0), "Rune_HighBalcony_Grenade");
                // West Catacombs Cavern Ledge
                SpawnPatrolEnemy(enemiesRoot, grenadeEnemyPrefab, new Vector3(-62f, -8f, 0), "Catacomb_Ledge_Grenade");
                // Conduit Machine Upper Catwalk
                SpawnPatrolEnemy(enemiesRoot, grenadeEnemyPrefab, new Vector3(95f, 19.8f, 0), "Facility_Reactor_Grenade");
                // Vault Antechamber Guard
                SpawnPatrolEnemy(enemiesRoot, grenadeEnemyPrefab, new Vector3(58f, 0.8f, 0), "Vault_Antechamber_Grenade");
            }

            // Mark Scene Dirty so everything is saved
            EditorSceneManager.MarkSceneDirty(activeScene);
            Debug.Log("<color=#00FFAA><b>5X EXPANDED PRESENT MAP SUCCESSFULLY GENERATED!</b></color>");
        }

        // =============================================================
        // HELPER BUILDER METHODS
        // =============================================================

        private static Sprite LoadSprite(string path)
        {
            var spr = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (spr == null)
            {
                var all = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (var a in all)
                {
                    if (a is Sprite s) return s;
                }
            }
            return spr;
        }

        private static GameObject CreateSpriteObject(GameObject parent, string name, Vector3 pos, Vector3 scale, Sprite sprite, string sortingLayer, int order, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.position = pos;
            go.transform.localScale = scale;

            if (sprite != null)
            {
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingLayerName = sortingLayer;
                sr.sortingOrder = order;
                sr.color = color;
            }
            return go;
        }

        private static GameObject CreateTiledPlatform(GameObject parent, string name, Vector3 pos, Vector2 size, Sprite surfaceSpr, Sprite bodySpr)
        {
            GameObject platform = new GameObject(name);
            platform.transform.SetParent(parent.transform);
            platform.transform.position = pos;

            var col = platform.AddComponent<BoxCollider2D>();
            col.size = size;

            // Visual Representation
            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(platform.transform);
            visual.transform.localPosition = new Vector3(-size.x * 0.5f, size.y * 0.5f, 0);
            visual.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = surfaceSpr != null ? surfaceSpr : bodySpr;
            sr.drawMode = SpriteDrawMode.Simple;
            sr.sortingLayerName = "Main";
            sr.sortingOrder = 0;
            sr.color = new Color(0.9f, 0.92f, 0.95f, 1f);

            return platform;
        }

        private static void CreateSuspendedSphere(GameObject parent, Vector3 center)
        {
            GameObject sphereRoot = new GameObject("Suspended_Chrono_Sphere");
            sphereRoot.transform.SetParent(parent.transform);
            sphereRoot.transform.position = center;

            // Suspension Cables hanging from the vault roof
            GameObject cableLeft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cableLeft.name = "Suspension_Cable_Left";
            cableLeft.transform.SetParent(sphereRoot.transform);
            cableLeft.transform.position = center + new Vector3(-2f, 4.5f, 0);
            cableLeft.transform.localScale = new Vector3(0.08f, 4.5f, 0.08f);
            Object.DestroyImmediate(cableLeft.GetComponent<Collider>());

            GameObject cableRight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cableRight.name = "Suspension_Cable_Right";
            cableRight.transform.SetParent(sphereRoot.transform);
            cableRight.transform.position = center + new Vector3(2f, 4.5f, 0);
            cableRight.transform.localScale = new Vector3(0.08f, 4.5f, 0.08f);
            Object.DestroyImmediate(cableRight.GetComponent<Collider>());

            // Glowing Chrono Core (Nested Spheres)
            GameObject outerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            outerSphere.name = "Chrono_Outer_Gimbal";
            outerSphere.transform.SetParent(sphereRoot.transform);
            outerSphere.transform.position = center;
            outerSphere.transform.localScale = new Vector3(2.8f, 2.8f, 0.2f);
            Object.DestroyImmediate(outerSphere.GetComponent<Collider>());

            GameObject innerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            innerSphere.name = "Chrono_Core";
            innerSphere.transform.SetParent(sphereRoot.transform);
            innerSphere.transform.position = center;
            innerSphere.transform.localScale = new Vector3(1.6f, 1.6f, 0.2f);
            Object.DestroyImmediate(innerSphere.GetComponent<Collider>());

            // 2D Light casting celestial radiance
            CreatePointLight(sphereRoot, "Sphere_Light_Cyan", center, new Color(0.2f, 0.9f, 1f), 12f, 3.2f);
            CreatePointLight(sphereRoot, "Sphere_Light_Amber", center + new Vector3(0, -0.5f, 0), new Color(1f, 0.7f, 0.2f), 8f, 1.8f);
        }

        private static void CreateEtchedMonolith(GameObject parent, string name, Vector3 pos, string label, Color runeColor, Sprite colSpr, Sprite statueSpr)
        {
            GameObject monolith = new GameObject(name);
            monolith.transform.SetParent(parent.transform);
            monolith.transform.position = pos;

            // Pillar column
            CreateSpriteObject(monolith, "Column_Base", pos, new Vector3(2.2f, 5f, 1f), colSpr, "Main", 0, new Color(0.85f, 0.85f, 0.9f));
            // Etched Statue/Rune Relief
            CreateSpriteObject(monolith, "Rune_Relief", pos + new Vector3(0, 1.5f, 0), new Vector3(1.6f, 1.6f, 1f), statueSpr, "Foreground", 1, runeColor);

            // Radiant Rune Glow Light
            CreatePointLight(monolith, "Rune_Glow_Light", pos + new Vector3(0, 1.5f, 0), runeColor, 6f, 2.5f);

            // Label Object
            GameObject labelGo = new GameObject("Rune_Label");
            labelGo.transform.SetParent(monolith.transform);
            labelGo.transform.localPosition = new Vector3(0, 4.2f, 0);
            var tm = labelGo.AddComponent<TextMesh>();
            tm.text = label;
            tm.fontSize = 28;
            tm.characterSize = 0.1f;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = runeColor;
        }

        private static void CreateConduitPipingSystem(GameObject parent, Vector3 center)
        {
            GameObject sys = new GameObject("Massive_Conduit_Pipes_Photo2");
            sys.transform.SetParent(parent.transform);
            sys.transform.position = center;

            // Main Thick Energy Core Pipes (using segmented cylinders for industrial look)
            string[] pipeNames = { "Conduit_Loop_Alpha", "Conduit_Loop_Beta", "Conduit_Main_Trunk" };
            Vector3[] offsets = { new Vector3(-12f, 0, 0), new Vector3(0, 2f, 0), new Vector3(12f, -1f, 0) };
            Vector3[] rotations = { new Vector3(0, 0, 45f), new Vector3(0, 0, -30f), new Vector3(0, 0, 90f) };

            for (int i = 0; i < pipeNames.Length; i++)
            {
                GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pipe.name = pipeNames[i];
                pipe.transform.SetParent(sys.transform);
                pipe.transform.localPosition = offsets[i];
                pipe.transform.localRotation = Quaternion.Euler(rotations[i]);
                pipe.transform.localScale = new Vector3(1.2f, 5f, 1.2f);
                Object.DestroyImmediate(pipe.GetComponent<Collider>());

                // Pulsing Green/Cyan Light
                CreatePointLight(pipe, $"Pipe_Light_{i}", pipe.transform.position, new Color(0.1f, 1f, 0.7f), 8f, 2f);
            }
        }

        private static void CreatePointLight(GameObject parent, string name, Vector3 pos, Color color, float radius, float intensity)
        {
            GameObject lightGo = new GameObject(name);
            lightGo.transform.SetParent(parent.transform);
            lightGo.transform.position = pos;

            var l2d = lightGo.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Point;
            l2d.color = color;
            l2d.pointLightOuterRadius = radius;
            l2d.pointLightInnerRadius = radius * 0.25f;
            l2d.intensity = intensity;
        }

        private static GameObject InstantiatePrefab(GameObject prefab, Transform parent, Vector3 pos)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            inst.transform.position = pos;
            return inst;
        }

        private static void SpawnPatrolEnemy(GameObject parent, GameObject prefab, Vector3 pos, string name)
        {
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
            inst.name = name;
            inst.transform.position = pos;
        }

        private static void AnchorObject(string objectName, Vector3 targetPos)
        {
            GameObject obj = GameObject.Find(objectName);
            if (obj != null)
            {
                Undo.RecordObject(obj.transform, $"Re-anchor {objectName}");
                obj.transform.position = targetPos;
                Debug.Log($"Re-anchored {objectName} -> {targetPos}");
            }
            else
            {
                Debug.LogWarning($"Could not find scene object: {objectName} to re-anchor.");
            }
        }
    }
}
