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
           
                particles[i].position = new Vector3(Random.value  * Mathf.PI, Random.value  * Mathf.PI, Random.value  * Mathf.PI);
                particles[i].v = new Vector3(0, 0, 0);
                particles[i].a = new Vector3(0, 0, 0);
                particles[i].color = new Vector3(1, 1, 1);
        }

        ComputeBuffer computeBuffer = new ComputeBuffer(N, 48);
        ParticleBuffer = computeBuffer;
        ParticleBuffer.SetData(particles);   //使用数组中的值设置该缓冲区

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
    }   //要绘制的网格/要绘制网格的哪个子集/使用的材料/打算绘制的实例周围的边界体积/GPU缓冲区，包含要绘制此网格的多少个实例的参数


    void updatebuffer()
    {
        int kernelId = _computeShader.FindKernel("MainCS");  //找到计算shader
        _computeShader.SetFloat("_Time", time);  //传入参数time
        _computeShader.SetBuffer(kernelId, "_ParticleBuffer", ParticleBuffer);  //设置计算shader的缓存
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