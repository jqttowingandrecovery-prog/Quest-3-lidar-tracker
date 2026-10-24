
using UnityEngine;
using System.Collections.Generic;

public class RoomPointCloud : MonoBehaviour
{
    public enum Density { Sparse, Dense, UltraDense }

    public Transform roomMeshRoot;
    public Density density = Density.Dense;
    public float pointSize = 0.008f;
    public int maxPoints = 80000;

    private ParticleSystem particles;

    void Start()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }

        particles = GetComponent<ParticleSystem>();
        if (particles == null)
            particles = gameObject.AddComponent<ParticleSystem>();

        var main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSpeed = 0;
        main.startLifetime = 999999;
        main.maxParticles = maxPoints;
        main.startSize = pointSize;

        var emission = particles.emission;
        emission.enabled = false;

        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SetDensity(Density.Sparse);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SetDensity(Density.Dense);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SetDensity(Density.UltraDense);
    }

    public void SetDensity(Density value)
    {
        density = value;
        RefreshRoom();
    }

    public void SetSparse() => SetDensity(Density.Sparse);
    public void SetDense() => SetDensity(Density.Dense);
    public void SetUltraDense() => SetDensity(Density.UltraDense);

    public void RefreshRoom()
    {
        if (particles == null || roomMeshRoot == null)
            return;

        var points = new List<ParticleSystem.Particle>();
        var meshes = roomMeshRoot.GetComponentsInChildren<MeshFilter>(true);

        int stride = density == Density.Sparse ? 24 :
                     density == Density.Dense ? 8 : 2;

        foreach (var mesh in meshes)
        {
            if (mesh.sharedMesh == null) continue;

            var vertices = mesh.sharedMesh.vertices;
            var matrix = mesh.transform.localToWorldMatrix;

            for (int i = 0; i < vertices.Length; i += stride)
            {
                var p = new ParticleSystem.Particle();
                p.position = matrix.MultiplyPoint3x4(vertices[i]);
                p.startColor = Color.white;
                p.startSize = pointSize;
                p.startLifetime = 999999;
                p.remainingLifetime = 999999;
                points.Add(p);

                if (points.Count >= maxPoints) break;
            }

            if (points.Count >= maxPoints) break;
        }

        particles.Clear();
        if (points.Count > 0)
            particles.SetParticles(points.ToArray(), points.Count);
    }
}

