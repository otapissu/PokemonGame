using UnityEngine;
using UnityEditor;
using System.IO;

public class PokemonDataGeneratorWindow : EditorWindow
{
    private int startID = 1;
    private int endID = 151;

    [MenuItem("Tools/Pokemon Data Generator")]
    public static void ShowWindow()
    {
        GetWindow<PokemonDataGeneratorWindow>("Pokemon Data Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Pokemon Data 자동 생성", EditorStyles.boldLabel);

        startID = EditorGUILayout.IntField("Start ID", startID);
        endID = EditorGUILayout.IntField("End ID", endID);

        if (GUILayout.Button("Generate"))
        {
            GeneratePokemonData(startID, endID);
        }
    }

    void GeneratePokemonData(int start, int end)
    {
        if (start > end)
        {
            Debug.LogError("Start ID가 End ID보다 클 수 없습니다.");
            return;
        }

        string folderPath = "Assets/Resources/PokemonData";

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        for (int i = start; i <= end; i++)
        {
            string idString = i.ToString("D3");
            string assetPath = folderPath + "/" + idString + ".asset";

            if (File.Exists(assetPath))
            {
                Debug.Log("이미 존재함: " + idString);
                continue;
            }

            PokemonData data = ScriptableObject.CreateInstance<PokemonData>();

            data.id = i;
            data.pokemonName = "Pokemon_" + idString;
            data.canHatch = true;

            // 기본 성별 설정
            data.genderVisualType = GenderVisualType.SameVisual;
            data.maleRatio = 0.5f;

            // 진화는 수동으로 연결
            data.nextEvolution = null;
            data.secondEvolution = null;

            AssetDatabase.CreateAsset(data, assetPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("생성 완료: " + start + " ~ " + end);
    }
}