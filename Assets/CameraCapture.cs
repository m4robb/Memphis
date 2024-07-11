using System.IO;
using System.Collections;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Rendering.HighDefinition;

public class CameraCapture : MonoBehaviour
{
    public KeyCode screenshotKey;
    public Camera _camera;
    public AudioSource AS;
    public GameObject VolumeGO;
    public HDAdditionalCameraData CameraData;
    public LayerMask layerMask;


    void Start()
    {
        VolumeGO.SetActive(false);
        _camera.enabled = false;
        


       //S _camera.Render();
    }

  

    bool TriggerShot;
    Texture2D image = null;

    public IEnumerator ExecuteShot()
    {
        VolumeGO.SetActive(true);
        yield return new WaitForEndOfFrame();

        RenderTexture activeRenderTexture = new RenderTexture(2048, 2048, 24, RenderTextureFormat.ARGB32);

        _camera.targetTexture = activeRenderTexture;
        RenderTexture.active = _camera.targetTexture;

        _camera.Render();

        image = new Texture2D(2048, 2048, TextureFormat.ARGB32, true, true);
        
        image.ReadPixels(new Rect(0, 0, 2048, 2048), 0, 0);

        NativeArray<byte> byteArray = image.GetRawTextureData<byte>();
        for (int i = 0; i < byteArray.Length; i += 4)
        {
            byte gray = (byte)(0.2126f * byteArray[i + 1] + 0.7152f * byteArray[i + 2] + 0.0722f * byteArray[i + 3]);
            byteArray[i + 3] = byteArray[i + 2] = byteArray[i + 1] = gray;
        }


        image.filterMode = FilterMode.Trilinear;


        image.Apply();
        byte[] bytes = image.EncodeToPNG();
        string Filename = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
        File.WriteAllBytes(Path.Combine(Application.persistentDataPath, Filename + ".png"), bytes);
        yield return null;
        AS.PlayOneShot(AS.clip);
        VolumeGO.SetActive(false);
        RenderTexture.active = null;
        Destroy(image);


    }

    public IEnumerator TakeScreenshot()
    {
        byte[] bytes = null;
        

        Debug.Log("Start Shot");



        //while(bytes== null)
        //{


        //RenderTexture activeRenderTexture = RenderTexture.active;
        VolumeGO.SetActive(true);

        //RenderTexture activeRenderTexture = new RenderTexture(2048, 2048, 24, RenderTextureFormat.Default)
        //        {
        //            antiAliasing = 4
        //        };

        //        _camera.targetTexture = activeRenderTexture;
        //        RenderTexture.active = _camera.targetTexture;
        //        _camera.Render();

        //        image = new Texture2D(2048, 2048);
        //        image.ReadPixels(new Rect(0, 0, _camera.targetTexture.width, _camera.targetTexture.height), 0, 0);
        //        image.Apply();

        //        bytes = image.EncodeToPNG();
        //    //}

           


            yield return new WaitForSeconds(1);
        //AS.PlayOneShot(AS.clip);
        //Destroy(image);

      
            //RenderTexture.active = null;
            Debug.Log("Finish Shot");
           
            string Filename = System.DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");

        //File.WriteAllBytes(Path.Combine(Application.persistentDataPath, Filename + ".png"), bytes);
        
            //ScreenCapture.CaptureScreenshot(Path.Combine(Application.persistentDataPath, Filename + ".png"), 2, ScreenCapture.StereoScreenCaptureMode.LeftEye);

            VolumeGO.SetActive(false);

    }


   

}
