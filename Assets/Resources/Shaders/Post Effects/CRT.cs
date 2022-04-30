using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(CRTRenderer), PostProcessEvent.BeforeStack, "Effects/CRT",allowInSceneView:false)]
public sealed class CRT : PostProcessEffectSettings{

    [Range(140, 1080)]
    public IntParameter Height              = new IntParameter()  {value = 720};

    [Range(0, 1)]
    public FloatParameter LineDarkness      = new FloatParameter() {value = 0 };

    [Range(0, 1)]
    public FloatParameter Warping           = new FloatParameter() {value = 0 };

    [Range(0.0001f, 10)]
    public FloatParameter VignetteIntensity = new FloatParameter() {value = 0.0001f };

    [Range(0,1)]
    public FloatParameter VignetteOpacity   = new FloatParameter() {value = 0 };



}

public sealed class CRTRenderer : PostProcessEffectRenderer<CRT>{
    public override void Render(PostProcessRenderContext context)
    {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/Effects/CRT"));

        sheet.properties.SetInt("_Height", settings.Height);
        sheet.properties.SetFloat("_LineDarkness", settings.LineDarkness);
        sheet.properties.SetFloat("_Warping", settings.Warping);
        sheet.properties.SetFloat("_VignetteIntensity", settings.VignetteIntensity);
        sheet.properties.SetFloat("_VignetteOpacity", settings.VignetteOpacity);

        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);        
    }

}