using UnityEngine;
using System.Collections;

namespace TwoDimFluidSolver {
    public class FluidSolver : MonoBehaviour {
    	
    	private bool m_simulating = true;

    	private int m_width = 512;
    	public int Width {
    		get {
    			return m_width;
    		}
    	}

    	private int m_height = 512;
    	public int Height {
    		get {
    			return m_height;
    		}
    	}

    	private int m_numJacobiIterations = 40;
    	private float m_deltaT = 0.125f;
    	private bool m_enableRgb = false;
    	private float m_velocityViscosity = 0.001f;
    	private float m_densityViscosity = 0.001f;
    	private float m_ambientTemperature = 0.0f;
    	private float m_smokeBuoyancy = 5.0f;
    	private float m_smokeWeight = 0.05f;
    	private float m_fadeSpeed = 0.99f;

    	// RenderTextures
    	private RenderTexture m_divergenceTex;
    	private RenderTexture m_obstaclesTex;
    	private RenderTexture[] m_velocityTex;
    	private RenderTexture[] m_densityTex; 		// used if not RGB
    	private RenderTexture[] m_colorTex;		// // used if RGB
    	private RenderTexture[] m_pressureTex;
    	private RenderTexture[] m_temperatureTex;

    	// Materials
    	public Material m_advectMat;
    	public Material m_obstaclesMat;
    	public Material m_divergenceMat;
    	public Material m_jacobi1dMat;
    	public Material m_jacobi2dMat;
    	public Material m_jacobi3dMat;
    	public Material m_gradientMat;
    	public Material m_impluseMat;
    	public Material m_buoyancyMat;
    	public Material m_fadeMat;

    	// Other helper vars
    	private Vector2 m_inverseSize;

    	void Start() {
    		StartCoroutine(Simulate());
    	}

    	public void SetSimulating(bool simulating) {
    		m_simulating = simulating;
    	}

    	public void SetViscosity(float viscosity) {
    		m_velocityViscosity = viscosity;
    		m_densityViscosity = viscosity;
    	}

    	public void SetBuoyancy(float buoyancy) {
    		m_smokeBuoyancy = buoyancy;
    	}

    	public void SetFadeSpeed(float fadeSpeed) {
    		m_fadeSpeed = fadeSpeed;
    	}

    	public void Setup(int width, int height, bool enableRgb, float deltaT) {
    		m_width = width;
    		m_height = height;
    		m_enableRgb = enableRgb;
    		m_deltaT = deltaT;

    		// Compute helpter vars
    		m_inverseSize = new Vector2(1.0f/(float)m_width, 1.0f/(float)m_height);

    		_CreateAllTextures();
    		_CreateAllMaterials();

            _AddObstacles();
    	}

    	private void _CreateTexture(ref RenderTexture tex, RenderTextureFormat format, FilterMode filter) {
    		tex = new RenderTexture(m_width, m_height, 0, format);
    		tex.filterMode = filter;
    		tex.wrapMode = TextureWrapMode.Clamp;
    		tex.Create();

    		Graphics.SetRenderTarget(tex);
    		GL.Clear(false, true, new Color(0, 0, 0, 0));		
    		Graphics.SetRenderTarget(null);
    	}

    	private void _CreateAllTextures() {
    		_CreateTexture(ref m_divergenceTex, RenderTextureFormat.RFloat, FilterMode.Point);

    		_CreateTexture(ref m_obstaclesTex, RenderTextureFormat.ARGB32, FilterMode.Bilinear);
    	
    		m_velocityTex  = new RenderTexture[2];
    		_CreateTexture(ref m_velocityTex[0], RenderTextureFormat.RGFloat, FilterMode.Bilinear);
    		_CreateTexture(ref m_velocityTex[1], RenderTextureFormat.RGFloat, FilterMode.Bilinear);

    		if (m_enableRgb) {
    			m_colorTex  = new RenderTexture[2];
    			_CreateTexture(ref m_colorTex[0], RenderTextureFormat.ARGB32, FilterMode.Bilinear);
    			_CreateTexture(ref m_colorTex[1], RenderTextureFormat.ARGB32, FilterMode.Bilinear);
    		} else {
    			m_densityTex  = new RenderTexture[2];
    			_CreateTexture(ref m_densityTex[0], RenderTextureFormat.RFloat, FilterMode.Bilinear);
    			_CreateTexture(ref m_densityTex[1], RenderTextureFormat.RFloat, FilterMode.Bilinear);
    		}

    		m_pressureTex  = new RenderTexture[2];
    		_CreateTexture(ref m_pressureTex[0], RenderTextureFormat.RFloat, FilterMode.Point);
    		_CreateTexture(ref m_pressureTex[1], RenderTextureFormat.RFloat, FilterMode.Point);

    		m_temperatureTex = new RenderTexture[2];
    		_CreateTexture(ref m_temperatureTex[0], RenderTextureFormat.RFloat, FilterMode.Bilinear);
    		_CreateTexture(ref m_temperatureTex[1], RenderTextureFormat.RFloat, FilterMode.Bilinear);
    	}

    	private void _ReleaseAllTextures() {
    		m_divergenceTex.Release();
    		m_obstaclesTex.Release();
    		m_velocityTex[0].Release();
    		m_velocityTex[1].Release();
    		if (m_enableRgb) {
    			m_colorTex[0].Release();
    			m_colorTex[1].Release();
    		} else {
    			m_densityTex[0].Release();
    			m_densityTex[1].Release();
    		}
    		m_pressureTex[0].Release();
    		m_pressureTex[1].Release();
    		m_temperatureTex[0].Release();
    		m_temperatureTex[1].Release();
    	}

    	private void _CreateAllMaterials() {
    		m_advectMat = new Material(Shader.Find("2DFluidSim/Advect"));
    		m_obstaclesMat = new Material(Shader.Find("2DFluidSim/Obstacle"));
    		m_divergenceMat = new Material(Shader.Find("2DFluidSim/Divergence"));
    		m_jacobi1dMat = new Material(Shader.Find("2DFluidSim/Jacobi1d"));
    		m_jacobi2dMat = new Material(Shader.Find("2DFluidSim/Jacobi2d"));
    		m_jacobi3dMat = new Material(Shader.Find("2DFluidSim/Jacobi3d"));
    		m_gradientMat = new Material(Shader.Find("2DFluidSim/SubtractGradient"));
    		m_impluseMat = new Material(Shader.Find("2DFluidSim/Impulse"));
    		m_buoyancyMat = new Material(Shader.Find("2DFluidSim/Buoyancy"));
    		m_fadeMat = new Material(Shader.Find("2DFluidSim/Fade"));
    	}

    	private void _AddObstacles() {
    		m_obstaclesMat.SetVector("_InverseSize", m_inverseSize);

    		Graphics.Blit(null, m_obstaclesTex, m_obstaclesMat);
    	}

    	public RenderTexture GetFuildDrawInfo() {
    		if (m_enableRgb) {
    			return m_colorTex[0];
    		} else {
    			return m_densityTex[0];
    		}
    	}

    	public RenderTexture GetObstacleTexture() {
    		return m_obstaclesTex;
    	}

    	public void AddSource(Vector2 pos, Vector3 amount, float radius) {
    		if (m_enableRgb) {
    			_ApplyImpulse(m_colorTex[0], pos, amount, radius);
    		} else {
    			_ApplyImpulse(m_densityTex[0], pos, amount, radius);
    		}

    		float tempAmount = (amount.x * 0.299f + amount.y * 0.587f + amount.z * 0.114f) * 10.0f;
    		_ApplyImpulse(m_temperatureTex[0], pos, new Vector3(tempAmount, tempAmount, tempAmount), radius);
    	}

    	private void _Swap(RenderTexture[] texs) {
    		RenderTexture temp = texs[0];	
    		texs[0] = texs[1];
    		texs[1] = temp;
    	}

    	private void _ClearTexture(RenderTexture tex)
    	{
    		Graphics.SetRenderTarget(tex);
    		GL.Clear(false, true, new Color(0, 0, 0, 0));		
    		Graphics.SetRenderTarget(null);	
    	}

    	private void _Advect(RenderTexture src, RenderTexture dest, RenderTexture verlocity) {
    		m_advectMat.SetTexture("_SrcTex", src);
    		m_advectMat.SetTexture("_VelocityTex", verlocity);
    		m_advectMat.SetTexture("_ObstacleTex", m_obstaclesTex);
    		m_advectMat.SetFloat("_DeltaT", m_deltaT);

    		Graphics.Blit(null, dest, m_advectMat);
    	}

    	private void _ComputeDivergence(RenderTexture verlocity, RenderTexture dest) {
    		m_divergenceMat.SetTexture("_VelocityTex", verlocity);
    		m_divergenceMat.SetTexture("_ObstacleTex", m_obstaclesTex);

    		Graphics.Blit(null, dest, m_divergenceMat);
    	}

    	private void _Jacobi1d(RenderTexture xTex, RenderTexture bTex, RenderTexture dest, float alpha, float beta) {
    		m_jacobi1dMat.SetTexture("_XTex", xTex);
    		m_jacobi1dMat.SetTexture("_BTex", bTex);
    		m_jacobi1dMat.SetFloat("_Alpha", alpha);
    		m_jacobi1dMat.SetFloat("_rBeta", 1.0f/beta);
    		m_jacobi1dMat.SetTexture("_ObstacleTex", m_obstaclesTex);

    		Graphics.Blit(null, dest, m_jacobi1dMat);
    	}

    	private void _Jacobi2d(RenderTexture xTex, RenderTexture bTex, RenderTexture dest, float alpha, float beta) {
    		m_jacobi2dMat.SetTexture("_XTex", xTex);
    		m_jacobi2dMat.SetTexture("_BTex", bTex);
    		m_jacobi2dMat.SetFloat("_Alpha", alpha);
    		m_jacobi2dMat.SetFloat("_rBeta", 1.0f/beta);
    		m_jacobi2dMat.SetTexture("_ObstacleTex", m_obstaclesTex);
    		
    		Graphics.Blit(null, dest, m_jacobi2dMat);
    	}

    	private void _Jacobi3d(RenderTexture xTex, RenderTexture bTex, RenderTexture dest, float alpha, float beta) {
    		m_jacobi3dMat.SetTexture("_XTex", xTex);
    		m_jacobi3dMat.SetTexture("_BTex", bTex);
    		m_jacobi3dMat.SetFloat("_Alpha", alpha);
    		m_jacobi3dMat.SetFloat("_rBeta", 1.0f/beta);
    		m_jacobi3dMat.SetTexture("_ObstacleTex", m_obstaclesTex);
    		
    		Graphics.Blit(null, dest, m_jacobi3dMat);
    	}

    	private void _SubtractGradient(RenderTexture velocity, RenderTexture pressure, RenderTexture dest) {
    		m_gradientMat.SetTexture("_VelocityTex", velocity);
    		m_gradientMat.SetTexture("_PressureTex", pressure);
    		m_gradientMat.SetTexture("_ObstacleTex", m_obstaclesTex);

    		Graphics.Blit(null, dest, m_gradientMat);
    	}

    	private void _Project() {
    		//Calculates how divergent the velocity is
    		_ComputeDivergence(m_velocityTex[0], m_divergenceTex);
    		
    		_ClearTexture(m_pressureTex[0]);
    		
    		// Compute pressure
    		for(int i = 0; i < m_numJacobiIterations; i++) {
    			_Jacobi1d(m_pressureTex[0], m_divergenceTex, m_pressureTex[1], -1.0f, 4.0f);
    			_Swap(m_pressureTex);
    		}
    		
    		//Use the pressure tex that was last rendered into. This computes divergence free velocity
    		_SubtractGradient(m_velocityTex[0], m_pressureTex[0], m_velocityTex[1]);
    		_Swap(m_velocityTex);
    	}

    	private void _VelocityDiffusion() {
    		for(int i = 0; i < m_numJacobiIterations; ++i) {
    			float alpha = 1.0f/(m_deltaT * m_velocityViscosity);
    			_Jacobi2d(m_velocityTex[0], m_velocityTex[0], m_velocityTex[1], alpha, 4.0f + alpha);
    			_Swap(m_velocityTex);
    		}
    	}

    	private void _DensityDiffusion() {
    		if (m_enableRgb) {
    			for(int i = 0; i < m_numJacobiIterations; ++i) {
    				float alpha = 1.0f/(m_deltaT * m_densityViscosity);
    				_Jacobi3d(m_colorTex[0], m_colorTex[0], m_colorTex[1], alpha, 4.0f + alpha);
    				_Swap(m_colorTex);
    			}
    		} else {
    			for(int i = 0; i < m_numJacobiIterations; ++i) {
    				float alpha = 1.0f/(m_deltaT * m_densityViscosity);
    				_Jacobi1d(m_densityTex[0], m_densityTex[0], m_densityTex[1], alpha, 4.0f + alpha);
    				_Swap(m_densityTex);
    			}
    		}
    	}

    	private void _ApplyImpulse(RenderTexture dest, Vector2 pos, Vector3 amount, float radius) {
    		m_impluseMat.SetFloat("_Aspect", m_inverseSize.y/m_inverseSize.x);
    		m_impluseMat.SetVector("_ImpulsePos", pos);
    		m_impluseMat.SetVector("_Amount", amount);
    		m_impluseMat.SetFloat("_Radius", radius);

    		Graphics.Blit(null, dest, m_impluseMat);
    	}

    	private void _ApplyBuoyancy(RenderTexture velocity, RenderTexture temperature, RenderTexture density, RenderTexture dest) {
    		m_buoyancyMat.SetTexture("_VelocityTex", velocity);
    		m_buoyancyMat.SetTexture("_TemperatureTex", temperature);
    		m_buoyancyMat.SetTexture("_DensityTex", density);
    		m_buoyancyMat.SetFloat("_AmbientTemperature", m_ambientTemperature);
    		m_buoyancyMat.SetFloat("_DeltaT", m_deltaT);
    		m_buoyancyMat.SetFloat("_Sigma", m_smokeBuoyancy);
    		m_buoyancyMat.SetFloat("_Kappa", m_smokeWeight);

    		Graphics.Blit(null, dest, m_buoyancyMat);
    	}

    	private void _Fade(RenderTexture src, RenderTexture dest, float speed) {
    		m_fadeMat.SetTexture("_SrcTex", src);
    		m_fadeMat.SetFloat("_FadeSpeed", speed);

    		Graphics.Blit(null, dest, m_fadeMat);
    	}

    	IEnumerator Simulate() {
    		while (true) {
    			if (!m_simulating) {
    				yield return 0;
    				continue;
    			}

                _AddObstacles();

    			///
    			/// Verlocity steps
    			/// 

    			// 1. Self advect
    			_Advect(m_velocityTex[0], m_velocityTex[1], m_velocityTex[0]);
    			_Swap(m_velocityTex);

    			// 2. Pressure
    			_Project();

    			// 3. Diffusion (viscosity)
    			_VelocityDiffusion();

    			///
    			/// Density steps
    			///

    			// 1. Advect
    			if (m_enableRgb) {
    				_Advect(m_colorTex[0], m_colorTex[1], m_velocityTex[0]);
    				_Swap(m_colorTex);
    			} else {
    				_Advect(m_densityTex[0], m_densityTex[1], m_velocityTex[0]);
    				_Swap(m_densityTex);
    			}

    			// 2. Diffusion
    			_DensityDiffusion();

    			///
    			/// Other forces
    			///

    			_Advect(m_temperatureTex[0], m_temperatureTex[1], m_velocityTex[0]);
    			_Swap(m_temperatureTex);

    			if (m_enableRgb) {
    				_ApplyBuoyancy(m_velocityTex[0], m_temperatureTex[0], m_colorTex[0], m_velocityTex[1]);
    			} else {
    				_ApplyBuoyancy(m_velocityTex[0], m_temperatureTex[0], m_densityTex[0], m_velocityTex[1]);
    			}
    			_Swap(m_velocityTex);

    			///
    			/// Fade
    			/// 
    			if (m_enableRgb) {
    				_Fade(m_colorTex[0], m_colorTex[1], m_fadeSpeed);
    				_Swap(m_colorTex);
    			} else {
    				_Fade(m_densityTex[0], m_densityTex[1], m_fadeSpeed);
    				_Swap(m_densityTex);
    			}

//    			yield return new WaitForSeconds(m_deltaT);
                yield return null;
    		}
    	}

    	void OnDestroy() {
    		_ReleaseAllTextures();
    	}
    }
}
