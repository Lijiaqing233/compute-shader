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
    public bool use_GPU = true;
    static public int N = 4000;   // 16384
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
            double z = Random.value * 2 - 1;
            particles[i].position = new Vector3((float)x, (float)y, (float)z);
            particles[i].v = new Vector3(0, 0, 0);
            //   particles[i].v = new Vector3((float)x_v, (float)y_v, 0);
            particles[i].a = new Vector3(0, 0, 0);
                particles[i].color = new Vector3(1, (float)0.21, (float)0.31);
        }

        ComputeBuffer computeBuffer = new ComputeBuffer(N, 48);
        ParticleBuffer = computeBuffer;
        ParticleBuffer.SetData(particles);   

        _args = new uint[5] { 0, 0, 0, 0, 0 };
        argsBuffer = new ComputeBuffer(1, _args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        blockPerGrid = (N + ThreadBlockSize - 1) / ThreadBlockSize;
        
    }

    
    void Update(){
        //transform.Rotate(new Vector3(0f, Time.deltaTime * 10f, 0f));

            updatebuffer();
            argsBuffer.SetData(_args);
            Graphics.DrawMeshInstancedIndirect(Particle_Mesh, 0, _material, new Bounds(Vector3.zero, new Vector3(100f, 100f, 100f)), argsBuffer);
        

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

}