using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace f16sbe.Graphics.Lines
{
    public class Line
    {
        public Line(Vector3 start, Vector3 end, Color color)
        {
            this.start = start;
            this.end = end;
            this.color = color;

            this.lifeTime = -1f;
        }

        public float lifeTime;
        public Vector3 start;
        public Vector3 end;
        public Color color;
    }

    public class PolyLine
    {
        public List<Vector3> nodesList;
        public Color color;
        public bool isClosed;
        public bool isAlternating;
        public float lifeTime;

        public PolyLine(List<Vector3> nodesList, Color color)
            : this(nodesList, color, false)
        {
        }

        public PolyLine(List<Vector3> nodesList, Color color, bool isClosed)
            : this(nodesList, color, isClosed, false)
        {
        }
        public PolyLine(List<Vector3> nodesList, Color color, bool isClosed, bool isAlternating)
        {
            this.nodesList = nodesList;
            this.color = color;
            this.isClosed = isClosed;
            this.isAlternating = isAlternating;
        }
    }

    public class LineHelper : MonoBehaviour
    {
        private List<PolyLine> polylineList = null;

        private List<Line> lines = null;

        private Material lineMaterial;

        void Awake()
        {
            this.Initialize();
        }

        public void Initialize()
        {
            this.lines = new List<Line>();
            this.polylineList = new List<PolyLine>();
            this.CreateLineMaterial();
        }

        public Line AddLine(Vector3 start, Vector3 end, Color color)
        {
            Line result = new Line(start, end, color);
            lines.Add(result);
            return result;
        }

        public void AddPolyLine(PolyLine polyLine)
        {
            polylineList.Add(polyLine);
        }

        public void RemovePolyLine(PolyLine polyLine)
        {
            polylineList.Remove(polyLine);
        }

        private void CreateLineMaterial()
        {
            //Renderable graph = MeshManager.Instance[MeshManager.MeshType.BaseNode];
            //lineMaterial = Object.Instantiate(graph.material) as Material;
            lineMaterial = new Material(Shader.Find("Lines/Colored Blended"));
            //lineMaterial = new Material("Shader \"Lines/Colored Blended\" {" +
            //    "SubShader { Pass { " +
            //    "    Blend SrcAlpha OneMinusSrcAlpha " +
            //    "    ZWrite On Cull Off Fog { Mode Off } " +
            //    "    BindChannels {" +
            //    "      Bind \"vertex\", vertex Bind \"color\", color }" +
            //    "} } }");
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
        }

        void OnPostRender()
        {
            GL.PushMatrix();
            lineMaterial.SetPass(0);
            GL.Begin(GL.LINES);

            List<Line> tmpLines = new List<Line>(this.lines);
            foreach (Line line in tmpLines)
            {
                GL.Color(line.color);
                GL.Vertex(line.start);
                GL.Vertex(line.end);

                line.lifeTime -= Time.deltaTime;
                if (line.lifeTime < 0)
                {
                    this.lines.Remove(line);
                }
            }

            GL.End();

            List<PolyLine>.Enumerator polyLineEnumarator = polylineList.GetEnumerator();


            List<PolyLine> tmpPolyLines = new List<PolyLine>(this.polylineList);
            foreach (PolyLine polyLine in tmpPolyLines)
            {
                // if there are at least two nodes to draw a line
                if (polyLine.nodesList.Count == 1)
                    continue;

                GL.Begin(GL.LINES);

                GL.Color(polyLine.color);

                List<Vector3>.Enumerator nodeEnumarator = polyLine.nodesList.GetEnumerator();
                nodeEnumarator.MoveNext();
                Vector3 first = nodeEnumarator.Current;
                Vector3 previous = first;

                bool fill = true;

                while (true)
                {
                    if (nodeEnumarator.MoveNext())
                    {
                        if (!polyLine.isAlternating || (polyLine.isAlternating && fill))
                        {
                            GL.Vertex(previous);
                            GL.Vertex(nodeEnumarator.Current);
                        }
                        fill = !fill;
                        previous = nodeEnumarator.Current;
                    }
                    else
                    {
                        if (polyLine.isClosed)
                        {
                            GL.Vertex(nodeEnumarator.Current);
                            GL.Vertex(first);
                        }
                        break;
                    }
                }
                polyLine.lifeTime -= Time.deltaTime;
                if (polyLine.lifeTime < 0)
                {
                    this.polylineList.Remove(polyLine);
                }
                GL.End();
            }


            GL.PopMatrix();
        }
    }
}