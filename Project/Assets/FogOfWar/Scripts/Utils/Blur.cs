//参考：https://gameinstitute.qq.com/community/detail/120432

using UnityEngine;

public class Blur : MonoBehaviour
{
    [SerializeField]
    [Range(0, 1)]
    float m_blurSize = 0.5f;

    Shader m_blurShader;
    Material m_blurMat;

    Material BlurMat
    {
        get
        {
            if(m_blurMat == null)
            {
                m_blurMat = new Material(m_blurShader);
                m_blurMat.hideFlags = HideFlags.HideAndDontSave;
            }

            return m_blurMat;
        }
    }

    void Start()
    {
        if(!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }

        m_blurShader = Shader.Find("KaimaChen/Blur");
        if (!m_blurShader || !m_blurShader.isSupported)
            enabled = false;
    }

    public void FourTapCone(RenderTexture source, RenderTexture dest, int iteration)
    {
        float offset = m_blurSize * iteration + 0.5f;
        Graphics.BlitMultiTap(source, dest, BlurMat,
                                        new Vector2(-offset, -offset),
                                        new Vector2(-offset, offset),
                                        new Vector2(offset, offset),
                                        new Vector2(offset, -offset));
    }

    private void DownSample4x(RenderTexture source, RenderTexture dest)
    {
        float offset = 1.0f;
        Graphics.BlitMultiTap(source, dest, BlurMat,
                                        new Vector2(offset, offset),
                                        new Vector2(-offset, offset),
                                        new Vector2(offset, offset),
                                        new Vector2(offset, -offset));
    }

    void OnRenderImage(RenderTexture sourceTex, RenderTexture destTex)
    {
        if (sourceTex == null)
            return;

        if(m_blurSize != 0 && m_blurShader != null)
        {
            int rtW = sourceTex.width / 8;
            int rtH = sourceTex.height / 8;
            RenderTexture buffer = RenderTexture.GetTemporary(rtW, rtH, 0);
            DownSample4x(sourceTex, buffer);
            for(int i = 0; i < 2; i++)
            {
                RenderTexture buffer2 = RenderTexture.GetTemporary(rtW, rtH, 0);
                FourTapCone(buffer, buffer2, i);
                RenderTexture.ReleaseTemporary(buffer);
                buffer = buffer2;
            }

            Graphics.Blit(buffer, destTex);
            RenderTexture.ReleaseTemporary(buffer);
        }
        else
        {
            Graphics.Blit(sourceTex, destTex);
        }
    }

    void OnDisable()
    {
        if (m_blurMat)
            DestroyImmediate(m_blurMat);
    }
}