using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AevenScnTool
{
    [ExecuteInEditMode]
    public class CameraScreenshot : MonoBehaviour
    {
        [Button("Say Cheese!")] public ButtonAction takeScreenShot;

        #region private fields
        private Camera mainCam;
        #endregion

        private void OnEnable()
        {
            takeScreenShot = new ButtonAction(TakeScreenShot);

            mainCam = gameObject.GetComponent<Camera>();
        }

        void TakeScreenShot()
		{
            var old_flag = mainCam.clearFlags;
            var old_color = mainCam.backgroundColor;

            mainCam.clearFlags = CameraClearFlags.SolidColor;

            mainCam.backgroundColor = Color.black;
            Texture2D textureBlack = RenderCamToTexture(mainCam);

            mainCam.backgroundColor = Color.white;
            Texture2D textureWhite = RenderCamToTexture(mainCam);
            mainCam.backgroundColor = Color.clear;

            Texture2D output = CalculateOutputTexture(textureWhite, textureBlack);
            SavePng(output);

            mainCam.clearFlags = old_flag;
            mainCam.backgroundColor = old_color;
        }



        Texture2D RenderCamToTexture(Camera cam)
        {
            Texture2D tex = new Texture2D(cam.pixelWidth, cam.pixelHeight, TextureFormat.RGB24, false);

            cam.Render();
            tex.ReadPixels(new Rect(0, 0, cam.pixelWidth, cam.pixelHeight), 0, 0);
            tex.Apply();

            return tex;
        }

        Texture2D CalculateOutputTexture( Texture2D textureWhite, Texture2D textureBlack)
        {

            Texture2D output = new Texture2D(textureWhite.width, textureWhite.width, TextureFormat.ARGB32, false);
            Color color;
            for (int y = 0; y < output.height; ++y)
            {
                // each row
                for (int x = 0; x < output.width; ++x)
                {
                    // each column
                    Color whitePixel = textureWhite.GetPixel(x, y);
                    Color blackPixel = textureBlack.GetPixel(x, y);
                    float alphaR = whitePixel.r - blackPixel.r;
                    float alphaG = whitePixel.g - blackPixel.g;
                    float alphaB = whitePixel.b - blackPixel.b;
                    float alpha = (alphaR + alphaG + alphaB) / 3f;

                    alpha = 1.0f - alpha;
                    if (alpha == 0)
                    {
                        color = Color.clear;
                    }
                    else
                    {
                        color = textureBlack.GetPixel(x, y) / alpha;
                    }
                    color.a = alpha;
                    output.SetPixel(x, y, color);
                }
            }
            return output;
        }

        void SavePng(Texture2D texture)
        {
            File.WriteAllBytes("Picture.png", texture.EncodeToPNG());
        }
    }
}
