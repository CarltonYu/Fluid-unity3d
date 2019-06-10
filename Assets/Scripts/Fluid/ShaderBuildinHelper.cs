using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderBuildinHelper : MonoBehaviour
{

    public List<Shader> shaderlist;

    public Shader GetShaderByName(string name){
        foreach (Shader item in shaderlist)
        {
            if(name.Equals(item.name)){
                return item;
            }
        }
        Debug.Log("GetShaderByName[\""+name+"\"]:+null");
        return null;
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
