using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ACGLab.Model;

namespace ACGLab.FileParser
{
    public class FileParser
    {
        public static DrawingObject ParseFile(string filePath)
        {
            List<Vertex> vertices = new List<Vertex>();
            List<VertexNormal> verticesNormal = new List<VertexNormal>();
            List<Polygon> instance = new List<Polygon>();
            List<string> lines = new List<string>();

            string l;
            int skip = 1;

            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    lines.Add(reader.ReadLine());
                }
            }

            foreach (string line in lines)
            {
                skip = 1;
                if (line.Length > 2)
                {
                    
                    if (line[line.Length - 1] == ' ')
                    {
                        l = line.Substring(0, line.Length - 1);
                    }
                    else
                    {
                        l = line;
                    }
                    if(l[2] == ' ')
                    {
                        skip = 2;
                    }
                    string letter = line.ToLower().Substring(0, 2);
                    switch (letter)
                    {
                        case "v ":
                            var v = l.Split(' ')
                                .Skip(skip)
                                .Select(v => Double.Parse(v.Replace('.', ',')))
                                .ToArray();
                            switch (v.Length)
                            {
                                case 3:
                                    vertices.Add(new Vertex(v[0], v[1], v[2], 1));
                                    break;
                                case 4:
                                    vertices.Add(new Vertex(v[0], v[1], v[2], v[3]));
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "vt":
                            /*var vt = line.Split(' ')
                                .Skip(1)
                                .Select(vt => Double.Parse(vt.Replace('.', ',')))
                                .ToArray();
                            switch (vt.Length)
                            {
                                case 1:
                                    vertexTexture.Add(new List<double>() { vt[0], 0, 0 });
                                    break;
                                case 2:
                                    vertexTexture.Add(new List<double>() { vt[0], vt[1], 0 });
                                    break;
                                case 3:
                                    vertexTexture.Add(new List<double>() { vt[0], vt[1], vt[2] });
                                    break;
                                default:
                                    break;
                            }*/
                            break;
                        case "vn":
                            var vn = line.Split(' ')
                                .Skip(1)
                                .Select(vn => Double.Parse(vn.Replace('.', ',')))
                                .ToArray();
                            switch (vn.Length)
                            {
                                case 3:
                                    verticesNormal.Add(new VertexNormal(vn[0], vn[1], vn[2]));
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "f ":
                            var f = line.Split(' ')
                                    .Skip(1)
                                    .Select(c => c.Split('/'))
                                    .Select(c => c.Select(a => Int32.TryParse(a, out int res) ? res : 0).ToArray())
                                    .ToArray();
                            var vert = new List<Vertex>();
                            var vertN = new List<VertexNormal>();
                            for (int i = 0; i < f.Length; i++)
                            {
                                if (f[i].Length >= 2)
                                {
                                    vert.Add(vertices[f[i][0] - 1]);
                                    vertN.Add(verticesNormal[f[i][2] - 1]);
                                }
                                
                            }
                            instance.Add(new Polygon(vert, vertN));
                            break;
                        default:
                            break;
                    }
                }
            }
            return new DrawingObject(instance);
        }
    }
}
