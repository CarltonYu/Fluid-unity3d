using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoubleFBO
{
    RenderTexture readfbo,writefbo;
    public DoubleFBO(int w, int h,int d, RenderTextureFormat format, FilterMode filterMode){
        RenderTexture rendertexture = new RenderTexture(w,h,d,format);
		rendertexture.filterMode = filterMode;
		rendertexture.Create();
		readfbo = rendertexture;

        rendertexture = new RenderTexture(w,h,d,format);
		rendertexture.filterMode = filterMode;
		rendertexture.Create();
		writefbo = rendertexture;
    }

    public void SetReadFBO(RenderTexture tex){
        readfbo = tex;
    }
    public void SetWriteFBO(RenderTexture tex){
        writefbo = tex;
    }
    public RenderTexture GetReadFBO(){
        return readfbo;
    }
    public RenderTexture GetWriteFBO(){
        return writefbo;
    }

    public void Swap(){
        RenderTexture temp = readfbo;
        readfbo = writefbo;
        writefbo = temp;
    }

}
