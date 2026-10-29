using UnityEngine;

// Draws the score, timer and win/lose screen using Unity's immediate-mode GUI (OnGUI).
public class HUD : MonoBehaviour
{
    private GUIStyle label;
    private GUIStyle big;

    void OnGUI()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        // Scale text with screen height so it looks the same at any resolution.
        float scale = Screen.height / 720f;
        if (label == null)
        {
            label = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            big = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        }
        label.fontSize = Mathf.RoundToInt(28 * scale);
        big.fontSize = Mathf.RoundToInt(64 * scale);

        // Top-left: score and progress. Top-right: timer.
        GUI.Box(new Rect(10, 10, 360 * scale, 90 * scale), "");
        GUI.Label(new Rect(20, 12, 400 * scale, 40 * scale), $"Score: {gm.Score}", label);
        GUI.Label(new Rect(20, 12 + 40 * scale, 400 * scale, 40 * scale), $"Cubes: {gm.Collected} / {gm.TotalCollectibles}", label);

        GUI.Box(new Rect(Screen.width - 230 * scale, 10, 220 * scale, 50 * scale), "");
        label.normal.textColor = gm.TimeLeft < 10f ? Color.red : Color.white;
        GUI.Label(new Rect(Screen.width - 215 * scale, 12, 220 * scale, 45 * scale), $"Time: {Mathf.CeilToInt(gm.TimeLeft)}", label);
        label.normal.textColor = Color.white;

        if (gm.IsPlaying) return;

        // End screen: dark overlay + result.
        GUI.color = new Color(0, 0, 0, 0.65f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        big.normal.textColor = gm.State == GameManager.GameState.Won ? Color.green : Color.red;
        string title = gm.State == GameManager.GameState.Won ? "YOU WIN!" : "GAME OVER";
        GUI.Label(new Rect(0, Screen.height * 0.3f, Screen.width, 100 * scale), title, big);

        var small = new GUIStyle(big) { fontSize = Mathf.RoundToInt(30 * scale) };
        small.normal.textColor = Color.white;
        GUI.Label(new Rect(0, Screen.height * 0.3f + 110 * scale, Screen.width, 50 * scale), gm.EndMessage, small);
        GUI.Label(new Rect(0, Screen.height * 0.3f + 160 * scale, Screen.width, 50 * scale), $"Final score: {gm.Score}", small);
        GUI.Label(new Rect(0, Screen.height * 0.3f + 230 * scale, Screen.width, 50 * scale), "Press R to restart", small);
    }
}
