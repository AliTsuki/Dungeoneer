using System;

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(MaskEffectRenderer), PostProcessEvent.AfterStack, "Ali/CameraMaskEffectShader")]
public sealed class CameraMaskEffect : PostProcessEffectSettings
{
    public TextureParameter MaskTex = new TextureParameter();
}

public sealed class MaskEffectRenderer : PostProcessEffectRenderer<CameraMaskEffect>
{
    public override void Render(PostProcessRenderContext context)
    {
        PropertySheet sheet = context.propertySheets.Get(Shader.Find("Ali/CameraMaskEffectShader"));
        sheet.properties.SetTexture("_MaskTex", this.settings.MaskTex);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
