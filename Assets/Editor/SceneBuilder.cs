using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Builds the whole level out of cubes so it can be regenerated any time:
// menu  Tools > Cube Collector > Build Scene.
public static class SceneBuilder
{
    const string ScenePath = "Assets/Scenes/Main.unity";

    [MenuItem("Tools/Cube Collector/Build Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Light
        var light = new GameObject("Directional Light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        light.shadows = LightShadows.Soft;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Materials
        var ground = Mat("Ground", new Color(0.35f, 0.55f, 0.35f));
        var wall = Mat("Wall", new Color(0.55f, 0.55f, 0.6f));
        var blue = Mat("Player", new Color(0.15f, 0.45f, 1f));
        var yellow = Mat("Collectible", new Color(1f, 0.85f, 0.1f));
        var red = Mat("Enemy", new Color(0.6f, 0.1f, 0.1f));
        var orange = Mat("Hazard", new Color(1f, 0.45f, 0.05f));

        // Level root objects keep the Hierarchy tidy
        var level = new GameObject("Level").transform;
        var items = new GameObject("Collectibles").transform;
        var hazards = new GameObject("Obstacles").transform;

        // Ground + border walls (arena is 30 x 30)
        Cube("Ground", level, new Vector3(0, -0.5f, 0), new Vector3(30, 1, 30), ground);
        Cube("Wall N", level, new Vector3(0, 1, 15.5f), new Vector3(32, 2, 1), wall);
        Cube("Wall S", level, new Vector3(0, 1, -15.5f), new Vector3(32, 2, 1), wall);
        Cube("Wall E", level, new Vector3(15.5f, 1, 0), new Vector3(1, 2, 30), wall);
        Cube("Wall W", level, new Vector3(-15.5f, 1, 0), new Vector3(1, 2, 30), wall);

        // Static obstacles (block the player and the enemy)
        Cube("Block 1", level, new Vector3(-6, 1, 0), new Vector3(8, 2, 1), wall);
        Cube("Block 2", level, new Vector3(8, 1, 4), new Vector3(1, 2, 8), wall);
        Cube("Block 3", level, new Vector3(0, 1, 9), new Vector3(6, 2, 1), wall);

        // Player
        var player = Cube("Player", null, new Vector3(0, 0.5f, -12), Vector3.one, blue);
        var prb = player.AddComponent<Rigidbody>();
        prb.constraints = RigidbodyConstraints.FreezeRotation;
        prb.interpolation = RigidbodyInterpolation.Interpolate;
        player.AddComponent<PlayerController>();

        // Collectibles (trigger colliders)
        Vector3[] spots =
        {
            new(-11, 0.75f, -11), new(11, 0.75f, -11), new(-11, 0.75f, 11), new(11, 0.75f, 11),
            new(-4, 0.75f, 4), new(4, 0.75f, -5), new(0, 0.75f, 12), new(12, 0.75f, 0),
        };
        for (int i = 0; i < spots.Length; i++)
        {
            var c = Cube($"Collectible {i + 1}", items, spots[i], Vector3.one * 0.6f, yellow);
            c.GetComponent<BoxCollider>().isTrigger = true;
            c.AddComponent<Collectible>();
        }

        // Moving hazards
        Hazard("Hazard 1", hazards, new Vector3(-13, 0.75f, -6), new Vector3(8, 0, 0), 0.4f, orange);
        Hazard("Hazard 2", hazards, new Vector3(9, 0.75f, -7), new Vector3(0, 0, -6), 0.5f, orange);
        Hazard("Hazard 3", hazards, new Vector3(-8, 0.75f, 7), new Vector3(0, 0, 6), 0.45f, orange);

        // Enemy + patrol waypoints
        var wpRoot = new GameObject("Enemy Waypoints").transform;
        Vector3[] wps = { new(-1, 0.5f, 5), new(5, 0.5f, 5), new(5, 0.5f, -2), new(-1, 0.5f, -2) };
        var waypoints = new Transform[wps.Length];
        for (int i = 0; i < wps.Length; i++)
        {
            waypoints[i] = new GameObject($"Waypoint {i + 1}").transform;
            waypoints[i].SetParent(wpRoot);
            waypoints[i].position = wps[i];
        }
        var enemy = Cube("Enemy", null, wps[0], Vector3.one * 1.1f, red);
        var erb = enemy.AddComponent<Rigidbody>();
        erb.constraints = RigidbodyConstraints.FreezeRotation;
        var ai = enemy.AddComponent<EnemyAI>();
        ai.player = player.transform;
        ai.waypoints = waypoints;
        ai.detectionRange = 7f;

        // Camera
        var cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        var camera = cam.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.12f, 0.14f, 0.2f);
        cam.AddComponent<AudioListener>();
        var follow = cam.AddComponent<CameraFollow>();
        follow.target = player.transform;
        cam.transform.position = player.transform.position + follow.offset;
        cam.transform.LookAt(player.transform);

        // Managers
        var managers = new GameObject("Game Manager");
        managers.AddComponent<GameManager>();
        managers.AddComponent<HUD>();

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        Debug.Log("[SceneBuilder] Scene built at " + ScenePath);
    }

    // Batch-mode entry point: builds a macOS app into Builds/
    public static void BuildMac()
    {
        Build();
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { ScenePath },
            locationPathName = "Builds/CubeCollector.app",
            target = BuildTarget.StandaloneOSX,
        });
        Debug.Log("[SceneBuilder] Build result: " + report.summary.result);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }

    static GameObject Cube(string name, Transform parent, Vector3 pos, Vector3 scale, Material mat)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        if (parent != null) go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = mat;
        return go;
    }

    static void Hazard(string name, Transform parent, Vector3 pos, Vector3 offset, float speed, Material mat)
    {
        var h = Cube(name, parent, pos, new Vector3(1.5f, 1.5f, 1.5f), mat);
        h.AddComponent<Rigidbody>();
        var mo = h.AddComponent<MovingObstacle>();
        mo.moveOffset = offset;
        mo.speed = speed;
    }

    static Material Mat(string name, Color color)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
        string path = $"Assets/Materials/{name}.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        EditorUtility.SetDirty(mat);
        return mat;
    }
}
