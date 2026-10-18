using System.Collections;
using UnityEditor;
using UnityEngine;
using TimeTravel;
using Blocks.Character;

namespace FracturesOfTime.Editor
{
    public static class TestFracturedCityPlayability
    {
        [MenuItem("FracturesOfTime/Run PlayMode Diagnostic Test")]
        public static string RunDiagnostic()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== FRACTURES OF TIME: PLAYABILITY & BLUEPRINT VERIFICATION ===");

            // 1. Verify Player
            var player = GameObject.Find("Player");
            if (player == null)
            {
                sb.AppendLine("FAIL: Player not found!");
                return sb.ToString();
            }
            sb.AppendLine($"[1] Player: pos={player.transform.position}, active={player.activeInHierarchy}");

            var hit = Physics2D.Raycast(player.transform.position, Vector2.down, 5f);
            if (hit.collider != null)
            {
                sb.AppendLine($"    Ground beneath player: '{hit.collider.name}' at distance {hit.distance:F2}m (Point: {hit.point})");
            }
            else
            {
                sb.AppendLine("    WARNING: No ground collider directly beneath player!");
            }

            // 2. Verify Ladders & Water Escapes (NEVER GET TRAPPED)
            string[] ladders = {
                "Start_WaterEscape_Ladder",
                "Ladder_To_Zone3_Roof",
                "Ladder_Bridge_To_Underground",
                "Tower_Ladder_F1_to_F2",
                "Tower_Ladder_F2_to_F3",
                "Tower_Ladder_F3_to_Apex",
                "TheGreatLadder_Grotto_To_FinalDoor"
            };

            sb.AppendLine("[2] Vertical Ladders (Two-Way Traversal):");
            int ladderPassCount = 0;
            foreach (var lName in ladders)
            {
                var lObj = GameObject.Find(lName);
                if (lObj != null)
                {
                    var climb = lObj.GetComponent<LadderClimb>();
                    var col = lObj.GetComponent<Collider2D>();
                    sb.AppendLine($"    OK: '{lName}' at {lObj.transform.position}, Climb={climb != null}, Trigger={col != null && col.isTrigger}");
                    ladderPassCount++;
                }
                else
                {
                    sb.AppendLine($"    MISSING: '{lName}'");
                }
            }
            sb.AppendLine($"    Summary: {ladderPassCount}/{ladders.Length} ladders verified.");

            // 3. Verify Moving Platforms
            sb.AppendLine("[3] Moving Platforms & Lifts:");
            string[] movers = { "Bridge_Moving_Platform", "Tower_Energy_Lift" };
            foreach (var mName in movers)
            {
                var mObj = GameObject.Find(mName);
                if (mObj != null)
                {
                    var mover = mObj.GetComponent<PingPongMover>();
                    sb.AppendLine($"    OK: '{mName}' at {mObj.transform.position}, Mover={mover != null}, Speed={mover?.speed}");
                }
                else
                {
                    sb.AppendLine($"    MISSING: '{mName}'");
                }
            }

            // 4. Verify Enemies & Dynamic Orb Drops
            sb.AppendLine("[4] Enemy Orb Drops:");
            var sentry = GameObject.Find("Rooftop_Orb_Sentry");
            if (sentry != null)
            {
                var drop = sentry.GetComponent<EnemyOrbDrop>();
                sb.AppendLine($"    Rooftop Sentry: found, EnemyOrbDrop={drop != null}, Drops={drop?.orbToDrop}");
            }
            else
            {
                sb.AppendLine("    MISSING: Rooftop_Orb_Sentry");
            }

            var boss = GameObject.Find("WolfBossEnemy");
            if (boss != null)
            {
                var drop = boss.GetComponent<EnemyOrbDrop>();
                sb.AppendLine($"    Wolf Boss (Grotto Guardian): found, EnemyOrbDrop={drop != null}, Drops={drop?.orbToDrop}");
            }
            else
            {
                sb.AppendLine("    MISSING: WolfBossEnemy");
            }

            // 5. Verify Puzzles & Final Door
            sb.AppendLine("[5] Puzzle Machinery & Chrono Vault Door:");
            string[] puzzleObjs = { "ChronoControlPanel", "RunePillar_1_Chronos (I)", "RunePillar_2_Sol (II)", "RunePillar_3_Abyss (III)", "ChronoVaultDoor", "MasterTimelineCore" };
            foreach (var pName in puzzleObjs)
            {
                var pObj = GameObject.Find(pName);
                sb.AppendLine($"    {(pObj != null ? "OK" : "MISSING")}: '{pName}' pos={(pObj != null ? pObj.transform.position.ToString() : "N/A")}");
            }

            // 6. Verify Elimination Zone
            var elimZone = GameObject.Find("EliminationZone");
            if (elimZone != null)
            {
                var box = elimZone.GetComponent<BoxCollider2D>();
                sb.AppendLine($"[6] EliminationZone: pos={elimZone.transform.position}, size={box?.size}, isTrigger={box?.isTrigger}");
            }

            sb.AppendLine("=== DIAGNOSTIC COMPLETE ===");
            Debug.Log(sb.ToString());
            return sb.ToString();
        }
    }
}
