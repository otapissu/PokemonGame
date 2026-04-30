using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class SpriteBatchProcessor
{
    private const int TargetPixelsPerUnit = 64;
    private const int SliceColumns = 2;
    private const int SliceRows = 1;

    [MenuItem("Tools/Sprites/Apply Pixel Settings And Slice 2x1")]
    public static void ApplySettingsToSelectedTextures()
    {
        Object[] selectedObjects = Selection.objects;

        if (selectedObjects == null || selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("�˸�", "Project â���� �ؽ�ó�� ���� �����ϼ���.", "Ȯ��");
            return;
        }

        int processedCount = 0;

        foreach (Object selectedObject in selectedObjects)
        {
            string assetPath = AssetDatabase.GetAssetPath(selectedObject);

            if (string.IsNullOrEmpty(assetPath))
            {
                continue;
            }

            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (textureImporter == null)
            {
                continue;
            }

            ApplyTextureSettings(textureImporter);
            ApplyGridSlice(textureImporter, assetPath, SliceColumns, SliceRows);

            processedCount++;
        }

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog( "�Ϸ�", $"�� {processedCount}���� �ؽ�ó�� ������ �����߽��ϴ�.", "Ȯ��" );
    }

    private static void ApplyTextureSettings(TextureImporter textureImporter)
    {
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Multiple;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.spritePixelsPerUnit = TargetPixelsPerUnit;
        textureImporter.mipmapEnabled = false;
        textureImporter.alphaIsTransparency = true;
        textureImporter.SaveAndReimport();
    }

    private static void ApplyGridSlice(TextureImporter textureImporter, string assetPath, int columns, int rows)
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

        if (texture == null)
        {
            return;
        }

        int textureWidth = texture.width;
        int textureHeight = texture.height;

        if (columns <= 0 || rows <= 0)
        {
            Debug.LogWarning($"�߸��� �����̽� ��: {assetPath}");
            return;
        }

        int cellWidth = textureWidth / columns;
        int cellHeight = textureHeight / rows;

        List<SpriteRect> spriteRects = new List<SpriteRect>();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int x = col * cellWidth;
                int y = textureHeight - ((row + 1) * cellHeight);

                SpriteRect spriteRect = new SpriteRect
                {
                    name = $"{texture.name}_{row}_{col}", rect = new Rect(x, y, cellWidth, cellHeight), alignment = SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f)
                };

                spriteRects.Add(spriteRect);
            }
        }

        SpriteDataProviderFactories factories = new SpriteDataProviderFactories();
        factories.Init();

        ISpriteEditorDataProvider dataProvider =
            factories.GetSpriteEditorDataProviderFromObject(textureImporter);

        if (dataProvider == null)
        {
            Debug.LogWarning($"SpriteEditorDataProvider�� �������� ���߽��ϴ�: {assetPath}");
            return;
        }

        dataProvider.InitSpriteEditorDataProvider();
        dataProvider.SetSpriteRects(spriteRects.ToArray());
        dataProvider.Apply();

        textureImporter.SaveAndReimport();
    }
}