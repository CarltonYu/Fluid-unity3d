using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FluidBehaviourScript : MonoBehaviour
{
    public float sim_resolution = 256;
	public float dye_resolution = 512;
	public float splat_radius = 0.5f;

	public float bloom_intensity = 0.8f;
	public float bloom_threshold = 0.6f;
	const int BLOOM_ITERATIONS = 8;
	const int BLOOM_RESOLUTION = 256;
	const float BLOOM_SOFT_KNEE = 0.7f;
	const int CURL = 30;
	const float VELOCITY_DISSIPATION = 0.98f;
	const float DENSITY_DISSIPATION = 0.97f;
	const float PRESSURE_DISSIPATION = 0.8f;
	const int PRESSURE_ITERATIONS = 20;


	RenderTextureFormat format_rgba = RenderTextureFormat.ARGBHalf;
	RenderTextureFormat format_rg = RenderTextureFormat.RGHalf;
	RenderTextureFormat format_r = RenderTextureFormat.RHalf;

	public DoubleFBO density_dfbo,velocity_dfbo,pressure_dfbo;
	public RenderTexture divergence_fbo,curl_fbo,bloom_fbo;
	List<RenderTexture> bloomFramebuffers = new List<RenderTexture>();

	List<FluidPoint> fluidpointlist = new List<FluidPoint>();
	FluidPoint point = new FluidPoint();

	FluidBlendOption.BlendOption blendOption = FluidBlendOption.BlendOption.Disable;
	float screen_w,screen_h;
	Material default_mat;
	FluidProgram clearProgram,colorProgram,backgroundProgram,displayProgram,displayBloomProgram,displayShadingProgram,
			displayBloomShadingProgram,bloomPrefilterProgram,bloomBlurProgram,bloomFinalProgram,splatProgram,advectionProgram,
			divergenceProgram,curlProgram,vorticityProgram,pressureProgram,gradienSubtractProgram;

	int simWidth,simHeight,dyeWidth,dyeHeight;
	
	bool supportLinearFiltering = true;
	public Texture2D ditheringTexture;
	public ShaderBuildinHelper shaderBuildinHelper;
	long frameid = 0;
	void Awake () {
		Application.targetFrameRate = 60;
		default_mat = new Material(Shader.Find("Standard"));

        ditheringTexture = null;
		if(ditheringTexture==null){
            ditheringTexture = Resources.Load("Fluid/LDR_RGB1_0") as Texture2D;
		}
		InitFramebuffers();
		InitProgram(shaderBuildinHelper);
	}
	void Start() {

	}
	
	void Update () {
		frameid++;
		if(frameid%60 == 0 && FluidCtrl.autohit_enable)
			HitMany();
		CheckInput();
		if (!FluidCtrl.paused_check)
			Step(Time.deltaTime);
	}
	public void Hit(){
		if(FluidCtrl.paused_check)
			return;
		FluidPoint p = new FluidPoint();
		p.x = screen_w * Random.Range(0.0f,1.0f);
		p.y = screen_h * Random.Range(0.0f,1.0f);
		p.dx = 1000 * (Random.Range(0.0f,1.0f) - 0.5f);
		p.dy = 1000 * (Random.Range(0.0f,1.0f) - 0.5f);
		p.color = GenerateColor();
		p.color.r *=10.0f;
		p.color.g *=10.0f;
		p.color.b *=10.0f;
		Splat(p);
	}

	public void HitMany(){
		if(FluidCtrl.paused_check)
			return;
		int rcount = Random.Range(20,40);
		for (int i = 0; i < rcount; i++)
		{
			FluidPoint p = new FluidPoint();
			p.x = screen_w * Random.Range(0.0f,1.0f);
			p.y = screen_h * Random.Range(0.0f,1.0f);
			p.dx = 1000 * (Random.Range(0.0f,1.0f) - 0.5f);
			p.dy = 1000 * (Random.Range(0.0f,1.0f) - 0.5f);
			p.color = GenerateColor();
			p.color.r *=10.0f;
			p.color.g *=10.0f;
			p.color.b *=10.0f;
			Splat(p);
		}
	}
	float lastColorChangeTime;
	void CheckInput(){
		if(Input.GetKeyDown (KeyCode.Mouse0)){
			point.down = true;
			lastColorChangeTime = Time.realtimeSinceStartup;
			Debug.Log("lastColorChangeTime:"+lastColorChangeTime);
			point.color = GenerateColor();
		}
		if (Input.GetKeyUp (KeyCode.Mouse0)) {
            point.down = false;
        }
		if(Input.GetKeyUp(KeyCode.P)){
			FluidCtrl.paused_check = !FluidCtrl.paused_check;
		}
		if(Input.GetKeyUp(KeyCode.C)){
			FluidCtrl.colorful_enable = !FluidCtrl.colorful_enable;
		}
		if(Input.GetKeyUp(KeyCode.B)){
			FluidCtrl.bloom_enabled = !FluidCtrl.bloom_enabled;
		}
		if(Input.GetKeyUp(KeyCode.S)){
			FluidCtrl.shading_enable = !FluidCtrl.shading_enable;
		}
		if(Input.GetKeyDown(KeyCode.R)){
			HitMany();
		}
		if(point.down){
			point.moved = point.down;
			point.dx = (Input.mousePosition.x - point.x) * 5.0f;
			point.dy = (Input.mousePosition.y - point.y) * 5.0f;
			point.x = Input.mousePosition.x;
			point.y = Input.mousePosition.y;
		}
		if(point.moved){
            point.moved = false;
			if(point.x>0&&point.x<screen_w&&point.y>0&&point.y<screen_h){
				Splat(point);
			}
		}
		if(!FluidCtrl.colorful_enable){
			return;
		}
		if ((lastColorChangeTime + 0.1f) < Time.realtimeSinceStartup)
		{
			lastColorChangeTime = Time.realtimeSinceStartup;
			point.color = GenerateColor();
		}
	}
	void InitProgram(ShaderBuildinHelper shaderBuildinHelper){
		if(shaderBuildinHelper==null){
			Debug.Log("ShaderBuildinHelper is null");
			return;
		}
		clearProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/ClearShader"));
		colorProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/ColorShader"));
		backgroundProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/BackgroundShader"));
		displayProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/DisplayShader"));
		displayBloomProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/DisplayBloomShader"));
		displayShadingProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/DisplayShadingShader"));
		displayBloomShadingProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/DisplayBloomShadingShader"));
		bloomPrefilterProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/BloomPrefilterShader"));
		bloomBlurProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/BloomBlurShader"));
		bloomFinalProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/BloomFinalShader"));
		splatProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/SplatShader"));
		advectionProgram = new FluidProgram(supportLinearFiltering?shaderBuildinHelper.GetShaderByName("Custom/Fluid/AdvectionShader"):shaderBuildinHelper.GetShaderByName("Custom/Fluid/AdvectionManualFilteringShader"));
		divergenceProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/DivergenceShader"));
		curlProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/CurlShader"));
		vorticityProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/VorticityShader"));
		pressureProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/PressureShader"));
		gradienSubtractProgram = new FluidProgram(shaderBuildinHelper.GetShaderByName("Custom/Fluid/GradientSubtractShader"));
	}
	void Splat(FluidPoint point){
		splatProgram.SetTexture("_Target",velocity_dfbo.GetReadFBO());
		splatProgram.SetFloat("_aspectRatio",screen_w / screen_h);
		splatProgram.SetColor("_point",new Color(point.x / screen_w, point.y / screen_h,0f));
		splatProgram.SetColor("_color",new Color(point.dx, point.dy, 1.0f));
		splatProgram.SetFloat("_radius",splat_radius / 100.0f);
		splatProgram.Blit(velocity_dfbo.GetWriteFBO(),velocity_dfbo.GetWriteFBO(),blendOption);
		velocity_dfbo.Swap();
		splatProgram.SetTexture("_Target",density_dfbo.GetReadFBO());
		splatProgram.SetColor("_color",point.color);
		splatProgram.Blit(density_dfbo.GetWriteFBO(),density_dfbo.GetWriteFBO(),blendOption);
		density_dfbo.Swap();
	}
	RenderTexture ApplyBloom(RenderTexture source,RenderTexture destination){
		if (bloomFramebuffers.Count < 2){
			Debug.Log("ApplyBloom return  frameid:"+frameid);
			return destination;
		}
		RenderTexture last = destination;
		blendOption = FluidBlendOption.BlendOption.Disable;
		float knee = bloom_threshold * BLOOM_SOFT_KNEE + 0.0001f;
		float curve0 = bloom_threshold - knee;
		float curve1 = knee * 2;
		float curve2 = 0.25f / knee;
		bloomPrefilterProgram.SetFloat("_curve_x",curve0);
		bloomPrefilterProgram.SetFloat("_curve_y",curve1);
		bloomPrefilterProgram.SetFloat("_curve_z",curve2);
		bloomPrefilterProgram.SetFloat("_threshold",bloom_threshold);
		bloomPrefilterProgram.SetTexture("_uTexture",source);
		bloomPrefilterProgram.Blit(last,last,blendOption);

		for (int i = 0; i < bloomFramebuffers.Count; i++) {
			RenderTexture dest = bloomFramebuffers[i];
			bloomBlurProgram.SetTexture("_uTexture",last);
			bloomBlurProgram.SetFloat("_texelSize_x",1.0f / last.width);
			bloomBlurProgram.SetFloat("_texelSize_y",1.0f / last.height);
			bloomBlurProgram.Blit(dest,dest,blendOption);
			last = dest;
		}

		blendOption = FluidBlendOption.BlendOption.Enable_One_One;
		for (int i = bloomFramebuffers.Count - 2; i >= 0; i--) {
			RenderTexture baseTex = bloomFramebuffers[i];
			bloomBlurProgram.SetTexture("_uTexture",last);
			bloomBlurProgram.SetFloat("_texelSize_x",1.0f / last.width);
			bloomBlurProgram.SetFloat("_texelSize_y",1.0f / last.height);
			bloomBlurProgram.Blit(baseTex,baseTex,blendOption);
			last = baseTex;
		}

		blendOption = FluidBlendOption.BlendOption.Disable;
		bloomFinalProgram.SetFloat("_texelSize_x",1.0f / last.width);
		bloomFinalProgram.SetFloat("_texelSize_y",1.0f / last.height);
		bloomFinalProgram.SetTexture("_uTexture",last);
		bloomFinalProgram.SetFloat("_intensity",bloom_intensity);
		bloomFinalProgram.Blit(destination,destination,blendOption);
		return destination;
	}
	void Step(float dt){
		blendOption = FluidBlendOption.BlendOption.Disable;

		curlProgram.SetFloat("_texelSize_x",1.0f / simWidth);
		curlProgram.SetFloat("_texelSize_y",1.0f / simHeight);
		curlProgram.SetTexture("_Velocity",velocity_dfbo.GetReadFBO());
		curlProgram.Blit(curl_fbo,curl_fbo,blendOption);

		vorticityProgram.SetFloat("_texelSize_x",1.0f / simWidth);
		vorticityProgram.SetFloat("_texelSize_y",1.0f / simHeight);
		vorticityProgram.SetTexture("_Velocity",velocity_dfbo.GetReadFBO());
		vorticityProgram.SetTexture("_Curl",curl_fbo);
		vorticityProgram.SetFloat("_curl",CURL);
		vorticityProgram.SetFloat("_dt",dt);
		vorticityProgram.Blit(velocity_dfbo.GetWriteFBO(),velocity_dfbo.GetWriteFBO(),blendOption);
		velocity_dfbo.Swap();

		divergenceProgram.SetFloat("_texelSize_x",1.0f / simWidth);
		divergenceProgram.SetFloat("_texelSize_y",1.0f / simHeight);
		divergenceProgram.SetTexture("_Velocity",velocity_dfbo.GetReadFBO());
		divergenceProgram.Blit(divergence_fbo,divergence_fbo,blendOption);

		clearProgram.SetTexture("_uTexture",pressure_dfbo.GetReadFBO());
		clearProgram.SetFloat("_value",PRESSURE_DISSIPATION);
		clearProgram.Blit(pressure_dfbo.GetWriteFBO(),pressure_dfbo.GetWriteFBO(),blendOption);
		pressure_dfbo.Swap();

		pressureProgram.SetFloat("_texelSize_x",1.0f / simWidth);
		pressureProgram.SetFloat("_texelSize_y",1.0f / simHeight);
		pressureProgram.SetTexture("_Divergence",divergence_fbo);
		for (int i = 0; i < PRESSURE_ITERATIONS; i++) {
			pressureProgram.SetTexture("_Pressure",pressure_dfbo.GetReadFBO());
			pressureProgram.Blit(pressure_dfbo.GetWriteFBO(),pressure_dfbo.GetWriteFBO(),blendOption);
			pressure_dfbo.Swap();
		}

		gradienSubtractProgram.SetFloat("_texelSize_x",1.0f / simWidth);
		gradienSubtractProgram.SetFloat("_texelSize_y",1.0f / simHeight);
		gradienSubtractProgram.SetTexture("_Pressure",pressure_dfbo.GetReadFBO());
		gradienSubtractProgram.SetTexture("_Velocity",velocity_dfbo.GetReadFBO());
		gradienSubtractProgram.Blit(velocity_dfbo.GetWriteFBO(),velocity_dfbo.GetWriteFBO(),blendOption);
		velocity_dfbo.Swap();

		advectionProgram.SetFloat("_texelSize_x",1.0f / simWidth);
		advectionProgram.SetFloat("_texelSize_y",1.0f / simHeight);
		if(!supportLinearFiltering){
			advectionProgram.SetFloat("_dyeTexelSize_x",1.0f / simWidth);
			advectionProgram.SetFloat("_dyeTexelSize_y",1.0f / simHeight);
		}
		advectionProgram.SetTexture("_Velocity",velocity_dfbo.GetReadFBO());
		advectionProgram.SetTexture("_Source",velocity_dfbo.GetReadFBO());
		advectionProgram.SetFloat("_dt",dt);
		advectionProgram.SetFloat("_dissipation",VELOCITY_DISSIPATION);
		advectionProgram.Blit(velocity_dfbo.GetWriteFBO(),velocity_dfbo.GetWriteFBO(),blendOption);
		velocity_dfbo.Swap();

		if(!supportLinearFiltering){
			advectionProgram.SetFloat("_dyeTexelSize_x",1.0f / dyeWidth);
			advectionProgram.SetFloat("_dyeTexelSize_y",1.0f / dyeHeight);
		}
		advectionProgram.SetTexture("_Velocity",velocity_dfbo.GetReadFBO());
		advectionProgram.SetTexture("_Source",density_dfbo.GetReadFBO());
		advectionProgram.SetFloat("_dissipation",DENSITY_DISSIPATION);
		advectionProgram.Blit(density_dfbo.GetWriteFBO(),density_dfbo.GetWriteFBO(),blendOption);
		density_dfbo.Swap();
	}
	RenderTexture Render(RenderTexture target){//(RenderTexture source,RenderTexture target){
		if(target == null){
			return null;
		}

		if(FluidCtrl.bloom_enabled){
			ApplyBloom(density_dfbo.GetReadFBO(),bloom_fbo);
		}
		blendOption = FluidBlendOption.BlendOption.Enable_One_OMSA;
		int width = target.width;
		int height = target.height;

		if(FluidCtrl.transparent_enable){
			backgroundProgram.SetFloat("_aspectRatio",(float)Screen.width / (float)Screen.height);
			backgroundProgram.Blit(target,target,blendOption);
		}else{
			colorProgram.SetColor("_color",FluidCtrl.BACK_COLOR);
			colorProgram.Blit(target,target,blendOption);
		}
		if(FluidCtrl.shading_enable){
			FluidProgram program = FluidCtrl.bloom_enabled ? displayBloomShadingProgram : displayShadingProgram;
			program.SetFloat("_texelSize_x",1.0f / simWidth);
			program.SetFloat("_texelSize_y",1.0f / simHeight);
			program.SetTexture("_uTexture",density_dfbo.GetReadFBO());
			if(FluidCtrl.bloom_enabled){
				program.SetTexture("_Bloom",bloom_fbo);
				program.SetTexture("_Dithering",ditheringTexture);
				Vector2 scale = GetTextureScale(ditheringTexture, width, height);
				program.SetFloat("_ditherScale_x",scale.x);
				program.SetFloat("_ditherScale_y",scale.y);
			}
			program.Blit(target,target,blendOption);
		}
		else{
			FluidProgram program = FluidCtrl.bloom_enabled ? displayBloomProgram : displayProgram;
			program.SetTexture("_uTexture",density_dfbo.GetReadFBO());
			if(FluidCtrl.bloom_enabled){
				program.SetTexture("_Bloom",bloom_fbo);
				program.SetTexture("_Dithering",ditheringTexture);
				Vector2 scale = GetTextureScale(ditheringTexture, width, height);
				program.SetFloat("_ditherScale_x",scale.x);
				program.SetFloat("_ditherScale_y",scale.y);
			}
			program.Blit(target,target,blendOption);
		}
		return target;
	}

	Vector2 GetTextureScale (Texture texture, int width, int height) {
    	return new Vector2((float)width / (float)texture.width,(float)height / (float)texture.height);
    }
	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{	
		Graphics.Blit(Render(source),destination);
	}	
	void InitFramebuffers(){

		screen_w = Screen.width;
		screen_h = Screen.height;
		Vector2 simv = GetResolution(sim_resolution);
		Vector2 dyev = GetResolution(dye_resolution);
		simWidth = (int)simv.x;
		simHeight = (int)simv.y;
		dyeWidth  = (int)dyev.x;
		dyeHeight = (int)dyev.y;
		FilterMode filtering = supportLinearFiltering?FilterMode.Bilinear:FilterMode.Point;
		 if (density_dfbo == null)
			density_dfbo = CreateDoubleFBO(dyeWidth, dyeHeight, 0, format_rgba, filtering);
		else
			density_dfbo = ResizeDoubleFBO(density_dfbo, dyeWidth, 0, dyeHeight, format_rgba, filtering);

		if (velocity_dfbo == null)
			velocity_dfbo = CreateDoubleFBO(simWidth, simHeight, 0, format_rg, filtering);
		else
			velocity_dfbo = ResizeDoubleFBO(velocity_dfbo, simWidth, 0, simHeight, format_rg, filtering);

		divergence_fbo = CreateFBO      (simWidth, simHeight, 0, format_r, filtering);
		curl_fbo       = CreateFBO      (simWidth, simHeight, 0, format_r, filtering);
		pressure_dfbo  = CreateDoubleFBO(simWidth, simHeight, 0, format_r, filtering);

		InitBloomFramebuffers(dyeWidth, dyeHeight, 0, format_rgba, filtering);
	}
	Vector2 GetResolution (float resolution){
		float aspectRatio = (float)Screen.width / (float)Screen.height;
		if (aspectRatio < 1)
        	aspectRatio = 1.0f / aspectRatio;
		int max = Mathf.RoundToInt(resolution * aspectRatio);
    	int min = Mathf.RoundToInt(resolution);
		if (Screen.width > Screen.height)
			return new Vector2(max,min);
		else
			return new Vector2(min,max);
	}

	
	void InitBloomFramebuffers (int w, int h,int d, RenderTextureFormat format, FilterMode filterMode) {
		bloom_fbo = CreateFBO(w, h, 0, format, filterMode);

		bloomFramebuffers.Clear();
		for (int i = 0; i < BLOOM_ITERATIONS; i++)
		{
			int width = w >> (i + 1);
			int height = h >> (i + 1);

			if (width < 2 || height < 2) break;

			RenderTexture fbo = CreateFBO(width, height, 0, format, filterMode);
			bloomFramebuffers.Add(fbo);
		}
	}

	RenderTexture ResizeFBO (RenderTexture target, int w, int h,int d, RenderTextureFormat format, FilterMode filterMode) {
		RenderTexture newFBO = CreateFBO(w, h, 0, format, filterMode);
		clearProgram.SetTexture("_uTexture",target);
		clearProgram.SetFloat("_value",1.0f);
		clearProgram.Blit(newFBO,newFBO,blendOption);
		return newFBO;
	}

	DoubleFBO ResizeDoubleFBO (DoubleFBO target, int w, int h,int d, RenderTextureFormat format, FilterMode filterMode) {
		target.SetReadFBO(ResizeFBO(target.GetReadFBO(), w, h, 0, format, filterMode));
		target.SetWriteFBO(CreateFBO(w, h, 0, format, filterMode));
		return target;
	}
	public DoubleFBO CreateDoubleFBO(int w, int h,int d, RenderTextureFormat format, FilterMode filterMode){
		return new DoubleFBO(w,h,d,format,filterMode);
	}
	public RenderTexture CreateFBO(int w, int h,int d, RenderTextureFormat format, FilterMode filterMode){
		
		RenderTexture rendertexture = new RenderTexture(w,h,d,format);
		rendertexture.filterMode = filterMode;
		rendertexture.Create();
		return rendertexture;
	}

	public Color GenerateColor () {
		Color c = Color.HSVToRGB(Random.Range(0.0f,1.0f), 1.0f, 1.0f);
		c.a = 1.0f;
		c.r *= 0.15f;
		c.g *= 0.15f;
		c.b *= 0.15f;
		return c;
	}
}
