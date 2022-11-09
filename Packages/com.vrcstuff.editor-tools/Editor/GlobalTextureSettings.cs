#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEditor.AnimatedValues;
using System.Linq;
using System;
using System.Collections.Generic;

public class GlobalTextureSettings : EditorWindow
{
    //TODO
    //Save variables put in the editor. So they are persistant between sessions.
    //Change platform specific to android override. So PC uses the Default while android is the exception.
    //Add option to revert all import settings in case someone do an oopsie with someones GUI. This is quite important. Can i clean up meta files?
    //Make GUI better? Really low priority!

    //Hiding stuff.
    //https://answers.unity.com/questions/192895/hideshow-properties-dynamically-in-inspector.html

    //Saving settings
    //https://forum.unity.com/threads/custom-editor-variables-not-saving.513406/

    //AndroidOverride
    //TextureImporterPlatformSettings is kinda cool and I want to check it out!
    //This will probably eventually handle double crunch settings.
    //https://docs.unity3d.com/ScriptReference/TextureImporterPlatformSettings-maxTextureSize. //<- This
    //https://docs.unity3d.com/ScriptReference/TextureImporter.SetPlatformTextureSettings.html 
    //https://bilalakil.me/unity-editor-hack-2-programatically-change-texture-import-settings-different-platforms

    //misc
    bool platformSpecific = false;
    bool resetToDefault = false;
    bool forceMaxres = false;

    string[] names = new string[] { "32", "64", "128", "256", "512", "1024", "2048", "4096", "8192" };
    int[] sizes = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192 };

    //Settings for both platforms
    bool bothCrunchEnabled = true;
    int bothCrunchQuality = 75;
    bool bothScalingEnabled = false;
    int bothSelectedMaxSize = 2048;

    //Android Settings
    bool androidCrunchEnabled = true;
    int androidCrunchQuality = 75;
    bool androidScalingEnabled = false;
    int androidSelectedMaxSize = 2048;

    //PC settings
    bool pcCrunchEnabled = true;
    int pcCrunchQuality = 75;
    bool pcScalingEnabled = false;
    int pcSelectedMaxSize = 2048;

    //Counting end result
    int numberOfCrunchedTextures = 0;
    int numberOfScaledTextures = 0;

    //Textures to process.
    bool onlyInScene = false;
    bool isBlackList = true;
    public string[] Folders = { "Assets/VRCSDK", "Assets/ExampleFolder" };


    [MenuItem("TummyTime/GlobalTextureSettings")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(GlobalTextureSettings));

    }

    void OnGUI()
    {



        GUILayout.Label("By Tummy / Version 0.3", EditorStyles.boldLabel);
        GUILayout.Label("Experimental. have you heard of regular backups or GIT?", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("If you have a large avatar that is using an atlas. You might not wont to change the texture size.", MessageType.Info, wide: true);
        //resetToDefault = EditorGUILayout.BeginToggleGroup("Untick to reset all textures to default import settings.", resetToDefault);
        resetToDefault = GUILayout.Toggle(resetToDefault, "Reset to default");

        if (resetToDefault)
        {
            EditorGUILayout.HelpBox("This still dosn't work!", MessageType.Error, wide: true);
            //This needs to grab the standard settings for each texture. There is no cool function to fix this.

        }
        else
        {
            forceMaxres = GUILayout.Toggle(forceMaxres, "Force the max res.");
            platformSpecific = GUILayout.Toggle(platformSpecific, "Platform specific settings.");
            EditorGUILayout.Space();
            //EditorGUI.indentLevel++;
            if (platformSpecific)
            {
                EditorGUILayout.HelpBox("This still dosn't work as it should.", MessageType.Error, wide: true);

                //PC

                GUILayout.Label("PC settings", EditorStyles.label);
                EditorGUI.indentLevel++;
                pcCrunchEnabled = EditorGUILayout.BeginToggleGroup("Enable Crunch", pcCrunchEnabled);
                pcCrunchQuality = EditorGUILayout.IntSlider("Crunch Quality", pcCrunchQuality, 0, 100);
                EditorGUILayout.EndToggleGroup();

                pcScalingEnabled = EditorGUILayout.BeginToggleGroup("Set texture max size", pcScalingEnabled);
                pcSelectedMaxSize = EditorGUILayout.IntPopup("Max Size: ", pcSelectedMaxSize, names, sizes);
                EditorGUILayout.EndToggleGroup();
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                //Android

                GUILayout.Label("Android settings", EditorStyles.label);
                EditorGUI.indentLevel++;
                androidCrunchEnabled = EditorGUILayout.BeginToggleGroup("Enable Crunch", androidCrunchEnabled);
                androidCrunchQuality = EditorGUILayout.IntSlider("Crunch Quality", androidCrunchQuality, 0, 100);
                EditorGUILayout.EndToggleGroup();

                androidScalingEnabled = EditorGUILayout.BeginToggleGroup("Set texture max size", androidScalingEnabled);
                androidSelectedMaxSize = EditorGUILayout.IntPopup("Max Size: ", androidSelectedMaxSize, names, sizes);
                EditorGUILayout.EndToggleGroup();
                EditorGUILayout.Space();

            }
            else
            {
                //Both

                //EditorGUI.indentLevel++;
                GUILayout.Label("Both platform settings", EditorStyles.label);
                EditorGUI.indentLevel++;
                bothCrunchEnabled = EditorGUILayout.BeginToggleGroup("Enable Crunch", bothCrunchEnabled);
                bothCrunchQuality = EditorGUILayout.IntSlider("Crunch Quality", bothCrunchQuality, 0, 100);
                EditorGUILayout.EndToggleGroup();

                bothScalingEnabled = EditorGUILayout.BeginToggleGroup("Set texture max size", bothScalingEnabled);
                bothSelectedMaxSize = EditorGUILayout.IntPopup("Max Size: ", bothSelectedMaxSize, names, sizes);
                EditorGUILayout.EndToggleGroup();
            }
        }

        onlyInScene = GUILayout.Toggle(onlyInScene, "Only run on textures loaded in scene.");

        if (onlyInScene)
        {
            EditorGUILayout.HelpBox("This only applies to what you can see. If you swap materials or textures with animations or other means, they will not be processed. I will try to fix this in a later version.", MessageType.Warning, wide: true);

        }
        else
        {
            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty stringsProperty = so.FindProperty("Folders");
            isBlackList = GUILayout.Toggle(isBlackList, "Is the following folders a blacklist?");
            if (isBlackList)
            {
                GUILayout.Label("Folders to skip", EditorStyles.boldLabel);
            }
            else
            {
                GUILayout.Label("Folders to include", EditorStyles.boldLabel);
            }
            EditorGUILayout.PropertyField(stringsProperty, true); // True means show children
            so.ApplyModifiedProperties(); // Remember to apply modified properties

        }



        EditorGUI.indentLevel--;

        if (GUILayout.Button("Apply selected settings."))
        {

            if (onlyInScene)
            {
                RunAllInScene();
            }
            else
            {
                RunAllInAssets();
            }
        }

    }

    void RunAllInScene()
    {
        numberOfCrunchedTextures = 0;
        numberOfScaledTextures = 0;
        List<string> foundTextures = new List<string>();

        foreach (Renderer renderer in UnityEngine.Object.FindObjectsOfType<Renderer>())
        {
            foreach (Material mat in renderer.sharedMaterials)
            {

                Shader shader = mat.shader;
                for (int i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                    {
                        Texture texture = renderer.sharedMaterial.GetTexture(ShaderUtil.GetPropertyName(shader, i));
                        if (texture != null)
                        {
                            string texPath = AssetDatabase.GetAssetPath(texture.GetInstanceID());
                            if (!foundTextures.Contains(texPath))
                            {
                                Debug.Log("Found " + AssetDatabase.GetAssetPath(texture.GetInstanceID()) + " in scene.");
                                foundTextures.Add(texPath);
                                //AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(texture.GetInstanceID()));
                            }
                        }
                    }
                }
            }
        }
        foreach (string path in foundTextures)
        {
            if (resetToDefault)
            {
                Debug.Log("You didn't read in the editor.");

            }
            else
            {
                TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                if (textureImporter)
                {
                if (platformSpecific)
                {
                    CrunchAndMaxtexRun(textureImporter, "android", androidCrunchEnabled, androidCrunchQuality, androidScalingEnabled, androidSelectedMaxSize);
                    CrunchAndMaxtexRun(textureImporter, "Standalone", pcCrunchEnabled, pcCrunchQuality, pcScalingEnabled, pcSelectedMaxSize);
                }
                else
                {
                    CrunchAndMaxtexRun(textureImporter, "both", bothCrunchEnabled, bothCrunchQuality, bothScalingEnabled, bothSelectedMaxSize);
                }
                }
            }
        }
        Debug.Log(numberOfCrunchedTextures + " crunched and " + numberOfScaledTextures + " scaled textures.");
    }
    void RunAllInAssets()
    {
        Debug.Log("Started job on both platforms with same settings. Please be patient.");
        numberOfCrunchedTextures = 0;
        numberOfScaledTextures = 0;
        foreach (string guid in AssetDatabase.FindAssets("t:texture", null))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (CheckIfIgnorePath(path)) { Debug.Log("Skipped " + path); continue; } //This will skip stuff if applicable. And create a log message
            if (textureImporter)
            {
                if (resetToDefault)
                {
                    Debug.Log("You didn't read in the editor.");
                }
                else
                {
                    if (platformSpecific)
                    {
                        CrunchAndMaxtexRun(textureImporter, "android", androidCrunchEnabled, androidCrunchQuality, androidScalingEnabled, androidSelectedMaxSize);
                        CrunchAndMaxtexRun(textureImporter, "Standalone", pcCrunchEnabled, pcCrunchQuality, pcScalingEnabled, pcSelectedMaxSize);
                    }
                    else
                    {
                        CrunchAndMaxtexRun(textureImporter, "both", bothCrunchEnabled, bothCrunchQuality, bothScalingEnabled, bothSelectedMaxSize);
                    }
                }
            }
        }
        //This needs to be changed.
        Debug.Log(numberOfCrunchedTextures + " crunched and " + numberOfScaledTextures + " scaled textures.");
    }

    bool CheckIfIgnorePath(string incPath) //This will give back a true if path should be ignored. Works.
    {
        foreach (string folder in Folders)
        {
            bool contains = incPath.ToLower().Contains(folder.ToLower());//Makes stuff lowercase and checks it.

            if (contains && isBlackList || !contains && !isBlackList)
            {
                return true;
            }
        }
        return false;
    }

    void ResetImportSettings() //Will reset all images in the whole project. Currently dosn't work at all.
    {
        //This needs to check the standard import settings and use them. seems like there is no method to reset import settings... Weird...
        foreach (string guid in AssetDatabase.FindAssets("t:texture", null))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (textureImporter)
            {
                textureImporter.ClearPlatformTextureSettings(platform: "Android");
                textureImporter.ClearPlatformTextureSettings(platform: "Standalone"); //Standalone refers to Mac, Windows and Linux.


                AssetDatabase.ImportAsset(path);
                Debug.Log("Reset " + textureImporter.assetPath);
            }
        }
    }

    //Please fix the android part.
    void CrunchAndMaxtexRun(TextureImporter texImporter, string platform, bool crunchEnable, int crunchLevel, bool texMaxResEnable, int texMaxRes)
    {
        bool reimport = false;

        if (platform == "both")
        {

            if ((crunchEnable && !texImporter.crunchedCompression || texImporter.compressionQuality != crunchLevel))
            {

                texImporter.crunchedCompression = true;
                texImporter.compressionQuality = bothCrunchQuality;
                numberOfCrunchedTextures++;
                reimport = true;
            }

            if (texMaxResEnable && ((forceMaxres) || (!forceMaxres && texImporter.maxTextureSize > texMaxRes)))
            {
                texImporter.maxTextureSize = (int)bothSelectedMaxSize;
                numberOfScaledTextures++;
                reimport = true;
            }

            if (reimport) AssetDatabase.ImportAsset(texImporter.assetPath);

            return;
        }

        if (crunchEnable && texImporter.compressionQuality != crunchLevel)
        {

        }

        if (texMaxResEnable && ((forceMaxres) || (!forceMaxres && texImporter.maxTextureSize > texMaxRes)))
        {

        }

        if (reimport) AssetDatabase.ImportAsset(texImporter.assetPath);

        return;
    }

}
#endif