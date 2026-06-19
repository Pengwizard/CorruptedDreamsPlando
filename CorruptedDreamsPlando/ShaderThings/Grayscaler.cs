using ItemChanger.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CorruptedDreamsPlando.ShaderThings
{
    public class Grayscaler
    {
        private static Shader GrayscaleShader;

        private static float colorPercent = 1f;

        public static void Initialize()
        {
            LoadAssets();
        }

        public static void LoadAssets()
        {
            var platform = Application.platform switch
            {
                RuntimePlatform.WindowsPlayer => "windows",
                RuntimePlatform.OSXPlayer => "osx",
                _ => throw new PlatformNotSupportedException("What platform are you even on??")
            };

            var assetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream($"CorruptedDreamsPlando.Resources.shader.grayscaler_{platform}.unity3d"));
            GrayscaleShader = assetBundle.LoadAsset<Shader>("assets/shaders/grayscaler.shader");
        }

        public static void GrayscaleObject(GameObject obj)
        {
            // TODO Fix
            //try
            //{
            //    var p = obj.GetOrAddComponent<LocalPostProcess>();
            //    p.material.SetFloat("RedPercent", 0);
            //    p.material.SetFloat("GreenPercent", 0);
            //    p.material.SetFloat("BluePercent", 0);
            //    obj.GetComponent<MeshRenderer>().material.shader = p.material.shader;
            //} catch (Exception e)
            //{
            //    CorruptedDreamsPlandoMod.Instance.Log($"Failed to add local post process");
            //}
        }

        public static void SetupCamera(On.GameManager.orig_OnNextLevelReady orig, GameManager self)
        {
            var mc = GameCameras.instance.mainCamera.gameObject.GetOrAddComponent<CameraPostProcess>();
            var hc = GameCameras.instance.hudCamera.gameObject.GetOrAddComponent<CameraPostProcess>();
        }

        public static void TickCamera()
        {
            GameCameras.instance.mainCamera.gameObject.GetOrAddComponent<CameraPostProcess>().StartCoroutine(CameraPostProcess.TickMaterialDown());
        }

        public static void RestoreCamera()
        {
            GameCameras.instance.mainCamera.gameObject.GetOrAddComponent<CameraPostProcess>().StartCoroutine(CameraPostProcess.TickMaterialUp());
            //CameraPostProcess.RestoreMaterial();
        }

        public class LocalPostProcess : MonoBehaviour
        {
            public Shader shader;
            public Material material;

            void OnEnable()
            {
                if (shader == null)
                {
                    shader = Grayscaler.GrayscaleShader;
                }

                if (material == null)
                {
                    material = new Material(shader);
                }
            }

            void OnRenderImage(RenderTexture source, RenderTexture destination)
            {
                if (material == null)
                {
                    Graphics.Blit(source, destination);
                }

                Graphics.Blit(source, destination, material);
            }
        }

        public class CameraPostProcess : MonoBehaviour
        {
            public Shader shader;
            public static Material material;

            void OnEnable()
            {
                if (shader == null)
                {
                    shader = Grayscaler.GrayscaleShader;
                }

                if (material == null)
                {
                    material = new Material(shader);
                }
            }

            void OnRenderImage(RenderTexture source, RenderTexture destination)
            {
                if (material == null)
                {
                    Graphics.Blit(source, destination);
                }

                Graphics.Blit(source, destination, material);
            }
            
            public static IEnumerator TickMaterialDown()
            {
                colorPercent = 1f;
                var interval = 0.01f;
                while (colorPercent > 0)
                {
                    CameraPostProcess.material.SetFloat("RedPercent", colorPercent);
                    CameraPostProcess.material.SetFloat("GreenPercent", colorPercent);
                    CameraPostProcess.material.SetFloat("BluePercent", colorPercent);

                    colorPercent -= 0.01f;

                    yield return new WaitForSeconds(interval);
                }
            }

            public static IEnumerator TickMaterialUp()
            {
                colorPercent = 0f;
                var interval = 0.01f;
                while (colorPercent < 1f)
                {
                    CameraPostProcess.material.SetFloat("RedPercent", colorPercent);
                    CameraPostProcess.material.SetFloat("GreenPercent", colorPercent);
                    CameraPostProcess.material.SetFloat("BluePercent", colorPercent);

                    colorPercent += 0.05f;

                    yield return new WaitForSeconds(interval);
                }
            }

            public static void RestoreMaterial()
            {
                CameraPostProcess.material.SetFloat("RedPercent", 1f);
                CameraPostProcess.material.SetFloat("GreenPercent", 1f);
                CameraPostProcess.material.SetFloat("BluePercent", 1f);
            }
        }
    }
}
