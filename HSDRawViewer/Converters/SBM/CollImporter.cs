﻿using HSDRaw;
using HSDRaw.Melee.Gr;
using HSDRawViewer.GUI.Plugins.Melee;
using System.Collections.Generic;
using System.IO;

namespace HSDRawViewer.Converters.SBM
{
    public class CollImporter
    {
        public static void ImportColl(string filePath, SBM_Coll_Data coll)
        {
            List<CollLineGroup> groups = new List<CollLineGroup>();
            List<CollLine> lines = new List<CollLine>();

            using (FileStream s = new FileStream(filePath, FileMode.Open))
            using (BinaryReaderExt r = new BinaryReaderExt(s))
            {
                r.BigEndian = true;

                int veccount = r.ReadInt16();
                int linecount = r.ReadInt16();
                int groupcount = r.ReadInt16();
                int count4 = r.ReadInt16();

                uint vecoff = r.ReadUInt32();
                uint lineoff = r.ReadUInt32();
                uint groupoff = r.ReadUInt32();

                List<CollVertex> points = new List<CollVertex>();
                r.Seek(vecoff);
                for (int i = 0; i < veccount; i++)
                    points.Add(new CollVertex(r.ReadSingle(), r.ReadSingle()));

                r.Seek(lineoff);
                for (int i = 0; i < linecount; i++)
                {
                    lines.Add(new CollLine()
                    {
                        v1 = points[r.ReadInt16()],
                        v2 = points[r.ReadInt16()],
                    });
                    r.Skip(0x0C);
                }

                for (uint i = 0; i < groupcount; i++)
                {
                    r.Seek(groupoff + i * 0x6C);
                    var start = r.ReadInt16();
                    var count = r.ReadInt16();

                    var group = new CollLineGroup();
                    for(var j = start; j < start + count; j++)
                        lines[j].Group = group;
                    groups.Add(group);
                }

                foreach (var l in lines)
                    l.GuessCollisionFlag();
            }

            CollDataBuilder.GenerateCollData(lines, groups, coll);
        }
    }
}
