using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class GpuParticle : MonoBehaviour{
    struct Particle
    {
        public Vector3 position;   //vec3 = float3 = 12B
        public Vector3 v;
        public Vector3 a;
        public Vector3 color;
    }
    int ThreadBlockSize = 256;
    int blockPerGrid;
    ComputeBuffer ParticleBuffer,argsBuffer;
    private uint[] _args;
    private float time;
    private double dt = 0.01;
    public bool use_GPU = true;
    static public int N = 16384;
    [Range(0, 1)]
    public float lerpt;
    [Range(0.001f, 0.008f)]
    public float size=0.01f;  //0.01
    [SerializeField]
    private Mesh Particle_Mesh;
    [SerializeField]
    ComputeShader _computeShader;
    [SerializeField]
    private Material _material;


    Particle[] particles = new Particle[N];

    void Start(){
        
      
        for (int i = 0; i < N; i++)
        {
               double x = Random.value * 2 - 1;       
               double y = Random.value * 2 - 1;
            double x_v = Random.value * 2 - 1;
            double y_v = Random.value * 2 - 1;
            particles[i].position = new Vector3((float)x, (float)y, 0);
            particles[i].v = new Vector3(0, 0, 0);
            //   particles[i].v = new Vector3((float)x_v, (float)y_v, 0);
            particles[i].a = new Vector3(0, 0, 0);
                particles[i].color = new Vector3(1, 1, 1);
        }

        ComputeBuffer computeBuffer = new ComputeBuffer(N, 48);
        ParticleBuffer = computeBuffer;
        ParticleBuffer.SetData(particles);   

        _args = new uint[5] { 0, 0, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        time = 4f;
        blockPerGrid = (N + ThreadBlockSize - 1) / ThreadBlockSize;
        
    }

    
    void Update(){
        time -= Time.deltaTime;

        //transform.Rotate(new Vector3(0f, Time.deltaTime * 10f, 0f));
        if (use_GPU == true)
        {
            updatebuffer();
            argsBuffer.SetData(_args);
            Graphics.DrawMeshInstancedIndirect(Particle_Mesh, 0, _material, new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f)), argsBuffer);
        }
        else
        {
            cpu_updatebuffer();
            for (int i = 0; i < N; i++)
            {
                Graphics.DrawMesh(Particle_Mesh, particles[i].position, Quaternion.identity, _material, 0);

            }
        }

    }   


    void updatebuffer()
    {
        int kernelId = _computeShader.FindKernel("MainCS");  
   
        _computeShader.SetBuffer(kernelId, "_ParticleBuffer", ParticleBuffer);  
        _computeShader.Dispatch(kernelId, blockPerGrid, 1, 1);
        _args[0] = (uint)Particle_Mesh.GetIndexCount(0);  //36
        _args[1] = (uint)N;
        _args[2] = (uint)Particle_Mesh.GetIndexStart(0);  //0
        _args[3] = (uint)Particle_Mesh.GetBaseVertex(0);  //0

     
        _material.SetBuffer("_ParticleBuffer", ParticleBuffer);
        _material.SetMatrix("_GameobjectMatrix", transform.localToWorldMatrix);
        _material.SetFloat("_Size", size);
       
    }


    void cpu_updatebuffer() {
        Vector3 r;
        double soften = 1e-6;
        float dist2, dist6, invDist3, s;

        for (int i = 0; i < N; i++)
            particles[i].a = Vector3.zero;

        for (int i = 0; i < N; i++) {
            for (int j = 0; j < N; j++) {
                r = particles[j].position - particles[i].position;
                dist2 = r.x * r.x + r.y * r.y + (float)soften;
                dist6 = dist2 * dist2 * dist2;
                invDist3 = (float)1.0 / Mathf.Sqrt(dist6);

                particles[i].a = particles[i].a + r * invDist3;

            }
        }

        for (int i = 0; i < N; i++) {
            particles[i].position = particles[i].position + (float)dt * particles[i].v;
            particles[i].v = particles[i].v + (float)dt * particles[i].a;

        }

    }




}