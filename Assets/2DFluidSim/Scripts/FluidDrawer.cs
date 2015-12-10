using UnityEngine;
using System.Collections;

namespace TwoDimFluidSolver {
    public class FluidDrawer : MonoBehaviour {
    	public enum DrawMode {
    		Opaque = 0,
    		Transparent
    	}

    	private DrawMode m_drawMode = DrawMode.Opaque;

    	private FluidSolver m_fluidSolver;

    	private RenderTexture m_fluidTex;
    	private Material m_fluidMat;

    	private float m_transparancy = 1.0f;

    	public void Setup(FluidSolver fluidSolver) {
    		m_fluidSolver = fluidSolver;

    		_CreateGUI();
    	}

    	public void SetTransparancy(float transparancy) {
    		m_transparancy = transparancy;
    	}

    	public void SetDrawMode(DrawMode mode) {
    		m_drawMode = mode;
    		_SetDrawMaterial();
    	}

    	private void _CreateGUI() {
    		// Create the gui texture
    		_CreateTexture(ref m_fluidTex, RenderTextureFormat.ARGB32, FilterMode.Bilinear);

    		// Create the mat that convert fluid info to real rgb texture
    		_SetDrawMaterial();
    	}

    	private void _CreateTexture(ref RenderTexture tex, RenderTextureFormat format, FilterMode filter) {
    		tex = new RenderTexture(m_fluidSolver.Width, m_fluidSolver.Height, 0, format);
    		tex.filterMode = filter;
    		tex.wrapMode = TextureWrapMode.Clamp;
    		tex.Create();
    		
    		Graphics.SetRenderTarget(tex);
    		GL.Clear(false, true, new Color(0, 0, 0, 0));		
    		Graphics.SetRenderTarget(null);
    	}

    	private void _SetDrawMaterial() {
    		switch(m_drawMode) {
    		case DrawMode.Opaque:
    			m_fluidMat = new Material(Shader.Find("2DFluidSim/DrawOpaque"));
    			break;
    		case DrawMode.Transparent:
    			m_fluidMat = new Material(Shader.Find("2DFluidSim/DrawTransparent"));
    			break;
    		}

    		gameObject.GetComponent<Renderer>().material = m_fluidMat;
    		m_fluidMat.SetTexture("_MainTex", m_fluidTex);
    	}

    	void Update() {
    		RenderTexture fluidInfoTex = m_fluidSolver.GetFuildDrawInfo();
    		RenderTexture obstacleTex = m_fluidSolver.GetObstacleTexture();
    		m_fluidMat.SetTexture("_MainTex", fluidInfoTex);
    		m_fluidMat.SetTexture("_ObstacleTex", obstacleTex);
    		m_fluidMat.SetFloat("_Transparancy", m_transparancy);
    	}

    	void OnDestroy() {
    		m_fluidTex.Release();
    	}
    }
}
