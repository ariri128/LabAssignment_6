using UnityEngine;

public class CreateScene : MonoBehaviour
{
    // Required inspector controls
    public int sizeOfForest; // How many trees
    public float forestProximity; // Spread size of trees
    public int baseGridSize; // Pyramid base size

    // Variables used to keep the trees on the plane used as the ground
    private float groundHalfSize = 15f;
    private Vector3 forestCenter = new Vector3(-8f, 0f, -3f);
    private Vector3 pyramidCenter = new Vector3(6f, 0f, 4f);
    private float pyramidPadding = 1.5f;

    // Parent objects for a clean hierarchy
    GameObject ground;
    GameObject forest;
    GameObject pyramid;
    GameObject celestial;

    // Light references for the celestial object
    Light sun;
    Light celestialLight;

    // Variables used to switch celestial object from day to night
    float dayNightTimer;
    bool isNight;

    void Start()
    {
        InitializeVariables();
        CreateGround();
        CreateRandomForest();
        CreatePyramid();
        CreateCelestialObject();
    }

    void Update()
    {
        // Rotates the celestial object and runs the day/night logic
        if (celestial != null) celestial.transform.Rotate(0f, 25f * Time.deltaTime, 0f);
        DayNightCycle();
    }

    void InitializeVariables()
    {
        // Forces inspector values if a value of 0 or negative gets inputed
        if (sizeOfForest < 1) sizeOfForest = 10;
        if (forestProximity <= 0f) forestProximity = 1.5f;
         
        // Clamps the grid size to a min of 3 and max of 10; forces inspector value if 0 is inputed
        baseGridSize = Mathf.Clamp(baseGridSize, 3, 10);
        if (baseGridSize == 0) baseGridSize = 6;

        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
            if (lights[i].type == LightType.Directional) { sun = lights[i]; break; }
    }

    void CreateGround()
    {
        // Makes a parent container
        ground = new GameObject("Ground");

        // Creates a plane primitive and puts it under parent container
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.SetParent(ground.transform);
        plane.transform.position = Vector3.zero;
        plane.transform.localScale = new Vector3(3f, 1f, 3f);

        Renderer r = plane.GetComponent<Renderer>();
        Material m = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        m.color = new Color(0.65f, 0.25f, 0.25f, 1f);
        r.material = m;
    }

    void CreateRandomForest()
    {
        // Makes a parent container
        forest = new GameObject("Forest");

        int count = Mathf.Max(1, sizeOfForest);
        float half = Mathf.Max(1f, groundHalfSize);

        // Makes sure trees don't spawn inside or right next to pyramid as to not create overlap
        float pyramidHalfWidth = (baseGridSize * 1f) / 2f + pyramidPadding;

        int made = 0;
        int tries = 0;

        // Tree spawning loop that picks random position on the ground, makes sure it doesn't overlap with the pyramid, then spawns a tree there
        while (made < count && tries < count * 50)
        {
            tries++;

            float x = Random.Range(-half, half);
            float z = Random.Range(-half, half);

            Vector3 pos = new Vector3(x, 0f, z);

            float dx = Mathf.Abs(pos.x - pyramidCenter.x);
            float dz = Mathf.Abs(pos.z - pyramidCenter.z);

            if (dx < pyramidHalfWidth && dz < pyramidHalfWidth) continue;

            GameObject tree = new GameObject("Tree_" + made);
            tree.transform.SetParent(forest.transform);
            tree.transform.position = pos;

            GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(tree.transform);

            float h = Random.Range(1.5f, 3.2f);
            float r = Random.Range(0.25f, 0.5f);

            trunk.transform.localScale = new Vector3(r, h / 2f, r);
            trunk.transform.localPosition = new Vector3(0f, h / 2f, 0f);

            Renderer tr = trunk.GetComponent<Renderer>();
            Material tm = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            tm.color = new Color(0.2f, 0.5f, 0.2f, 1f);
            tr.material = tm;

            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.transform.SetParent(tree.transform);

            float s = Random.Range(0.9f, 1.8f);
            canopy.transform.localScale = new Vector3(s, s, s);
            canopy.transform.localPosition = new Vector3(0f, h + s * 0.35f, 0f);

            Renderer cr = canopy.GetComponent<Renderer>();
            Material cm = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            cm.color = new Color(0.15f, 0.7f, 0.15f, 1f);
            cr.material = cm;

            made++;
        }
    }

    void CreatePyramid()
    {
        // Makes a parent container
        pyramid = new GameObject("Pyramid");

        // Sets the pyramid placement and sizing
        Vector3 center = new Vector3(6f, 0f, 4f);
        float size = 1f;
        float gap = 0.02f;

        // Level loop ensuring each level has one less cube per side
        for (int level = 0; level < baseGridSize; level++)
        {
            int n = baseGridSize - level;
            float y = size * 0.5f + level * size;

            float width = n * size + (n - 1) * gap;
            float startX = center.x - width / 2f + size / 2f;
            float startZ = center.z - width / 2f + size / 2f;

            // Calls color method used to color each level of the pyramid
            Color c = PyramidColor(level, baseGridSize);

            // Nested loop used for the placement of each cube in the pyramid
            for (int x = 0; x < n; x++)
            {
                for (int z = 0; z < n; z++)
                {
                    GameObject b = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    b.transform.SetParent(pyramid.transform);
                    b.transform.position = new Vector3(startX + x * (size + gap), y, startZ + z * (size + gap));

                    Renderer br = b.GetComponent<Renderer>();
                    Material bm = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    bm.color = c;
                    br.material = bm;
                }
            }
        }
    }

    void CreateCelestialObject()
    {
        // Makes a parent container
        celestial = new GameObject("Celestial");

        // Creates a sphere under the parent container and positions it in the sky (above the other objects in the scene)
        GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        s.transform.SetParent(celestial.transform);
        s.transform.position = new Vector3(0f, 8f, 0f);
        s.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

        Renderer sr = s.GetComponent<Renderer>();
        Material sm = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        sm.color = new Color(0.9f, 0.9f, 1f, 1f);
        sr.material = sm;

        // Attaches a light component directly to the sphere
        celestialLight = s.AddComponent<Light>();
        celestialLight.type = LightType.Point;
        celestialLight.range = 18f;
        celestialLight.intensity = 0.8f;
        celestialLight.color = Color.white;
    }

    void DayNightCycle()
    {
        // Uses a timer to toggle when it's day or night
        dayNightTimer += Time.deltaTime;
        if (dayNightTimer < 6f) return;

        dayNightTimer = 0f;
        isNight = !isNight;

        // Adjusts intensity of the light to ensure visible difference between day and night
        if (sun != null) sun.intensity = isNight ? 0.15f : 1f;

        if (celestialLight != null)
        {
            celestialLight.intensity = isNight ? 1.6f : 0.8f;
            celestialLight.color = isNight ? new Color(0.7f, 0.8f, 1f, 1f) : Color.white;
        }
    }

    // Ensures each level of the pyramid is a different color
    Color PyramidColor(int level, int levels)
    {
        Color a = new Color(0.95f, 0.9f, 0.2f, 1f);
        Color b = new Color(0.95f, 0.7f, 0.35f, 1f);
        Color c = new Color(0.95f, 0.6f, 0.75f, 1f);
        Color d = new Color(0.65f, 0.4f, 0.9f, 1f);
        Color e = new Color(0.9f, 0.15f, 0.15f, 1f);

        float t = (levels <= 1) ? 0f : level / (float)(levels - 1);

        if (t < 0.25f) return Color.Lerp(a, b, t / 0.25f);
        if (t < 0.5f) return Color.Lerp(b, c, (t - 0.25f) / 0.25f);
        if (t < 0.75f) return Color.Lerp(c, d, (t - 0.5f) / 0.25f);
        return Color.Lerp(d, e, (t - 0.75f) / 0.25f);
    }
}
