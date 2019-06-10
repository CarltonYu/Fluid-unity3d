using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidBlendOption
{
    public FluidBlendOption(){
        option = BlendOption.Disable;
    }
    BlendOption option;
    /*
        float4 result = SrcFactor * fragment_output + DstFactor * pixel_color;
        Blend SrcFactor DstFactor

        One                                 float4(1.0)
        Zero                                float4(0.0)
        SrcColor                            fragment_output
        SrcAlpha                            float4(fragment_output.a)
        DstColor                            pixel_color
        DstAlpha                            float4(pixel_color.a)
        OneMinusSrcColor                    float4(1.0) - fragment_output
        OneMinusSrcAlpha                    float4(1.0 - fragment_output.a)
        OneMinusDstColor                    float4(1.0) - pixel_color
        OneMinusDstAlpha                    float4(1.0 - pixel_color.a)


     */

    public enum BlendOption
    {
        Disable = 0,
        Enable_SA_OMSA = 1,// Blend SrcAlpha OneMinusSrcAlpha        //alpha blending
        Enable_One_OMSA = 2,// Blend One OneMinusSrcAlpha             //premultiplied alpha blending
        Enable_One_One = 3,// Blend One One                          //additive
        Enable_SA_One = 4,// Blend SrcAlpha One                     //additive blending
        Enable_OMDC_One = 5,// Blend OneMinusDstColor One             //soft additive
        Enable_DC_Zero = 6,// Blend DstColor    Zero                 //multiplicative
        Enable_DC_SC = 7,// Blend DstColor SrcColor                //2x multiplicative
        Enable_Zero_SA = 8// Blend Zero SrcAlpha                    //multiplicative blending for attenuation by the 
    }
    
    public void Porcess(Material mat){
        // UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
        switch (option)
        {
            case BlendOption.Enable_SA_OMSA:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                break;
            case BlendOption.Enable_One_OMSA:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                break;
            case BlendOption.Enable_One_One:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                break;
            case BlendOption.Enable_SA_One:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                break;
            case BlendOption.Enable_OMDC_One:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusDstColor);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                break;
            case BlendOption.Enable_DC_Zero:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                break;
            case BlendOption.Enable_DC_SC:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcColor);
                break;
            case BlendOption.Enable_Zero_SA:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                break;
            default:
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                break;
        }
    }

    public BlendOption GetBlendOption(){
        return option;
    }
    public void SetBlendOption(BlendOption blendoption){
		option = blendoption;
	}
}
