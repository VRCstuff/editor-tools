#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

internal class ExcludeTagsFromBuild : IProcessSceneWithReport
{
	public int callbackOrder => 0;

	public void OnProcessScene(Scene scene, BuildReport report)
	{
		string tagToRemove = "";
		string tagToEnable = "";
#if UNITY_ANDROID
		tagToRemove = "PcOnly";
		tagToEnable = "QuestOnly";
#else
		tagToRemove = "QuestOnly";
		tagToEnable = "PcOnly";
#endif
		GameObject[] goToRemove = GameObject.FindGameObjectsWithTag(tagToRemove);
		foreach (GameObject obj in goToRemove)
		{
			obj.SetActive(false);
			Debug.Log($"Hiding {obj.name}");
		}
		GameObject[] goToEnable = GameObject.FindGameObjectsWithTag(tagToEnable);
		foreach (GameObject obj in goToEnable)
		{
			obj.SetActive(true);
			Debug.Log($"Enabling {obj.name}");
		}
		goToRemove = GameObject.FindGameObjectsWithTag("EditorOnly");
		foreach (GameObject obj in goToRemove)
		{
			if (obj.name.StartsWith("__ClientSim")) continue;
			obj.SetActive(false);
			Debug.Log($"Hiding {obj.name}");
		}
	}
}
#endif