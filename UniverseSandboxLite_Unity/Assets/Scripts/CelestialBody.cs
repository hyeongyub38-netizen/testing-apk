using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class CelestialBody : MonoBehaviour
{
    [Header("Identity")]
    public string bodyName = "Body";

    [Header("Simulation")]
    public float mass = 1f;
    public float radius = 1f;
    public Vector3 velocity;
    public bool isStar;
    public bool isBlackHole;
    public bool canCollide = true;

    [Header("Guided Impact")]
    public bool guidedImpact;
    public CelestialBody guidedTarget;
    public float guideStrength = 1.15f;

    [Header("Visual")]
    public Color bodyColor = Color.white;
    public TrailRenderer trail;

    public void Init(string newName, float newMass, float newRadius, Vector3 newVelocity, Color newColor, bool star = false, bool blackHole = false)
    {
        bodyName = newName;
        mass = Mathf.Max(0.001f, newMass);
        radius = Mathf.Max(0.01f, newRadius);
        velocity = newVelocity;
        bodyColor = newColor;
        isStar = star;
        isBlackHole = blackHole;

        gameObject.name = bodyName;
        transform.localScale = Vector3.one * radius * 2f;

        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = 0.5f;
        sphereCollider.isTrigger = true;

        ApplyMaterial();
        CreateTrail();
    }

    public void RefreshScale()
    {
        radius = Mathf.Max(0.01f, radius);
        transform.localScale = Vector3.one * radius * 2f;
    }

    public void ClearTrail()
    {
        if (trail != null)
        {
            trail.Clear();
        }
    }

    private void ApplyMaterial()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null) return;

        Shader shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Diffuse");

        Material material = new Material(shader);
        material.color = bodyColor;

        if (shader != null && shader.name == "Standard")
        {
            material.SetFloat("_Glossiness", isBlackHole ? 0.05f : 0.35f);
            material.SetFloat("_Metallic", isBlackHole ? 0.15f : 0f);

            if (isStar)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", bodyColor * 2.6f);
            }
        }

        renderer.material = material;
    }

    private void CreateTrail()
    {
        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = isStar || isBlackHole ? 0f : 80f;
        trail.minVertexDistance = 0.08f;
        trail.startWidth = Mathf.Clamp(radius * 0.12f, 0.03f, 0.25f);
        trail.endWidth = 0f;
        trail.startColor = new Color(bodyColor.r, bodyColor.g, bodyColor.b, 0.8f);
        trail.endColor = new Color(bodyColor.r, bodyColor.g, bodyColor.b, 0f);

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            trail.material = new Material(shader);
        }
    }
}
