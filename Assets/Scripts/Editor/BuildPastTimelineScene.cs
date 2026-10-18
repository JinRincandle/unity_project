using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;
using TimeTravel;
using Blocks.Movement;
using Blocks.Attack;
using Blocks.Character;

namespace FracturesOfTime.Editor
{
    public static class BuildPastTimelineScene
    {
        private const string ScenePath = "Assets/Scenes/past.unity";
        private const string RootName = "[PAST_TIMELINE_WORLD]";

        [MenuItem("FracturesOfTime/Build Past Level (Battlefield & Volcano)")]
        public static void BuildPastLevel()
        {
            Debug.Log("<color=#FF4400><b>Building Past Timeline Level in Assets/Scenes/past.unity...</b></color>");

            // 0. Ensure enemy prefabs are tuned for fighting back and boss behavior
            ConfigureEnemyPrefabs();

            // 1. Open or Create Scene
            UnityEngine.SceneManagement.Scene pastScene = EditorSceneManager.GetActiveScene();
            if (pastScene.path != ScenePath)
            {
                if (System.IO.File.Exists(ScenePath))
                {
                    pastScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                }
                else
                {
                    pastScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                    EditorSceneManager.SaveScene(pastScene, ScenePath);
                }
            }

            EnsureSceneInBuildSettings(ScenePath);

            // 2. Clean out old scene objects (except Main Camera)
            var roots = pastScene.GetRootGameObjects();
            foreach (var r in roots)
            {
                if (r == null) continue;
                if (r.name.Contains("Main Camera")) continue;
                Object.DestroyImmediate(r);
            }

            // 3. Load Compatible Sprites and Prefabs
            // A. Ashfall Parallax Fiery Sky
            Sprite sprSky = LoadSprite("Assets/ashfall_parallax_pack/layers/01_sky.png");
            Sprite sprMid = LoadSubSprite("Assets/ashfall_parallax_pack/layers/02_mid.png", "02_mid_3");
            Sprite sprFront = LoadSprite("Assets/ashfall_parallax_pack/layers/03_front.png");

            // B. GrottoEscape2 Dark Lava & Stone Tiles
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
            Sprite sprLavaSolid = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/lava-solid.png");
            Sprite sprRockBigLava = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/rockblock-big-lava.png");
            Sprite sprBridgeLava = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/rockblock-bridge-lava.png");
            Sprite sprColumn = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/column.png");
            Sprite sprStairs1 = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/stairs-1.png");
            Sprite sprStairs2 = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/stairs-2.png");
            Sprite sprLadder = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/ladder.png");
            Sprite sprBgBrick = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/background/background_brick.png");
            Sprite sprBgDoor = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/background/background_door.png");

            // C. Under Fire Ancient Monumental Structures
            Sprite sprAncientAltar = LoadSprite("Assets/underfire/1x/01_the_altar.png");
            Sprite sprAncientHall = LoadSubSprite("Assets/underfire/1x/04_the_hall.png", "04_the_hall_0");

            // D. Prefabs (Animated Fire, Smoke, Lava, Torches, Player)
            GameObject fire1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Asset FTTGR/Prefabs/Enviroment/Decorations/FireType1.prefab");
            GameObject fire2Prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Asset FTTGR/Prefabs/Enviroment/Decorations/FireType2.prefab");
            GameObject smokePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Asset FTTGR/Prefabs/VFXs/EffectSmoke .prefab");
            GameObject lavaTilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GrottoEscape2/GrottoEscape2Artwork/Prefabs/AnimatedlavaTile.prefab");
            GameObject lavaFallsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GrottoEscape2/GrottoEscape2Artwork/Prefabs/AnimatedLavaFalls.prefab");
            GameObject torchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GrottoEscape2/GrottoEscape2Artwork/Prefabs/AnimatedTorch.prefab");
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/[BuildingBlocks]/Prefabs/Player.prefab");

            // 4. Create Root Object
            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Create Past Timeline World");

            // Palette container
            PastTilePalette pal = new PastTilePalette
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
                bridge = sprBridgeLava != null ? sprBridgeLava : sprSolidGround,
                column = sprColumn,
                stairs = sprStairs2 != null ? sprStairs2 : sprStairs1,
                ladder = sprLadder,
                fire1Prefab = fire1Prefab,
                fire2Prefab = fire2Prefab,
                smokePrefab = smokePrefab,
                lavaTilePrefab = lavaTilePrefab,
                lavaFallsPrefab = lavaFallsPrefab,
                torchPrefab = torchPrefab
            };

            // 5. Setup Camera & 2D Scene Lighting
            ConfigureCameraAndLighting(pastScene);

            // 6. Fiery Parallax Sky & Distant Volcano
            BuildParallaxBackground(root, sprSky, sprMid, sprFront);

            // 7. ZONE 1: Starting Bastion & Ruined Battlefield (X in [-40, -18], Y in [0, 8])
            GameObject zone1 = new GameObject("Zone1_StartingBastion_Battlefield");
            zone1.transform.SetParent(root.transform);

            // Left boundary cliff (sealing the level)
            CreateModularPlatform(zone1, "Start_LeftBoundaryWall", -42f, -38f, 16f, -4f, pal);
            // Starting Main Stone Bastion (Surface Y = 2.0) - SAFE ZONE
            CreateModularPlatform(zone1, "Start_MainPlateau", -38f, -18f, 2f, -2f, pal);
            AddSafePlatformZone(zone1, -38f, -18f, 2.0f);

            // Burning ruins details: bonfires, torches, smoke
            if (fire1Prefab != null) InstantiatePrefab(fire1Prefab, zone1.transform, new Vector3(-28f, 2.0f, 0));
            if (fire2Prefab != null) InstantiatePrefab(fire2Prefab, zone1.transform, new Vector3(-23f, 2.0f, 0));
            if (smokePrefab != null)
            {
                InstantiatePrefab(smokePrefab, zone1.transform, new Vector3(-28f, 2.5f, 0));
                InstantiatePrefab(smokePrefab, zone1.transform, new Vector3(-23f, 2.5f, 0));
            }
            if (torchPrefab != null)
            {
                InstantiatePrefab(torchPrefab, zone1.transform, new Vector3(-36f, 3.5f, 0));
                InstantiatePrefab(torchPrefab, zone1.transform, new Vector3(-20f, 3.5f, 0));
            }

            // Arrival Temporal Rift Archway
            CreateSpriteObject(zone1, "Start_TemporalRiftArch", new Vector3(-35.0f, 3.2f, 0), new Vector3(1.3f, 1.3f, 1f), sprBgDoor, "Main", 0, new Color(1.0f, 0.65f, 0.35f, 1f));
            CreatePointLight(zone1, "Start_Rift_Light", new Vector3(-35.0f, 3.2f, 0), new Color(1.0f, 0.6f, 0.2f), 7f, 2.5f);

            // 8. ZONE 2: Broken Bridge & Molten Chasm (X in [-18, 16], Y in [-5, 8])
            GameObject zone2 = new GameObject("Zone2_BrokenBridge_MoltenChasm");
            zone2.transform.SetParent(root.transform);

            // Broken Bridge Heat Zone (gradual heat buildup over boiling chasm)
            CreateHeatZone(zone2, "Zone2_BridgeChasm_HeatZone", -18f, 12f, -3f, 7f);

            // Broken suspension bridge spans at comfortable jump heights and gaps
            CreateBridgeScaffold(zone2, "Bridge_Abutment_Left", -18f, -15f, 2.0f, pal);
            CreateBridgeScaffold(zone2, "Bridge_Plank_1", -13.5f, -9.5f, 1.2f, pal);
            CreateBridgeScaffold(zone2, "Bridge_Plank_2", -7.5f, -3.5f, 2.2f, pal);

            // Stepping Pier Stone (connecting Plank 2 smoothly to Upper Pier)
            CreateModularPlatform(zone2, "Bridge_MidSteppingLedge", -2.5f, -0.5f, 3.5f, 2.0f, pal);

            // High Ruined Stone Pier (Upper Route, Surface Y = 4.8) - SAFE PLATFORM
            CreateModularPlatform(zone2, "Bridge_UpperStonePier", 0.5f, 5.0f, 4.8f, 3.0f, pal);
            AddSafePlatformZone(zone2, 0.5f, 5.0f, 4.8f);
            CreateSpriteObject(zone2, "Pier_Pillar_L", new Vector3(1.5f, 1.5f, 0), new Vector3(1f, 3f, 1f), sprColumn, "Main", 0, Color.gray);
            CreateSpriteObject(zone2, "Pier_Pillar_R", new Vector3(4.0f, 1.5f, 0), new Vector3(1f, 3f, 1f), sprColumn, "Main", 0, Color.gray);

            // Past Orb Collectible 1 perched on Upper Stone Pier
            CreateTemporalOrb(zone2, "PastOrb_1_BridgePier", new Vector3(2.8f, 6.2f, 0), TemporalOrb.OrbKind.Past);

            // Light Bandit patrolling the high stone pier
            SpawnEnemy(zone2, "Bandit_PierScout", "Assets/Prefabs/Enemies/Bandit_Light.prefab", new Vector3(2.5f, 4.8f, 0));

            // Lower Bridge Span (Lower Route, Surface Y = 1.5)
            CreateBridgeScaffold(zone2, "Bridge_LowerPlank_3", 1.0f, 4.5f, 1.0f, pal);
            CreateBridgeScaffold(zone2, "Bridge_Plank_4", 6.5f, 9.5f, 1.8f, pal);

            // Right Bridge Abutment leading into cavern (Surface Y = 3.0)
            CreateModularPlatform(zone2, "Bridge_Abutment_Right", 11.5f, 16.5f, 3.0f, 1.0f, pal);

            // Flaming brazier on upper pier
            if (fire1Prefab != null) InstantiatePrefab(fire1Prefab, zone2.transform, new Vector3(2.8f, 4.8f, 0));
            if (torchPrefab != null) InstantiatePrefab(torchPrefab, zone2.transform, new Vector3(13.5f, 4.5f, 0));

            // Molten Lava Sea beneath the bridge (Y = -4.5) with visual steam warnings
            CreateLavaPit(zone2, "Bridge_LavaSea", -20f, 16f, -4.5f, -7.5f, pal);
            // Molten Lava Fall cascading down the back gorge
            if (lavaFallsPrefab != null)
            {
                var lf = (GameObject)PrefabUtility.InstantiatePrefab(lavaFallsPrefab, zone2.transform);
                lf.name = "Bridge_Gorge_LavaFall";
                lf.transform.position = new Vector3(-5.5f, 0.5f, 0);
            }

            // 9. ZONE 3: Volcanic Cavern with Multiple Heights & Ladders (X in [16, 56], Y in [-6, 14])
            GameObject zone3 = new GameObject("Zone3_VolcanicCavern");
            zone3.transform.SetParent(root.transform);

            // Cavern Ceiling Overhang
            CreateModularPlatform(zone3, "Cavern_Ceiling_Overhang", 16.5f, 56f, 14f, 12f, pal);

            // Cavern Background Bricks
            for (float bx = 17f; bx < 55f; bx += 3f)
            {
                CreateSpriteObject(zone3, $"Cavern_Bg_{bx}", new Vector3(bx + 1.5f, 7f, 0), new Vector3(3f, 8f, 1f), sprBgBrick, "Background", -2, new Color(0.4f, 0.25f, 0.22f, 0.9f));
            }

            // Lower Path: Stepping down into subterranean basalt floor (SAFE ZONE)
            CreateStairs(zone3, "Cavern_DownStairs_1", new Vector3(16.5f, 3.0f, 0), 2, false, pal.stairs);
            CreateModularPlatform(zone3, "Cavern_Floor_West", 19f, 26f, 1.0f, -1.0f, pal);
            AddSafePlatformZone(zone3, 19f, 26f, 1.0f);
            // Light Bandit on lower cavern floor
            SpawnEnemy(zone3, "Bandit_CavernFloorScout", "Assets/Prefabs/Enemies/Bandit_Light.prefab", new Vector3(22.5f, 1.0f, 0));

            // Subterranean Lava Trench & Stepping Islands
            CreateLavaPit(zone3, "Cavern_LavaTrench", 26f, 34f, -3.0f, -6.0f, pal);
            CreateModularPlatform(zone3, "Cavern_StepIsland_1", 27.5f, 29.5f, -1.0f, -3.0f, pal);
            CreateModularPlatform(zone3, "Cavern_StepIsland_2", 31.0f, 33.0f, -1.0f, -3.0f, pal);

            // Lower Cavern Floor East
            CreateModularPlatform(zone3, "Cavern_Floor_East", 34f, 44f, 0.0f, -2.0f, pal);

            // Secret Lower Alcove with Past Orb Collectible 2
            CreateModularPlatform(zone3, "Cavern_SecretAlcove_Ledge", 37f, 41f, -2.5f, -4.0f, pal);
            CreateTemporalOrb(zone3, "PastOrb_2_CavernAlcove", new Vector3(39f, -1.5f, 0), TemporalOrb.OrbKind.Past);

            // Upper Path: Ascending Wall Ladder at X = 25.0
            CreateLadder(zone3, "Cavern_Ladder_To_UpperLedges", new Vector3(25.0f, 1.0f, 0), 6.0f, pal.ladder);

            // Upper Rocky Ledge 1 (Y = 7.0)
            CreateModularPlatform(zone3, "Cavern_UpperLedge_1", 25f, 33f, 7.0f, 5.5f, pal);
            // Heavy Bandit patrolling upper ledge
            SpawnEnemy(zone3, "Bandit_UpperLedgeGuard", "Assets/Prefabs/Enemies/Bandit_Heavy.prefab", new Vector3(29.0f, 7.0f, 0));
            // Upper Bridge Spire (Y = 8.5)
            CreateBridgeScaffold(zone3, "Cavern_UpperBridge_2", 35.5f, 41.5f, 8.5f, pal);
            // High Observation Overlook (Y = 9.5)
            CreateModularPlatform(zone3, "Cavern_UpperOverlook_3", 44f, 52f, 9.5f, 8.0f, pal);
            // Stone Stairs descending from overlook back down to cavern exit
            CreateStairs(zone3, "Cavern_Overlook_DownStairs", new Vector3(52f, 9.5f, 0), 4, false, pal.stairs);

            // Cavern Exit Threshold (Surface Y = 5.5) - SAFE PLATFORM & CHECKPOINT
            CreateModularPlatform(zone3, "Cavern_Exit_Threshold", 52f, 58f, 5.5f, 3.5f, pal);
            AddSafePlatformZone(zone3, 52f, 58f, 5.5f);

            // CRITICAL CHECKPOINT BEFORE THE HARDEST SECTION (Caldera Lava Crossing)
            CreateCheckpoint(zone3, "ChronoCheckpoint_CalderaThreshold", new Vector3(54.5f, 5.5f, 0), sprColumn);

            // Cavern Torches & Warm Ambient Lights
            if (torchPrefab != null)
            {
                InstantiatePrefab(torchPrefab, zone3.transform, new Vector3(20f, 2.5f, 0));
                InstantiatePrefab(torchPrefab, zone3.transform, new Vector3(28f, 8.5f, 0));
                InstantiatePrefab(torchPrefab, zone3.transform, new Vector3(48f, 11.0f, 0));
            }
            CreatePointLight(zone3, "Cavern_Ambiance_1", new Vector3(28f, 4.0f, 0), new Color(1.0f, 0.45f, 0.15f), 9f, 2.2f);
            CreatePointLight(zone3, "Cavern_Ambiance_2", new Vector3(45f, 6.0f, 0), new Color(1.0f, 0.5f, 0.2f), 10f, 2.2f);

            // 10. ZONE 4: Caldera Stepping Islands (X in [56, 80], Y in [-4, 8]) - HARDEST SECTION
            GameObject zone4 = new GameObject("Zone4_Caldera_SteppingIslands");
            zone4.transform.SetParent(root.transform);

            // Caldera Open Heat Hazard Zone
            CreateHeatZone(zone4, "Zone4_Caldera_HeatZone", 57f, 79f, -2f, 8f);

            // Stepping islands across wide volcanic caldera
            CreateModularPlatform(zone4, "Caldera_Island_1", 60f, 65f, 4.5f, 2.5f, pal);
            CreateModularPlatform(zone4, "Caldera_Island_2", 67f, 72f, 3.0f, 1.0f, pal);
            // Heavy Bandit patrolling Caldera Island 2
            SpawnEnemy(zone4, "Bandit_CalderaIslandGuard", "Assets/Prefabs/Enemies/Bandit_Heavy.prefab", new Vector3(69.5f, 3.0f, 0));

            // High Ruined Pillar with Past Orb Collectible 3
            CreateModularPlatform(zone4, "Caldera_HighPillar_Optional", 69f, 73f, 7.5f, 6.0f, pal);
            CreateSpriteObject(zone4, "HighPillar_Column", new Vector3(71f, 3.5f, 0), new Vector3(1.2f, 5f, 1f), sprColumn, "Main", 0, Color.gray);
            CreateTemporalOrb(zone4, "PastOrb_3_CalderaHighPillar", new Vector3(71f, 8.8f, 0), TemporalOrb.OrbKind.Past);

            // Stepping Island 3 connecting into Ancient Structure
            CreateModularPlatform(zone4, "Caldera_Island_3", 74.5f, 79.5f, 4.0f, 2.0f, pal);

            // Wide Caldera Lava Sea underneath with steam warnings
            CreateLavaPit(zone4, "Caldera_LavaSea", 56f, 80f, -3.5f, -7.0f, pal);
            if (lavaFallsPrefab != null)
            {
                var lf2 = (GameObject)PrefabUtility.InstantiatePrefab(lavaFallsPrefab, zone4.transform);
                lf2.name = "Caldera_Background_LavaFall";
                lf2.transform.position = new Vector3(66f, 1.0f, 0);
            }

            // 11. ZONE 5: Grand Ancient Structure & Final Objective Sanctum (X in [80, 114], Y in [0, 20])
            GameObject zone5 = new GameObject("Zone5_GrandAncientStructure_Sanctum");
            zone5.transform.SetParent(root.transform);

            // Grand Temple Terrace Foundation (Surface Y = 4.0) - SAFE ZONE
            CreateModularPlatform(zone5, "Sanctum_GrandTerrace", 80f, 110f, 4.0f, -2.0f, pal);
            AddSafePlatformZone(zone5, 80f, 110f, 4.0f);

            // Monumental Sanctuary Ceiling
            CreateModularPlatform(zone5, "Sanctum_MonumentalCeiling", 80f, 110f, 18.0f, 16.0f, pal);
            // Right Perimeter Wall (sealing level)
            CreateModularPlatform(zone5, "Sanctum_RightBoundaryWall", 110f, 114f, 18.0f, -2.0f, pal);

            // Ancient Ruined Hall monumental background sprite
            if (sprAncientHall != null)
            {
                CreateSpriteObject(zone5, "Sanctum_AncientHall_Facade", new Vector3(95.0f, 9.5f, 0), new Vector3(3.2f, 3.2f, 1f), sprAncientHall, "Background", -1, new Color(0.95f, 0.85f, 0.8f, 0.95f));
            }

            // Colonnade supporting the grand sanctuary
            CreateSpriteObject(zone5, "Sanctum_Colonnade_1", new Vector3(84f, 10f, 0), new Vector3(1.5f, 12f, 1f), sprColumn, "Main", 0, Color.white);
            CreateSpriteObject(zone5, "Sanctum_Colonnade_2", new Vector3(90f, 10f, 0), new Vector3(1.5f, 12f, 1f), sprColumn, "Main", 0, Color.white);
            CreateSpriteObject(zone5, "Sanctum_Colonnade_3", new Vector3(100f, 10f, 0), new Vector3(1.5f, 12f, 1f), sprColumn, "Main", 0, Color.white);
            CreateSpriteObject(zone5, "Sanctum_Colonnade_4", new Vector3(106f, 10f, 0), new Vector3(1.5f, 12f, 1f), sprColumn, "Main", 0, Color.white);

            // The Sacred Ancient Altar (Central Final Objective Sanctuary)
            GameObject altar = new GameObject("Ancient_Past_Vault_Altar");
            altar.transform.SetParent(zone5.transform);
            altar.transform.position = new Vector3(95.0f, 6.2f, 0);
            var altarSr = altar.AddComponent<SpriteRenderer>();
            altarSr.sprite = sprAncientAltar != null ? sprAncientAltar : sprAncientHall;
            altarSr.sortingLayerName = "Main";
            altarSr.sortingOrder = 1;
            altar.transform.localScale = new Vector3(1.6f, 1.6f, 1f);

            var altarCol = altar.AddComponent<BoxCollider2D>();
            altarCol.size = new Vector2(3.5f, 3.5f);
            altarCol.isTrigger = true;

            // Altar Eternal Flame
            if (fire2Prefab != null)
            {
                var altarFire = (GameObject)PrefabUtility.InstantiatePrefab(fire2Prefab, altar.transform);
                altarFire.name = "Altar_EternalFlame";
                altarFire.transform.position = new Vector3(95.0f, 6.5f, 0);
                altarFire.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            }

            // Sanctum Arena Barrier (Seals entrance at X = 81.0 when boss fight starts)
            GameObject barrier = new GameObject("Sanctum_ArenaBarrier");
            barrier.transform.SetParent(zone5.transform);
            barrier.transform.position = new Vector3(81.0f, 9.0f, 0);
            var barCol = barrier.AddComponent<BoxCollider2D>();
            barCol.size = new Vector2(1.5f, 12.0f);
            var barLight = barrier.AddComponent<Light2D>();
            barLight.lightType = Light2D.LightType.Point;
            barLight.color = new Color(1.0f, 0.25f, 0.05f, 1f);
            barLight.pointLightOuterRadius = 6.0f;
            barLight.intensity = 3.0f;
            if (fire1Prefab != null)
            {
                for (float by = 4.0f; by <= 14.0f; by += 2.5f)
                {
                    var bf = (GameObject)PrefabUtility.InstantiatePrefab(fire1Prefab, barrier.transform);
                    bf.transform.position = new Vector3(81.0f, by, 0);
                }
            }

            // Boss Teleport Arena Nodes
            GameObject nodeL = new GameObject("BossNode_Left");
            nodeL.transform.SetParent(zone5.transform);
            nodeL.transform.position = new Vector3(88.0f, 4.0f, 0);

            GameObject nodeC = new GameObject("BossNode_Center");
            nodeC.transform.SetParent(zone5.transform);
            nodeC.transform.position = new Vector3(95.0f, 4.0f, 0);

            GameObject nodeR = new GameObject("BossNode_Right");
            nodeR.transform.SetParent(zone5.transform);
            nodeR.transform.position = new Vector3(102.0f, 4.0f, 0);

            // Objective Exit Beacon / Gateway (Final Objective Platform)
            GameObject exitGateway = CreateSpriteObject(zone5, "Past_Objective_ExitGateway", new Vector3(104.0f, 5.4f, 0), new Vector3(1.5f, 1.5f, 1f), sprBgDoor, "Main", 0, new Color(0.3f, 0.85f, 1.0f, 1f));
            CreatePointLight(exitGateway, "Exit_Portal_Glow", new Vector3(104.0f, 5.4f, 0), new Color(0.3f, 0.85f, 1.0f), 8f, 3.0f);

            // Spawn Evil Wizard Boss & Hook up EvilWizardBoss component
            GameObject boss = SpawnEnemy(zone5, "EvilWizard_Boss_IgnisSorcerer", "Assets/Prefabs/Enemies/EvilWizard_Boss.prefab", new Vector3(95.0f, 4.0f, 0));
            if (boss != null)
            {
                var wizardComp = boss.GetComponent<EvilWizardBoss>();
                if (wizardComp == null) wizardComp = boss.AddComponent<EvilWizardBoss>();

                var so = new SerializedObject(wizardComp);
                so.FindProperty("arenaBarrier").objectReferenceValue = barrier;
                so.FindProperty("exitGateway").objectReferenceValue = exitGateway;

                var nodesProp = so.FindProperty("teleportNodes");
                nodesProp.arraySize = 3;
                nodesProp.GetArrayElementAtIndex(0).objectReferenceValue = nodeL.transform;
                nodesProp.GetArrayElementAtIndex(1).objectReferenceValue = nodeC.transform;
                nodesProp.GetArrayElementAtIndex(2).objectReferenceValue = nodeR.transform;

                var fbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GrottoEscape2/GrottoEscape2Artwork/Prefabs/AnimatedFireball.prefab");
                if (fbPrefab != null) so.FindProperty("fireballPrefab").objectReferenceValue = fbPrefab;

                if (smokePrefab != null) so.FindProperty("smokeVfxPrefab").objectReferenceValue = smokePrefab;

                var bossMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/[BuildingBlocks]/Audio/SFX/Music_BossArea.ogg");
                if (bossMusic != null) so.FindProperty("bossMusic").objectReferenceValue = bossMusic;

                var castSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/[BuildingBlocks]/Audio/SFX/Player_ProjectileAttack.wav");
                if (castSound != null) so.FindProperty("castSound").objectReferenceValue = castSound;

                var tpSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/[BuildingBlocks]/Audio/SFX/WolfBoss_Shockwave.wav");
                if (tpSound != null) so.FindProperty("teleportSound").objectReferenceValue = tpSound;

                var vicSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/[BuildingBlocks]/Audio/SFX/Stinger_Victory.wav");
                if (vicSound != null) so.FindProperty("victorySound").objectReferenceValue = vicSound;

                so.ApplyModifiedProperties();
            }

            // Ceremonial bonfires & glowing lights at the altar
            if (fire1Prefab != null)
            {
                InstantiatePrefab(fire1Prefab, zone5.transform, new Vector3(92.0f, 4.0f, 0));
                InstantiatePrefab(fire1Prefab, zone5.transform, new Vector3(98.0f, 4.0f, 0));
            }
            if (torchPrefab != null)
            {
                InstantiatePrefab(torchPrefab, zone5.transform, new Vector3(88.0f, 5.5f, 0));
                InstantiatePrefab(torchPrefab, zone5.transform, new Vector3(102.0f, 5.5f, 0));
            }

            // Radiant golden/amber sanctum illumination
            CreatePointLight(zone5, "Altar_RadiantLight", new Vector3(95.0f, 7.0f, 0), new Color(1.0f, 0.8f, 0.3f), 12f, 3.5f);
            CreatePointLight(zone5, "Sanctum_AmberGlow", new Vector3(95.0f, 12.0f, 0), new Color(1.0f, 0.5f, 0.2f), 15f, 2.8f);

            // 12. Instantiate Player Controller with Reliable Physics & Collider Setup
            GameObject playerGo = null;
            if (playerPrefab != null)
            {
                playerGo = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                playerGo.name = "Player";
                // Platform top is Y = 2.0. Player collider offset Y = -0.61, size Y = 2.14 -> bottom is Y - 1.68.
                // Spawning at Y = 3.68 puts feet exactly at Y = 2.00 on the stone platform!
                playerGo.transform.position = new Vector3(-32.0f, 3.68f, 0);

                // Assign frictionless PhysicsMaterial2D to prevent sticking to vertical walls
                var playerMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Dark Platformer/Physics Material/Player Physics Mat.physicsMaterial2D");
                var cap = playerGo.GetComponent<CapsuleCollider2D>();
                if (cap != null && playerMat != null)
                {
                    cap.sharedMaterial = playerMat;
                }

                // Ensure optimal Rigidbody2D settings
                var rb = playerGo.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.interpolation = RigidbodyInterpolation2D.Interpolate;
                    rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                    rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                }

                // Ensure PlayerHealthFeedback for audio, sprite flash, and health notifications
                var healthFeedback = playerGo.GetComponent<TimeTravel.PlayerHealthFeedback>();
                if (healthFeedback == null) healthFeedback = playerGo.AddComponent<TimeTravel.PlayerHealthFeedback>();
                var hurtClip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/[BuildingBlocks]/Audio/SFX/MeleeEnemy_HitReact_01.wav");
                var hfSo = new SerializedObject(healthFeedback);
                hfSo.FindProperty("hurtSound").objectReferenceValue = hurtClip;
                hfSo.ApplyModifiedProperties();

                Undo.RegisterCreatedObjectUndo(playerGo, "Spawn Player");
            }

            // 13. Ensure EventSystem for InputSystem
            var es = Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es == null)
            {
                GameObject esGo = new GameObject("EventSystem");
                esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
                esGo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }

            // 14. Setup Cinemachine Virtual Camera with Level Boundaries Confiner
            SetupCinemachineCamera(playerGo);

            // 15. Global Bottom Elimination Zone (catching any deep falls)
            GameObject elimZone = new GameObject("EliminationZone");
            elimZone.transform.position = new Vector3(35.0f, -14.0f, 0);
            var ezCol = elimZone.AddComponent<BoxCollider2D>();
            ezCol.size = new Vector2(250.0f, 6.0f);
            ezCol.isTrigger = true;
            elimZone.AddComponent<Blocks.Extras.EliminationZone>();

            // Save Scene
            EditorSceneManager.MarkSceneDirty(pastScene);
            EditorSceneManager.SaveScene(pastScene);
            Debug.Log("<color=#00FFAA><b>PAST TIMELINE LEVEL SUCCESSFULLY BUILT & SAVED IN Assets/Scenes/past.unity!</b></color>");
        }

        private static void ConfigureCameraAndLighting(UnityEngine.SceneManagement.Scene scene)
        {
            var camGo = GameObject.Find("Main Camera");
            if (camGo == null)
            {
                camGo = new GameObject("Main Camera");
                var cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = 7.5f;
                camGo.AddComponent<AudioListener>();
            }

            var mainCam = camGo.GetComponent<Camera>();
            if (mainCam != null)
            {
                mainCam.orthographic = true;
                mainCam.orthographicSize = 7.5f;
                mainCam.backgroundColor = new Color(0.12f, 0.05f, 0.04f, 1f); // Deep volcanic dark
            }

            // Ensure UniversalAdditionalCameraData so 2D lights render
            var camData = camGo.GetComponent<UniversalAdditionalCameraData>();
            if (camData == null) camData = camGo.AddComponent<UniversalAdditionalCameraData>();

            // Ensure CinemachineBrain
            var brain = camGo.GetComponent<CinemachineBrain>();
            if (brain == null) brain = camGo.AddComponent<CinemachineBrain>();

            // Global 2D Light (single unified global light for volcanic ambiance)
            var existingLights = Object.FindObjectsByType<Light2D>();
            Light2D globalLight = null;
            foreach (var l in existingLights)
            {
                if (l != null && l.lightType == Light2D.LightType.Global)
                {
                    if (globalLight == null) globalLight = l;
                    else Object.DestroyImmediate(l.gameObject);
                }
            }

            if (globalLight == null)
            {
                GameObject lightMainGo = new GameObject("GlobalLight_VolcanicMain");
                globalLight = lightMainGo.AddComponent<Light2D>();
                globalLight.lightType = Light2D.LightType.Global;
            }
            globalLight.name = "GlobalLight_VolcanicMain";
            globalLight.color = new Color(1.0f, 0.65f, 0.45f, 1f); // Warm fiery volcanic ambiance
            globalLight.intensity = 0.95f;
        }

        private static void SetupCinemachineCamera(GameObject playerGo)
        {
            // Destroy any legacy or conflicting camera objects
            var oldVcam1 = GameObject.Find("CM vcam1");
            if (oldVcam1 != null) Object.DestroyImmediate(oldVcam1);

            var vcamGo = GameObject.Find("CinemachineCamera");
            if (vcamGo == null)
            {
                vcamGo = new GameObject("CinemachineCamera");
            }

            var vcam = vcamGo.GetComponent<CinemachineCamera>();
            if (vcam == null) vcam = vcamGo.AddComponent<CinemachineCamera>();

            vcam.Lens.OrthographicSize = 7.5f;

            if (playerGo != null)
            {
                vcam.Target.TrackingTarget = playerGo.transform;
                vcam.Target.LookAtTarget = playerGo.transform;
                vcam.Follow = playerGo.transform;

                // Explicitly persist serialized properties
                var vcamSo = new SerializedObject(vcam);
                var trackProp = vcamSo.FindProperty("Target.TrackingTarget");
                if (trackProp != null) trackProp.objectReferenceValue = playerGo.transform;
                var lookProp = vcamSo.FindProperty("Target.LookAtTarget");
                if (lookProp != null) lookProp.objectReferenceValue = playerGo.transform;
                vcamSo.ApplyModifiedProperties();
            }

            // Ensure auto-tracker component so target is never lost during play or respawn
            var tracker = vcamGo.GetComponent<TimeTravel.CinemachineTargetAutoTracker>();
            if (tracker == null) tracker = vcamGo.AddComponent<TimeTravel.CinemachineTargetAutoTracker>();
            tracker.EnsureTrackingTarget();

            // Setup PositionComposer
            var composer = vcamGo.GetComponent<CinemachinePositionComposer>();
            if (composer == null) composer = vcamGo.AddComponent<CinemachinePositionComposer>();
            composer.Damping = new Vector3(0.8f, 0.6f, 0f);
            composer.TargetOffset = new Vector3(0f, 0.8f, 0f);
            composer.Composition.DeadZone.Size = new Vector2(0.2f, 0.2f);
            composer.Composition.ScreenPosition = new Vector2(0f, 0f);

            // Level camera boundaries: bounds collider spanning entire level
            GameObject boundsGo = GameObject.Find("LevelCameraBounds");
            if (boundsGo != null) Object.DestroyImmediate(boundsGo);

            boundsGo = new GameObject("LevelCameraBounds");
            var poly = boundsGo.AddComponent<PolygonCollider2D>();
            poly.isTrigger = true;
            poly.points = new Vector2[]
            {
                new Vector2(-42.0f, -8.0f),
                new Vector2(114.0f, -8.0f),
                new Vector2(114.0f, 22.0f),
                new Vector2(-42.0f, 22.0f)
            };

            var confiner = vcamGo.GetComponent<CinemachineConfiner2D>();
            if (confiner == null) confiner = vcamGo.AddComponent<CinemachineConfiner2D>();
            confiner.BoundingShape2D = poly;
            confiner.Damping = 0.5f;

            // Ensure Main Camera has CinemachineBrain
            var mainCamGo = GameObject.Find("Main Camera");
            if (mainCamGo != null)
            {
                var brain = mainCamGo.GetComponent<CinemachineBrain>();
                if (brain == null) brain = mainCamGo.AddComponent<CinemachineBrain>();
            }
        }

        private static void BuildParallaxBackground(GameObject root, Sprite sprSky, Sprite sprMid, Sprite sprFront)
        {
            GameObject bgRoot = new GameObject("00_Parallax_Background");
            bgRoot.transform.SetParent(root.transform);

            // Fiery Sky ($X \in [-60, 140]$, $Y = 10.0$)
            if (sprSky != null)
            {
                for (float sx = -50f; sx <= 140f; sx += 28f)
                {
                    CreateSpriteObject(bgRoot, $"Sky_{sx}", new Vector3(sx, 10f, 10f), new Vector3(3.0f, 3.0f, 1f), sprSky, "Background", -10, Color.white);
                }
            }

            // Mid Smoking Volcanoes ($X \in [-60, 140]$, $Y = 3.0$)
            if (sprMid != null)
            {
                for (float mx = -50f; mx <= 140f; mx += 28f)
                {
                    CreateSpriteObject(bgRoot, $"MidVolcano_{mx}", new Vector3(mx, 3f, 8f), new Vector3(3.0f, 3.0f, 1f), sprMid, "Background", -8, new Color(0.95f, 0.55f, 0.45f, 0.95f));
                }
            }

            // Front Jagged Ridges ($X \in [-60, 140]$, $Y = -2.0$)
            if (sprFront != null)
            {
                for (float fx = -50f; fx <= 140f; fx += 28f)
                {
                    CreateSpriteObject(bgRoot, $"FrontRidge_{fx}", new Vector3(fx, -2f, 6f), new Vector3(3.0f, 3.0f, 1f), sprFront, "Background", -6, new Color(0.85f, 0.40f, 0.30f, 0.9f));
                }
            }
        }

        public static GameObject CreateModularPlatform(GameObject parent, string name, float leftX, float rightX, float topY, float bottomY, PastTilePalette pal)
        {
            float width = rightX - leftX;
            float height = topY - bottomY;
            if (width <= 0f || height <= 0f) return null;

            GameObject platform = new GameObject(name);
            platform.transform.SetParent(parent.transform);
            platform.transform.position = new Vector3((leftX + rightX) * 0.5f, (topY + bottomY) * 0.5f, 0);

            var col = platform.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, height);
            col.offset = Vector2.zero;

            int xCount = Mathf.RoundToInt(width);
            int yCount = Mathf.RoundToInt(height);

            for (int xi = 0; xi < xCount; xi++)
            {
                float worldX = leftX + xi;
                for (int yi = 0; yi < yCount; yi++)
                {
                    float worldY = bottomY + yi + 1;
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
                    else // Interior walls
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

        public static GameObject CreateBridgeScaffold(GameObject parent, string name, float leftX, float rightX, float topY, PastTilePalette pal)
        {
            float width = rightX - leftX;
            GameObject bridge = new GameObject(name);
            bridge.transform.SetParent(parent.transform);
            bridge.transform.position = new Vector3((leftX + rightX) * 0.5f, topY - 0.5f, 0);

            var col = bridge.AddComponent<BoxCollider2D>();
            col.size = new Vector2(width, 1.0f);
            col.offset = Vector2.zero;

            int count = Mathf.RoundToInt(width);
            for (int i = 0; i < count; i++)
            {
                float px = leftX + i;
                CreateSpriteObject(bridge, $"Plank_{i}", new Vector3(px, topY, 0), Vector3.one, pal.bridge, "Main", 0, Color.white);
            }

            return bridge;
        }

        public static GameObject CreateStairs(GameObject parent, string name, Vector3 startPos, int steps, bool ascendRight, Sprite stairSprite)
        {
            GameObject stairs = new GameObject(name);
            stairs.transform.SetParent(parent.transform);
            stairs.transform.position = startPos;

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
                float x = ascendRight ? i : -(i + 1);
                float y = i + 1;
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

        public static void CreateLadder(GameObject parent, string name, Vector3 bottomPos, float height, Sprite ladderSpr)
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
                float y = bottomPos.y + i + 1;
                CreateSpriteObject(ladder, $"Rung_{i}", new Vector3(bottomPos.x - 0.5f, y, 0), Vector3.one, ladderSpr, "Main", 1, Color.white);
            }
        }

        public static void CreateLavaPit(GameObject parent, string name, float leftX, float rightX, float surfaceY, float bedY, PastTilePalette pal)
        {
            GameObject lavaGo = new GameObject(name);
            lavaGo.transform.SetParent(parent.transform);

            // Animated bubbling lava surface
            if (pal.lavaTilePrefab != null)
            {
                for (float lx = leftX + 1.5f; lx < rightX; lx += 3.0f)
                {
                    var lt = (GameObject)PrefabUtility.InstantiatePrefab(pal.lavaTilePrefab, lavaGo.transform);
                    lt.transform.position = new Vector3(lx, surfaceY, 0);
                }
            }

            // Visual Warnings: Rising Steam/Smoke Plumes
            if (pal.smokePrefab != null)
            {
                for (float sx = leftX + 2.0f; sx < rightX; sx += 6.0f)
                {
                    var smoke = (GameObject)PrefabUtility.InstantiatePrefab(pal.smokePrefab, lavaGo.transform);
                    smoke.name = $"{name}_SteamVent_{sx}";
                    smoke.transform.position = new Vector3(sx, surfaceY + 0.5f, 0);
                }
            }

            // Visual Warnings: Amber-red hazard warning lights along the bubbling pit
            for (float hx = leftX + 4.0f; hx < rightX; hx += 8.0f)
            {
                CreatePointLight(lavaGo, $"{name}_HazardWarningLight_{hx}", new Vector3(hx, surfaceY + 0.8f, 0), new Color(1.0f, 0.25f, 0.05f), 5.5f, 2.2f);
            }

            // Lethal hazard trigger
            GameObject hazard = new GameObject($"{name}_HazardTrigger");
            hazard.transform.SetParent(lavaGo.transform);
            hazard.transform.position = new Vector3((leftX + rightX) * 0.5f, (surfaceY + bedY) * 0.5f, 0);

            var box = hazard.AddComponent<BoxCollider2D>();
            box.size = new Vector2(rightX - leftX, surfaceY - bedY + 1.0f);
            box.isTrigger = true;
            hazard.AddComponent<Blocks.Extras.EliminationZone>();
        }

        public static GameObject CreateHeatZone(GameObject parent, string name, float leftX, float rightX, float bottomY, float topY)
        {
            GameObject heatGo = new GameObject(name);
            heatGo.transform.SetParent(parent.transform);
            heatGo.transform.position = new Vector3((leftX + rightX) * 0.5f, (bottomY + topY) * 0.5f, 0);

            var col = heatGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(rightX - leftX, topY - bottomY);
            col.isTrigger = true;

            heatGo.AddComponent<PastHeatZone>();

            // Ambient heat shimmer light
            var lightGo = new GameObject($"{name}_AmbientGlow");
            lightGo.transform.SetParent(heatGo.transform);
            lightGo.transform.localPosition = Vector3.zero;
            var l2d = lightGo.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Point;
            l2d.color = new Color(1.0f, 0.35f, 0.05f, 0.8f);
            l2d.pointLightOuterRadius = Mathf.Max((rightX - leftX) * 0.5f, 6.0f);
            l2d.intensity = 1.2f;

            return heatGo;
        }

        public static GameObject AddSafePlatformZone(GameObject parent, float leftX, float rightX, float topY)
        {
            GameObject safeGo = new GameObject($"SafeZone_{leftX}_{rightX}");
            safeGo.transform.SetParent(parent.transform);
            // Platform surface is topY, collider reaches 2.5m above it so player stands inside safe zone
            safeGo.transform.position = new Vector3((leftX + rightX) * 0.5f, topY + 1.25f, 0);

            var col = safeGo.AddComponent<BoxCollider2D>();
            col.size = new Vector2(rightX - leftX, 2.5f);
            col.isTrigger = true;

            safeGo.AddComponent<PastSafePlatform>();

            // Cooling cyan light indicator
            var lightGo = new GameObject("Safe_CoolingGlow");
            lightGo.transform.SetParent(safeGo.transform);
            lightGo.transform.localPosition = Vector3.zero;
            var l2d = lightGo.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Point;
            l2d.color = new Color(0.2f, 0.85f, 1.0f, 0.7f);
            l2d.pointLightOuterRadius = 4.5f;
            l2d.intensity = 1.5f;

            return safeGo;
        }

        public static GameObject CreateCheckpoint(GameObject parent, string name, Vector3 pos, Sprite runeSprite)
        {
            GameObject checkpoint = new GameObject(name);
            checkpoint.transform.SetParent(parent.transform);
            checkpoint.transform.position = pos;

            var col = checkpoint.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2.5f, 3.5f);
            col.offset = new Vector2(0, 1.75f);
            col.isTrigger = true;

            var sr = checkpoint.AddComponent<SpriteRenderer>();
            sr.sprite = runeSprite;
            sr.color = new Color(0.9f, 0.5f, 0.15f, 0.85f);
            sr.sortingLayerName = "Main";
            sr.sortingOrder = 2;
            checkpoint.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

            var lightGo = new GameObject("Checkpoint_RuneLight");
            lightGo.transform.SetParent(checkpoint.transform);
            lightGo.transform.localPosition = new Vector3(0, 1.75f, 0);
            var l2d = lightGo.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Point;
            l2d.color = new Color(0.9f, 0.5f, 0.15f, 0.85f);
            l2d.pointLightOuterRadius = 5.0f;
            l2d.intensity = 2.0f;

            // Spawn point offset slightly in front
            GameObject respawnGo = new GameObject("RespawnPoint");
            respawnGo.transform.SetParent(checkpoint.transform);
            // Spawning at pos.y + 1.68 puts feet at pos.y (the platform surface)
            respawnGo.transform.position = new Vector3(pos.x, pos.y + 1.68f, 0);

            var cp = checkpoint.AddComponent<ChronoCheckpoint>();
            cp.SetRespawnPoint(respawnGo.transform);

            return checkpoint;
        }

        public static GameObject CreateTemporalOrb(GameObject parent, string name, Vector3 pos, TemporalOrb.OrbKind kind)
        {
            GameObject orb = new GameObject(name);
            orb.transform.SetParent(parent.transform);
            orb.transform.position = pos;

            var comp = orb.AddComponent<TemporalOrb>();
            comp.SetOrbKind(kind);

            var col = orb.AddComponent<CircleCollider2D>();
            col.radius = 0.8f;
            col.isTrigger = true;

            var sr = orb.AddComponent<SpriteRenderer>();
            sr.sprite = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/small-grass.png"); // clean orb base
            Color orbCol = (kind == TemporalOrb.OrbKind.Past) ? new Color(1.0f, 0.4f, 0.1f) : new Color(0.2f, 0.85f, 1f);
            sr.color = orbCol;
            sr.sortingLayerName = "Main";
            sr.sortingOrder = 5;
            orb.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

            var lightGo = new GameObject("Orb_Light");
            lightGo.transform.SetParent(orb.transform);
            lightGo.transform.localPosition = Vector3.zero;
            var l2d = lightGo.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Point;
            l2d.color = orbCol;
            l2d.pointLightOuterRadius = 4.5f;
            l2d.intensity = 2.5f;

            return orb;
        }

        public static GameObject CreateSpriteObject(GameObject parent, string name, Vector3 pos, Vector3 scale, Sprite sprite, string sortingLayer, int order, Color color)
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

        public static void CreatePointLight(GameObject parent, string name, Vector3 pos, Color color, float radius, float intensity)
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

        public static GameObject SpawnEnemy(GameObject parent, string name, string prefabPath, Vector3 pos)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[BuildPastTimelineScene] Enemy prefab at {prefabPath} not found!");
                return null;
            }

            var enemy = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent.transform);
            enemy.name = name;
            enemy.transform.position = pos;
            return enemy;
        }

        private static GameObject InstantiatePrefab(GameObject prefab, Transform parent, Vector3 pos)
        {
            if (prefab == null) return null;
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

        private static Sprite LoadSubSprite(string path, string subName)
        {
            var all = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in all)
            {
                if (a is Sprite s && s.name == subName) return s;
            }
            return LoadSprite(path);
        }

        private static void EnsureSceneInBuildSettings(string scenePath)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var s in scenes)
            {
                if (s.path == scenePath) return;
            }
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        public static void ConfigureEnemyPrefabs()
        {
            // 1. Bandit Light
            string lightPath = "Assets/Prefabs/Enemies/Bandit_Light.prefab";
            var lightObj = PrefabUtility.LoadPrefabContents(lightPath);
            if (lightObj != null)
            {
                var c = lightObj.GetComponent<BuildingBlocksCharacter>();
                if (c != null)
                {
                    var so = new SerializedObject(c);
                    var tagProp = so.FindProperty("targetTag");
                    if (tagProp != null) tagProp.stringValue = "Player";
                    var radProp = so.FindProperty("targetRadius");
                    if (radProp != null) radProp.floatValue = 9.0f;
                    var memProp = so.FindProperty("targetMemoryDuration");
                    if (memProp != null) memProp.floatValue = 2.0f;
                    var statsProp = so.FindProperty("stats");
                    if (statsProp != null && statsProp.arraySize > 0)
                    {
                        var maxProp = statsProp.GetArrayElementAtIndex(0).FindPropertyRelative("maxValue");
                        if (maxProp != null) maxProp.floatValue = 35f;
                        var startProp = statsProp.GetArrayElementAtIndex(0).FindPropertyRelative("startValue");
                        if (startProp != null) startProp.floatValue = 35f;
                    }
                    so.ApplyModifiedProperties();
                }

                var move = lightObj.GetComponent<Blocks.Movement.EnemyMovementAbility>();
                if (move != null)
                {
                    var so = new SerializedObject(move);
                    var wsp = so.FindProperty("wanderSpeed");
                    if (wsp != null) wsp.floatValue = 2.2f;
                    var csp = so.FindProperty("chaseSpeed");
                    if (csp != null) csp.floatValue = 4.5f;
                    var sdp = so.FindProperty("stopDistance");
                    if (sdp != null) sdp.floatValue = 1.2f;
                    var wdp = so.FindProperty("wanderDistance");
                    if (wdp != null) wdp.floatValue = 4.0f;
                    so.ApplyModifiedProperties();
                }

                var atk = lightObj.GetComponent<Blocks.Attack.EnemyMeleeAttackAbility>();
                if (atk != null)
                {
                    var so = new SerializedObject(atk);
                    var rp = so.FindProperty("range");
                    if (rp != null) rp.floatValue = 2.0f;
                    var dp = so.FindProperty("damage");
                    if (dp != null) dp.floatValue = 10f;
                    var wp = so.FindProperty("windupTime");
                    if (wp != null) wp.floatValue = 0.25f;
                    var cp = so.FindProperty("cooldown");
                    if (cp != null) cp.floatValue = 1.2f;
                    var bsp = so.FindProperty("boxSize");
                    if (bsp != null) bsp.vector2Value = new Vector2(1.8f, 1.6f);
                    var bop = so.FindProperty("boxOffset");
                    if (bop != null) bop.vector2Value = new Vector2(1.0f, 0f);
                    so.ApplyModifiedProperties();
                }

                PrefabUtility.SaveAsPrefabAsset(lightObj, lightPath);
                PrefabUtility.UnloadPrefabContents(lightObj);
            }

            // 2. Bandit Heavy
            string heavyPath = "Assets/Prefabs/Enemies/Bandit_Heavy.prefab";
            var heavyObj = PrefabUtility.LoadPrefabContents(heavyPath);
            if (heavyObj != null)
            {
                var c = heavyObj.GetComponent<BuildingBlocksCharacter>();
                if (c != null)
                {
                    var so = new SerializedObject(c);
                    var tagProp = so.FindProperty("targetTag");
                    if (tagProp != null) tagProp.stringValue = "Player";
                    var radProp = so.FindProperty("targetRadius");
                    if (radProp != null) radProp.floatValue = 10.0f;
                    var memProp = so.FindProperty("targetMemoryDuration");
                    if (memProp != null) memProp.floatValue = 2.0f;
                    var statsProp = so.FindProperty("stats");
                    if (statsProp != null && statsProp.arraySize > 0)
                    {
                        var maxProp = statsProp.GetArrayElementAtIndex(0).FindPropertyRelative("maxValue");
                        if (maxProp != null) maxProp.floatValue = 65f;
                        var startProp = statsProp.GetArrayElementAtIndex(0).FindPropertyRelative("startValue");
                        if (startProp != null) startProp.floatValue = 65f;
                    }
                    so.ApplyModifiedProperties();
                }

                var move = heavyObj.GetComponent<Blocks.Movement.EnemyMovementAbility>();
                if (move != null)
                {
                    var so = new SerializedObject(move);
                    var wsp = so.FindProperty("wanderSpeed");
                    if (wsp != null) wsp.floatValue = 1.8f;
                    var csp = so.FindProperty("chaseSpeed");
                    if (csp != null) csp.floatValue = 3.8f;
                    var sdp = so.FindProperty("stopDistance");
                    if (sdp != null) sdp.floatValue = 1.3f;
                    var wdp = so.FindProperty("wanderDistance");
                    if (wdp != null) wdp.floatValue = 4.0f;
                    so.ApplyModifiedProperties();
                }

                var atk = heavyObj.GetComponent<Blocks.Attack.EnemyMeleeAttackAbility>();
                if (atk != null)
                {
                    var so = new SerializedObject(atk);
                    var rp = so.FindProperty("range");
                    if (rp != null) rp.floatValue = 2.2f;
                    var dp = so.FindProperty("damage");
                    if (dp != null) dp.floatValue = 18f;
                    var wp = so.FindProperty("windupTime");
                    if (wp != null) wp.floatValue = 0.35f;
                    var cp = so.FindProperty("cooldown");
                    if (cp != null) cp.floatValue = 1.5f;
                    var bsp = so.FindProperty("boxSize");
                    if (bsp != null) bsp.vector2Value = new Vector2(2.0f, 1.8f);
                    var bop = so.FindProperty("boxOffset");
                    if (bop != null) bop.vector2Value = new Vector2(1.1f, 0f);
                    so.ApplyModifiedProperties();
                }

                PrefabUtility.SaveAsPrefabAsset(heavyObj, heavyPath);
                PrefabUtility.UnloadPrefabContents(heavyObj);
            }

            // 3. Evil Wizard Boss
            string bossPath = "Assets/Prefabs/Enemies/EvilWizard_Boss.prefab";
            var bossObj = PrefabUtility.LoadPrefabContents(bossPath);
            if (bossObj != null)
            {
                var c = bossObj.GetComponent<BuildingBlocksCharacter>();
                if (c != null)
                {
                    var so = new SerializedObject(c);
                    var tagProp = so.FindProperty("targetTag");
                    if (tagProp != null) tagProp.stringValue = "Player";
                    var radProp = so.FindProperty("targetRadius");
                    if (radProp != null) radProp.floatValue = 16.0f;
                    var memProp = so.FindProperty("targetMemoryDuration");
                    if (memProp != null) memProp.floatValue = 3.0f;
                    var statsProp = so.FindProperty("stats");
                    if (statsProp != null && statsProp.arraySize > 0)
                    {
                        var maxProp = statsProp.GetArrayElementAtIndex(0).FindPropertyRelative("maxValue");
                        if (maxProp != null) maxProp.floatValue = 220f;
                        var startProp = statsProp.GetArrayElementAtIndex(0).FindPropertyRelative("startValue");
                        if (startProp != null) startProp.floatValue = 220f;
                    }
                    so.ApplyModifiedProperties();
                }

                var move = bossObj.GetComponent<Blocks.Movement.EnemyMovementAbility>();
                if (move != null)
                {
                    var so = new SerializedObject(move);
                    var wsp = so.FindProperty("wanderSpeed");
                    if (wsp != null) wsp.floatValue = 1.5f;
                    var csp = so.FindProperty("chaseSpeed");
                    if (csp != null) csp.floatValue = 3.0f;
                    var sdp = so.FindProperty("stopDistance");
                    if (sdp != null) sdp.floatValue = 1.8f;
                    var wdp = so.FindProperty("wanderDistance");
                    if (wdp != null) wdp.floatValue = 5.0f;
                    so.ApplyModifiedProperties();
                }

                var atk = bossObj.GetComponent<Blocks.Attack.EnemyMeleeAttackAbility>();
                if (atk != null)
                {
                    var so = new SerializedObject(atk);
                    var rp = so.FindProperty("range");
                    if (rp != null) rp.floatValue = 2.5f;
                    var dp = so.FindProperty("damage");
                    if (dp != null) dp.floatValue = 20f;
                    var wp = so.FindProperty("windupTime");
                    if (wp != null) wp.floatValue = 0.35f;
                    var cp = so.FindProperty("cooldown");
                    if (cp != null) cp.floatValue = 1.4f;
                    var bsp = so.FindProperty("boxSize");
                    if (bsp != null) bsp.vector2Value = new Vector2(2.2f, 2.0f);
                    var bop = so.FindProperty("boxOffset");
                    if (bop != null) bop.vector2Value = new Vector2(1.2f, 0f);
                    so.ApplyModifiedProperties();
                }

                var wiz = bossObj.GetComponent<EvilWizardBoss>();
                if (wiz == null) bossObj.AddComponent<EvilWizardBoss>();

                PrefabUtility.SaveAsPrefabAsset(bossObj, bossPath);
                PrefabUtility.UnloadPrefabContents(bossObj);
            }
        }

        public class PastTilePalette
        {
            public Sprite topL, topM1, topM2, topR;
            public Sprite wallL, wallR;
            public Sprite botL, botM, botR;
            public Sprite fill, bridge, column, stairs, ladder;
            public GameObject fire1Prefab, fire2Prefab, smokePrefab;
            public GameObject lavaTilePrefab, lavaFallsPrefab, torchPrefab;
        }
    }
}
