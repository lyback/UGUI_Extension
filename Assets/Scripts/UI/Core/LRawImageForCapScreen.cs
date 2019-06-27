using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.UI
{
    public class LRawImageForCapScreen : LUIEffect_Prop_CapScreen
    {
        void Init()
        {
            if (s_CopyId == 0)
            {
                s_CopyId = Shader.PropertyToID("_LRawImageForCapScreen_ScreenCopyId");
                s_EffectId1 = Shader.PropertyToID("_LRawImageForCapScreen_EffectId1");
                s_EffectId2 = Shader.PropertyToID("_LRawImageForCapScreen_s_EffectId2");
                s_EffectFactorId = Shader.PropertyToID("_EffectFactor");
            }
            if (s_CommandBuffer == null)
            {
                s_CommandBuffer = new CommandBuffer();
            }

            if (effectMaterial == null)
            {
                Debug.LogError("LRawImageForCapScreen not Fine effectMaterial");
            }
        }
        public void Capture()
        {
            Init();

            int w, h;
            GetDesamplingSize(desamplingRate, out w, out h);
            // If size of result RT has changed, release it.
            if (_rt && (_rt.width != w || _rt.height != h))
            {
                _ReleaseRT(ref _rt);
            }

            // Generate RT for result.
            if (_rt == null)
            {
                _rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
                _rt.filterMode = filterMode;
                _rt.useMipMap = false;
                _rt.wrapMode = TextureWrapMode.Clamp;
                _rtId = new RenderTargetIdentifier(_rt);
            }
            SetupCommandBuffer();
        }

        void SetupCommandBuffer()
        {

            // [1] Capture from back buffer (back buffer -> copied screen).
            int w, h;
            GetDesamplingSize(DesamplingRate.None, out w, out h);
            s_CommandBuffer.GetTemporaryRT(s_CopyId, w, h, 0, filterMode);
#if UNITY_EDITOR
            s_CommandBuffer.Blit(Resources.FindObjectsOfTypeAll<RenderTexture>().FirstOrDefault(x => x.name == "GameView RT"), s_CopyId, effectMaterial, 0);
#else
		    s_CommandBuffer.Blit(BuiltinRenderTextureType.BindableTexture, s_CopyId, effectMaterial, 0);
#endif

            //Iterate blurring operation.
            if (blurMode != BlurMode.None)
            {
                GetDesamplingSize(reductionRate, out w, out h);
                s_CommandBuffer.GetTemporaryRT(s_EffectId1, w, h, 0, filterMode);
                for (int i = 0; i < blurIterations; i++)
                {
                    // [2] Apply blurring with reduction buffer (effect1 -> effect2, or effect2 -> effect1).
                    s_CommandBuffer.SetGlobalVector(s_EffectFactorId, new Vector4(blurFactor, 0));
                    s_CommandBuffer.Blit(s_CopyId, s_EffectId1, effectMaterial, 1);
                    s_CommandBuffer.SetGlobalVector(s_EffectFactorId, new Vector4(0, blurFactor));
                    s_CommandBuffer.Blit(s_EffectId1, s_CopyId, effectMaterial, 1);
                }
                s_CommandBuffer.ReleaseTemporaryRT(s_EffectId1);
            }

            // [3] Copy to result RT.
            s_CommandBuffer.Blit(s_CopyId, _rtId);
            s_CommandBuffer.ReleaseTemporaryRT(s_CopyId);

            Graphics.ExecuteCommandBuffer(s_CommandBuffer);
            UpdateTexture();

        }
        /// <summary>
        /// Gets the size of the desampling.
        /// </summary>
        public void GetDesamplingSize(DesamplingRate rate, out int w, out int h)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var res = UnityEditor.UnityStats.screenRes.Split('x');
                w = int.Parse(res[0]);
                h = int.Parse(res[1]);
            }
            else
#endif
            {
                w = Screen.width;
                h = Screen.height;
            }

            if (rate == DesamplingRate.None)
                return;

            float aspect = (float)w / h;
            if (w < h)
            {
                h = Mathf.ClosestPowerOfTwo(h / (int)rate);
                w = Mathf.CeilToInt(h * aspect);
            }
            else
            {
                w = Mathf.ClosestPowerOfTwo(w / (int)rate);
                h = Mathf.CeilToInt(w / aspect);
            }
        }
        /// <summary>
        /// Release captured image.
        /// </summary>
        public void Release()
        {
            _Release(true);
            texture = null;
            _SetDirty();
        }
        void _Release(bool releaseRT)
        {
            if (releaseRT)
            {
                texture = null;
                _ReleaseRT(ref _rt);
            }

            if (s_CommandBuffer != null)
            {
                s_CommandBuffer.Clear();

                if (releaseRT)
                {
                    s_CommandBuffer.Release();
                    s_CommandBuffer = null;
                }
            }
        }

        void _ReleaseRT(ref RenderTexture obj)
        {
            if (obj)
            {
                obj.Release();
                RenderTexture.ReleaseTemporary(obj);
                obj = null;
            }
        }
        void UpdateTexture()
        {
            Graphics.ExecuteCommandBuffer(s_CommandBuffer);
            _Release(false);
            texture = capturedTexture;
            _SetDirty();
        }
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        void _SetDirty()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }
    }
}