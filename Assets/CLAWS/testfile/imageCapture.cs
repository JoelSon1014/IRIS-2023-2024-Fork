using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using UnityEngine.Windows.WebCam;
using System;
using System.Text;
using WebSocketSharp;
using TMPro;


public class imageCapture : MonoBehaviour
{
    PhotoCapture photoCaptureObject = null;
    Texture2D targetTexture = null;    
    WebSocket ws;
    string webSocketUrl = "ws://100.64.4.172:5001";
    string lastConnected = "ws://100.64.4.172:5001";
    string IPString = "100.64.4.172:5001";

    public TMP_Text ip_textbox;
    public TMP_Text raycast_textbox;
    public GameObject sphere;

    Ray ray = new Ray(new Vector3(0,0,0), new Vector3(0,0,1));
    bool new_cast = false;

    public void Start()
    {
        ip_textbox.SetText(webSocketUrl);
        raycast_textbox.SetText("Waiting for raycast");
        Debug.Log("Connecting Websocket...");        
        ws = new WebSocket(webSocketUrl);
        ws.OnMessage += OnWebSocketMessage;
        ws.Connect();
        if (ws == null || !ws.IsAlive)
        {
            Debug.Log("Failed to connect websocket");        
        }
        else
        {
            lastConnected = webSocketUrl;
        }

        Debug.Log("Starting camera...");
        StartPhotoCapture();
        if (photoCaptureObject == null)
        {
            Debug.Log("Unable to access camera, loading in example texture instead...");
            string filename = "captured_image.jpg";
            string filePath = Path.Combine(Application.persistentDataPath, filename);
            byte[] imageData = File.ReadAllBytes(filePath);
            // Create a new texture to hold the loaded image
            targetTexture = new Texture2D(3904, 2196);
            // Load the image data into the texture
            targetTexture.LoadImage(imageData);            
            Debug.Log("Done.");
        }
        else
        {
            Debug.Log("Done.");
        }
    }

    void Update()
    {
        if (new_cast)
        {
            Debug.Log("Performing Raycast...");
            RaycastHit hit;
            if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000f))
            {
                Debug.Log("Identified Loc: " + hit.point.ToString());
                raycast_textbox.SetText(hit.point.ToString());
                sphere.transform.position = hit.point;
                Debug.DrawLine(ray.origin, hit.point, Color.yellow, 30f);
                new_cast = false;
            }
            else
            {
                Debug.Log("No hit");
                raycast_textbox.SetText("No hit");
                new_cast = false;
            }
        }
    }

    Vector3[] StringToUnityVectors(string inputString)
    {
        string[] vectorStrings = inputString.Split(':');
        Vector3[] vectors = new Vector3[2];

        for (int i = 0; i < vectorStrings.Length; i++)
        {
            string vectorStr = vectorStrings[i];
            // Remove parentheses and split into components
            string[] components = vectorStr.Trim('(', ')').Split(',');

            // Parse components as floats
            float x = float.Parse(components[0]);
            float y = float.Parse(components[1]);
            float z = float.Parse(components[2]);

            // Create Vector3 object
            vectors[i] = new Vector3(x, y, z);
        }

        return vectors;
    }

    private void OnWebSocketMessage(object sender, MessageEventArgs e)
    {
        if (e.Data != null)
        {
            Vector3[] pos_rot = StringToUnityVectors(e.Data);
            ray = new Ray(pos_rot[0], pos_rot[1]);
            Debug.DrawRay(pos_rot[0], pos_rot[1], Color.blue, 30f);                        
            Debug.Log(ray);
            new_cast = true;
        }
        else
        {
            Debug.Log("Empty response");
        }
    }

    public void StopCamera()
    {
        Debug.Log("Stopping camera");
        photoCaptureObject.StopPhotoModeAsync(OnPhotoModeStopped);
    }

    public void dick() {        
        if (photoCaptureObject == null)
        {
            Debug.Log("Not taking a picture cuz camera is not initialized, using default pic");
            SavePhotoAndSend();
        }
        else
        {
            Debug.Log("Taking a picture");
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
            SavePhotoAndSend();
        }
    }

    public void dickDelay()
    {
        if (photoCaptureObject == null)
        {
            Debug.Log("No camera found, sending burst of default pic");
            for (int i = 0; i < 3; i++)
            {
                SavePhotoAndSend();
            }
        }
        else
        {
            Debug.Log("Sending burst of 3 images");
            for (int i = 0; i < 3; i++)
            {
                photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToWebSocket);
            }
        }
    }

    public void pices()
    {
        if (photoCaptureObject == null)
        {
            Debug.Log("No camera found, sending burst of default pic");
            for (int i = 0; i < 60; i++)
            {
                SavePhotoAndSend();
            }
        }
        else
        {
            Debug.Log("Sending burst of 60 images");
            for (int i = 0; i < 60; i++)
            {                
                photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToWebSocket);                
            }
        }
    }

    void StartPhotoCapture()
    {
        PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        try
        {
            photoCaptureObject = captureObject;

            CameraParameters cameraParameters = new CameraParameters(WebCamMode.PhotoMode);
            cameraParameters.hologramOpacity = 0.0f;
            cameraParameters.cameraResolutionWidth = 1024;
            cameraParameters.cameraResolutionHeight = 1024;
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

            // Start Camera
            captureObject.StartPhotoModeAsync(cameraParameters, OnPhotoModeStarted);            
        }
        catch (Exception ex)
        {
            Debug.Log("Failed to start camera");
            Debug.Log(ex.ToString());
        }

    }

    void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            Debug.Log("Successfully Initialized Camera");
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
            Debug.Log("Saving picture to local state...");
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);                                   
        }
        else
        {
            Debug.LogError("Failed to capture photo to memory!");
        }        
    }
    void OnCapturedPhotoToWebSocket(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);
            SendTextureWS();
        }
        else
        {
            Debug.LogError("Failed to capture photo to memory!");
        }
    }

    void SendTextureWS()
    {
        if (ws == null || !ws.IsAlive || !webSocketUrl.Equals(lastConnected))
        {
            Debug.Log("Restarting websocket...");
            ws = new WebSocket(webSocketUrl);
            ws.OnMessage += OnWebSocketMessage;
            ws.Connect();
            lastConnected = webSocketUrl;
        }
        Debug.Log("Sending Image");
        string head_pos = Camera.main.transform.position.ToString() + ":" + Camera.main.transform.forward.ToString();
        string resp = head_pos + "}$#EndHeadCoord" + Convert.ToBase64String(targetTexture.EncodeToJPG());
        ws.Send(resp);
    }

    void SavePhotoAndSend()
    {
        string filename = "feed.jpg";
        string filePath = Path.Combine(Application.persistentDataPath, filename);

        File.WriteAllBytes(filePath, targetTexture.EncodeToJPG());

        Debug.Log("Attempting to send encoded JPG through websocket...");
        SendTextureWS();
    }
    
    void OnPhotoModeStopped(PhotoCapture.PhotoCaptureResult result)
    {
        // Release the PhotoCapture object
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    public void TypingLol(int val)
    {
        IPString += val.ToString();
        setURL();
    }

    public void TypingStrLol(string val)
    {
        IPString += val;
        setURL();
    }

    public void TypingTruncate()
    {
        IPString = IPString.Remove(IPString.Length - 1, 1);
        setURL();
    }

    void setURL()
    {       
        webSocketUrl = "ws://" + IPString;
        ip_textbox.SetText(webSocketUrl);
    }
}
