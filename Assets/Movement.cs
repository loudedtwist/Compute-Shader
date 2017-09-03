using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

	public ComputeShaderOutput shader;
    // Use this for initialization
    void Start () {
        yaw = transform.eulerAngles.y;

    }

    void Update()
    {
        var x = Input.GetAxis("Horizontal") * Time.deltaTime * 3.0f;
        var z = Input.GetAxis("Vertical") * Time.deltaTime * 3.0f;

        transform.Translate(x, 0, z);
        yaw += speedH * Input.GetAxis("Mouse X");
        pitch -= speedV * Input.GetAxis("Mouse Y");

        transform.eulerAngles = new Vector3(pitch, yaw, 0);

		if (Input.GetKey (KeyCode.G)) {
			shader.snowGrowProgression = 0.001f;
		} 

		if (Input.GetKey (KeyCode.B)){
			shader.snowGrowProgression = 0.001f / 10;
		}

		if (Input.GetKey (KeyCode.F)) {
			shader.snowVerteilung = 0.3f;
		} 

		if (Input.GetKey (KeyCode.V)) {
			shader.snowVerteilung = 0.05f;
		}
    }
}
