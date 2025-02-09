using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;

[CustomEditor(typeof(DrawAIGraph))]
public class DrawAIGraphEditor : Editor
{
    private Texture2D graphTexture;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // デフォルトのInspectorを描画

        // グラフを描画
        DrawGraph();
    }

    private void DrawGraph()
    {
        DrawAIGraph graph = (DrawAIGraph)target;
        if (graph.graphCurve == null) return;

        int width = 200;
        int height = 100;
        graphTexture = new Texture2D(width, height);

        // 背景を黒に
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black;
        }
        graphTexture.SetPixels(pixels);

        // グラフをプロット
        for (int x = 0; x < width; x++)
        {
            float t = (float)x / width;
            float y = graph.graphCurve.Evaluate(t); // カーブから値を取得
            int pixelY = (int)(y * height);

            if (pixelY >= 0 && pixelY < height)
            {
                graphTexture.SetPixel(x, pixelY, Color.green); // 緑色で描画
            }
        }

        graphTexture.Apply();
        GUILayout.Label(graphTexture);
    }
}

