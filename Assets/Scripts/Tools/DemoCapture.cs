using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

// Screenshot helper, NOT part of the gameplay.
// Only runs when the built game is launched with "-demo win" or "-demo lose".
// It plays the game automatically and saves screenshots to the folder given by "-shots <dir>".
public class DemoCapture : MonoBehaviour
{
    string mode;
    string folder;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        var args = System.Environment.GetCommandLineArgs().ToList();
        int i = args.IndexOf("-demo");
        if (i < 0 || i + 1 >= args.Count) return;

        var go = new GameObject("DemoCapture");
        var d = go.AddComponent<DemoCapture>();
        d.mode = args[i + 1];
        int s = args.IndexOf("-shots");
        d.folder = s >= 0 && s + 1 < args.Count ? args[s + 1] : Application.persistentDataPath;
        Directory.CreateDirectory(d.folder);
    }

    IEnumerator Start()
    {
        var player = FindAnyObjectByType<PlayerController>();
        var playerRb = player.GetComponent<Rigidbody>();
        var enemy = FindAnyObjectByType<EnemyAI>();

        yield return new WaitForSecondsRealtime(1.5f);
        yield return Shot("01_start");

        if (mode == "lose")
        {
            // Walk straight into the enemy.
            while (GameManager.Instance.IsPlaying)
            {
                Vector3 dir = enemy.transform.position - player.transform.position;
                dir.y = 0;
                player.overrideInput = dir.normalized;
                yield return null;
            }
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Shot("06_lose");
            Application.Quit();
            yield break;
        }

        // Show the enemy switching to chase when the player gets close.
        playerRb.position = enemy.transform.position + new Vector3(0, 0, -6f);
        yield return new WaitForSecondsRealtime(0.6f);
        yield return Shot("02_enemy_chase");

        // Take the enemy and hazards out of the way so the win run can't fail.
        enemy.gameObject.SetActive(false);
        foreach (var o in FindObjectsByType<MovingObstacle>())
            o.GetComponent<Collider>().enabled = false;

        yield return new WaitForSecondsRealtime(0.5f);
        yield return Shot("03_obstacles");

        int n = 0;
        foreach (var c in FindObjectsByType<Collectible>().OrderBy(c => c.transform.position.x))
        {
            playerRb.position = c.transform.position + new Vector3(0, -0.25f, -1.5f);
            player.overrideInput = Vector3.forward;
            yield return new WaitForSecondsRealtime(0.5f);
            n++;
            if (n == 4) yield return Shot("04_collecting_score");
        }
        player.overrideInput = Vector3.zero;
        yield return new WaitForSecondsRealtime(0.5f);
        yield return Shot("05_win");
        Application.Quit();
    }

    IEnumerator Shot(string name)
    {
        yield return new WaitForEndOfFrame();
        ScreenCapture.CaptureScreenshot(Path.Combine(folder, name + ".png"));
        yield return null;
        yield return null;
        Debug.Log("[DemoCapture] saved " + name + " | state=" + GameManager.Instance.State + " score=" + GameManager.Instance.Score);
    }
}
