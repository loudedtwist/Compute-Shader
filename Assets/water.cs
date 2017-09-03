using UnityEngine;
using System.Collections;

public class water : MonoBehaviour {
 
 
    private int anzRects; 

    [Range(1, 104)]
    public int anzRectHeight = 4;
    [Range(4, 104)] 
    public int anzRectWidth = 4;  

    private Mesh mesh  ; 
	public float width = 1f;

    void Start () {
        anzRects = anzRectWidth * anzRectWidth;
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null) {
            Debug.Log("No mesh filter");
            return;
        } 

        //Vector3[] newVert = rectVert(width, anzRectHeight, anzRectWidth);
        Vector3[] newVert = triangleVert(width, anzRectHeight, anzRectWidth);

        //int[] indic = rectToTrianglesIndic(anzRects);
        int[] indic = trianglesIndic(anzRects);

        mesh = mf.mesh;
        mesh.vertices = newVert;
        mesh.SetIndices(indic, MeshTopology.Triangles, 0);
    }

    Vector3[] rectVert(float width,int anzRectHeight, int anzRectWidth)
    {
        int anzVertInRect = 4;
        int anzRects = anzRectWidth * anzRectWidth;
        Vector3[] newVert = new Vector3[anzRects * anzVertInRect];
        for (int iDown = 0; iDown < anzRectHeight; iDown++)
        {
            for (int i = 0, iRight = 0; i < anzRectWidth; i++, iRight += anzVertInRect)
            {
                newVert[anzVertInRect * anzRectWidth * iDown + iRight + 0] = new Vector3(0 + iDown * width, 0, 0 + i * width);
                newVert[anzVertInRect * anzRectWidth * iDown + iRight + 1] = new Vector3(0 + iDown * width, 0, 1 + i * width);
                newVert[anzVertInRect * anzRectWidth * iDown + iRight + 2] = new Vector3(1 + iDown * width, 0, 1 + i * width);
                newVert[anzVertInRect * anzRectWidth * iDown + iRight + 3] = new Vector3(1 + iDown * width, 0, 0 + i * width);
            }
        }
        return newVert;
    }
    Vector3[] triangleVert(float width,int anzHeight, int anzWidth)
    {
        int anzVertInOne = 6;
        int anzRects = anzWidth * anzWidth;
        Vector3[] newVert = new Vector3[anzRects * anzVertInOne];
        for (int iDown = 0; iDown < anzHeight; iDown++)
        {
            for (int i = 0, iRight = 0; i < anzWidth; i++, iRight += anzVertInOne)
            {
                newVert[anzVertInOne * anzWidth * iDown + iRight + 0] = new Vector3(0 + iDown * width, 0, 0 + i * width); 
				newVert[anzVertInOne * anzWidth * iDown + iRight + 1] = new Vector3(width + iDown * width, 0, width + i * width);
				newVert[anzVertInOne * anzWidth * iDown + iRight + 2] = new Vector3(width + iDown * width, 0, 0 + i * width);
                newVert[anzVertInOne * anzWidth * iDown + iRight + 3] = new Vector3(0 + iDown * width, 0, 0 + i * width);
				newVert[anzVertInOne * anzWidth * iDown + iRight + 4] = new Vector3(0 + iDown * width, 0, width + i * width);
				newVert[anzVertInOne * anzWidth * iDown + iRight + 5] = new Vector3(width + iDown * width, 0, width + i * width);
            }
        }
        return newVert;
    }
    int[] rectToTrianglesIndic(int anzRects)
    {
        int anzVertInRect = 4;
        int[] indic = new int[anzRects * anzVertInRect + anzRects * anzVertInRect / 2];
        for (int i = 0, rect = 0; i < anzRects; i++, rect += anzVertInRect)
        {
            indic[i * 6 + 0] = 0 + rect;
            indic[i * 6 + 1] = 2 + rect;
            indic[i * 6 + 2] = 3 + rect;
            indic[i * 6 + 3] = 0 + rect;
            indic[i * 6 + 4] = 1 + rect;
            indic[i * 6 + 5] = 2 + rect;
        }
        return indic;
    }
    int[] trianglesIndic(int anzRects)
    {
        int anzVertInOne = 6;
        int[] indic = new int[anzRects * anzVertInOne];
        for (int i = 0; i < indic.Length; i++)
        {
            indic[i] = i;
        }
        return indic;
    }

    void Update () {

        mesh.RecalculateNormals();
        for(int i = 0; i < mesh.normals.Length;i++)
        { 
            Vector3 norm = transform.TransformDirection(mesh.normals[i]);
            Vector3 vert = transform.TransformPoint(mesh.vertices[i]);
            Debug.DrawRay(vert, norm * 3, Color.red);
        }
    }
     
}
