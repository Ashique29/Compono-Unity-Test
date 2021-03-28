using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ScreenCapture : MonoBehaviour
{
    Camera snapCam;
    public GameObject[] prefabs;
    int frameCounter = 0;
    int prefabDirectoryIndex = 0;
    int resHeight = 512;
    int resWidth = 512;


    void Awake()
    {
        snapCam = GetComponent<Camera>();
        if (snapCam.targetTexture == null)
        {
            snapCam.targetTexture = new RenderTexture(resWidth, resHeight, 24);
        }
        else
        {
            resWidth = snapCam.targetTexture.width;
            resHeight = snapCam.targetTexture.height;
        }
    }

    void Start()
    {
        createPrefabDirectory(prefabDirectoryIndex);
        StartCoroutine(CreateScreenshots(Application.dataPath + "/Output/" + prefabs[prefabDirectoryIndex].name + "/"));
    }

    private IEnumerator CreateScreenshots(string path)
    {
        //Spawning prefabs
        GameObject SpawnedObject = Instantiate(prefabs[prefabDirectoryIndex], new Vector3(0, 0, 6), Quaternion.identity);
       
        //Rotate prefabs and take snapshot
        for (float i = 0; i < 360; i += 22.5f)
        {
            // Wait untill rendered
            yield return new WaitForEndOfFrame();

            Texture2D texture = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            snapCam.Render();
            RenderTexture.active = snapCam.targetTexture;


            // Read screen contents into the texture
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();

            // Write to file
            byte[] bytes = texture.EncodeToPNG();
            string frameName = "frame" + frameCounter.ToString().PadLeft(4, '0');
            System.IO.File.WriteAllBytes(path + frameName  + ".png", bytes);
            frameCounter++;
            if (frameCounter == 16)
            {
                Destroy(SpawnedObject);
                prefabDirectoryIndex++;
                if (prefabDirectoryIndex < prefabs.Length)
                {
                    createPrefabDirectory(prefabDirectoryIndex);
                    StartCoroutine(CreateScreenshots(Application.dataPath + "/Output/" + prefabs[prefabDirectoryIndex].name + "/"));
                }
                else
                {
                    Quit();
                }

                frameCounter = 0;
            }

            // Clean up the used texture
            Destroy(texture);

            // Object rotation 
            SpawnedObject.transform.Rotate(new Vector3(0, i, 0));
        }
    }

    //Directory Creation
    void createPrefabDirectory(int index)
    {
        if (!Directory.Exists("Assets/Output/" + prefabs[index].name))
            Directory.CreateDirectory("Assets/Output/" + prefabs[index].name);
    }

    //Quiting unity editor application
    public static void Quit()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
