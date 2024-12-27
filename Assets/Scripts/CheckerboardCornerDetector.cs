using UnityEngine;
using System.Collections;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
// Ekledik:
using System.Drawing;

public class CheckerboardCornerDetector_SystemDrawing : MonoBehaviour
{
    [SerializeField] private int patternRows = 6;
    [SerializeField] private int patternCols = 6;

    [SerializeField] private Texture2D checkerboardImg1;
    [SerializeField] private Texture2D checkerboardImg2;
    [SerializeField] private Texture2D checkerboardImg3;


    /// <summary>
    /// Inspector'da script'e sağ tıklayınca veya
    /// scriptin üç nokta menüsünden "Detect Checkerboards" diyerek
    /// tüm resimler üzerinde köşe tespiti yapar.
    /// </summary>
    [ContextMenu("Detect Checkerboards")]
    public void DetectAllCheckerboards()
    {
        if (checkerboardImg1 != null)
            DetectCorners(checkerboardImg1, "Image1");

        if (checkerboardImg2 != null)
            DetectCorners(checkerboardImg2, "Image2");

        if (checkerboardImg3 != null)
            DetectCorners(checkerboardImg3, "Image3");
    }

    /// <summary>
    /// Tek bir Texture2D üzerinde checkerboard köşelerini tespit eden yardımcı fonksiyon.
    /// </summary>
    private void DetectCorners(Texture2D tex, string imgName)
    {
        Color32[] pixels = tex.GetPixels32();
        int width = tex.width;
        int height = tex.height;

        // Emgu CV Image<Bgr, byte> oluşturuyoruz
        using (Image<Bgr, byte> bgrImg = new Image<Bgr, byte>(width, height))
        {
            // Texture2D -> Bgr
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    Color32 c = pixels[index];
                    bgrImg.Data[y, x, 0] = c.b;
                    bgrImg.Data[y, x, 1] = c.g;
                    bgrImg.Data[y, x, 2] = c.r;
                }
            }

            // Burada System.Drawing.Size kullanıyoruz.
            Size patternSize = new Size(patternCols, patternRows);

            using (VectorOfPointF corners = new VectorOfPointF())
            {
                bool found = CvInvoke.FindChessboardCorners(
                    bgrImg,
                    patternSize,
                    corners,
                    CalibCbType.AdaptiveThresh | CalibCbType.NormalizeImage | CalibCbType.FastCheck
                );

                if (!found)
                {
                    Debug.LogWarning($"{imgName}: Checkerboard bulunamadı veya köşeler algılanamadı!");
                    return;
                }

                // cornerSubPix parametreleri de genelde System.Drawing.Size bekliyor:
                using (Image<Gray, byte> grayImg = bgrImg.Convert<Gray, byte>())
                {
                    CvInvoke.CornerSubPix(
                        grayImg,
                        corners,
                        new Size(5, 5),           // subpixel penceresi
                        new Size(-1, -1),         // yok
                        new MCvTermCriteria(30, 0.01)
                    );
                }

                Vector2[] cornerList = new Vector2[corners.Size];
                for (int i = 0; i < corners.Size; i++)
                {
                    var p = corners[i];
                    cornerList[i] = new Vector2(p.X, p.Y);
                }

                Debug.Log($"{imgName} => {cornerList.Length} köşe bulundu.");
                for (int i = 0; i < cornerList.Length; i++)
                {
                    Debug.Log($"{imgName} corner[{i}] = {cornerList[i]}");
                }
            }
        }
    }
}