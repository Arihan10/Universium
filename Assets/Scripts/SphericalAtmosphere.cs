using UnityEngine;

public class SphericalAtmosphere : MonoBehaviour {
    public Material atmosphereMaterial;
    public Transform planetTransform;
    public float atmosphereThickness = 0.2f;

    private void OnRenderObject() {
        if (atmosphereMaterial == null || planetTransform == null) {
            return;
        }

        // Set shader properties
        atmosphereMaterial.SetVector("_PlanetPosition", planetTransform.position);
        // atmosphereMaterial.SetFloat("_AtmosphereThickness", atmosphereThickness);

        /*
        // Draw atmosphere
        Graphics.DrawMeshNow(
            sphereMesh,
            Matrix4x4.TRS(
                planetTransform.position,
                Quaternion.identity,
                Vector3.one * planetTransform.localScale.x * (1 + atmosphereThickness)
            ),
            atmosphereMaterial,
            0
        );
        */
    }

    /* // A sphere mesh to use for rendering the atmosphere
    private static readonly Mesh sphereMesh = CreateSphereMesh();

    private static Mesh CreateSphereMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = new[]
        {
            new Vector3(-1, -1, -1),
            new Vector3(-1, -1, 1),
            new Vector3(-1, 1, -1),
            new Vector3(-1, 1, 1),
            new Vector3(1, -1, -1),
            new Vector3(1, -1, 1),
            new Vector3(1, 1, -1),
            new Vector3(1, 1, 1)
        };
        mesh.triangles = new[]
        {
            0, 1, 2, 2, 1, 3, 3, 1, 7, 7, 1, 5,
            5, 1, 4, 4, 1, 0, 0, 2, 6, 6, 4, 0,
            6, 2, 3, 3, 7, 6, 4, 6, 7, 7, 5, 4
        };
        mesh.normals = mesh.vertices;
        return mesh;
    }
    */
}