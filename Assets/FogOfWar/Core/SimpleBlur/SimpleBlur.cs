using UnityEngine;

public static class SimpleBlur
{
    public static void Blur(Texture source, RenderTexture destination, int blurStrenght, bool useUpscaling = true)
    {
        var shader = Shader.Find("Hidden/SimpleBlur");
        var material = new Material(shader);

        int width = source.width;
        int height = source.height;

        var endRenderTexture = RenderTexture.GetTemporary(width, height, 0);
        Graphics.Blit(source, endRenderTexture);
        var startRenderTexture = endRenderTexture;

        for (int i = 0; i < blurStrenght; i++)
        {
            width = width / 2;
            height = height / 2;
            endRenderTexture = RenderTexture.GetTemporary(width, height, 0);
            Graphics.Blit(startRenderTexture, endRenderTexture, material, 0);
            RenderTexture.ReleaseTemporary(startRenderTexture);
            startRenderTexture = endRenderTexture;
        }

        if (useUpscaling == true)
        {
            for (int i = 0; i < blurStrenght; i++)
            {
                width = width * 2;
                height = height * 2;
                endRenderTexture = RenderTexture.GetTemporary(width, height, 0);
                Graphics.Blit(startRenderTexture, endRenderTexture, material, 1);
                RenderTexture.ReleaseTemporary(startRenderTexture);
                startRenderTexture = endRenderTexture;
            }
        }

        Graphics.Blit(startRenderTexture, destination);

        RenderTexture.ReleaseTemporary(startRenderTexture);
    }

}