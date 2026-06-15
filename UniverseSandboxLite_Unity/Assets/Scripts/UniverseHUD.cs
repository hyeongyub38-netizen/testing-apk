using UnityEngine;

public class UniverseHUD : MonoBehaviour
{
    public bool forceMobileHudInEditor;
    public bool showDesktopHudInMobile;

    private Vector2 bodyScroll;

    private bool UseMobileHud => Application.isMobilePlatform || forceMobileHudInEditor;

    private void OnGUI()
    {
        UniverseSimulation simulation = UniverseSimulation.Instance;
        if (simulation == null) return;

        GUI.skin.label.wordWrap = true;

        if (UseMobileHud)
        {
            DrawMobileHUD(simulation);
            if (!showDesktopHudInMobile) return;
        }

        DrawDesktopHUD(simulation);
    }

    private void DrawDesktopHUD(UniverseSimulation simulation)
    {
        GUILayout.BeginArea(new Rect(10f, 10f, 310f, Screen.height - 20f), GUI.skin.box);

        GUILayout.Label("<b>Universe Sandbox Lite</b>");
        GUILayout.Label("Space: pause | L: asteroid | B: black hole | F: focus | V: follow | R: reset");

        GUILayout.Space(8f);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(simulation.paused ? "▶ Resume" : "Ⅱ Pause")) simulation.paused = !simulation.paused;
        if (GUILayout.Button("Reset")) simulation.BuildDefaultSystem();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("🌍 지구로 소행성 발사")) simulation.LaunchAsteroidAtEarth();
        if (GUILayout.Button("🕳 블랙홀 추가")) simulation.AddBlackHoleNearView();
        if (GUILayout.Button("궤도 흔적 지우기")) simulation.ClearAllTrails();

        GUILayout.Space(8f);
        GUILayout.Label("Time Scale: " + simulation.timeScale.ToString("0.0") + "x");
        simulation.timeScale = GUILayout.HorizontalSlider(simulation.timeScale, 0.5f, 120f);

        GUILayout.Label("Gravity: " + simulation.gravityConstant.ToString("0.000"));
        simulation.gravityConstant = GUILayout.HorizontalSlider(simulation.gravityConstant, 0.03f, 0.5f);

        simulation.collisionsEnabled = GUILayout.Toggle(simulation.collisionsEnabled, "Collision / Merge enabled");

        GUILayout.Space(8f);
        DrawSelectedInfo(simulation);

        GUILayout.Space(8f);
        GUILayout.Label("<b>Bodies</b>");
        bodyScroll = GUILayout.BeginScrollView(bodyScroll, GUILayout.Height(220f));
        foreach (CelestialBody body in simulation.bodies)
        {
            if (body == null) continue;
            string prefix = body == simulation.selectedBody ? "● " : "○ ";
            if (GUILayout.Button(prefix + body.bodyName))
            {
                simulation.selectedBody = body;
            }
        }
        GUILayout.EndScrollView();

        GUILayout.Space(8f);
        GUILayout.Label("마우스 우클릭 드래그: 시점 회전\nWASD/QE: 이동\n좌클릭: 행성 선택\n더블클릭: 선택 행성으로 이동");

        GUILayout.EndArea();
    }

    private void DrawMobileHUD(UniverseSimulation simulation)
    {
        float buttonHeight = Mathf.Clamp(Screen.height * 0.075f, 46f, 68f);
        float buttonWidth = Mathf.Clamp(Screen.width * 0.18f, 94f, 150f);
        float gap = 8f;

        GUIStyle bigButton = new GUIStyle(GUI.skin.button)
        {
            fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height * 0.026f, 16f, 24f)),
            wordWrap = true
        };

        GUIStyle label = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.height * 0.023f, 14f, 21f)),
            wordWrap = true
        };

        GUILayout.BeginArea(new Rect(8f, 8f, Screen.width - 16f, buttonHeight + 14f), GUI.skin.box);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(simulation.paused ? "▶" : "Ⅱ", bigButton, GUILayout.Width(buttonWidth * 0.58f), GUILayout.Height(buttonHeight))) simulation.paused = !simulation.paused;
        if (GUILayout.Button("Reset", bigButton, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight))) simulation.BuildDefaultSystem();
        if (GUILayout.Button("T-", bigButton, GUILayout.Width(buttonWidth * 0.62f), GUILayout.Height(buttonHeight))) simulation.timeScale = Mathf.Max(0.5f, simulation.timeScale * 0.5f);
        if (GUILayout.Button("T+", bigButton, GUILayout.Width(buttonWidth * 0.62f), GUILayout.Height(buttonHeight))) simulation.timeScale = Mathf.Min(120f, simulation.timeScale * 1.5f);
        if (GUILayout.Button("성능", bigButton, GUILayout.Width(buttonWidth * 0.85f), GUILayout.Height(buttonHeight))) simulation.ApplyMobilePerformancePreset();
        GUILayout.FlexibleSpace();
        GUILayout.Label("x" + simulation.timeScale.ToString("0"), label, GUILayout.Width(buttonWidth * 0.65f));
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        float rightX = Screen.width - buttonWidth - 10f;
        float startY = buttonHeight + 28f;
        GUILayout.BeginArea(new Rect(rightX, startY, buttonWidth, Screen.height - startY - 12f));
        if (GUILayout.Button("🌍\n발사", bigButton, GUILayout.Height(buttonHeight + 8f))) simulation.LaunchAsteroidAtEarth();
        GUILayout.Space(gap);
        if (GUILayout.Button("🕳\n블랙홀", bigButton, GUILayout.Height(buttonHeight + 8f))) simulation.AddBlackHoleNearView();
        GUILayout.Space(gap);
        if (GUILayout.Button("Focus", bigButton, GUILayout.Height(buttonHeight))) FocusCamera();
        GUILayout.Space(gap);
        if (GUILayout.Button("Follow", bigButton, GUILayout.Height(buttonHeight))) ToggleFollowCamera();
        GUILayout.Space(gap);
        if (GUILayout.Button("Trail X", bigButton, GUILayout.Height(buttonHeight))) simulation.ClearAllTrails();
        GUILayout.Space(gap);
        simulation.collisionsEnabled = GUILayout.Toggle(simulation.collisionsEnabled, "충돌", label, GUILayout.Height(buttonHeight * 0.7f));
        GUILayout.EndArea();

        DrawMobileJoystickHint(label);
        DrawMobileSelectedPanel(simulation, label);
    }

    private void DrawMobileJoystickHint(GUIStyle label)
    {
        float size = Mathf.Clamp(Screen.width * 0.25f, 150f, 230f);
        Rect rect = new Rect(12f, Screen.height - size - 16f, size, size);
        GUI.Box(rect, "");
        GUILayout.BeginArea(rect);
        GUILayout.FlexibleSpace();
        GUILayout.Label("왼쪽 아래\n드래그 = 이동", label);
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();

        Rect lookRect = new Rect(Screen.width * 0.36f, Screen.height - 86f, Screen.width * 0.34f, 72f);
        GUI.Box(lookRect, "오른쪽 드래그 = 시점 / 두 손가락 = 줌 / 행성 탭 = 선택");
    }

    private void DrawMobileSelectedPanel(UniverseSimulation simulation, GUIStyle label)
    {
        float width = Mathf.Clamp(Screen.width * 0.42f, 260f, 430f);
        float height = Mathf.Clamp(Screen.height * 0.22f, 130f, 190f);
        Rect rect = new Rect(10f, Screen.height * 0.13f, width, height);

        GUILayout.BeginArea(rect, GUI.skin.box);
        GUILayout.Label("<b>Universe Sandbox Lite Mobile</b>", label);

        CelestialBody selected = simulation.selectedBody;
        if (selected == null)
        {
            GUILayout.Label("선택: 없음", label);
        }
        else
        {
            GUILayout.Label("선택: " + selected.bodyName, label);
            GUILayout.Label("질량 " + selected.mass.ToString("0.0") + " / 반지름 " + selected.radius.ToString("0.00"), label);
            GUILayout.Label("속도 " + selected.velocity.magnitude.ToString("0.0"), label);
        }

        GUILayout.EndArea();
    }

    private void DrawSelectedInfo(UniverseSimulation simulation)
    {
        CelestialBody selected = simulation.selectedBody;
        if (selected != null)
        {
            GUILayout.Label("<b>Selected</b>");
            GUILayout.Label(selected.bodyName);
            GUILayout.Label("Mass: " + selected.mass.ToString("0.00"));
            GUILayout.Label("Radius: " + selected.radius.ToString("0.00"));
            GUILayout.Label("Velocity: " + selected.velocity.magnitude.ToString("0.00"));
            GUILayout.Label("Position: " + selected.transform.position.ToString("F1"));
        }
        else
        {
            GUILayout.Label("Selected: none");
        }
    }

    private void FocusCamera()
    {
        Camera camera = Camera.main;
        if (camera == null) return;

        CameraController controller = camera.GetComponent<CameraController>();
        if (controller != null) controller.FocusSelectedInstant();
    }

    private void ToggleFollowCamera()
    {
        Camera camera = Camera.main;
        if (camera == null) return;

        CameraController controller = camera.GetComponent<CameraController>();
        if (controller != null) controller.ToggleFollowSelected();
    }
}
