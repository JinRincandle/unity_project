using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TimeTravel;

namespace FracturesOfTime.Editor
{
    public static class BuildFireWorldScene
    {
        private const string ScenePath = "Assets/Scenes/Fire.unity";

        [MenuItem("FracturesOfTime/Build Fire World Scene (Past Era)")]
        public static void BuildScene()
        {
            Debug.Log("<color=#FF4400><b>Building Fire World Scene (Past Timeline)...</b></color>");

            // 1. Create or Open Scene
            UnityEngine.SceneManagement.Scene fireScene;
            if (System.IO.File.Exists(ScenePath))
            {
                fireScene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
            else
            {
                fireScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.SaveScene(fireScene, ScenePath);
            }

            // Ensure in Build Settings
            AddSceneToBuildSettings(ScenePath);

            // Clean scene
            var roots = fireScene.GetRootGameObjects();
            foreach (var r in roots)
            {
                if (r.name.Contains("Camera") || r.name.Contains("Light")) continue;
                Undo.DestroyObjectImmediate(r);
            }

            // 2. Load Fire & Lava Assets
            Sprite sprSky = LoadSprite("Assets/ashfall_parallax_pack/layers/01_sky.png");
            Sprite sprMid = LoadSprite("Assets/ashfall_parallax_pack/layers/02_mid.png");
            Sprite sprFront = LoadSprite("Assets/ashfall_parallax_pack/layers/03_front.png");

            Sprite sprAltar = LoadSprite("Assets/underfire/1x/01_the_altar.png");
            Sprite sprLavaRock1 = LoadSprite("Assets/craftpix-161519-free-lava-fields-level-map-2d-backgrounds/_PNG/Parts/part01.png");
            Sprite sprLavaRock2 = LoadSprite("Assets/craftpix-161519-free-lava-fields-level-map-2d-backgrounds/_PNG/Parts/part02.png");
            Sprite sprLavaRock3 = LoadSprite("Assets/craftpix-161519-free-lava-fields-level-map-2d-backgrounds/_PNG/Parts/part03.png");
            Sprite sprLavaRock4 = LoadSprite("Assets/craftpix-161519-free-lava-fields-level-map-2d-backgrounds/_PNG/Parts/part04.png");

            Sprite sprGrottoLavaRock = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/rockblock-big-lava.png");
            Sprite sprGrottoLavaBridge = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/rockblock-bridge-lava.png");
            Sprite sprLavaSolid = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/lava-solid.png");
            Sprite sprLadder = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/ladder.png");
            Sprite sprBgDoor = LoadSprite("Assets/GrottoEscape2/GrottoEscape2Artwork/background/background_door.png");

            GameObject lavaTilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GrottoEscape2/GrottoEscape2Artwork/Prefabs/AnimatedlavaTile.prefab");
            GameObject lavaFallsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/GrottoEscape2/GrottoEscape2Artwork/Prefabs/AnimatedLavaFalls.prefab");
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/[BuildingBlocks]/Prefabs/Player.prefab");
            GameObject heavyBanditPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Bandits - Pixel Art/Demo/HeavyBandit.prefab");
            GameObject lightBanditPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Bandits - Pixel Art/Demo/LightBandit.prefab");
            GameObject meleeEnemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/[BuildingBlocks]/Prefabs/MeleeEnemy.prefab");

            // 3. Setup Fiery Volcanic Lighting
            SetupFireLighting();

            // Root container
            GameObject root = new GameObject("[FIRE_WORLD_PAST]");

            // 4. Parallax Volcanic Sky & Mountains
            GameObject bgGo = new GameObject("00_Volcanic_Parallax");
            bgGo.transform.SetParent(root.transform);
            if (sprSky != null)
                CreateSprite(bgGo, "Sky_Volcanic", new Vector3(0, 10f, 0), new Vector3(4.5f, 4.0f, 1f), sprSky, "Background", -10, new Color(1.0f, 0.65f, 0.45f, 1f));
            if (sprMid != null)
                CreateSprite(bgGo, "Mid_SmokingVolcanos", new Vector3(0, 4f, 0), new Vector3(4.5f, 3.5f, 1f), sprMid, "Background", -8, new Color(0.95f, 0.5f, 0.35f, 0.9f));
            if (sprFront != null)
                CreateSprite(bgGo, "Front_VolcanicRidges", new Vector3(0, 0f, 0), new Vector3(4.5f, 3.0f, 1f), sprFront, "Background", -6, new Color(0.85f, 0.35f, 0.25f, 0.85f));

            // 5. Build Volcanic Lava Platforms
            // Zone A: Arrival Shore ($X \in [-30, -16]$, $Y = 3.0$)
            CreatePlatform(root, "Arrival_Shore", new Vector3(-23f, 2.0f, 0), new Vector2(14f, 2f), sprLavaRock1 != null ? sprLavaRock1 : sprGrottoLavaRock);
            
            // Return Portal to Present at Arrival Shore
            GameObject portal = CreatePortal(root, "ChronoPortal_ToPresent", new Vector3(-27f, 4.0f, 0), sprBgDoor, "SampleScene", Era.Present);

            // Zone B: Lava Crossing Stepping Islands ($X \in [-14, 2]$, $Y \in [1, 5]$)
            CreatePlatform(root, "Lava_Island_1", new Vector3(-12f, 2.5f, 0), new Vector2(5f, 1.5f), sprLavaRock2 != null ? sprLavaRock2 : sprGrottoLavaBridge);
            CreatePlatform(root, "Lava_Island_2", new Vector3(-5f, 4.0f, 0), new Vector2(5f, 1.5f), sprLavaRock3 != null ? sprLavaRock3 : sprGrottoLavaBridge);
            CreatePlatform(root, "Lava_Island_3", new Vector3(2f, 2.5f, 0), new Vector2(5f, 1.5f), sprLavaRock2 != null ? sprLavaRock2 : sprGrottoLavaBridge);

            // Zone C: Molten Conduit Bridge ($X \in [4, 18]$, $Y = 5.0$)
            CreatePlatform(root, "Conduit_Ridge", new Vector3(11f, 4.0f, 0), new Vector2(14f, 2f), sprLavaRock4 != null ? sprLavaRock4 : sprGrottoLavaRock);
            
            // Zone D: Ancient Altar of the Chrono Vault ($X \in [20, 36]$, $Y = 6.0$)
            CreatePlatform(root, "Altar_Sanctuary_Floor", new Vector3(28f, 5.0f, 0), new Vector2(16f, 2f), sprLavaRock1 != null ? sprLavaRock1 : sprGrottoLavaRock);
            CreatePlatform(root, "Altar_Sanctuary_Roof", new Vector3(28f, 13.0f, 0), new Vector2(16f, 2f), sprLavaRock1 != null ? sprLavaRock1 : sprGrottoLavaRock);
            CreatePlatform(root, "Altar_Sanctuary_BackWall", new Vector3(36f, 9.0f, 0), new Vector2(2f, 8f), sprLavaRock4 != null ? sprLavaRock4 : sprGrottoLavaRock);

            // The Ancient Inscription Altar holding the Vault Code!
            GameObject altarGo = new GameObject("Ancient_Vault_Cipher_Altar");
            altarGo.transform.SetParent(root.transform);
            altarGo.transform.position = new Vector3(30f, 7.5f, 0);
            var altarCol = altarGo.AddComponent<BoxCollider2D>();
            altarCol.size = new Vector2(3f, 3f);
            altarCol.isTrigger = true;

            var altarSr = altarGo.AddComponent<SpriteRenderer>();
            altarSr.sprite = sprAltar != null ? sprAltar : sprLavaRock3;
            altarSr.sortingLayerName = "Main";
            altarSr.sortingOrder = 1;
            altarSr.color = Color.white;
            altarGo.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

            var terminal = altarGo.AddComponent<AncientInscriptionTerminal>();
            var altarLight = altarGo.AddComponent<Light2D>();
            altarLight.lightType = Light2D.LightType.Point;
            altarLight.color = new Color(1.0f, 0.45f, 0.1f, 1f);
            altarLight.pointLightOuterRadius = 7.0f;
            altarLight.intensity = 2.5f;

            // 6. Molten Lava Sea & Lava Falls across the bottom
            GameObject lavaGo = new GameObject("Molten_Lava_Sea");
            lavaGo.transform.SetParent(root.transform);
            if (lavaTilePrefab != null)
            {
                for (float lx = -35f; lx <= 40f; lx += 3.0f)
                {
                    var lt = (GameObject)PrefabUtility.InstantiatePrefab(lavaTilePrefab, lavaGo.transform);
                    lt.transform.position = new Vector3(lx, -1.0f, 0);
                }
            }
            if (lavaFallsPrefab != null)
            {
                var lf1 = (GameObject)PrefabUtility.InstantiatePrefab(lavaFallsPrefab, lavaGo.transform);
                lf1.transform.position = new Vector3(-18f, 1.0f, 0);
                var lf2 = (GameObject)PrefabUtility.InstantiatePrefab(lavaFallsPrefab, lavaGo.transform);
                lf2.transform.position = new Vector3(18f, 3.0f, 0);
            }

            // Lava Sea hazard trigger
            GameObject lavaHazard = new GameObject("Lava_Death_Hazard");
            lavaHazard.transform.SetParent(root.transform);
            lavaHazard.transform.position = new Vector3(0, -2.5f, 0);
            var lCol = lavaHazard.AddComponent<BoxCollider2D>();
            lCol.size = new Vector2(100f, 2f);
            lCol.isTrigger = true;
            lavaHazard.AddComponent<Blocks.Extras.EliminationZone>();

            // 7. Heat Hazard System
            GameObject heatGo = new GameObject("HeatHazardSystem");
            heatGo.transform.SetParent(root.transform);
            heatGo.AddComponent<HeatHazard>();

            // 8. Enemies: Fire Sorcerer / Bandits
            if (heavyBanditPrefab != null)
            {
                var bandit = (GameObject)PrefabUtility.InstantiatePrefab(heavyBanditPrefab, root.transform);
                bandit.name = "Fire_Guardian_Bandit";
                bandit.transform.position = new Vector3(24f, 6.5f, 0);
            }
            else if (meleeEnemyPrefab != null)
            {
                var enemy = (GameObject)PrefabUtility.InstantiatePrefab(meleeEnemyPrefab, root.transform);
                enemy.name = "Fire_Guardian_Melee";
                enemy.transform.position = new Vector3(24f, 6.5f, 0);
            }

            if (lightBanditPrefab != null)
            {
                var sentry1 = (GameObject)PrefabUtility.InstantiatePrefab(lightBanditPrefab, root.transform);
                sentry1.name = "Ridge_Lava_Sentry";
                sentry1.transform.position = new Vector3(11f, 5.5f, 0);
            }

            // 9. Instantiate Player & Temporal HUD
            if (playerPrefab != null)
            {
                var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
                player.name = "Player";
                player.transform.position = new Vector3(-23f, 4.68f, 0); // Touching the arrival shore
            }

            // Add EliminationZone far below
            GameObject elim = new GameObject("EliminationZone");
            elim.transform.position = new Vector3(0, -15f, 0);
            var ezCol = elim.AddComponent<BoxCollider2D>();
            ezCol.size = new Vector2(200f, 4f);
            ezCol.isTrigger = true;
            elim.AddComponent<Blocks.Extras.EliminationZone>();

            // Save Scene
            EditorSceneManager.MarkSceneDirty(fireScene);
            EditorSceneManager.SaveScene(fireScene);
            Debug.Log("<color=#FF6600><b>FIRE WORLD SCENE (PAST TIMELINE) SUCCESSFULLY BUILT & SAVED!</b></color>");
        }

        private static void SetupFireLighting()
        {
            var lights = Object.FindObjectsByType<Light2D>();
            bool hasGlobal = false;
            foreach (var l in lights)
            {
                if (l.lightType == Light2D.LightType.Global)
                {
                    hasGlobal = true;
                    l.intensity = 0.95f;
                    l.color = new Color(1.0f, 0.45f, 0.2f, 1f); // Fiery molten amber glow
                }
            }

            if (!hasGlobal)
            {
                GameObject globalLightGo = new GameObject("Global Light - Volcanic");
                var l2d = globalLightGo.AddComponent<Light2D>();
                l2d.lightType = Light2D.LightType.Global;
                l2d.color = new Color(1.0f, 0.5f, 0.25f, 1f);
                l2d.intensity = 0.95f;
            }
        }

        private static GameObject CreatePlatform(GameObject parent, string name, Vector3 pos, Vector2 size, Sprite spr)
        {
            GameObject plat = new GameObject(name);
            plat.transform.SetParent(parent.transform);
            plat.transform.position = pos;

            var col = plat.AddComponent<BoxCollider2D>();
            col.size = size;

            var sr = plat.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.sortingLayerName = "Main";
            sr.sortingOrder = 0;
            sr.color = Color.white;

            return plat;
        }

        private static GameObject CreatePortal(GameObject parent, string name, Vector3 pos, Sprite spr, string targetScene, Era reqOrb)
        {
            GameObject portal = new GameObject(name);
            portal.transform.SetParent(parent.transform);
            portal.transform.position = pos;

            var col = portal.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2.5f, 3.5f);
            col.isTrigger = true;

            var sr = portal.AddComponent<SpriteRenderer>();
            sr.sprite = spr;
            sr.sortingLayerName = "Main";
            sr.sortingOrder = 0;
            sr.color = new Color(0.3f, 0.9f, 1.0f, 1.0f); // Blue portal returning to Present
            portal.transform.localScale = new Vector3(1.3f, 1.3f, 1f);

            var cp = portal.AddComponent<ChronoPortal>();
            var so = new SerializedObject(cp);
            so.FindProperty("targetSceneName").stringValue = targetScene;
            so.FindProperty("requiredEraOrb").enumValueIndex = (int)reqOrb;
            so.ApplyModifiedProperties();

            var lightGo = new GameObject("PortalLight");
            lightGo.transform.SetParent(portal.transform);
            lightGo.transform.localPosition = Vector3.zero;
            var l2d = lightGo.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Point;
            l2d.color = new Color(0.3f, 0.9f, 1.0f, 1.0f);
            l2d.pointLightOuterRadius = 6.0f;
            l2d.intensity = 2.5f;

            return portal;
        }

        private static GameObject CreateSprite(GameObject parent, string name, Vector3 pos, Vector3 scale, Sprite spr, string layer, int order, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.transform.position = pos;
            go.transform.localScale = scale;

            if (spr != null)
            {
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = spr;
                sr.sortingLayerName = layer;
                sr.sortingOrder = order;
                sr.color = color;
            }
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

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var existing = EditorBuildSettings.scenes;
            foreach (var s in existing)
            {
                if (s.path == scenePath) return;
            }

            var updated = new List<EditorBuildSettingsScene>(existing);
            updated.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = updated.ToArray();
        }
    }
}
