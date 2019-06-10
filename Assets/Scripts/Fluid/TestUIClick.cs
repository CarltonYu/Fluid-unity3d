using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestUIClick : MonoBehaviour
{

    // public RenderTexture target;
    public FluidCtrl fluidCtrl;
    public CanvasRenderer canvasRender;

    public RawImage rawimage;

    public ShaderBuildinHelper shaderBuildinHelper;

    void Start()
    {
        // canvasRender.GetMaterial().GetTexture();
        ;
    }

    // Update is called once per frame
    void Update()
    {
        if(fluidCtrl!=null && fluidCtrl.CouldProcess()){
            rawimage.texture = fluidCtrl.Process(Time.deltaTime);
        }
            
    }

    public void On_My_click(){
        if(fluidCtrl==null){
            RenderTexture rt = new RenderTexture(300,100,0,RenderTextureFormat.ARGBHalf);
            rawimage.texture = rt;
            // target = rt;
            fluidCtrl = new FluidCtrl(rt,null,shaderBuildinHelper);
        }
    }

    public void Set100Draw(){
        fluidCtrl.drawcount = 100;
    }

    public void Click_Switch_Transparent(){
        FluidCtrl.transparent_enable = !FluidCtrl.transparent_enable;
    }
    public void Click_Switch_Colorfull(){
        FluidCtrl.colorful_enable = !FluidCtrl.colorful_enable;
    }
    public void Click_Switch_Bloom(){
        FluidCtrl.bloom_enabled = !FluidCtrl.bloom_enabled;
    }
    public void Click_Switch_Shading(){
        FluidCtrl.shading_enable = !FluidCtrl.shading_enable;
    }
    public void Click_Switch_Paused(){
        FluidCtrl.paused_check = !FluidCtrl.paused_check;
    }

    public void Click_Switch_Color(){
        FluidCtrl.SwithBackgroundColor();
    }

    public void Click_Switch_AutoHit(){
        FluidCtrl.autohit_enable = !FluidCtrl.autohit_enable;
    }
}
