using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
//using UnityEngine.XR.ARCore;
using UnityEngine.XR.ARSubsystems;
//using GoogleARCore;
using UnityEngine.Android;
using Facebook.Unity;

public class MainButtonScript : MonoBehaviour
{
    // Start is called before the first frame update

    public Sprite redButton;
    public Sprite greenButton;
    public Sprite grayButton;
    float currentSlope;
    public Button colorButton;

    public GameObject androidTargetFinder;
    public GameObject defaultTargetFinder;
    
    public Image bottomImage;
    public Sprite toSetTarget;
    public Sprite toGuessTarget;

    Vector3 currentHitPosition;

    bool dinoHasGone;
    public ARSessionOrigin aRSessionOrigin;
    public ARSession aRSession;
    public Camera ARCamera;
    public RenderTexture renderTexture;
    ARRaycastManager raycastManager;
    XRCameraSubsystem aRCameraSubsystem;
    List<ARRaycastHit> hits;
    XRCpuImage.ConversionParams convParams;
    XRCameraSubsystem camsys;

    WebCamTexture androidTexture;

    public GameObject dinoNoir;

    public GameObject confirmWindow;
    public GameObject loadingWindow;
    public Camera currentCamera;

    public TextMeshProUGUI getCloserText;

    bool isGame;

    NativeArray<byte> buffer;
    XRCpuImage theImage;

    Texture2D previewTexture;

    Color currentColor;
    public GameObject buttonArray;

    RaycastHit rayHit;

    void TryRaycast()
    {
        Debug.Log("beachside");
        //camsys.Try
        //.CameraImage.AcquireCameraImageBytes();
        //Ray newRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        Ray ray = currentCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        //Ray myRay = new Ray(new Vector3(0, 0, 0), Vector3.forward);
        Debug.Log("westside");
        if (Physics.Raycast(ray,out rayHit))
        {
            bool gameWon = true;
            rayHit.collider.GetComponent<ColliderResponder>().TargetIsHit();
            for(int i = 0;i < 5; i++)
            {

                if(buttonArray.gameObject.transform.GetChild(i).GetComponent<TargetButtonScript>().gameObject.activeSelf == true)
                {
                    if(buttonArray.gameObject.transform.GetChild(i).GetComponent<TargetButtonScript>().isFound == false)
                    { gameWon = false; }
                    
                }
               
                


            }

            if(gameWon == true)
            {
                Debug.Log("YOU WON!");
                FB.LogAppEvent("Level Complete");
                dinoNoir.GetComponent<NoirMasterScript>().ShowText(new string[] { "HOORAY, you found all the targets! Do you want to play again?" }, 3);
            }
        }
    }




    public void SetMode(bool isGameIn)
    {
        isGame = isGameIn;

        GetComponent<Button>().onClick.RemoveAllListeners();

        if(isGameIn == true)
        {
            GetComponent<Button>().onClick.AddListener(delegate
            {
                TryRaycast();
            });
            GetComponent<Button>().interactable = true;
            GetComponent<Image>().sprite = grayButton;
           
            bottomImage.GetComponent<Image>().sprite = toGuessTarget;
          
            getCloserText.gameObject.SetActive(false);
        }
        else
        {
            GetComponent<Image>().sprite = redButton;
            bottomImage.GetComponent<Image>().sprite = toSetTarget;
           
            GetComponent<Button>().onClick.AddListener(delegate
            {
                confirmWindow.SetActive(true);
                confirmWindow.GetComponent<ConfirmTargetScript>().ConfigureWindow(-1, currentColor, currentHitPosition, previewTexture.GetRawTextureData(),false);

            });
        }

    }

    

    unsafe void CheckColor()
    {
        Color[] frameColors;

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Debug.Log("capture 1");

            //imgSize = theImage.GetConvertedDataSize(convParams);
            Debug.Log("capture 2");
            //Debug.Log(theImage);
            buffer = new NativeArray<byte>(theImage.GetConvertedDataSize(convParams), Allocator.Temp);
            Debug.Log("capture 3");
            //previewImgBuffer = new NativeArray<byte>(imgSize, Allocator.Temp);
            theImage.Convert(convParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);
            Debug.Log("capture 4");
            //imgPlane = theImage.GetPlane(0);
            theImage.Dispose();
            Debug.Log("capture 5");
            //previewImgBuffer = buffer;
            previewTexture.LoadRawTextureData(buffer);
            Debug.Log("capture 5");
            previewTexture.Apply();
            buffer.Dispose();
            


        }
        else
        {
           
        }




        Debug.Log("Color check started");
      


        float newR = 0f;
        float newG = 0f;
        float newB = 0f;
        int count = 0;
        //Debug.Log("blooo" + previewTexture.width + " " + previewTexture.height);
        
        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            frameColors = previewTexture.GetPixels((int)(previewTexture.width * 0.4), (int)(previewTexture.height * 0.4), (int)(previewTexture.width * 0.2), (int)(previewTexture.height * 0.2));
        }
        else
        {
            frameColors = m_LastCameraTexture.GetPixels((int)(m_LastCameraTexture.width * 0.4), (int)(m_LastCameraTexture.height * 0.6), (int)(m_LastCameraTexture.width * 0.2), (int)(m_LastCameraTexture.height * 0.2));
        }
        
        Debug.Log("Frame " + frameColors.Length);



        foreach (Color currentColor in frameColors)
        {
            newR += currentColor.r;
            newG += currentColor.g;
            newB += currentColor.b;
            count++;
        }






        Debug.Log("trains");

        currentColor = new Color(newR / (count * 0.65f), newG / (count * 0.65f), newB / (count * 0.65f));
        Debug.Log("planes");
        colorButton.GetComponent<Image>().color = currentColor;
        Debug.Log("autos");
        Debug.Log("Color is " + newR + " " + newG + " " + newB);
    }

    Texture2D ss;
    Renderer renderer;
    Renderer newRenderer;
    WebCamTexture camera;
    void Start()
    {
        
        raycastManager = aRSessionOrigin.GetComponent<ARRaycastManager>();
        Debug.Log(ARSession.CheckAvailability());
        //aRSessionOrigin.
        //aRCameraSubsystem = aRSessionOrigin.gameObject.transform.GetChild(0).gameObject.GetComponent<ARCameraManager>().subsystem;
        camsys = ARCamera.GetComponent<ARCameraManager>().subsystem;
        
        Debug.Log("camsys found");
        hits = new List<ARRaycastHit>();
        Debug.Log("hits found");

        m_LastCameraTexture = new Texture2D(Screen.width / 2, Screen.height / 2, TextureFormat.RGBA32, true);
        

        GetComponent<Button>().onClick.AddListener(delegate
        {
            //gameObject.SetActive(false);
            
            byte[] rawTexData;
            if(Application.platform == RuntimePlatform.Android)
            {

                /*gameObject.GetComponent<MeshRenderer>().enabled = false;
                confirmWindow.SetActive(true);
                rawTexData = m_LastCameraTexture.GetRawTextureData();
                confirmWindow.GetComponent<ConfirmTargetScript>().ConfigureWindow(-1, currentColor, currentHitPosition, rawTexData, false);
                //isTakingPicture = false;
                gameObject.GetComponent<MeshRenderer>().enabled = true;
                //rawTexData = photo.GetRawTextureData();*/

                /*
                //yield return null;
                Debug.Log("russell11");
                GetComponent<Image>().enabled = false;
                Debug.Log("russell22");

                Debug.Log("allison");
                confirmWindow.SetActive(true);
                Debug.Log("jefferson");
                //photo = ScreenCapture.CaptureScreenshotAsTexture();
                Debug.Log("lincoln");
               // yield return new WaitForEndOfFrame();
                confirmWindow.GetComponent<ConfirmTargetScript>().ConfigureWindow(-1, currentColor, currentHitPosition, imageBytes, false);
                Debug.Log("washington");
                gameObject.GetComponent<Image>().enabled = true;
                */

                /******
                Debug.Log("android wash");
                StartCoroutine(takePhoto());
                Debug.Log("android mac");

                ****/


            }
            else
            {
                confirmWindow.SetActive(true);
                rawTexData = previewTexture.GetRawTextureData();
                confirmWindow.GetComponent<ConfirmTargetScript>().ConfigureWindow(-1, currentColor, currentHitPosition, rawTexData, false);
            }
            
            

        });
        
        if (Application.platform == RuntimePlatform.Android)
        {

            
            
        }

        //currentTargets = new List<SpyTarget>();

        //rayHit = new RaycastHit();

        convParams = new XRCpuImage.ConversionParams
        {
            // Get the entire image.
            inputRect = new RectInt(Screen.width / 4, Screen.height / 4, Screen.height, Screen.width),

            // Downsample by 2.
            outputDimensions = new Vector2Int(Screen.height, Screen.width),

            // Choose RGBA format.
            outputFormat = TextureFormat.RGBA32,

            // Flip across the vertical axis (mirror image).
            //transformation = CameraImageTransformation.MirrorY
        };

        previewTexture = new Texture2D(convParams.outputDimensions.x, convParams.outputDimensions.y, convParams.outputFormat, false);
        
        ss = new Texture2D(androidTexture.width, androidTexture.height, convParams.outputFormat, false);

        
        Debug.Log("subway");
       Debug.Log("wendys");
        dinoHasGone = false;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    // Update is called once per frame
    Color32[] androidFrameColors;
    Texture2D photo;
    public RawImage rawimage;
    IEnumerator takePhoto()
    {

        yield return null;
        Debug.Log("russell11");
        bottomImage.gameObject.SetActive(false);
        Debug.Log("russell22");
        
        Debug.Log("allison");
        confirmWindow.SetActive(true);
        Debug.Log("jefferson");
        //photo = ScreenCapture.CaptureScreenshotAsTexture();
        Debug.Log("lincoln");
        yield return new WaitForEndOfFrame();
        confirmWindow.GetComponent<ConfirmTargetScript>().ConfigureWindow(-1, currentColor, currentHitPosition, imageBytes, false);
        Debug.Log("washington");
        bottomImage.gameObject.SetActive(true);


    }
    bool isTakingPicture = false;
    Texture2D m_LastCameraTexture;
    byte[] imageBytes;
    void Update()
    {
       
        //Debug.Log("Position Is " + ARCamera.transform.);
        if(isGame == false)
        {



            //Debug.Log("My Tex " + myTex);

            

            //Debug.Log("Raycast status " + raycastManager.Raycast(new Vector2(Screen.height / 2, Screen.width / 2), hits));
            //Debug.Log("Update Loop "+hits);
            if (raycastManager.Raycast(new Vector2(Screen.height / 2, Screen.width / 2), hits,TrackableType.FeaturePoint))
            {
                Debug.Log("droid raycast");
                loadingWindow.SetActive(false);
                if (dinoHasGone == false)
                {
                    dinoHasGone = true;
                    dinoNoir.GetComponent<NoirMasterScript>().ShowText(new string[] { "Greetings Gumshoe, Welcome to I SPY. I'm your sleuth guide, DINO NOIR", "This game is easy! First, look around the room for some colorful targets.", "Then, get close to one and press the Big Button to take its picture", "You can set up to 5 targets using this screen", "Once you've set your targets, press START GAME and give your phone to another sleuth to try and find the targets." },0);

                }

                float rayDist;

                if(Application.platform == RuntimePlatform.Android)
                {
                    rayDist = 0.5f;
                }
                else
                {
                    rayDist = 0.3f;
                }


                Debug.Log("it started");
                Debug.Log("Hits " + hits[0].distance);
                if (hits.Count == 2)
                {
                    Debug.Log("Hits2 " + hits[1].distance);
                }
                if (hits[0].distance < rayDist)
                {
                    Debug.Log("Yup" + hits);
                    GetComponent<Image>().sprite = greenButton;
                    GetComponent<Button>().interactable = true;
                    currentHitPosition = hits[0].pose.position;
                    currentSlope = Vector3.Angle(transform.position, currentHitPosition);
                    Debug.Log("Angle Is " + currentSlope);
                    getCloserText.gameObject.SetActive(false);
                }

                else
                {
                    Debug.Log("Nope " + hits.Count);
                    GetComponent<Image>().sprite = redButton;
                    GetComponent<Button>().interactable = false;
                    getCloserText.gameObject.SetActive(true);
                }



            }
            else
            {
                Debug.Log("Raycast error");
            }
            Debug.Log("pictureHEE");

            //here

            Debug.Log("Dimens " + theImage.width);
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                camsys.TryAcquireLatestCpuImage(out theImage);
                CheckColor();

                theImage.Dispose();
            }
            else
            {

              

                m_LastCameraTexture.ReadPixels(new Rect(Screen.width / 4, Screen.height / 4, Screen.width / 2, Screen.height / 2), 0, 0);
                m_LastCameraTexture.Apply();
                imageBytes = m_LastCameraTexture.GetRawTextureData();
                CheckColor();
               
               


            }

            

        }
        else
        {
            
        }
       





        




    }
}
