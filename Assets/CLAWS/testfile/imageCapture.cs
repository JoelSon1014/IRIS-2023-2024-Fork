using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.Windows.WebCam;

public class imageCapture : MonoBehaviour
{
    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;

    public void dick() {
        Debug.LogError("Testing error");
        Debug.Log("Testing log");
        StartPhotoCapture();
        Debug.LogError("Start photo capture done");
        photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        Debug.LogError("Take photo capture done");
    }

    void StartPhotoCapture()
    {
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        photoCaptureObject = captureObject;

        CameraParameters cameraParameters = new CameraParameters();
        cameraParameters.hologramOpacity = 0.0f;
        cameraParameters.cameraResolutionWidth = 1024;
        cameraParameters.cameraResolutionHeight = 1024;
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        // Capture a photo
        captureObject.StartPhotoModeAsync(cameraParameters, OnPhotoModeStarted);
    }

    void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            // Take the photo
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            Debug.LogError("Starting photo taking stuff");
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);
            Debug.LogError("Texture thing");
            string filename = "captured_image.jpg";
            string filePath = Path.Combine(Application.persistentDataPath, filename);
            File.WriteAllBytes(filePath, targetTexture.EncodeToJPG());
            Debug.LogError("Wrote file");
            // Process the photo data as needed

            // Stop the photo mode
            photoCaptureObject.StopPhotoModeAsync(OnPhotoModeStopped);
        }
        else
        {
            Debug.LogError("Failed to capture photo to memory!");
        }
    }
    
    void OnPhotoModeStopped(PhotoCapture.PhotoCaptureResult result)
    {
        // Release the PhotoCapture object
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }
}
