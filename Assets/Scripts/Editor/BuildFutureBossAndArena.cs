using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering.Universal;
using TimeTravel;

namespace TimeTravel.Editor
{
    public static class BuildFutureBossAndArena
    {
        [MenuItem("Tools/Fractures of Time/Build Future Boss Arena & Full Map Interconnection")]
        public static void BuildFutureScene()
        {
            BuildInScene("Assets/Scenes/FutureEra.unity");
            BuildInScene("Assets/Scenes/Future.unity");
            Debug.Log("<color=#00FF88><b>[SUCCESS] Future Boss Arena & Full Map Interconnection built in both Future scenes!</b></color>");
        }

        private static void BuildInScene(string scenePath)
        {
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Asset references
            var platformSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Dark Platformer/Sprites/Extra/square.png");
            var ladderSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/GrottoEscape2/GrottoEscape2Artwork/tiles/ladder.png");
            var projSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/[BuildingBlocks]/Art/VFX/Textures/Sprite_Slash.png");
            var droppedOrbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/TimeTravel/DroppedOrb_Future.prefab");
            var bossController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/2D Pixel Art Black Knight/Animations/Black_Knight_Boss.controller");

            // Audio references
            var bossMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/[BuildingBlocks]/Audio/SFX/Music_BossArea.ogg");
            var swordSwing = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/[BuildingBlocks]/Audio/SFX/WolfBoss_Strike.wav");
            var rangedCast = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/[BuildingBlocks]/Audio/SFX/WolfBoss_ChargedStrike.wav");
            var shieldDeflect = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/[BuildingBlocks]/Audio/SFX/WolfBoss_Shockwave.wav");
            var hitGrunt = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/[BuildingBlocks]/Audio/SFX/WolfBoss_HitReact_01.wav");
            var victoryMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/[BuildingBlocks]/Audio/SFX/Stinger_Victory.wav");

            // Knight sprites
            Sprite knightIdleSprite = null;
            var knightAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/2D Pixel Art Black Knight/Sprites/Black_Knight_All_Frame_With_Border.png");
            foreach (var a in knightAssets)
            {
                if (a is Sprite s && (s.name == "Black_Knight_All_Frame_With_Border_18" || (knightIdleSprite == null && s.name.Contains("18"))))
                {
                    knightIdleSprite = s;
                    break;
                }
            }

            var mazeRoot = GameObject.Find("[Platforms_Maze]");
            if (mazeRoot == null)
            {
                mazeRoot = new GameObject("[Platforms_Maze]");
            }

            // =================================================================
            // 1. MAP IMPROVEMENTS — BIDIRECTIONAL CONNECTIVITY (LADDERS & STEPS)
            // =================================================================
            var navFolder = GameObject.Find("[Navigation_Interconnections]");
            if (navFolder != null) Object.DestroyImmediate(navFolder);
            navFolder = new GameObject("[Navigation_Interconnections]");

            // A. Catacombs West to Plaza West Ladder (from -7.5 to -0.6)
            CreateLadder(navFolder.transform, "Ladder_Catacombs_West", new Vector3(-18.0f, -4.0f, 0f), 7.8f, ladderSprite, platformSurfaceY: -0.6f);

            // B. Catacombs East to Plaza East Ladder (from -7.5 to -0.6)
            CreateLadder(navFolder.transform, "Ladder_Catacombs_East", new Vector3(18.0f, -4.0f, 0f), 7.8f, ladderSprite, platformSurfaceY: -0.6f);

            // C. Plaza West to West Terrace L1 Ladder (from -0.6 to 3.8)
            CreateLadder(navFolder.transform, "Ladder_Plaza_To_WestTerrace", new Vector3(-22.0f, 1.6f, 0f), 5.4f, ladderSprite, platformSurfaceY: 3.8f);

            // D. Plaza East to East Terrace L1 Ladder (from -0.6 to 3.5)
            CreateLadder(navFolder.transform, "Ladder_Plaza_To_EastTerrace", new Vector3(22.0f, 1.45f, 0f), 5.4f, ladderSprite, platformSurfaceY: 3.5f);

            // E. Catacombs Chasm Stepping Stones (crossing -7.5 chasm)
            CreatePlatform(navFolder.transform, "Step_Catacomb_1", new Vector3(-6.5f, -7.5f, 0f), new Vector2(4.0f, 0.8f), platformSprite, new Color(0.2f, 0.25f, 0.35f, 1f));
            CreatePlatform(navFolder.transform, "Step_Catacomb_Center", new Vector3(0.0f, -7.5f, 0f), new Vector2(4.0f, 0.8f), platformSprite, new Color(0.2f, 0.25f, 0.35f, 1f));
            CreatePlatform(navFolder.transform, "Step_Catacomb_2", new Vector3(6.5f, -7.5f, 0f), new Vector2(4.0f, 0.8f), platformSprite, new Color(0.2f, 0.25f, 0.35f, 1f));

            // F. Terrace Intermediate Ascent Stepping Stones
            CreatePlatform(navFolder.transform, "Step_West_L1_to_L2", new Vector3(-21.5f, 5.65f, 0f), new Vector2(3.5f, 0.7f), platformSprite, new Color(0.35f, 0.42f, 0.62f, 1f));
            CreatePlatform(navFolder.transform, "Step_East_L1_to_L2", new Vector3(21.5f, 5.35f, 0f), new Vector2(3.5f, 0.7f), platformSprite, new Color(0.35f, 0.42f, 0.62f, 1f));

            CreatePlatform(navFolder.transform, "Step_West_L2_to_High", new Vector3(-21.0f, 9.35f, 0f), new Vector2(3.5f, 0.7f), platformSprite, new Color(0.35f, 0.42f, 0.62f, 1f));
            CreatePlatform(navFolder.transform, "Step_East_L2_to_High", new Vector3(21.0f, 9.10f, 0f), new Vector2(3.5f, 0.7f), platformSprite, new Color(0.35f, 0.42f, 0.62f, 1f));

            // G. High Terraces to Vault Bridge Steps
            CreatePlatform(navFolder.transform, "Step_West_High_to_Vault", new Vector3(-11.5f, 12.0f, 0f), new Vector2(3.5f, 0.7f), platformSprite, new Color(0.35f, 0.42f, 0.62f, 1f));
            CreatePlatform(navFolder.transform, "Step_East_High_to_Vault", new Vector3(11.5f, 11.9f, 0f), new Vector2(3.5f, 0.7f), platformSprite, new Color(0.35f, 0.42f, 0.62f, 1f));

            // =================================================================
            // 2. BOSS ARENA ARCHITECTURE — THE CHRONO CITADEL (x = 35 to 76)
            // =================================================================
            var arenaFolder = GameObject.Find("[Boss_Arena_ChronoCitadel]");
            if (arenaFolder != null) Object.DestroyImmediate(arenaFolder);
            arenaFolder = new GameObject("[Boss_Arena_ChronoCitadel]");

            // Open doorway in Wall_FarEast (replace solid 25m wall with top lintel + portal arch)
            var oldWall = GameObject.Find("Wall_FarEast");
            if (oldWall != null)
            {
                oldWall.transform.position = new Vector3(35.0f, 14.0f, 0f);
                var col = oldWall.GetComponent<BoxCollider2D>();
                if (col != null) col.size = new Vector2(2.0f, 16.0f);
                var sr = oldWall.GetComponent<SpriteRenderer>();
                if (sr != null) sr.size = new Vector2(2.0f, 16.0f);
            }

            // A. Arena Entrance Barrier (seals from y = -0.6 to 6.0 upon boss engagement)
            var barrierGo = new GameObject("Boss_ArenaBarrier");
            barrierGo.transform.SetParent(arenaFolder.transform);
            barrierGo.transform.position = new Vector3(35.0f, 2.7f, 0f);

            var barrierSr = barrierGo.AddComponent<SpriteRenderer>();
            barrierSr.sprite = platformSprite;
            barrierSr.drawMode = SpriteDrawMode.Tiled;
            barrierSr.size = new Vector2(1.2f, 6.6f);
            barrierSr.color = new Color(0.7f, 0.2f, 1f, 0.85f); // Glowing violet forcefield
            barrierSr.sortingLayerName = "Foreground";
            barrierSr.sortingOrder = 5;

            var barrierCol = barrierGo.AddComponent<BoxCollider2D>();
            barrierCol.size = new Vector2(1.2f, 6.6f);

            var barrierLight = barrierGo.AddComponent<Light2D>();
            barrierLight.lightType = Light2D.LightType.Point;
            barrierLight.color = new Color(0.8f, 0.2f, 1f, 1f);
            barrierLight.pointLightOuterRadius = 6.0f;
            barrierLight.intensity = 2.5f;

            barrierGo.SetActive(false); // Starts open until player steps into arena

            // B. Arena Main Floor (seamless continuation of Plaza_Ground_East from 28 to 76)
            CreatePlatform(arenaFolder.transform, "BossArena_Floor", new Vector3(52.0f, -0.6f, 0f), new Vector2(48.0f, 1.2f), platformSprite, new Color(0.28f, 0.35f, 0.52f, 1f));

            // C. Arena Far East Wall
            CreatePlatform(arenaFolder.transform, "BossArena_Wall_East", new Vector3(76.0f, 10.0f, 0f), new Vector2(2.0f, 25.0f), platformSprite, new Color(0.22f, 0.28f, 0.42f, 1f));

            // D. Arena Ceiling
            CreatePlatform(arenaFolder.transform, "BossArena_Ceiling", new Vector3(55.0f, 18.0f, 0f), new Vector2(42.0f, 1.5f), platformSprite, new Color(0.22f, 0.28f, 0.42f, 1f));

            // E. Tactical Battlements & Ledges
            CreatePlatform(arenaFolder.transform, "BossArena_Ledge_West", new Vector3(44.0f, 3.6f, 0f), new Vector2(6.5f, 0.8f), platformSprite, new Color(0.35f, 0.42f, 0.62f, 1f));
            CreateLadder(arenaFolder.transform, "BossArena_Ladder_West", new Vector3(44.0f, 1.5f, 0f), 5.2f, ladderSprite, platformSurfaceY: 3.6f);

            CreatePlatform(arenaFolder.transform, "BossArena_Ledge_East", new Vector3(66.0f, 3.6f, 0f), new Vector2(6.5f, 0.8f), platformSprite, new Color(0.35f, 0.42f, 0.62f, 1f));
            CreateLadder(arenaFolder.transform, "BossArena_Ladder_East", new Vector3(66.0f, 1.5f, 0f), 5.2f, ladderSprite, platformSurfaceY: 3.6f);

            // F. Atmospheric Lighting in Arena
            float[] lightX = new float[] { 40f, 50f, 60f, 70f };
            for (int i = 0; i < lightX.Length; i++)
            {
                var lGo = new GameObject($"Arena_SconceLight_{i}");
                lGo.transform.SetParent(arenaFolder.transform);
                lGo.transform.position = new Vector3(lightX[i], 5.5f, 0f);
                var l = lGo.AddComponent<Light2D>();
                l.lightType = Light2D.LightType.Point;
                l.color = (i % 2 == 0) ? new Color(0.2f, 0.85f, 1f, 1f) : new Color(0.8f, 0.3f, 1f, 1f);
                l.pointLightOuterRadius = 7.5f;
                l.intensity = 1.9f;
            }

            // G. Victory Return Gateway / Exit Portal at x = 72
            var exitPortalGo = new GameObject("Boss_VictoryGateway");
            exitPortalGo.transform.SetParent(arenaFolder.transform);
            exitPortalGo.transform.position = new Vector3(72.0f, 1.5f, 0f);

            var portalSr = exitPortalGo.AddComponent<SpriteRenderer>();
            portalSr.sprite = platformSprite;
            portalSr.drawMode = SpriteDrawMode.Tiled;
            portalSr.size = new Vector2(2.5f, 3.8f);
            portalSr.color = new Color(0.2f, 1f, 0.6f, 0.9f); // Radiant green/cyan gateway
            portalSr.sortingLayerName = "Foreground";
            portalSr.sortingOrder = 4;

            var portalCol = exitPortalGo.AddComponent<BoxCollider2D>();
            portalCol.size = new Vector2(2.5f, 3.8f);
            portalCol.isTrigger = true;

            var cp = exitPortalGo.AddComponent<ChronoPortal>();
            var cpSo = new SerializedObject(cp);
            cpSo.FindProperty("targetSceneName").stringValue = "SampleScene";
            cpSo.FindProperty("requiredEraOrb").enumValueIndex = (int)Era.Present;
            cpSo.FindProperty("requireOrb").boolValue = false; // Free exit after beating boss!
            cpSo.ApplyModifiedProperties();

            var exitLight = exitPortalGo.AddComponent<Light2D>();
            exitLight.lightType = Light2D.LightType.Point;
            exitLight.color = new Color(0.2f, 1f, 0.7f, 1f);
            exitLight.pointLightOuterRadius = 6.0f;
            exitLight.intensity = 2.5f;

            exitPortalGo.SetActive(false); // Activates upon boss defeat

            // =================================================================
            // 3. BLACK KNIGHT BOSS ENTITY
            // =================================================================
            var existingBoss = GameObject.Find("BlackKnight_Boss");
            if (existingBoss != null) Object.DestroyImmediate(existingBoss);

            var bossGo = new GameObject("BlackKnight_Boss");
            float bossScale = 7.5f;
            bossGo.transform.position = new Vector3(58.0f, bossScale * 0.310f, 0f);
            bossGo.transform.localScale = new Vector3(bossScale, bossScale, 1.0f);
            bossGo.tag = "Untagged";

            var bSr = bossGo.AddComponent<SpriteRenderer>();
            bSr.sprite = knightIdleSprite;
            bSr.sortingLayerName = "Foreground";
            bSr.sortingOrder = 10;

            var bAnim = bossGo.AddComponent<Animator>();
            bAnim.runtimeAnimatorController = bossController;

            var bRb = bossGo.AddComponent<Rigidbody2D>();
            bRb.bodyType = RigidbodyType2D.Dynamic;
            bRb.mass = 2.5f;
            bRb.gravityScale = 2.0f;
            bRb.freezeRotation = true;
            bRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var bCol = bossGo.AddComponent<CapsuleCollider2D>();
            bCol.size = new Vector2(0.24f, 0.30f);
            bCol.offset = new Vector2(-0.075f, -0.160f);

            var bAudio = bossGo.AddComponent<AudioSource>();
            bAudio.playOnAwake = false;

            // Shield visual bubble
            var shieldGo = new GameObject("ChronoShieldVisual");
            shieldGo.transform.SetParent(bossGo.transform);
            shieldGo.transform.localPosition = new Vector3(-0.075f, -0.160f, 0f);
            shieldGo.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

            var sSr = shieldGo.AddComponent<SpriteRenderer>();
            sSr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/[BuildingBlocks]/Art/VFX/Textures/HealingParticle.png");
            sSr.color = new Color(0.2f, 0.85f, 1f, 0.75f);
            sSr.sortingLayerName = "Foreground";
            sSr.sortingOrder = 12;

            var sLight = shieldGo.AddComponent<Light2D>();
            sLight.lightType = Light2D.LightType.Point;
            sLight.color = new Color(0.2f, 0.9f, 1f, 1f);
            sLight.pointLightOuterRadius = 4.5f;
            sLight.intensity = 2.5f;

            // Boss controller
            var bossComp = bossGo.AddComponent<BlackKnightBoss>();
            var bossSo = new SerializedObject(bossComp);
            bossSo.FindProperty("bossName").stringValue = "Kaelen, the Void Sentinel";
            bossSo.FindProperty("bossMaxHealth").floatValue = 280f;
            bossSo.FindProperty("arenaTriggerX").floatValue = 38.0f;
            bossSo.FindProperty("arenaBarrier").objectReferenceValue = barrierGo;
            bossSo.FindProperty("exitGateway").objectReferenceValue = exitPortalGo;
            bossSo.FindProperty("chronoShieldVisual").objectReferenceValue = shieldGo;
            bossSo.FindProperty("shieldLight").objectReferenceValue = sLight;
            bossSo.FindProperty("projectileSprite").objectReferenceValue = projSprite;
            bossSo.FindProperty("bossMusic").objectReferenceValue = bossMusic;
            bossSo.FindProperty("swordSwingClip").objectReferenceValue = swordSwing;
            bossSo.FindProperty("rangedCastClip").objectReferenceValue = rangedCast;
            bossSo.FindProperty("shieldDeflectClip").objectReferenceValue = shieldDeflect;
            bossSo.FindProperty("hitGruntClip").objectReferenceValue = hitGrunt;
            bossSo.FindProperty("victoryMusic").objectReferenceValue = victoryMusic;
            bossSo.FindProperty("droppedOrbPrefab").objectReferenceValue = droppedOrbPrefab;

            // Link to switches
            var swA = GameObject.Find("ChronoSwitch_Alpha");
            var swB = GameObject.Find("ChronoSwitch_Beta");
            if (swA != null) bossSo.FindProperty("switchAlpha").objectReferenceValue = swA.GetComponent<ChronoSwitch>();
            if (swB != null) bossSo.FindProperty("switchBeta").objectReferenceValue = swB.GetComponent<ChronoSwitch>();

            bossSo.ApplyModifiedProperties();

            // Background extension into arena
            var bgRoot = GameObject.Find("[Background_PixelArtForest]");
            if (bgRoot != null && bgRoot.transform.childCount > 0)
            {
                var template = bgRoot.transform.GetChild(0);
                for (int i = 2; i <= 4; i++)
                {
                    string bgName = $"ForestLayer_0_{i}";
                    if (GameObject.Find(bgName) == null)
                    {
                        var clone = Object.Instantiate(template.gameObject, bgRoot.transform);
                        clone.name = bgName;
                        clone.transform.position = new Vector3(i * 18.5f, template.transform.position.y, template.transform.position.z);
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static GameObject CreatePlatform(Transform parent, string name, Vector3 pos, Vector2 size, Sprite sprite, Color color)
        {
            var existing = GameObject.Find(name);
            if (existing != null) Object.DestroyImmediate(existing);

            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = size;
            sr.color = color;
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 0;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = size;

            return go;
        }

        private static GameObject CreateLadder(Transform parent, string name, Vector3 pos, float height, Sprite rungSprite, float platformSurfaceY)
        {
            var existing = GameObject.Find(name);
            if (existing != null) Object.DestroyImmediate(existing);

            var go = new GameObject(name);
            go.transform.SetParent(parent);
            go.transform.position = pos;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.4f, height);
            col.isTrigger = true;

            var climb = go.AddComponent<LadderClimb>();
            var climbSo = new SerializedObject(climb);
            climbSo.FindProperty("platformSurfaceYOverride").floatValue = platformSurfaceY;
            climbSo.ApplyModifiedProperties();

            // Visual rungs
            int rungCount = Mathf.Max(3, Mathf.RoundToInt(height / 0.7f));
            float spacing = height / rungCount;
            float startY = -height * 0.5f + spacing * 0.5f;

            for (int i = 0; i < rungCount; i++)
            {
                var rung = new GameObject($"Rung_{i}");
                rung.transform.SetParent(go.transform);
                rung.transform.localPosition = new Vector3(0, startY + (i * spacing), 0);
                rung.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

                var sr = rung.AddComponent<SpriteRenderer>();
                sr.sprite = rungSprite;
                sr.sortingLayerName = "Default";
                sr.sortingOrder = 1;
                sr.color = new Color(0.85f, 0.85f, 0.95f, 1f);
            }

            return go;
        }
    }
}
