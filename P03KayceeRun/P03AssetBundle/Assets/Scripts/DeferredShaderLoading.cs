using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeferredShaderLoading : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!String.IsNullOrEmpty(ShaderName))
        {
            Shader shader = Shader.Find(ShaderName);
            if (shader != null)
            {
                foreach (var renderer in GetComponentsInChildren<Renderer>())
                {
                    renderer.material.shader = shader;
                    for (int i = 0; i < ColorKeywords.Length; i++)
                    {
                        renderer.material.SetColor(ColorKeywords[i], Colors[i]);
                    }
                }
            }
        }
    }

    [SerializeField]
    public string ShaderName;

    [SerializeField]
    public string[] ColorKeywords;

    [SerializeField]
    public Color[] Colors;
}
