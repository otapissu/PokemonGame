using UnityEngine;
using UnityEditor;

public class PokemonDataBatchTool : EditorWindow
{
    [MenuItem("Tools/Pokemon Data Batch Tool")]
    public static void ShowWindow()
    {
        GetWindow<PokemonDataBatchTool>("Pokemon Data Batch Tool");
    }

    void OnGUI()
    {
        GUILayout.Label("PokemonData 일괄 수정", EditorStyles.boldLabel);

        if (GUILayout.Button("모든 canHatch 체크 해제 (false)"))
        {
            SetCanHatch(false);
        }

        if (GUILayout.Button("모든 canHatch 체크 (true)"))
        {
            SetCanHatch(true);
        }
    }

    void SetCanHatch(bool value)
    {
        PokemonData[] all = Resources.LoadAll<PokemonData>("PokemonData");

        foreach (var data in all)
        {
            data.canHatch = value;
            EditorUtility.SetDirty(data);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("모든 PokemonData canHatch = " + value);
    }
}