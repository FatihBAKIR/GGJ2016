using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class DrawingInterface
{
    #region DrawLine
    public static void DrawLine(Vector3 start, Vector3 end)
    {
        DrawLine(start, end, Color.yellow, -1f);
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        DrawLine(start, end, color, -1);
    }

    public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration)
    {
        var ln = Camera.main.GetComponent<LineHelper>().AddLine(start, end, color);
        ln.lifeTime = duration;
    }
    #endregion

    #region DrawPolyLine
    public static void DrawPolyLine(List<Vector3> nodes)
    {
        DrawPolyLine(nodes, Color.yellow, -1);
    }
    public static void DrawPolyLine(List<Vector3> nodes, Color color)
    {
        DrawPolyLine(nodes, color, -1);
    }
    public static void DrawPolyLine(List<Vector3> nodes, Color color, float duration, bool isClosed = false, bool isAlternating = false)
    {
        var pLine = new PolyLine(nodes, color, isClosed, isAlternating);
        pLine.lifeTime = duration;
        Camera.main.GetComponent<LineHelper>().AddPolyLine(pLine);
    }
    #endregion
}