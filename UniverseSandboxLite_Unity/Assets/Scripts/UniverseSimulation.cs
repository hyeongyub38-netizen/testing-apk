using System.Collections.Generic;
using UnityEngine;

public class UniverseSimulation : MonoBehaviour
{
    public static UniverseSimulation Instance { get; private set; }

    [Header("Core Simulation")]
    public float gravityConstant = 0.18f;
    public float timeScale = 9f;
    public int physicsSubsteps = 2;
    public float gravitySoftening = 0.55f;
    public bool paused;

    [Header("Collision")]
    public bool collisionsEnabled = true;
    public float asteroidLaunchSpeed = 8.5f;

    [Header("Runtime")]
    public List<CelestialBody> bodies = new List<CelestialBody>();
    public CelestialBody selectedBody;
    public CelestialBody earthBody;

    private readonly List<CelestialBody> removeBuffer = new List<CelestialBody>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (Application.isMobilePlatform)
        {
            ApplyMobilePerformancePreset();
        }

        BuildDefaultSystem();
    }

    private void FixedUpdate()
    {
        if (paused || bodies.Count == 0) return;

        int substeps = Mathf.Max(1, physicsSubsteps);
        float dt = (Time.fixedDeltaTime * timeScale) / substeps;

        for (int step = 0; step < substeps; step++)
        {
            SimulateStep(dt);
            if (collisionsEnabled)
            {
                CheckCollisions();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) paused = !paused;
        if (Input.GetKeyDown(KeyCode.R)) BuildDefaultSystem();
        if (Input.GetKeyDown(KeyCode.L)) LaunchAsteroidAtEarth();
        if (Input.GetKeyDown(KeyCode.B)) AddBlackHoleNearView();
        if (Input.GetKeyDown(KeyCode.T)) ClearAllTrails();

        if (Input.GetKeyDown(KeyCode.Alpha1)) timeScale = Mathf.Max(0.5f, timeScale * 0.5f);
        if (Input.GetKeyDown(KeyCode.Alpha2)) timeScale = Mathf.Min(120f, timeScale * 1.5f);
    }

    private void SimulateStep(float dt)
    {
        int count = bodies.Count;
        if (count == 0) return;

        Vector3[] accelerations = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            CelestialBody a = bodies[i];
            if (a == null) continue;

            for (int j = i + 1; j < count; j++)
            {
                CelestialBody b = bodies[j];
                if (b == null) continue;

                Vector3 direction = b.transform.position - a.transform.position;
                float sqrDistance = Mathf.Max(direction.sqrMagnitude, gravitySoftening * gravitySoftening);
                Vector3 forceDirection = direction.normalized;

                accelerations[i] += forceDirection * gravityConstant * b.mass / sqrDistance;
                accelerations[j] -= forceDirection * gravityConstant * a.mass / sqrDistance;
            }
        }

        for (int i = 0; i < count; i++)
        {
            CelestialBody body = bodies[i];
            if (body == null) continue;

            if (body.guidedImpact && body.guidedTarget != null)
            {
                Vector3 toTarget = body.guidedTarget.transform.position - body.transform.position;
                float targetDistance = toTarget.magnitude;

                if (targetDistance > body.radius + body.guidedTarget.radius + 0.25f)
                {
                    float speed = Mathf.Max(asteroidLaunchSpeed, body.velocity.magnitude);
                    Vector3 desiredVelocity = body.guidedTarget.velocity + toTarget.normalized * speed;
                    body.velocity = Vector3.Lerp(body.velocity, desiredVelocity, Mathf.Clamp01(body.guideStrength * dt));
                }
            }

            body.velocity += accelerations[i] * dt;
            body.transform.position += body.velocity * dt;
        }
    }

    private void CheckCollisions()
    {
        removeBuffer.Clear();

        for (int i = 0; i < bodies.Count; i++)
        {
            CelestialBody a = bodies[i];
            if (a == null || removeBuffer.Contains(a) || !a.canCollide) continue;

            for (int j = i + 1; j < bodies.Count; j++)
            {
                CelestialBody b = bodies[j];
                if (b == null || removeBuffer.Contains(b) || !b.canCollide) continue;

                float collideDistance = a.radius + b.radius;
                float currentDistance = Vector3.Distance(a.transform.position, b.transform.position);

                if (currentDistance <= collideDistance)
                {
                    HandleCollision(a, b);
                    break;
                }
            }
        }
    }

    private void HandleCollision(CelestialBody a, CelestialBody b)
    {
        if (a == null || b == null) return;

        CelestialBody winner = a.mass >= b.mass ? a : b;
        CelestialBody loser = winner == a ? b : a;

        SpawnImpactEffect((a.transform.position + b.transform.position) * 0.5f, winner.bodyColor, loser.bodyColor, Mathf.Max(a.radius, b.radius));

        float newMass = winner.mass + loser.mass;
        Vector3 newVelocity = ((winner.velocity * winner.mass) + (loser.velocity * loser.mass)) / newMass;
        float newRadius = Mathf.Pow(Mathf.Pow(winner.radius, 3f) + Mathf.Pow(loser.radius, 3f), 1f / 3f);

        winner.mass = newMass;
        winner.velocity = newVelocity;
        winner.radius = newRadius;
        winner.RefreshScale();

        if (winner.isBlackHole || loser.isBlackHole)
        {
            winner.isBlackHole = true;
            winner.bodyColor = Color.black;
        }

        if (selectedBody == loser) selectedBody = winner;
        if (earthBody == loser) earthBody = winner;

        removeBuffer.Add(loser);
        bodies.Remove(loser);
        Destroy(loser.gameObject);
    }

    public void BuildDefaultSystem()
    {
        for (int i = bodies.Count - 1; i >= 0; i--)
        {
            if (bodies[i] != null)
            {
                Destroy(bodies[i].gameObject);
            }
        }

        bodies.Clear();
        selectedBody = null;
        earthBody = null;

        CreateSpaceLights();

        CelestialBody sun = CreateBody("Sun", Vector3.zero, 65000f, 3.0f, Vector3.zero, new Color(1f, 0.72f, 0.22f), true, false);

        CreateOrbitingBody("Mercury", sun, 8f, 0.23f, 0.9f, new Color(0.58f, 0.52f, 0.46f));
        CreateOrbitingBody("Venus", sun, 12f, 0.48f, 5.5f, new Color(0.95f, 0.68f, 0.37f));
        earthBody = CreateOrbitingBody("Earth", sun, 18f, 0.62f, 6.0f, new Color(0.15f, 0.45f, 1f));
        CreateMoon(earthBody);
        CreateOrbitingBody("Mars", sun, 25f, 0.38f, 3.2f, new Color(0.9f, 0.28f, 0.16f));
        CreateOrbitingBody("Jupiter", sun, 39f, 1.35f, 48f, new Color(0.92f, 0.68f, 0.46f));
        CelestialBody saturn = CreateOrbitingBody("Saturn", sun, 52f, 1.08f, 34f, new Color(0.95f, 0.82f, 0.52f));
        CreateRing(saturn, new Color(0.95f, 0.85f, 0.55f, 0.65f), 1.55f, 2.2f);
        CreateOrbitingBody("Uranus", sun, 66f, 0.72f, 14f, new Color(0.45f, 0.9f, 0.95f));
        CreateOrbitingBody("Neptune", sun, 80f, 0.70f, 14f, new Color(0.2f, 0.36f, 1f));

        selectedBody = earthBody;
    }

    private CelestialBody CreateBody(string bodyName, Vector3 position, float mass, float radius, Vector3 velocity, Color color, bool star, bool blackHole)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
        sphere.transform.SetParent(transform);

        CelestialBody body = sphere.AddComponent<CelestialBody>();
        body.Init(bodyName, mass, radius, velocity, color, star, blackHole);
        bodies.Add(body);

        if (star)
        {
            Light light = sphere.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 240f;
            light.intensity = 2.2f;
            light.color = color;
        }

        if (blackHole)
        {
            CreateRing(body, new Color(0.9f, 0.45f, 1f, 0.75f), 1.7f, 2.55f);
        }

        return body;
    }

    private CelestialBody CreateOrbitingBody(string bodyName, CelestialBody parent, float distance, float radius, float mass, Color color)
    {
        Vector3 position = parent.transform.position + new Vector3(distance, 0f, 0f);
        float speed = Mathf.Sqrt((gravityConstant * parent.mass) / Mathf.Max(distance, 0.1f));
        Vector3 velocity = parent.velocity + new Vector3(0f, 0f, speed);
        return CreateBody(bodyName, position, mass, radius, velocity, color, false, false);
    }

    private CelestialBody CreateMoon(CelestialBody earth)
    {
        if (earth == null) return null;

        Vector3 position = earth.transform.position + new Vector3(1.7f, 0f, 0f);
        float speed = Mathf.Sqrt((gravityConstant * earth.mass) / 1.7f);
        Vector3 velocity = earth.velocity + new Vector3(0f, 0f, speed);
        return CreateBody("Moon", position, 0.18f, 0.16f, velocity, new Color(0.82f, 0.82f, 0.78f), false, false);
    }

    public void LaunchAsteroidAtEarth()
    {
        if (earthBody == null) return;

        Camera camera = Camera.main;
        Vector3 outward = Vector3.forward;

        if (camera != null)
        {
            outward = (camera.transform.position - earthBody.transform.position).normalized;
            if (outward.sqrMagnitude < 0.1f) outward = Vector3.forward;
        }

        Vector3 spawnPosition = earthBody.transform.position + outward * 22f + Random.insideUnitSphere * 1.6f;
        Vector3 targetVelocity = earthBody.velocity + (earthBody.transform.position - spawnPosition).normalized * asteroidLaunchSpeed;

        CelestialBody asteroid = CreateBody("Guided Asteroid", spawnPosition, 0.08f, 0.18f, targetVelocity, new Color(0.55f, 0.45f, 0.36f), false, false);
        asteroid.guidedImpact = true;
        asteroid.guidedTarget = earthBody;
        asteroid.guideStrength = 1.4f;
        selectedBody = asteroid;
    }

    public void AddBlackHoleNearView()
    {
        Camera camera = Camera.main;
        Vector3 position = new Vector3(-24f, 0f, -16f);

        if (camera != null)
        {
            position = camera.transform.position + camera.transform.forward * 32f;
        }

        CelestialBody blackHole = CreateBody("Black Hole", position, 160000f, 1.15f, Vector3.zero, Color.black, false, true);
        selectedBody = blackHole;
    }

    public void ClearAllTrails()
    {
        foreach (CelestialBody body in bodies)
        {
            if (body != null) body.ClearTrail();
        }
    }

    public void ApplyMobilePerformancePreset()
    {
        physicsSubsteps = 1;
        gravitySoftening = 0.75f;
        timeScale = Mathf.Clamp(timeScale, 4f, 30f);
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        foreach (CelestialBody body in bodies)
        {
            if (body != null && body.trail != null)
            {
                body.trail.time = Mathf.Min(body.trail.time, 38f);
                body.trail.minVertexDistance = Mathf.Max(body.trail.minVertexDistance, 0.18f);
            }
        }
    }

    public void SelectNextBody()
    {
        if (bodies.Count == 0) return;

        int index = Mathf.Max(0, bodies.IndexOf(selectedBody));
        index = (index + 1) % bodies.Count;
        selectedBody = bodies[index];
    }

    private void CreateRing(CelestialBody body, Color color, float innerRadiusMultiplier, float outerRadiusMultiplier)
    {
        if (body == null) return;

        GameObject ring = new GameObject(body.bodyName + " Ring");
        ring.transform.SetParent(body.transform);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localRotation = Quaternion.Euler(75f, 0f, 0f);

        LineRenderer line = ring.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 160;
        line.widthMultiplier = Mathf.Max(0.02f, body.radius * 0.08f);
        line.startColor = color;
        line.endColor = color;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null) line.material = new Material(shader);

        float ringRadius = body.radius * ((innerRadiusMultiplier + outerRadiusMultiplier) * 0.5f);
        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = (Mathf.PI * 2f * i) / line.positionCount;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * ringRadius, 0f, Mathf.Sin(angle) * ringRadius));
        }
    }

    private void CreateSpaceLights()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.07f, 0.08f, 0.12f);
    }

    private void SpawnImpactEffect(Vector3 position, Color a, Color b, float size)
    {
        GameObject effectObject = new GameObject("Impact FX");
        effectObject.transform.position = position;

        ParticleSystem particleSystem = effectObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particleSystem.main;
        main.startLifetime = 1.2f;
        main.startSpeed = 5.2f + size;
        main.startSize = Mathf.Clamp(size * 0.18f, 0.08f, 0.35f);
        main.maxParticles = 240;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = Color.Lerp(a, b, 0.5f);

        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = Mathf.Clamp(size * 0.25f, 0.1f, 1.5f);

        particleSystem.Emit(Mathf.RoundToInt(Mathf.Clamp(55f + size * 24f, 55f, 180f)));
        Destroy(effectObject, 2.5f);
    }
}
