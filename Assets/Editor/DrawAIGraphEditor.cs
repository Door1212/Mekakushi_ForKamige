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
        base.OnInspectorGUI(); // �f�t�H���g��Inspector��`��

        // �O���t��`��
        DrawGraph();
    }

    private void DrawGraph()
    {
        DrawAIGraph graph = (DrawAIGraph)target;
        if (graph.graphCurve == null) return;

        int width = 200;
        int height = 100;
        graphTexture = new Texture2D(width, height);

        // �w�i������
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black;
        }
        graphTexture.SetPixels(pixels);

        // �O���t���v���b�g
        for (int x = 0; x < width; x++)
        {
            float t = (float)x / width;
            float y = graph.graphCurve.Evaluate(t); // �J�[�u����l���擾
            int pixelY = (int)(y * height);

            if (pixelY >= 0 && pixelY < height)
            {
                graphTexture.SetPixel(x, pixelY, Color.green); // �ΐF�ŕ`��
            }
        }

        graphTexture.Apply();
        GUILayout.Label(graphTexture);
    }
}

