using UnityEngine;
using System.Collections; 
public class SpriteGeometryShader : MonoBehaviour
{
	public Shader geomShader;
	Material material;

	public Texture2D sprite;
	public Vector2 size = Vector2.one;
	public Color color = new Color(1.0f, 0.6f, 0.3f, 0.03f);

	ComputeBuffer verticesBuffer; 

	ComputeShaderOutput cso; 

	private Vector3 wind;

	[Range(0,3)]
	[Tooltip("Billboard type 0 = static, 1 = Cylindrical, 2 = Sphecrical, 3 = Own rotation from compute shader")]
	public int billboardType = 2;
 

	void Start()
	{
		material = new Material(geomShader);
		cso = GetComponent<ComputeShaderOutput>();
		if (cso == null) {
			Debug.Log ("You need the ComputeShaderOutput component");
			Destroy (gameObject);
		}
	}

	void OnRenderObject()
	{ 
		cso.Dispatch(); 
		verticesBuffer = cso.flakesBuffer; 
		wind = cso.wind;

		material.SetPass(0);
		material.SetColor("_Color", color);
		material.SetBuffer("buf_Points", verticesBuffer); 
		material.SetTexture("_Sprite", sprite);
		material.SetVector("_Size", size);
		material.SetInt("_StaticCylinderSpherical", billboardType);
		material.SetVector("_worldPos", transform.position);
		material.SetVector("_Wind", wind);

		Graphics.DrawProcedural(MeshTopology.Points, verticesBuffer.count);
	}

	void OnDestroy()
	{
		verticesBuffer.Release();
	}
}
