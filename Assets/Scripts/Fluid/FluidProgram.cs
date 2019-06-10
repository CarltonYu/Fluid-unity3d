using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidProgram
{
    public Material material;
    public Shader shader;
    public FluidBlendOption blend;
    public FluidProgram(Shader _shader){
        if(_shader == null){
            Debug.Log("FluidProgram init with null shader");
        }
        shader = _shader;
        material = new Material(shader);
        material.renderQueue =2980;
        blend = new FluidBlendOption();
        blend.Porcess(material);
    }

    public void SetFloat(string key,float value){
        material.SetFloat(key,value);
    }
    public void SetColor(string key,Color value){
        material.SetColor(key,value);
    }
    public void SetTexture(string key,Texture value){
        material.SetTexture(key,value);
    }
    public void SetBlendOption(FluidBlendOption.BlendOption blendoption){
        if(blend.GetBlendOption()!=blendoption){
            blend.SetBlendOption(blendoption);
            blend.Porcess(material);
        }
    }
    public void Blit(RenderTexture source, RenderTexture destination, FluidBlendOption.BlendOption blendoption){
        SetBlendOption(blendoption);
        Graphics.Blit (source, destination, material);
    }
    // public void Blit(RenderTexture source, RenderTexture destination){
    //     Graphics.Blit (source, destination, material);
    // }
}
