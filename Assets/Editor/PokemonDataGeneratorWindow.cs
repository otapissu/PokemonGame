using UnityEngine;
using UnityEditor;

public class PokemonDataBulkUpdaterWindow : EditorWindow
{
    private string folderPath = "Assets/Resources/PokemonData";
    private bool fillEmptyPokemonName = true;
    private bool initializeEvolutionOptionsIfNull = true;
    private bool setDefaultEvolveRequireLevelIfZeroOrLess = true;
    private bool fixInvalidMaleRatio = true;
    private bool setCanHatchIfNeeded = false;
    private bool setGenderVisualTypeIfNeeded = false;

    private int defaultEvolveRequireLevel = 5;
    private float defaultMaleRatio = 0.5f;
    private bool defaultCanHatch = true;
    private GenderVisualType defaultGenderVisualType = GenderVisualType.SameVisual;

    [MenuItem("Tools/Pokemon Data Bulk Updater")]
    public static void ShowWindow()
    {
        GetWindow<PokemonDataBulkUpdaterWindow>("Pokemon Data Bulk Updater");
    }

    private void OnGUI()
    {
        GUILayout.Label("기존 PokemonData 일괄 수정", EditorStyles.boldLabel);

        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        GUILayout.Space(8f);
        GUILayout.Label("수정 옵션", EditorStyles.boldLabel);

        fillEmptyPokemonName = EditorGUILayout.Toggle("빈 이름 자동 채우기", fillEmptyPokemonName);
        initializeEvolutionOptionsIfNull = EditorGUILayout.Toggle("null evolutionOptions 초기화", initializeEvolutionOptionsIfNull);
        setDefaultEvolveRequireLevelIfZeroOrLess = EditorGUILayout.Toggle("evolveRequireLevel 0 이하 기본값 세팅", setDefaultEvolveRequireLevelIfZeroOrLess);
        fixInvalidMaleRatio = EditorGUILayout.Toggle("maleRatio 범위 보정", fixInvalidMaleRatio);
        setCanHatchIfNeeded = EditorGUILayout.Toggle("canHatch 강제 세팅", setCanHatchIfNeeded);
        setGenderVisualTypeIfNeeded = EditorGUILayout.Toggle("genderVisualType 강제 세팅", setGenderVisualTypeIfNeeded);

        GUILayout.Space(8f);
        GUILayout.Label("기본값", EditorStyles.boldLabel);

        defaultEvolveRequireLevel = EditorGUILayout.IntField("Default Evolve Level", defaultEvolveRequireLevel);
        defaultMaleRatio = EditorGUILayout.FloatField("Default Male Ratio", defaultMaleRatio);
        defaultCanHatch = EditorGUILayout.Toggle("Default Can Hatch", defaultCanHatch);
        defaultGenderVisualType = (GenderVisualType)EditorGUILayout.EnumPopup("Default Gender Visual Type", defaultGenderVisualType);

        GUILayout.Space(12f);

        if (GUILayout.Button("선택 폴더의 PokemonData 전부 수정"))
        {
            UpdateAllPokemonDataInFolder();
        }
    }

    private void UpdateAllPokemonDataInFolder()
    {
        string[] guids = AssetDatabase.FindAssets("t:PokemonData", new string[] { folderPath });

        if (guids == null || guids.Length == 0)
        {
            Debug.LogWarning("PokemonData 에셋을 찾지 못했습니다. 폴더 경로를 확인하세요: " + folderPath);
            return;
        }

        int updatedCount = 0;
        int skippedCount = 0;

        AssetDatabase.StartAssetEditing();

        try
        {
            for (int i = 0; i < guids.Length; i++)
            {
                string guid = guids[i];
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                PokemonData data = AssetDatabase.LoadAssetAtPath<PokemonData>(assetPath);

                if (data == null)
                {
                    skippedCount++;
                    continue;
                }

                bool changed = UpdateSinglePokemonData(data);

                if (changed)
                {
                    EditorUtility.SetDirty(data);
                    updatedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("PokemonData 일괄 수정 완료. 수정됨: " + updatedCount + " / 변경 없음: " + skippedCount);
    }

    private bool UpdateSinglePokemonData(PokemonData data)
    {
        bool changed = false;

        Undo.RecordObject(data, "Bulk Update PokemonData");

        if (fillEmptyPokemonName)
        {
            if (string.IsNullOrWhiteSpace(data.pokemonName))
            {
                data.pokemonName = "Pokemon_" + data.id.ToString("D3");
                changed = true;
            }
        }

        if (initializeEvolutionOptionsIfNull)
        {
            if (data.evolutionOptions == null)
            {
                data.evolutionOptions = new EvolutionOption[0];
                changed = true;
            }
        }


        if (fixInvalidMaleRatio)
        {
            float clamped = Mathf.Clamp01(data.maleRatio);

            if (!Mathf.Approximately(clamped, data.maleRatio))
            {
                data.maleRatio = clamped;
                changed = true;
            }

            if (data.genderVisualType != GenderVisualType.None && Mathf.Approximately(data.maleRatio, 0f))
            {
                data.maleRatio = defaultMaleRatio;
                changed = true;
            }
        }

        if (setCanHatchIfNeeded)
        {
            if (data.canHatch != defaultCanHatch)
            {
                data.canHatch = defaultCanHatch;
                changed = true;
            }
        }

        if (setGenderVisualTypeIfNeeded)
        {
            if (data.genderVisualType != defaultGenderVisualType)
            {
                data.genderVisualType = defaultGenderVisualType;
                changed = true;
            }
        }

        return changed;
    }
}