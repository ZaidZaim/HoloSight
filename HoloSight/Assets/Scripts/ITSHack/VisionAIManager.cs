using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.MixedReality.Toolkit.Audio;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Windows.WebCam;

public class VisionAIManager : MonoBehaviour
{
    private const string ApiKey = "721deb623abc4f41aa7cab2353b5f861";
    private const string Endpoint = "https://itshack21.cognitiveservices.azure.com";
    
    private TextToSpeech tts;
    Texture2D targetTexture = null;
    PhotoCapture photoCaptureObject = null;
    
    private void Awake()
    {
        tts = GetComponent<TextToSpeech>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        var cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
        targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

        // Create a PhotoCapture object
        PhotoCapture.CreateAsync(false, delegate(PhotoCapture captureObject) {
            photoCaptureObject = captureObject;
            var cameraParameters = new CameraParameters
            {
                hologramOpacity = 0.0f,
                cameraResolutionWidth = cameraResolution.width,
                cameraResolutionHeight = cameraResolution.height,
                pixelFormat = CapturePixelFormat.BGRA32
            };

            // Activate the camera
            photoCaptureObject.StartPhotoModeAsync(cameraParameters, delegate(PhotoCapture.PhotoCaptureResult result) {
                Debug.Log("Cam is active");
            });
        });  
    }
    
    public async void StartRecognition()
    {
        photoCaptureObject.TakePhotoAsync(OnPhotoCaputured);
    }
    
    async void OnPhotoCaputured(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        photoCaptureFrame.UploadImageDataToTexture(targetTexture);
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ApiKey);

        var content = new ByteArrayContent(targetTexture.EncodeToJPG());
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        var response =  await client.PostAsync($"{Endpoint}/vision/v3.2/analyze?visualFeatures=Description",  content);
        
        Debug.Log($"VisionAI Response Code {response.StatusCode}");

        var jsonStr = await response.Content.ReadAsStringAsync();
        
        Debug.Log(jsonStr);
        
        var data = JsonConvert.DeserializeObject<Data>(jsonStr);

        if (data != null && data.description != null && data.description.captions.Count > 0)
        {
            tts.StartSpeaking(data.description.captions[0].text);
        }
        tts.StopSpeaking();
    }
    

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        // Shutdown our photo capture resource
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    private void OnDestroy()
    {
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }
    
    public class FaceRectangle
    {
        public int left { get; set; }
        public int top { get; set; }
        public int width { get; set; }
        public int height { get; set; }
    }

    public class Celebrity
    {
        public string name { get; set; }
        public FaceRectangle faceRectangle { get; set; }
        public double confidence { get; set; }
    }

    public class Landmark
    {
        public string name { get; set; }
        public double confidence { get; set; }
    }

    public class Detail
    {
        public List<Celebrity> celebrities { get; set; }
        public List<Landmark> landmarks { get; set; }
    }

    public class Category
    {
        public string name { get; set; }
        public double score { get; set; }
        public Detail detail { get; set; }
    }

    public class Adult
    {
        public bool isAdultContent { get; set; }
        public bool isRacyContent { get; set; }
        public bool isGoryContent { get; set; }
        public double adultScore { get; set; }
        public double racyScore { get; set; }
        public double goreScore { get; set; }
    }

    public class Tag
    {
        public string name { get; set; }
        public double confidence { get; set; }
    }

    public class Caption
    {
        public string text { get; set; }
        public double confidence { get; set; }
    }

    public class Description
    {
        public List<string> tags { get; set; }
        public List<Caption> captions { get; set; }
    }

    public class Metadata
    {
        public int width { get; set; }
        public int height { get; set; }
        public string format { get; set; }
    }

    public class Face
    {
        public int age { get; set; }
        public string gender { get; set; }
        public FaceRectangle faceRectangle { get; set; }
    }

    public class Color
    {
        public string dominantColorForeground { get; set; }
        public string dominantColorBackground { get; set; }
        public List<string> dominantColors { get; set; }
        public string accentColor { get; set; }
        public bool isBWImg { get; set; }
    }

    public class ImageType
    {
        public int clipArtType { get; set; }
        public int lineDrawingType { get; set; }
    }

    public class Rectangle
    {
        public int x { get; set; }
        public int y { get; set; }
        public int w { get; set; }
        public int h { get; set; }
    }

    public class Object
    {
        public Rectangle rectangle { get; set; }
        public string @object { get; set; }
        public double confidence { get; set; }
    }

    public class Data
    {
        public List<Category> categories { get; set; }
        public Adult adult { get; set; }
        public List<Tag> tags { get; set; }
        public Description description { get; set; }
        public string requestId { get; set; }
        public Metadata metadata { get; set; }
        public string modelVersion { get; set; }
        public List<Face> faces { get; set; }
        public Color color { get; set; }
        public ImageType imageType { get; set; }
        public List<Object> objects { get; set; }
    }

}
