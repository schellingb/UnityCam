/*
  This sample code is for demonstrating and testing the functionality
  of Unity Capture, and is placed in the public domain.

  This code generates a scrolling color texture simply for the purposes of demonstration.
  Other uses may include sending a video, another webcam feed or a static image to the output.
*/

using UnityEngine;

public class CaptureRenderTexture : MonoBehaviour {
	public int width = 320;
	public int height = 240;
	public MeshRenderer outputRenderer;
	RenderTexture renderTex;
	Texture2D activeTex;
	UnityCaptureTexture capTex;
	int y = 0;
	Color color = Color.red;


	void Start () {
		// Create textures
		activeTex = new Texture2D(width, height);
		renderTex = new RenderTexture(width, height, 0);

		capTex = GetComponent<UnityCaptureTexture>();
		capTex.renderTex = renderTex;

		if (outputRenderer != null) outputRenderer.material.mainTexture = activeTex;
	}
	
	void Update() {
		UpdateTexture();
		Graphics.Blit(activeTex, renderTex);
	}

	void UpdateTexture() {
		for (int x = 0; x < width; x++) {
			activeTex.SetPixel(x, y, color);
		}

		y += 1;
		if (y > height) {
			y = 0;
			color = new Color(color.g, color.b, color.r);
		}
		activeTex.Apply();
	}
}