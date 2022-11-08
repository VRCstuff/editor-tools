#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public class GlobalTextureSettings : EditorWindow
{
//AndroidOverride
//TextureImporterPlatformSettings is kinda cool and I want to check it out!
//This will probably eventually handle double crunch settings.
//https://docs.unity3d.com/ScriptReference/TextureImporterPlatformSettings-maxTextureSize. //<- This
//https://docs.unity3d.com/ScriptReference/TextureImporter.SetPlatformTextureSettings.html 

    //Crunch
    bool crunchEnabled = true;
    bool useCrunchCompression = true;
    int crunchQuality = 75;
    int numberOfCrunchedTextures = 0;

    //Scale
    bool scalingEnabled = true;
    string[] names = new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" };
    int[] sizes = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };
    int selectedSize = 1;
    int numberOfScaledTextures = 0;


    [MenuItem("Window/GlobalTextureSettings")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(GlobalTextureSettings));
    }

    void OnGUI()
    {
        GUILayout.Label("Made By Tummy", EditorStyles.boldLabel);
        GUILayout.Label("Beware, all settings apply to all textures.", EditorStyles.boldLabel);
        //myString = EditorGUILayout.TextField("Text Field", myString);
        //myBool = EditorGUILayout.Toggle("Toggle", myBool);
        //crunchQuality = (int)EditorGUILayout.Slider("Slider", crunchQuality, 0, 100);

        //Crunch
        crunchEnabled = EditorGUILayout.BeginToggleGroup("Crunch Compression settings.", crunchEnabled);
        useCrunchCompression = EditorGUILayout.Toggle("Enable CrunchCompression", useCrunchCompression);
        crunchQuality = EditorGUILayout.IntSlider("Crunch Quality", crunchQuality, 0, 100);
        EditorGUILayout.EndToggleGroup();
        //Scaling
        scalingEnabled = EditorGUILayout.BeginToggleGroup("Set texture max size", scalingEnabled);
        selectedSize = EditorGUILayout.IntPopup("Max Size: ", selectedSize, names, sizes);
        EditorGUILayout.EndToggleGroup();

        //Button
        if (GUILayout.Button("Run all selected.")) {
            Everything(); 
        }
    }

    void Everything()
    {

        Debug.Log("Started the job. Please wait.");
        Debug.Log("Crunch set to: " + crunchQuality);
        Debug.Log("Max tex set to: " + selectedSize);
        foreach (string guid in AssetDatabase.FindAssets("t:texture", null))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (crunchEnabled)
            {
                if (useCrunchCompression)
                {
                    if (textureImporter && !textureImporter.crunchedCompression && textureImporter.compressionQuality != crunchQuality)
                    {
                        textureImporter.crunchedCompression = true;
                        textureImporter.compressionQuality = crunchQuality;
                        AssetDatabase.ImportAsset(path);
                        numberOfCrunchedTextures++;

                    }
                }
                else
                {
                    textureImporter.crunchedCompression = false;
                    AssetDatabase.ImportAsset(path);

                }
            }
            if (scalingEnabled)
            {
                Debug.Log(selectedSize);
                textureImporter.maxTextureSize = 64;
                AssetDatabase.ImportAsset(path);
                numberOfScaledTextures++;

            }


        }
        //This needs to be changed.
        Debug.Log(numberOfCrunchedTextures + " crunched and " + numberOfScaledTextures + " scaled textures.");
    }

}
#endif