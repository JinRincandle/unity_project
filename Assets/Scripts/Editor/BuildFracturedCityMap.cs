using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TimeTravel;

namespace FracturesOfTime.Editor
{
    public static class BuildFracturedCityMap
    {
        private const string RootName = "[FRACTURED_CITY_PRESENT]";

        [MenuItem("FracturesOfTime/Build Fractured City (Blueprint Exact)")]
        public static void BuildMap()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            Debug.Log($"<color=#00FFFF><b>Building The Fractured City Level in: {activeScene.name}</b></color>");

            // 1. Load All Pixel Art & Prefab Assets
            Sprite sprGroundTop1 = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/ground-top-1.png");
            Sprite sprGroundTop2 = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/ground-top-2.png");
            Sprite sprGroundTopL = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/ground-top-left.png");
            Sprite sprGroundTopR = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/ground-top-right.png");
            Sprite sprGroundLeft1 = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/ground-left-1.png");
            Sprite sprGroundRight1 = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/ground-right-1.png");
            Sprite sprGroundBot1 = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/ground-bottom-1.png");
            Sprite sprGroundBotL = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/ground-bottom-left.png");
            Sprite sprGroundBotR = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/ground-bottom-right.png");
            Sprite sprSolidGround = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/solid-ground.png");
            Sprite sprRockBig = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/rockblock-big.png");
            Sprite sprRockSlim = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/rockblock-slim.png");
            Sprite sprRockBridge = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/rockblock-bridge.png");
            Sprite sprColumn = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/column.png");
            Sprite sprStairs1 = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/stairs-1.png");
            Sprite sprStairs2 = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/stairs-2.png");
            Sprite sprLadder = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/ladder.png");
            Sprite sprSolidWater = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/solid-water.png");
            Sprite sprSmallGrass = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/small-grass.png");
            Sprite sprFloorPlants = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/floor-plants.png");
            Sprite sprBgBrick = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/background/background_brick.png");
            Sprite sprBgColumn = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/background/background_column.png");
            Sprite sprBgDoor = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/background/background_door.png");

            Sprite sprSky = LoadSprite("Assets/[BuildingBlocks]/Art/Environment/Textures/Sky.png");
            Sprite sprDistantMount = LoadSprite("Assets/[BuildingBlocks]/Art/Environment/Textures/DistantMount(tilable).png");
            Sprite sprClouds = LoadSprite("Assets/[BuildingBlocks]/Art/Environment/Textures/Clouds.png");
            Sprite sprMovingPlatform = LoadSprite("Assets/[BuildingBlocks]/Art/Environment/Textures/MovingPlatform.png");
            Sprite sprAlienStatue3 = LoadSprite("Assets/[BuildingBlocks]/Art/Environment/Textures/AlienStatue03.png");
            Sprite sprAlienStatue4 = LoadSprite("Assets/[BuildingBlocks]/Art/Environment/Textures/AlienStatue04.png");
            Sprite sprUndergroundRoots = LoadSprite("Assets/[BuildingBlocks]/Art/Environment/Textures/UndergroundRoots.png");
            Sprite sprRubble = LoadSprite("Assets/[BuildingBlocks]/Art/Environment/Textures/Rubble.png");
            Sprite sprWaterfall = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/animated-tiles/waterfalls/_0000_Layer-1.png");

            RuntimeAnimatorController waterfallAnim = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/GrottoEscape2/GrottoEscape2Artwork/animations/waterfallsAnimController.controller");
            GameObject waterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/GrottoEscape2/GrottoEscape2Artwork/Prefabs/AnimatedWater.prefab");
            GameObject torchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/GrottoEscape2/GrottoEscape2Artwork/Prefabs/AnimatedTorch.prefab");

            GameObject meleeEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/[BuildingBlocks]/Prefabs/MeleeEnemy.prefab");
            GameObject grenadeEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/[BuildingBlocks]/Prefabs/GrenadeEnemy.prefab");
            GameObject wolfBossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/[BuildingBlocks]/Prefabs/WolfBossEnemy.prefab");

            // 2. Clean out old conflicting prototype roots and stray objects
            string[] oldRoots = { "[5X_EXPANDED_WORLD]", "[Central_Chrono_Chamber]", "[Rune_Clue_Wall]", "BossArea", "FutureOrb_Pickup", "rockblock-big-2-plants", "rockblock-big-2-plants (1)" };
            foreach (var r in oldRoots)
            {
                var go = GameObject.Find(r);
                if (go != null) Undo.DestroyObjectImmediate(go);
            }

            // Disable old environment prototype groups
            string[] envDeactivate = { "Environment/Main", "Environment/Foreground", "Environment/Default", "Environment/Background", "Environment/Grid/Level", "Environment/Grid/Background" };
            foreach (var path in envDeactivate)
            {
                var go = GameObject.Find(path);
                if (go != null) go.SetActive(false);
            }

            // Re-create root
            GameObject existingRoot = GameObject.Find(RootName);
            if (existingRoot != null) Undo.DestroyObjectImmediate(existingRoot);

            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Fractured City");

            // 3. Fix Lighting: Ensure ALL layers are clearly, warmly lit like the blueprint!
            ConfigureSceneLighting();

            // 4. Set Elimination Zone below the lowest cavern
            GameObject elimZone = GameObject.Find("EliminationZone");
            if (elimZone != null)
            {
                Undo.RecordObject(elimZone.transform, "Adjust Elimination Zone");
                elimZone.transform.localScale = Vector3.one;
                elimZone.transform.position = new Vector3(0f, -18f, 0);
                var box = elimZone.GetComponent<BoxCollider2D>();
                if (box != null)
                {
                    Undo.RecordObject(box, "Adjust Elimination Box");
                    box.size = new Vector2(250f, 4f);
                    box.isTrigger = true;
                }
            }

            // 5. Build Panoramic Skyline Background
            GameObject bgParent = new GameObject("00_Skyline_Background");
            bgParent.transform.SetParent(root.transform);
            if (sprSky != null)
                CreateSpriteObject(bgParent, "Sky_Sunset", new Vector3(0, 8f, 0), new Vector3(1.7f, 1.8f, 1f), sprSky, "Background", -10, new Color(1f, 0.9f, 0.85f));
            if (sprDistantMount != null)
            {
                for (float mx = -36f; mx <= 36f; mx += 24f)
                {
                    CreateSpriteObject(bgParent, $"Distant_Mount_{mx}", new Vector3(mx, 5f, 0), new Vector3(2.0f, 2.5f, 1f), sprDistantMount, "Background", -8, new Color(0.85f, 0.75f, 0.85f, 0.85f));
                }
            }
            if (sprClouds != null)
            {
                for (float cx = -35f; cx <= 35f; cx += 25f)
                {
                    CreateSpriteObject(bgParent, $"Clouds_{cx}", new Vector3(cx, 12f, 0), new Vector3(2.5f, 4.0f, 1f), sprClouds, "Background", -6, new Color(1f, 0.85f, 0.8f, 0.6f));
                }
            }

            // Tile palette container
            TilePalette pal = new TilePalette
            {
                topL = sprGroundTopL,
                topM1 = sprGroundTop1,
                topM2 = sprGroundTop2,
                topR = sprGroundTopR,
                wallL = sprGroundLeft1,
                wallR = sprGroundRight1,
                botL = sprGroundBotL,
                botM = sprGroundBot1,
                botR = sprGroundBotR,
                fill = sprSolidGround,
                bridge = sprRockBridge,
                column = sprColumn,
                stairs = sprStairs2,
                ladder = sprLadder,
                water = sprSolidWater,
                waterfall = sprWaterfall,
                waterfallAnim = waterfallAnim,
                waterPrefab = waterPrefab,
                torchPrefab = torchPrefab
            };

            // -------------------------------------------------------------
            // ZONE 1: START AREA (Left Bastion, X in [-38, -18], Y in [-8, 6])
            // -------------------------------------------------------------
            GameObject zone1 = new GameObject("Zone1_StartArea");
            zone1.transform.SetParent(root.transform);

            // Leftmost ruined stone spire
            CreateModularPlatform(zone1, "Start_LeftSpire", -38f, -34f, 12f, 4f, pal);
            // Starting Main Plateau
            CreateModularPlatform(zone1, "Start_MainPlateau", -35f, -24f, 4f, 2f, pal);
            // Wooden Scaffolding Bridge extending rightwards
            CreateBridgeScaffold(zone1, "Start_ScaffoldBridge", -24f, -18f, 4f, pal);
            // Ascending stone stairs connecting scaffolding to Zone 2's elevated entrance
            CreateStairs(zone1, "Start_To_PuzzleRoom_Stairs", new Vector3(-18f, 4f, 0), 2, true, pal.stairs);

            // Water Basin below Start Area (Deep Water Pool at Y in [-8, -4])
            CreateModularPlatform(zone1, "Start_WaterBasin_Bedrock", -36f, -14f, -6.5f, -8.5f, pal);
            CreateWaterBody(zone1, "Start_WaterPool", -36f, -14f, -4.0f, -6.5f, pal);

            // Stepping Stone Ruins inside water
            CreateModularPlatform(zone1, "Water_SteppingPillar_1", -30f, -27f, -3.5f, -6.5f, pal);
            CreateModularPlatform(zone1, "Water_SteppingPillar_2", -25f, -22f, -2.0f, -6.5f, pal);
            CreateModularPlatform(zone1, "Water_SteppingPillar_3", -20f, -17f, -0.5f, -6.5f, pal);
            CreateStairs(zone1, "Water_Up_Stairs", new Vector3(-17f, -0.5f, 0), 2, true, pal.stairs);

            // Wall Escape Ladder on cliff: Player can ALWAYS climb back out of the water!
            CreateLadder(zone1, "Start_WaterEscape_Ladder", new Vector3(-34f, -4f, 0), 8f, pal.ladder);

            // Small enemy patrolling the water stepping stones
            if (meleeEnemyPrefab != null)
                SpawnPatrolEnemy(zone1, meleeEnemyPrefab, new Vector3(-23.5f, -1.2f, 0), "WaterRuins_PatrolEnemy");

            // Rubble and Start Torch
            if (sprRubble != null)
                CreateSpriteObject(zone1, "Start_Rubble", new Vector3(-33f, 4.8f, 0), new Vector3(1.5f, 1.5f, 1f), sprRubble, "Main", 1, Color.white);
            if (torchPrefab != null)
            {
                InstantiatePrefab(torchPrefab, zone1.transform, new Vector3(-35f, 5.5f, 0));
                InstantiatePrefab(torchPrefab, zone1.transform, new Vector3(-24f, 5.5f, 0));
            }

            // Player Spawns on Start Platform
            AnchorObject("Player", new Vector3(-32.0f, 4.8f, 0));
            // Portal to Past on Start Platform
            AnchorObject("ChronoPortal_ToPast", new Vector3(-27.0f, 5.0f, 0));
            var portalPast = GameObject.Find("ChronoPortal_ToPast");
            if (portalPast != null)
            {
                var sr = portalPast.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (sprBgDoor != null) sr.sprite = sprBgDoor;
                    sr.color = new Color(0.95f, 0.65f, 0.35f, 1.0f); // Warm amber timeline portal
                    sr.sortingLayerName = "Main";
                    sr.sortingOrder = 0;
                }
                portalPast.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            }

            // -------------------------------------------------------------
            // ZONE 2: PUZZLE ROOM (Elevated Temple, X in [-16, -1], Y in [4.5, 13.5])
            // -------------------------------------------------------------
            GameObject zone2 = new GameObject("Zone2_PuzzleRoom");
            zone2.transform.SetParent(root.transform);

            // Floor & Ceiling of temple
            CreateModularPlatform(zone2, "PuzzleRoom_Floor", -16f, -1f, 6f, 4.5f, pal);
            CreateModularPlatform(zone2, "PuzzleRoom_Ceiling", -16f, -1f, 13.5f, 12f, pal);
            // Left Wall (doorway at Y in [6, 9])
            CreateModularPlatform(zone2, "PuzzleRoom_LeftWallTop", -16f, -15f, 12f, 9f, pal);
            // Right Wall (doorway out to Broken Bridge at Y in [6, 9])
            CreateModularPlatform(zone2, "PuzzleRoom_RightWallTop", -2f, -1f, 12f, 9f, pal);

            // Background brick masonry inside temple
            for (float bx = -15f; bx < -1f; bx += 3f)
            {
                CreateSpriteObject(zone2, $"Puzzle_BgBrick_{bx}", new Vector3(bx + 1.5f, 9f, 0), new Vector3(3f, 6f, 1f), sprBgBrick, "Background", -2, new Color(0.5f, 0.5f, 0.6f, 0.85f));
            }

            // Classical stone columns supporting interior
            CreateSpriteObject(zone2, "Puzzle_Column_L", new Vector3(-13.5f, 8.5f, 0), new Vector3(1f, 5f, 1f), sprColumn, "Main", 0, Color.white);
            CreateSpriteObject(zone2, "Puzzle_Column_R", new Vector3(-3.5f, 8.5f, 0), new Vector3(1f, 5f, 1f), sprColumn, "Main", 0, Color.white);

            // Temple Torches
            if (torchPrefab != null)
            {
                InstantiatePrefab(torchPrefab, zone2.transform, new Vector3(-13.5f, 9.5f, 0));
                InstantiatePrefab(torchPrefab, zone2.transform, new Vector3(-3.5f, 9.5f, 0));
            }

            // Purple atmospheric light in Puzzle Room center
            CreatePointLight(zone2, "Puzzle_Center_Light", new Vector3(-8.5f, 8.5f, 0), new Color(0.85f, 0.35f, 1f), 10f, 2.5f);

            // Place Control Panel and 3 Rune Pillars
            AnchorObject("ChronoControlPanel", new Vector3(-8.5f, 6.8f, 0));
            AnchorObject("RunePillar_1_Chronos (I)", new Vector3(-12.0f, 6.8f, 0));
            AnchorObject("RunePillar_2_Sol (II)", new Vector3(-8.5f, 9.0f, 0));
            AnchorObject("RunePillar_3_Abyss (III)", new Vector3(-5.0f, 6.8f, 0));

            // Interior Temple Guard Enemy
            if (meleeEnemyPrefab != null)
                SpawnPatrolEnemy(zone2, meleeEnemyPrefab, new Vector3(-6.5f, 6.8f, 0), "Temple_Guard_Enemy");

            // Left Plunging Waterfall from under Zone 2 into bottom-left pool
            CreateWaterfall(zone2, "PuzzleRoom_LeftWaterfall", new Vector3(-15.5f, 0.5f, 0), 9.0f, pal);

            // Vertical Ladder climbing through roof to Zone 3 (Energy Orb)
            CreateLadder(zone2, "Ladder_To_Zone3_Roof", new Vector3(-2.5f, 6f, 0), 7.5f, pal.ladder);

            // -------------------------------------------------------------
            // ZONE 3: ENERGY ORB ROOFTOP (Elevated High Peak, X in [-16, -1], Y in [13.5, 17])
            // -------------------------------------------------------------
            GameObject zone3 = new GameObject("Zone3_EnergyOrbRooftop");
            zone3.transform.SetParent(root.transform);

            // Battlements and grass along rooftop
            CreateModularPlatform(zone3, "Rooftop_Battlement_L", -16f, -14f, 15.5f, 13.5f, pal);
            CreateModularPlatform(zone3, "Rooftop_Spire", -11f, -9f, 16.5f, 13.5f, pal);
            CreateModularPlatform(zone3, "Rooftop_Battlement_R", -3f, -1f, 15.5f, 13.5f, pal);

            // Future Portal on Rooftop
            AnchorObject("ChronoPortal_ToFuture", new Vector3(-4.5f, 14.8f, 0));
            var portalFut = GameObject.Find("ChronoPortal_ToFuture");
            if (portalFut != null)
            {
                var sr = portalFut.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (sprBgDoor != null) sr.sprite = sprBgDoor;
                    sr.color = new Color(0.4f, 0.75f, 1.0f, 1.0f); // Cool cyan/azure future portal
                    sr.sortingLayerName = "Main";
                    sr.sortingOrder = 0;
                }
                portalFut.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
            }

            // Sentry Enemy on Rooftop configured with EnemyOrbDrop (DROPS PRESENT ORB ON KILL)
            if (grenadeEnemyPrefab != null)
            {
                var sentry = (GameObject)PrefabUtility.InstantiatePrefab(grenadeEnemyPrefab, zone3.transform);
                sentry.name = "Rooftop_Orb_Sentry";
                sentry.transform.position = new Vector3(-7.5f, 14.5f, 0);
                var dropper = sentry.AddComponent<EnemyOrbDrop>();
                dropper.orbToDrop = TemporalOrb.OrbKind.Present;
                dropper.dropChance = 1.0f;
                dropper.dropMessage = "<color=#00FFFF><b>ROOFTOP SENTRY DEFEATED!</b> Dropped the Present Chronos Core!</color>";
            }

            // -------------------------------------------------------------
            // ZONE 4: BROKEN BRIDGE (Center Gorge, X in [-1, 12], Y in [2, 7])
            // -------------------------------------------------------------
            GameObject zone4 = new GameObject("Zone4_BrokenBridge");
            zone4.transform.SetParent(root.transform);

            // Left Bridge Abutment
            CreateModularPlatform(zone4, "Bridge_Abutment_Left", -1f, 2f, 6f, 4.5f, pal);
            // Right Bridge Abutment
            CreateModularPlatform(zone4, "Bridge_Abutment_Right", 10f, 13f, 6f, 4.5f, pal);

            // Broken suspension planks hanging down
            CreateBridgeScaffold(zone4, "Bridge_Plank_Left", 2f, 3.5f, 6f, pal);
            CreateBridgeScaffold(zone4, "Bridge_Plank_Right", 8.5f, 10f, 6f, pal);

            // Horizontal Moving Platform bridging the central gap (Oscillates between X=3.5 and X=8.5)
            CreateMovingPlatform(zone4, "Bridge_Moving_Platform", new Vector3(3.5f, 6.2f, 0), new Vector3(8.5f, 6.2f, 0), 2.2f, sprMovingPlatform);

            // Lower Stepping Stones in the gorge
            CreateModularPlatform(zone4, "Bridge_LowerStone_1", 3f, 5.5f, 2.5f, 1.5f, pal);
            CreateModularPlatform(zone4, "Bridge_LowerStone_2", 6.5f, 9f, 2.5f, 1.5f, pal);

            // Melee Enemy guarding lower bridge crossing
            if (meleeEnemyPrefab != null)
                SpawnPatrolEnemy(zone4, meleeEnemyPrefab, new Vector3(4.25f, 3.3f, 0), "Gorge_MeleeEnemy");

            // Central Waterfall plunging from bridge gorge into Zone 5
            CreateWaterfall(zone4, "Bridge_Gorge_Waterfall", new Vector3(6.0f, -0.5f, 0), 6.5f, pal);

            // Ladder descending from left abutment into Zone 5 Underground
            CreateLadder(zone4, "Ladder_Bridge_To_Underground", new Vector3(0.5f, 0.5f, 0), 5.5f, pal.ladder);

            // -------------------------------------------------------------
            // ZONE 5: UNDERGROUND TUNNEL & WATER GROTTO (X in [0, 24], Y in [-7, 2])
            // -------------------------------------------------------------
            GameObject zone5 = new GameObject("Zone5_UndergroundTunnel_And_Grotto");
            zone5.transform.SetParent(root.transform);

            // Upper Cavern Terrace
            CreateModularPlatform(zone5, "Grotto_UpperTerrace", 0f, 5f, 0.5f, -1.0f, pal);
            // Stone Stairs 1 descending from terrace to grotto floor
            CreateStairs(zone5, "Grotto_DownStairs_1", new Vector3(5f, -2.5f, 0), 3, true, pal.stairs);

            // Cavern Floor (West)
            CreateModularPlatform(zone5, "Grotto_Floor_West", 8f, 11f, -2.5f, -4.5f, pal);

            // Subterranean Water Channel (Y in [-5.5, -3.5])
            CreateModularPlatform(zone5, "Grotto_Channel_Bedrock", 10f, 18f, -5.5f, -7.5f, pal);
            CreateWaterBody(zone5, "Grotto_WaterChannel", 10f, 18f, -3.5f, -5.5f, pal);

            // Submerged Stepping Stone Blocks in channel
            CreateModularPlatform(zone5, "Grotto_Water_Step_1", 12f, 14f, -3.0f, -5.5f, pal);
            CreateModularPlatform(zone5, "Grotto_Water_Step_2", 15f, 17f, -3.0f, -5.5f, pal);

            // Grotto Guardian Lair (East)
            CreateModularPlatform(zone5, "Grotto_Floor_East_Lair", 17f, 24f, -2.5f, -4.5f, pal);

            // Wall Roots and Alien Crystal Shrines
            if (sprUndergroundRoots != null)
                CreateSpriteObject(zone5, "Grotto_Roots", new Vector3(20f, -0.5f, 0), new Vector3(2.5f, 2.5f, 1f), sprUndergroundRoots, "Background", -1, Color.white);

            var shrine1 = CreateSpriteObject(zone5, "Grotto_Shrine_1", new Vector3(18.5f, -1.7f, 0), new Vector3(1.5f, 1.5f, 1f), sprAlienStatue3, "Main", 0, new Color(0.9f, 0.2f, 1f));
            CreatePointLight(shrine1, "GrottoShrine1_Light", new Vector3(18.5f, -1.0f, 0), new Color(0.9f, 0.2f, 1f), 6f, 2.5f);

            var shrine2 = CreateSpriteObject(zone5, "Grotto_Shrine_2", new Vector3(23.0f, -1.7f, 0), new Vector3(1.5f, 1.5f, 1f), sprAlienStatue4, "Main", 0, new Color(0.9f, 0.2f, 1f));
            CreatePointLight(shrine2, "GrottoShrine2_Light", new Vector3(23.0f, -1.0f, 0), new Color(0.9f, 0.2f, 1f), 6f, 2.5f);

            // Boss Lair: WolfBossEnemy DROPS FUTURE ORB ON KILL
            AnchorObject("WolfBossEnemy", new Vector3(21.0f, -1.7f, 0));
            var bossGo = GameObject.Find("WolfBossEnemy");
            if (bossGo != null)
            {
                var drop = bossGo.GetComponent<EnemyOrbDrop>();
                if (drop == null) drop = bossGo.AddComponent<EnemyOrbDrop>();
                drop.orbToDrop = TemporalOrb.OrbKind.Future;
                drop.dropChance = 1.0f;
                drop.dropMessage = "<color=#C080FF><b>GROTTO GUARDIAN DEFEATED!</b> Dropped the Aether Future Core!</color>";
            }

            // Stone Stairs 2 ascending towards tower foundation
            CreateStairs(zone5, "Grotto_UpStairs_2", new Vector3(21.5f, -2.5f, 0), 3, true, pal.stairs);

            // THE PROMINENT TALL VERTICAL LADDER: Climbs ALL THE WAY from Zone 5 to Zone 7 Final Door Terrace!
            CreateLadder(zone5, "TheGreatLadder_Grotto_To_FinalDoor", new Vector3(24.5f, -2.5f, 0), 14.5f, pal.ladder);

            // -------------------------------------------------------------
            // ZONE 6: TOWER CLIMB (Fortress Tower, X in [12, 24], Y in [6, 19])
            // -------------------------------------------------------------
            GameObject zone6 = new GameObject("Zone6_TowerClimb");
            zone6.transform.SetParent(root.transform);

            // Tower Floors at multiple tiers
            CreateModularPlatform(zone6, "Tower_Floor_1", 12f, 23f, 6f, 4.5f, pal);
            CreateModularPlatform(zone6, "Tower_Floor_2", 14f, 23f, 10f, 8.5f, pal);
            CreateModularPlatform(zone6, "Tower_Floor_3", 14f, 21f, 14f, 12.5f, pal);
            CreateModularPlatform(zone6, "Tower_Apex_Roof", 15f, 21f, 18f, 16.5f, pal);

            // Tower Background Bricks
            for (float ty = 6f; ty < 17f; ty += 3f)
            {
                CreateSpriteObject(zone6, $"Tower_BgBrick_{ty}", new Vector3(18f, ty + 1.5f, 0), new Vector3(8f, 3f, 1f), sprBgBrick, "Background", -2, new Color(0.45f, 0.45f, 0.55f, 0.8f));
            }

            // Tower Wall Ladders
            CreateLadder(zone6, "Tower_Ladder_F1_to_F2", new Vector3(13.5f, 6f, 0), 4f, pal.ladder);
            CreateLadder(zone6, "Tower_Ladder_F2_to_F3", new Vector3(22.0f, 10f, 0), 4f, pal.ladder);
            CreateLadder(zone6, "Tower_Ladder_F3_to_Apex", new Vector3(16.0f, 14f, 0), 4f, pal.ladder);

            // Vertical Moving Platform Lift inside Tower Shaft
            CreateMovingPlatform(zone6, "Tower_Energy_Lift", new Vector3(18.5f, 6.5f, 0), new Vector3(18.5f, 13.5f, 0), 2.0f, sprMovingPlatform);

            // Radiant Cyan Temporal Beacon on Tower Apex
            CreatePointLight(zone6, "Tower_Apex_BeaconLight", new Vector3(18f, 19.5f, 0), new Color(0.2f, 0.85f, 1f), 10f, 3.0f);

            // Sentries patrolling Tower
            if (meleeEnemyPrefab != null)
                SpawnPatrolEnemy(zone6, meleeEnemyPrefab, new Vector3(15f, 6.8f, 0), "Tower_F1_Melee");
            if (grenadeEnemyPrefab != null)
            {
                SpawnPatrolEnemy(zone6, grenadeEnemyPrefab, new Vector3(20f, 10.8f, 0), "Tower_F2_Sentry");
                SpawnPatrolEnemy(zone6, grenadeEnemyPrefab, new Vector3(18f, 18.8f, 0), "Tower_Apex_Sentry");
            }

            // Rampart walkway connecting Tower to Final Door Terrace
            CreateModularPlatform(zone6, "Tower_To_FinalDoor_Rampart", 23f, 26f, 12f, 10.5f, pal);

            // -------------------------------------------------------------
            // ZONE 7: FINAL DOOR & INNER SANCTUM (Far Right Plateau, X in [24, 38], Y in [10, 19])
            // -------------------------------------------------------------
            GameObject zone7 = new GameObject("Zone7_FinalDoorSanctum");
            zone7.transform.SetParent(root.transform);

            // Terrace Floor
            CreateModularPlatform(zone7, "FinalDoor_TerraceFloor", 25f, 37f, 12f, 10.5f, pal);
            // Sanctum Ceiling
            CreateModularPlatform(zone7, "FinalDoor_SanctumCeiling", 31f, 37f, 18f, 16.5f, pal);
            // Right Perimeter Wall (sealing level)
            CreateModularPlatform(zone7, "FinalDoor_RightWall", 37f, 39f, 19f, 6f, pal);
            // Cliff facade underneath terrace
            CreateModularPlatform(zone7, "FinalDoor_UnderCliff", 25f, 37f, 10.5f, -2.5f, pal);

            // Flanking Torches & Statues
            if (torchPrefab != null)
            {
                InstantiatePrefab(torchPrefab, zone7.transform, new Vector3(27f, 13.5f, 0));
                InstantiatePrefab(torchPrefab, zone7.transform, new Vector3(36f, 13.5f, 0));
            }
            CreateSpriteObject(zone7, "FinalDoor_Statue", new Vector3(28f, 13f, 0), new Vector3(1.8f, 1.8f, 1f), sprAlienStatue4, "Main", 0, new Color(0.85f, 0.4f, 1f));

            // Elite Guard on Terrace
            if (meleeEnemyPrefab != null)
                SpawnPatrolEnemy(zone7, meleeEnemyPrefab, new Vector3(29f, 12.8f, 0), "FinalDoor_EliteGuard");

            // Vault Door & Master Timeline Core
            AnchorObject("ChronoVaultDoor", new Vector3(32.0f, 13.5f, 0));
            AnchorObject("MasterTimelineCore", new Vector3(35.5f, 13.5f, 0));
            var coreGo = GameObject.Find("MasterTimelineCore");
            if (coreGo != null)
            {
                var coreSr = coreGo.GetComponent<SpriteRenderer>();
                if (coreSr != null)
                {
                    if (sprAlienStatue3 != null) coreSr.sprite = sprAlienStatue3;
                    coreSr.color = new Color(1.0f, 0.85f, 0.3f, 1.0f);
                    coreSr.sortingLayerName = "Main";
                    coreSr.sortingOrder = 1;
                }
            }

            // Atmospheric Lights for Vault Door & Core
            CreatePointLight(zone7, "FinalDoor_Radiance", new Vector3(32.0f, 14.5f, 0), new Color(0.85f, 0.35f, 1f), 9f, 3.2f);
            CreatePointLight(zone7, "Sanctum_Core_GoldLight", new Vector3(35.5f, 14.5f, 0), new Color(1f, 0.85f, 0.3f), 8f, 2.8f);

            // 6. Connect TemporalVaultPuzzle references
            ConnectPuzzleReferences();

            // Mark Scene Dirty and Save
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Debug.Log("<color=#00FFAA><b>THE FRACTURED CITY (BLUEPRINT EXACT) SUCCESSFULLY BUILT & SAVED!</b></color>");
        }

        // =============================================================
        // GEOMETRY & MODULAR TILE BUILDERS
        // =============================================================

        private struct TilePalette
        {
            public Sprite topL, topM1, topM2, topR;
            public Sprite wallL, wallR;
            public Sprite botL, botM, botR;
            public Sprite fill, bridge, column, stairs, ladder, water, waterfall;
            public RuntimeAnimatorController waterfallAnim;
            public GameObject waterPrefab, torchPrefab;
        }

        private static GameObject CreateModularPlatform(GameObject parent, string name, float leftX, float rightX, float topY, float bottomY, TilePalette pal)
        {
            float width = rightX - leftX;
            float height = topY - bottomY;
            if (width <= 0f || height <= 0f) return null;

            GameObject platform = new GameObject(name);
            platform.transform.SetParent(parent.transform);
            platform.transform.position = new Vector3((leftX + rightX) * 0.5f, (topY + bottomY) * 0.5f, 0);

            var col = platform.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, height);

            int xCount = Mathf.RoundToInt(width);
            int yCount = Mathf.RoundToInt(height);

            for (int xi = 0; xi < xCount; xi++)
            {
                float worldX = leftX + xi + 0.5f;
                for (int yi = 0; yi < yCount; yi++)
                {
                    float worldY = bottomY + yi + 0.5f;
                    Sprite spr = pal.fill;

                    if (yCount == 1)
                    {
                        if (xi == 0) spr = pal.topL != null ? pal.topL : pal.topM1;
                        else if (xi == xCount - 1) spr = pal.topR != null ? pal.topR : pal.topM1;
                        else spr = (xi % 2 == 0) ? pal.topM1 : pal.topM2;
                    }
                    else if (yi == yCount - 1) // Top row
                    {
                        if (xi == 0) spr = pal.topL != null ? pal.topL : pal.topM1;
                        else if (xi == xCount - 1) spr = pal.topR != null ? pal.topR : pal.topM1;
                        else spr = (xi % 2 == 0) ? pal.topM1 : pal.topM2;
                    }
                    else if (yi == 0) // Bottom row
                    {
                        if (xi == 0) spr = pal.botL != null ? pal.botL : pal.botM;
                        else if (xi == xCount - 1) spr = pal.botR != null ? pal.botR : pal.botM;
                        else spr = pal.botM;
                    }
                    else // Middle rows
                    {
                        if (xi == 0) spr = pal.wallL != null ? pal.wallL : pal.fill;
                        else if (xi == xCount - 1) spr = pal.wallR != null ? pal.wallR : pal.fill;
                        else spr = pal.fill;
                    }

                    if (spr != null)
                    {
                        GameObject tile = new GameObject($"Tile_{xi}_{yi}");
                        tile.transform.SetParent(platform.transform);
                        tile.transform.position = new Vector3(worldX, worldY, 0);
                        var sr = tile.AddComponent<SpriteRenderer>();
                        sr.sprite = spr;
                        sr.sortingLayerName = "Main";
                        sr.sortingOrder = 0;
                        sr.color = Color.white;
                    }
                }
            }

            return platform;
        }

        private static GameObject CreateBridgeScaffold(GameObject parent, string name, float leftX, float rightX, float topY, TilePalette pal)
        {
            float width = rightX - leftX;
            GameObject bridge = new GameObject(name);
            bridge.transform.SetParent(parent.transform);
            bridge.transform.position = new Vector3((leftX + rightX) * 0.5f, topY - 0.5f, 0);

            var col = bridge.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, 1.0f);

            int count = Mathf.CeilToInt(width / 2.0f);
            for (int i = 0; i < count; i++)
            {
                float px = leftX + (i + 0.5f) * 2.0f;
                if (px > rightX) px = rightX - 1.0f;
                CreateSpriteObject(bridge, $"Plank_{i}", new Vector3(px, topY - 0.5f, 0), Vector3.one, pal.bridge, "Main", 0, Color.white);
            }

            return bridge;
        }

        private static GameObject CreateStairs(GameObject parent, string name, Vector3 startPos, int steps, bool ascendRight, Sprite stairSprite)
        {
            GameObject stairs = new GameObject(name);
            stairs.transform.SetParent(parent.transform);
            stairs.transform.position = startPos;

            // Add smooth polygon slope collider so character walks effortlessly
            var poly = stairs.AddComponent<PolygonCollider2D>();
            Vector2[] points;
            if (ascendRight)
            {
                points = new Vector2[]
                {
                    Vector2.zero,
                    new Vector2(steps, steps),
                    new Vector2(steps, 0)
                };
            }
            else
            {
                points = new Vector2[]
                {
                    Vector2.zero,
                    new Vector2(-steps, steps),
                    new Vector2(-steps, 0)
                };
            }
            poly.points = points;

            for (int i = 0; i < steps; i++)
            {
                float x = ascendRight ? (i + 0.5f) : -(i + 0.5f);
                float y = i + 0.5f;
                GameObject step = new GameObject($"Step_{i}");
                step.transform.SetParent(stairs.transform);
                step.transform.localPosition = new Vector3(x, y, 0);

                var sr = step.AddComponent<SpriteRenderer>();
                sr.sprite = stairSprite;
                sr.flipX = !ascendRight;
                sr.sortingLayerName = "Main";
                sr.sortingOrder = 0;
                sr.color = Color.white;
            }

            return stairs;
        }

        private static void CreateLadder(GameObject parent, string name, Vector3 bottomPos, float height, Sprite ladderSpr)
        {
            GameObject ladder = new GameObject(name);
            ladder.transform.SetParent(parent.transform);
            ladder.transform.position = bottomPos + new Vector3(0, height * 0.5f, 0);

            var col = ladder.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.8f, height);
            col.isTrigger = true;

            ladder.AddComponent<LadderClimb>();

            int count = Mathf.CeilToInt(height);
            for (int i = 0; i < count; i++)
            {
                float y = -height * 0.5f + i + 0.5f;
                CreateSpriteObject(ladder, $"Rung_{i}", ladder.transform.position + new Vector3(0, y, 0), Vector3.one, ladderSpr, "Main", 1, Color.white);
            }
        }

        private static void CreateWaterBody(GameObject parent, string name, float leftX, float rightX, float surfaceY, float bedY, TilePalette pal)
        {
            GameObject waterGo = new GameObject(name);
            waterGo.transform.SetParent(parent.transform);

            // Animated surface waves
            if (pal.waterPrefab != null)
            {
                for (float wx = leftX + 1.5f; wx < rightX; wx += 3.0f)
                {
                    var wave = (GameObject)PrefabUtility.InstantiatePrefab(pal.waterPrefab, waterGo.transform);
                    wave.transform.position = new Vector3(wx, surfaceY, 0);
                    wave.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                }
            }

            // Translucent water body filling underneath
            int xCount = Mathf.RoundToInt(rightX - leftX);
            int yCount = Mathf.RoundToInt(surfaceY - bedY);
            for (int xi = 0; xi < xCount; xi++)
            {
                float wx = leftX + xi + 0.5f;
                for (int yi = 0; yi < yCount; yi++)
                {
                    float wy = bedY + yi + 0.5f;
                    var tile = CreateSpriteObject(waterGo, $"WaterTile_{xi}_{yi}", new Vector3(wx, wy, 0), Vector3.one, pal.water, "Main", 1, new Color(0.2f, 0.7f, 1f, 0.7f));
                }
            }
        }

        private static void CreateWaterfall(GameObject parent, string name, Vector3 topPos, float height, TilePalette pal)
        {
            GameObject wf = new GameObject(name);
            wf.transform.SetParent(parent.transform);
            wf.transform.position = topPos - new Vector3(0, height * 0.5f, 0);

            int segments = Mathf.CeilToInt(height);
            for (int i = 0; i < segments; i++)
            {
                float sy = height * 0.5f - i - 0.5f;
                GameObject seg = new GameObject($"WF_Segment_{i}");
                seg.transform.SetParent(wf.transform);
                seg.transform.position = topPos - new Vector3(0, i + 0.5f, 0);

                var sr = seg.AddComponent<SpriteRenderer>();
                sr.sprite = pal.waterfall;
                sr.sortingLayerName = "Main";
                sr.sortingOrder = 1;
                sr.color = new Color(0.3f, 0.8f, 1f, 0.85f);

                if (pal.waterfallAnim != null)
                {
                    var anim = seg.AddComponent<Animator>();
                    anim.runtimeAnimatorController = pal.waterfallAnim;
                }
            }
        }

        private static void CreateMovingPlatform(GameObject parent, string name, Vector3 startPos, Vector3 endPos, float speed, Sprite platSpr)
        {
            GameObject plat = new GameObject(name);
            plat.transform.SetParent(parent.transform);
            plat.transform.position = startPos;

            var col = plat.AddComponent<BoxCollider2D>();
            col.size = new Vector2(3.5f, 0.8f);

            var sr = plat.AddComponent<SpriteRenderer>();
            sr.sprite = platSpr;
            sr.sortingLayerName = "Main";
            sr.sortingOrder = 1;
            sr.color = Color.white;
            plat.transform.localScale = Vector3.one;

            var mover = plat.AddComponent<PingPongMover>();
            mover.pointA = startPos;
            mover.pointB = endPos;
            mover.speed = speed;
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

        private static void CreatePointLight(GameObject parent, string name, Vector3 pos, Color color, float radius, float intensity)
        {
            GameObject lightGo = new GameObject(name);
            lightGo.transform.SetParent(parent.transform);
            lightGo.transform.position = pos;

            var l2d = lightGo.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Point;
            l2d.color = color;
            l2d.pointLightOuterRadius = radius;
            l2d.intensity = intensity;
        }

        private static void SpawnPatrolEnemy(GameObject parent, GameObject prefab, Vector3 pos, string name)
        {
            if (prefab == null) return;
            var enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
            enemy.name = name;
            enemy.transform.position = pos;
        }

        private static void AnchorObject(string name, Vector3 targetPos)
        {
            var go = GameObject.Find(name);
            if (go != null)
            {
                Undo.RecordObject(go.transform, $"Anchor {name}");
                go.transform.position = targetPos;
                go.SetActive(true);
            }
        }

        private static GameObject InstantiatePrefab(GameObject prefab, Transform parent, Vector3 pos)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.position = pos;
            return go;
        }

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

        private static void ConfigureSceneLighting()
        {
            var lights = Object.FindObjectsByType<Light2D>();
            foreach (var l in lights)
            {
                if (l.lightType == Light2D.LightType.Global)
                {
                    Undo.RecordObject(l, "Adjust Global Light");
                    string n = l.gameObject.name;
                    if (n.Contains("Background"))
                    {
                        l.intensity = 0.85f;
                        l.color = new Color(1.0f, 0.75f, 0.65f, 1f); // Warm sunset skyline glow
                    }
                    else
                    {
                        l.intensity = 0.95f;
                        l.color = new Color(1.0f, 0.94f, 0.88f, 1f); // Crisp warm daylight
                    }
                }
            }
        }

        private static void ConnectPuzzleReferences()
        {
            var puzzleGo = GameObject.Find("TemporalVaultPuzzleManager");
            if (puzzleGo == null)
            {
                puzzleGo = new GameObject("TemporalVaultPuzzleManager");
                puzzleGo.AddComponent<TemporalVaultPuzzle>();
            }

            var puzzle = puzzleGo.GetComponent<TemporalVaultPuzzle>();
            var doorGo = GameObject.Find("ChronoVaultDoor");
            if (puzzle != null && doorGo != null)
            {
                var so = new SerializedObject(puzzle);
                var doorProp = so.FindProperty("vaultDoor");
                var colProp = so.FindProperty("doorCollider");
                var lightProp = so.FindProperty("vaultLockLight");

                if (doorProp != null) doorProp.objectReferenceValue = doorGo.transform;
                if (colProp != null) colProp.objectReferenceValue = doorGo.GetComponent<Collider2D>();
                if (lightProp != null) lightProp.objectReferenceValue = doorGo.GetComponentInChildren<Light2D>();

                so.ApplyModifiedProperties();
            }
        }
    }
}
