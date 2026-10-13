using UnityEngine;

/// <summary>
/// Builds the whole level at runtime: platforms, coins, goal, player, camera, HUD.
/// Also tracks game state (coins, win, respawn).
/// HUD is made of sprites parented to the camera (no uGUI, no font atlases - reliable in WebGL).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static bool Won;

    int coinsTotal;
    int coinsGot;
    Vector3 spawnPoint = new Vector3(-12f, 0.9f, 0f);

    SpriteRenderer[] coinIcons;     // filled = gold, empty = dark
    SpriteRenderer winBanner;
    SpriteRenderer winStar;

    void Awake()
    {
        Instance = this;
        Won = false;
    }

    void Start()
    {
        BuildWorld();
        Time.timeScale = 1f;
    }

    // ---------------------------------------------------------------- world

    void Platform(float x, float y, float w, float h, Color c)
    {
        var go = new GameObject("Platform_" + x.ToString("0"));
        go.transform.position = new Vector3(x, y, 0f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.White();
        sr.color = c;
        sr.drawMode = UnityEngine.SpriteDrawMode.Sliced;
        sr.size = new Vector2(w, h);
        var box = go.AddComponent<BoxCollider2D>();
        box.size = new Vector2(w, h);
    }

    void Ground(float x, float w)
    {
        Platform(x, -1.25f, w, 2.5f, new Color(0.32f, 0.34f, 0.38f));
        Platform(x, 0f, w, 0.35f, new Color(0.36f, 0.62f, 0.36f));
    }

    void BuildWorld()
    {
        var wood = new Color(0.62f, 0.45f, 0.30f);

        // Ground segments: x-center and width
        Ground(-12f, 10f);
        Ground(1.5f, 9f);
        Ground(13.5f, 9f);

        // Floating platforms: x, y, width
        Platform(-8.5f, 1.1f, 3.2f, 0.5f, wood);
        Platform(-4.5f, 2.4f, 2.6f, 0.5f, wood);
        Platform(-0.5f, 1.4f, 2.6f, 0.5f, wood);
        Platform(3.2f, 2.6f, 3.0f, 0.5f, wood);
        Platform(7.0f, 1.3f, 2.4f, 0.5f, wood);
        Platform(10.6f, 2.8f, 2.8f, 0.5f, wood);
        Platform(14.4f, 1.6f, 2.6f, 0.5f, wood);

        // Wall at the start so the player cannot run left off the map
        Platform(-17.2f, 2f, 0.6f, 7f, new Color(0.32f, 0.34f, 0.38f));

        BuildPlayer();

        // Coins above platforms and over gaps
        AddCoinWorld(-8.5f, 2.1f);
        AddCoinWorld(-4.5f, 3.4f);
        AddCoinWorld(-0.5f, 2.4f);
        AddCoinWorld(3.2f, 3.6f);
        AddCoinWorld(7.0f, 2.3f);
        AddCoinWorld(10.6f, 3.8f);
        AddCoinWorld(14.4f, 2.6f);
        AddCoinWorld(-2f, 0.3f);
        AddCoinWorld(8.8f, 0.3f);
        coinsTotal = 9;

        // Goal flag at the end + its pole and landing platform
        var goal = new GameObject("Goal");
        goal.transform.position = new Vector3(16.5f, 2.8f, 0f);
        var gsr = goal.AddComponent<SpriteRenderer>();
        gsr.sprite = SpriteFactory.White();
        gsr.color = new Color(0.9f, 0.25f, 0.3f);
        gsr.drawMode = UnityEngine.SpriteDrawMode.Sliced;
        gsr.size = new Vector2(1.1f, 1.4f);
        var gbox = goal.AddComponent<BoxCollider2D>();
        gbox.isTrigger = true;
        gbox.size = new Vector2(1.1f, 1.4f);
        goal.AddComponent<Goal>();

        Platform(16.5f, 0.9f, 0.18f, 3.4f, new Color(0.85f, 0.85f, 0.88f));
        Platform(16.5f, -0.95f, 3.4f, 0.6f, new Color(0.32f, 0.34f, 0.38f));
        Platform(16.5f, -0.55f, 3.4f, 0.3f, new Color(0.36f, 0.62f, 0.36f));

        // Camera + HUD last, so the coin counter knows coinsTotal
        BuildCamera();
    }

    void AddCoinWorld(float x, float y)
    {
        var go = new GameObject("Coin");
        go.transform.position = new Vector3(x, y, 0f);
        var csr = go.AddComponent<SpriteRenderer>();
        csr.sprite = SpriteFactory.Circle();
        csr.color = new Color(1f, 0.82f, 0.2f);
        go.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        var circ = go.AddComponent<CircleCollider2D>();
        circ.isTrigger = true;
        circ.radius = 0.9f;
        go.AddComponent<Coin>();
    }

    void BuildPlayer()
    {
        var player = new GameObject("Player");
        player.transform.position = spawnPoint;
        var sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteFactory.Circle();
        sr.color = new Color(0.95f, 0.55f, 0.15f);
        var body = player.AddComponent<Rigidbody2D>();
        var cap = player.AddComponent<CapsuleCollider2D>();
        cap.size = new Vector2(0.9f, 1.5f);
        cap.offset = new Vector2(0f, 0.05f);
        player.AddComponent<PlayerController>();

        // Eyes so the circle reads as a character and shows direction
        var eye = new GameObject("Eye");
        eye.transform.SetParent(player.transform, false);
        eye.transform.localPosition = new Vector3(0.22f, 0.35f, 0f);
        var eyeSr = eye.AddComponent<SpriteRenderer>();
        eyeSr.sprite = SpriteFactory.Circle();
        eyeSr.color = Color.white;
        eyeSr.transform.localScale = new Vector3(0.22f, 0.22f, 1f);
        var pupil = new GameObject("Pupil");
        pupil.transform.SetParent(eye.transform, false);
        var pupilSr = pupil.AddComponent<SpriteRenderer>();
        pupilSr.sprite = SpriteFactory.Circle();
        pupilSr.color = new Color(0.15f, 0.15f, 0.18f);
        pupilSr.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        pupilSr.transform.localPosition = new Vector3(0.25f, 0f, -0.01f);

        player.AddComponent<PlayerFaceDirection>();
    }

    void BuildCamera()
    {
        var camGo = new GameObject("GameCamera");
        camGo.transform.position = new Vector3(spawnPoint.x, 1f, -10f);
        var cam = camGo.AddComponent<Camera>();
        cam.backgroundColor = new Color(0.55f, 0.75f, 0.92f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.cullingMask = ~0;
        camGo.AddComponent<AudioListener>();
        var follow = camGo.AddComponent<CameraFollow>();
        follow.target = GameObject.Find("Player").transform;

        BuildHud(camGo);
    }

    // ---------------------------------------------------------------- hud (sprites, camera-space)

    void BuildHud(GameObject camGo)
    {
        // Coin counter: filled gold circles for collected, dark for remaining
        coinIcons = new SpriteRenderer[coinsTotal];
        for (int i = 0; i < coinsTotal; i++)
        {
            var icon = new GameObject("CoinIcon_" + i);
            icon.transform.SetParent(camGo.transform, false);
            icon.transform.localPosition = new Vector3(-7.55f + i * 0.62f, 4.45f, 10f);
            icon.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            var sr = icon.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Circle();
            sr.sortingOrder = 100;
            coinIcons[i] = sr;
        }
        RefreshCoinIcons();

        // Win banner (hidden until victory)
        var bannerGo = new GameObject("WinBanner");
        bannerGo.transform.SetParent(camGo.transform, false);
        bannerGo.transform.localPosition = new Vector3(0f, 0.8f, 10f);
        winBanner = bannerGo.AddComponent<SpriteRenderer>();
        winBanner.sprite = SpriteFactory.White();
        winBanner.drawMode = UnityEngine.SpriteDrawMode.Sliced;
        winBanner.size = new Vector2(8f, 2.2f);
        winBanner.color = new Color(0.16f, 0.5f, 0.25f, 0.92f);
        winBanner.sortingOrder = 90;
        winBanner.enabled = false;

        var star = new GameObject("WinStar");
        star.transform.SetParent(bannerGo.transform, false);
        star.transform.localPosition = new Vector3(-3.2f, 0f, -0.01f);
        var starSr = star.AddComponent<SpriteRenderer>();
        starSr.sprite = SpriteFactory.Circle();
        starSr.color = new Color(1f, 0.82f, 0.2f);
        starSr.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
        starSr.sortingOrder = 91;
        starSr.enabled = false; // shown together with the banner on win
        winStar = starSr;
    }

    void RefreshCoinIcons()
    {
        if (coinIcons == null) return;
        for (int i = 0; i < coinIcons.Length; i++)
        {
            if (coinIcons[i] == null) continue;
            coinIcons[i].color = i < coinsGot
                ? new Color(1f, 0.82f, 0.2f, 1f)
                : new Color(0f, 0f, 0f, 0.25f);
        }
    }

    // ---------------------------------------------------------------- state

    public static void AddCoinToCounter()
    {
        if (Instance == null) return;
        Instance.coinsGot++;
        Instance.RefreshCoinIcons();
    }

    public static void Win()
    {
        if (Instance == null || Won) return;
        Won = true;
        if (Instance.winBanner != null)
            Instance.winBanner.enabled = true;
        if (Instance.winStar != null)
            Instance.winStar.enabled = true;
    }

    void OnGUI()
    {
        if (!Won) return;
        var box = new Rect(Screen.width / 2f - 260f, Screen.height / 2f - 90f, 520f, 60f);
        GUI.Box(box, "");
        GUI.Label(new Rect(box.x, box.y + 12f, box.width, 40f),
            "YOU WIN!  Coins: " + coinsGot + "/" + coinsTotal + "  (reload the page to play again)");
    }

    public static void Respawn(PlayerController p)
    {
        if (Instance == null) return;
        p.transform.position = Instance.spawnPoint;
        p.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
    }
}
