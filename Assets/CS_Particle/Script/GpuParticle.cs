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
  
    static public int N = 2560;
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
    void Start(){
        
        Particle[] particles= new Particle[N];
        for (int i = 0; i < N; i++)
        {
               double a = Random.value * Mathf.PI;
               double r = Mathf.Sqrt(Random.value) * 0.3;
               
               particles[i].position = new Vector3(Mathf.Cos((float)a) * (float)r, Mathf.Sin((float)a) * (float)r, 0);
                particles[i].v = new Vector3(0, 0, 0);
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
        if (time < 0)
            lerpt = Mathf.Lerp(lerpt, 1, 0.008f);
        updatebuffer();
        argsBuffer.SetData(_args);
        Graphics.DrawMeshInstancedIndirect(Particle_Mesh,0, _material, new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f)), argsBuffer);
    }   


    void updatebuffer()
    {
        int kernelId = _computeShader.FindKernel("MainCS");  
        _computeShader.SetFloat("_Time", time);  
        _computeShader.SetBuffer(kernelId, "_ParticleBuffer", ParticleBuffer);  
        _computeShader.Dispatch(kernelId, blockPerGrid, 1, 1);
        _args[0] = (uint)Particle_Mesh.GetIndexCount(0);  //36
        _args[1] = (uint)N;
        _args[2] = (uint)Particle_Mesh.GetIndexStart(0);  //0
        _args[3] = (uint)Particle_Mesh.GetBaseVertex(0);  //0

     
        _material.SetBuffer("_ParticleBuffer", ParticleBuffer);
        _material.SetMatrix("_GameobjectMatrix", transform.localToWorldMatrix);
        _material.SetFloat("_Size", size);
        _material.SetFloat("_lerp", lerpt);
    }
}