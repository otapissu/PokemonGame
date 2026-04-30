using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

public static class SpriteAutoGridByHeightTool
{
    private const int TargetPixelsPerUnit = 64;

    [MenuItem("Tools/Sprites/Apply Auto MaxSize + PPU64 + Point + None + Slice By Height")]
    public static void ApplyToSelectedTextures()
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

            int sourceWidth;
            int sourceHeight;
            GetSourceTextureSize(textureImporter, out sourceWidth, out sourceHeight);

            if (sourceWidth <= 0 || sourceHeight <= 0)
            {
                Debug.LogWarning($"���� �ؽ�ó ũ�⸦ ���� ���߽��ϴ�: {assetPath}");
                continue;
            }

            int targetMaxSize = GetTargetMaxSize(sourceWidth);

            ApplyTextureSettings(textureImporter, targetMaxSize);
            ApplyGridSliceByCellSize(textureImporter, assetPath, sourceWidth, sourceHeight, sourceHeight);

            processedCount++;
        }

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog( "�Ϸ�", $"�� {processedCount}���� �ؽ�ó�� ������ �����߽��ϴ�.", "Ȯ��" );
    }

    private static void GetSourceTextureSize(TextureImporter textureImporter, out int width, out int height)
    {
        width = 0;
        height = 0;

        textureImporter.GetSourceTextureWidthAndHeight(out width, out height);
    }

    private static int GetTargetMaxSize(int sourceWidth)
    {
        if (sourceWidth >= 8192)
        {
            return 16384;
        }
        else if (sourceWidth >= 4096)
        {
            return 8192;
        }
        else if (sourceWidth >= 2048)
        {
            return 4096;
        }
        else
        {
            return 2048;
        }
    }

    private static void ApplyTextureSettings(TextureImporter textureImporter, int targetMaxSize)
    {
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Multiple;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.spritePixelsPerUnit = TargetPixelsPerUnit;
        textureImporter.mipmapEnabled = false;
        textureImporter.alphaIsTransparency = true;
        textureImporter.maxTextureSize = targetMaxSize;

        textureImporter.SaveAndReimport();
    }

    private static void ApplyGridSliceByCellSize( TextureImporter textureImporter, string assetPath, int sourceWidth, int sourceHeight, int cellSize)
    {
        if (cellSize <= 0)
        {
            Debug.LogWarning($"�� ũ�Ⱑ �ùٸ��� �ʽ��ϴ�: {assetPath}");
            return;
        }

        int columns = Mathf.CeilToInt((float)sourceWidth / cellSize);
        int rows = Mathf.CeilToInt((float)sourceHeight / cellSize);

        List<SpriteRect> spriteRects = new List<SpriteRect>();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int x = col * cellSize;
                int yFromTop = row * cellSize;

                int rectWidth = Mathf.Min(cellSize, sourceWidth - x);
                int rectHeight = Mathf.Min(cellSize, sourceHeight - yFromTop);

                if (rectWidth <= 0 || rectHeight <= 0)
                {
                    continue;
                }

                float unityY = sourceHeight - yFromTop - rectHeight;

                SpriteRect spriteRect = new SpriteRect
                {
                    name = $"{System.IO.Path.GetFileNameWithoutExtension(assetPath)}_{row}_{col}", rect = new Rect(x, unityY, rectWidth, rectHeight), alignment = SpriteAlignment.Center, pivot = new Vector2(0.5f, 0.5f)
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