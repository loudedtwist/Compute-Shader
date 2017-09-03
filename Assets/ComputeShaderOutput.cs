using UnityEngine;
using System.Collections;
using System.Linq;

public class ComputeShaderOutput : MonoBehaviour {

    #region Compute Shader Fields and Properties
    /// <summary>
    /// The Campute Shader we will use
    /// </summary>
    public ComputeShader computeShader;
	public Transform abweisend;


    private const int numberOfThreadGroups = 8;
    /// <summary>
    /// The total number of vertices to calculate
    /// 10 * 10 * 10 block rendered in 10 * 10 * 10 threads in 1 * 1  * 1 groups
    /// </summary>
    public const int VertCount = (numberOfThreadGroups * numberOfThreadGroups * numberOfThreadGroups) * (10 * 10 * 10);

    /// <summary>
    /// This buffer will store the calculated data resulting from the Compute Shader
    /// </summary>
    public ComputeBuffer flakesBuffer;

    /// <summary>
    /// Every flake has its own speed as it falls down
    /// </summary>
	public ComputeBuffer accelerationBuffer;  

	public float speed = 0.005f;
	[Range(0.5f,2.0f)] 
	public float rotationRange = 1f; 
	public float rotationAmount = 0.3f;

    public float posScale = 1.0f;

    public bool wooble = false;

    public Vector3 wind = new Vector3 (0, 0, 0);

    public Vector3 worldSize = new Vector3(10.0f, 10.0f, 2.0f);

	public GameObject escapeObject;
    /// <summary>
    /// A reference to the CS Kernel we want to use
    /// </summary>
    int CSKernel;
    public Shader pointShader;
    Material pointMaterial;

    struct Data 
    {
		public Vector3 pos;
        public Vector3 rot;
    }

    public bool debufRender = false ;

    #endregion

    void initBuffers(){
		flakesBuffer = new ComputeBuffer(VertCount, 12 * 2); 
        accelerationBuffer = new ComputeBuffer(VertCount, 8); //2 floats
        Data [] data  = new Data[VertCount];
        Data [] bottom  = new Data[VertCount];
		Vector2[] acceleration = new Vector2[VertCount]; 

        for (int i = 0; i < data.Length; i++ )
        {
			var randomPostion = new Vector3 (Random.Range (-worldSize.x/2, worldSize.x/2), Random.Range (0f, worldSize.y), Random.Range (-worldSize.z / 2, worldSize.z/2));
			var randomRotation = new Vector3 (0.01f + Random.value, .01f + Random.value, .01f + Random.value);
			data[i] = new Data { pos = randomPostion, rot = randomRotation };
			bottom[i] = new Data { pos = new Vector3(randomPostion.x,0, randomPostion.z) };  
            acceleration[i] = new Vector2(0.1f + Random.value, .1f + Random.value); 
        }  
		flakesBuffer.SetData(data);  
        accelerationBuffer.SetData(acceleration);

		computeShader.SetFloat ("speed",speed);
        computeShader.SetFloat("initFlakeCoordY", worldSize.y);

		computeShader.SetBuffer(CSKernel, "vertPos", flakesBuffer);  
        computeShader.SetBuffer(CSKernel, "acceleration", accelerationBuffer);


		bottomArrayBuffer = new ComputeBuffer (vertices.Length,12);
		bottomArrayBuffer.SetData (vertices);
		computeShader.SetInt ("bottomArrayLength", vertices.Length);
		computeShader.SetBuffer(CSKernel,"bottomArrayBuffer",bottomArrayBuffer);


        if (debufRender) pointMaterial.SetBuffer("buf_points", flakesBuffer);
    }

    void SupportCheck()
    {
        var supportsComputeShaders = SystemInfo.supportsComputeShaders;
        if (!supportsComputeShaders)
        {
            Debug.Log("Compute shader not supported (not using DX 11?)");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Executes the Compute Shader.
    /// If computer does not support the Compute Render, throws a message
    /// </summary>
	public void Dispatch()
    {
		computeShader.SetFloat ("snowGrowProgression", snowGrowProgression);
		computeShader.SetFloat("rotationSpeed",rotationRange);  
		computeShader.SetFloat("rotationAmount",rotationAmount); 
		computeShader.SetFloat("speed",speed); 
        computeShader.SetFloat("wobble",wooble? 1 : 0);
        computeShader.SetFloat("timeOffset", Time.time * 0.01f);
        computeShader.SetFloat("initFlakeCoordY", worldSize.y);
		computeShader.SetVector ("escapeObj",escapeObject.transform.position);
		computeShader.SetVector ("wind",wind);
        computeShader.Dispatch(CSKernel, numberOfThreadGroups, numberOfThreadGroups, numberOfThreadGroups); 
    }

    public void ReleaseBuffers(){
        flakesBuffer.Release();
    }

	void Start () {
        SupportCheck();
        CSKernel = computeShader.FindKernel("CSMain"); 
        pointMaterial = new Material(pointShader);
        pointMaterial.SetVector("_worldPos", transform.position); 
        initBuffers();
	}

    void OnRenderObject(){
        if(debufRender){
            Dispatch();
            pointMaterial.SetPass(0);
            pointMaterial.SetVector("_worldPos", transform.position);
            pointMaterial.SetFloat("posScale", posScale);
            Graphics.DrawProcedural(MeshTopology.Points, VertCount);
        }
    }

    void OnDisable(){
        ReleaseBuffers();
    } 

	public int xSize, ySize;
	public float cellWidth= 1.0f;
	private Vector3[] vertices;
	Vector2[] uv;
	private Mesh mesh; 
	ComputeBuffer bottomArrayBuffer;
	[Range(0,0.001f)] 
	public float snowGrowProgression = 0.01f;

	private void Generate()
	{
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Grid";

		vertices = new Vector3[(xSize + 1) * (ySize + 1)];
		uv = new Vector2[vertices.Length];
		Vector4[] tangents = new Vector4[vertices.Length];
		Vector4 tangent = new Vector4(1f, 0f, 0f, -1f);
		for (int i = 0, y = 0; y <= ySize; y++)
		{
			for (int x = 0; x <= xSize; x++, i++)
			{
				vertices[i] = new Vector3(x*cellWidth,0, y*cellWidth);
				uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
				tangents[i] = tangent;
			}
		}
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.tangents = tangents;

		int[] triangles = new int[xSize * ySize * 6];
		for (int ti = 0, vi = 0, y = 0; y < ySize; y++, vi++)
		{
			for (int x = 0; x < xSize; x++, ti += 6, vi++)
			{
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
				triangles[ti + 5] = vi + xSize + 2;
			}
		}
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
	}

    [Range(0, 0.2f)]
    public float snowVerteilung =  0.05f;
    public int everySecond = 15;
    // Update is called once per frame
    void Update () {  
		bottomArrayBuffer.GetData (vertices);
         
        if ( --everySecond < 0)
        { 
            Debug.Log("VERT ANZ " + vertices.Length);
			smooth(vertices, xSize +1 , 0.05f, abweisend.position);
            bottomArrayBuffer.SetData(vertices);
            computeShader.SetInt("bottomArrayLength", vertices.Length);
            computeShader.SetBuffer(CSKernel, "bottomArrayBuffer", bottomArrayBuffer);

            everySecond = 15;
        }
        mesh.vertices = vertices;
		mesh.RecalculateNormals();
	}
	private void Awake()
	{
		Generate();
	}

	static void smooth(Vector3[] array, int width, float p , Vector3 abweisendPos)
	{
		Debug.LogError ("SMOOTH");
        float[] result = new float[array.Length];
        int height = array.Length / width;

        for (int i = 0; i < array.Length; i++)
        {

			if (Vector3.Distance (array [i], abweisendPos) < 0.5f) {
				Debug.LogError ("ABWEISEND");
				continue;
			}
            // Ecken

            // Oben links
            if (i == 0)
            {
                result[i] = (1 - 2 * p) * array[i].y + p * array[i + 1].y + p * array[i + width].y;
            }

            // Oben rechts
            else if (i == width - 1)
            {
                result[i] = (1 - 2 * p) * array[i].y + p * array[i - 1].y + p * array[i + width].y;
            }

            // Unten links
            else if (i == width * height - width)
            {
                result[i] = (1 - 2 * p) * array[i].y + p * array[i + 1].y + p * array[i - width].y;
            }

            // Unten rechts
            else if (i == width * height - 1)
            {
                result[i] = (1 - 2 * p) * array[i].y + p * array[i - 1].y + p * array[i - width].y;
            }

            // Ränder

            // Oben
            else if (i < width)
            {
                result[i] = (1 - 3 * p) * array[i].y + p * array[i - 1].y + p * array[i + 1].y + p * array[i + width].y;
            }

            // Unten
            else if (i > width * height - width)
            {
                //Debug.LogError("Width: " + width + ", height: " + height + ", length: " + array.Length);
                //Debug.LogError("index: " + i);
                result[i] = (1 - 3 * p) * array[i].y + p * array[i - 1].y + p * array[i + 1].y + p * array[i - width].y;
            }

            // Links
            else if (i % width == 0)
            {
                result[i] = (1 - 3 * p) * array[i].y + p * array[i + 1].y + p * array[i - width].y + p * array[i + width].y;
            }

            // Rechts
            else if (i % width == width - 1)
            {
                result[i] = (1 - 3 * p) * array[i].y + p * array[i - 1].y + p * array[i - width].y + p * array[i + width].y;
            }

            // Mitte
            else
            {
                result[i] = (1 - 4 * p) * array[i].y + p * array[i - 1].y + p * array[i + 1].y + p * array[i - width].y + p * array[i + width].y;
            }

        }

        //rewrite array
        for (int i = 0; i < array.Length; i++)
        {
            array[i].y = result[i];
        } 
    }
}
