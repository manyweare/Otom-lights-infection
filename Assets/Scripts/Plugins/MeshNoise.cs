using UnityEngine;
using System.Collections;

public class MeshNoise : MonoBehaviour
{
    public float scale = 0.05f;
    public float speed = 2f;
    public bool recalculateNormals = false;

    private Vector3[] baseVertices;
    private Perlin noise;

    void Start()
    {
        noise = new Perlin();
    }

    public void Morph()
    {
        StartCoroutine("CoMorph");
    }

    public void StopMorph()
    {
        StopCoroutine("CoMorph");
    }

    IEnumerator CoMorph()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        if (baseVertices == null)
            baseVertices = mesh.vertices;
        var vertices = new Vector3[baseVertices.Length];

        while (true)
        {
            var timex = Time.time * speed + 10.1365143f;
            var timey = Time.time * speed + 21.21688f;
            var timez = Time.time * speed + 32.5564f;

            for (var i = 0; i < vertices.Length; i++)
            {
                var vertex = baseVertices[i];

                vertex.x += noise.Noise(timex + vertex.x, timex + vertex.y, timex + vertex.z) * scale;
                vertex.y += noise.Noise(timey + vertex.x, timey + vertex.y, timey + vertex.z) * scale;
                vertex.z += noise.Noise(timez + vertex.x, timez + vertex.y, timez + vertex.z) * scale;

                vertices[i] = vertex;
            }

            mesh.vertices = vertices;

            if (recalculateNormals)
                mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            yield return null;
        }
    }
}