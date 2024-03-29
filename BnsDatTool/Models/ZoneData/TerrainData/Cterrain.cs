﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using Xylia.Extension;
using Xylia.Xml;

namespace Xylia.bns.Modules.DataFormat.ZoneData.TerrainData
{
	//GeoZone::InitializeByRecordMap zone(1290) boundary (sector) [-16,153] - [28,197] but terrain(1290) is too small [-96,101] - [2,195]
	//[geo-terran], FindCell failed; invalid z, zlow
	//[geo-cube], GeoCube initialize failed, nearby



	/// <summary>
	/// 地形数据结构
	/// </summary>
	public class CterrainFile
    {
        #region Fields
        /// <summary>
        /// 文件版本号 (2020年为 v23)
        /// </summary>
        public short Version = 23;

        /// <summary>
        /// 文件总长度
        /// </summary>
        public long FileSize;

        /// <summary>
        /// 地形编号
        /// </summary>
        public int TerrainID;

        /// <summary>
        /// 坐标系 起始位置
        /// </summary>
        public Vector32 Vector1;

        /// <summary>
        /// 坐标系 结束位置
        /// </summary>
        public Vector32 Vector2;

        public short Xmin;
        public short Xmax;
        public short Ymin;
        public short Ymax;
        public short XRange;
        public short YRange;



        public TerrainCell[] AreaList;

        /// <summary>
        /// 高度区域起始偏移
        /// </summary>
        public long Height1_Offset = 0;

        public List<short> Heights1 = new();


        /// <summary>
        /// HeightOffset2 的对象数
        /// </summary>
        public long Height2_Count = 0;

        /// <summary>
        /// HeightOffset2偏移
        /// </summary>
        public long Height2_Offset = 0;  

        public List<HeightParam> Heights2 = new();



        public long Unk4 = 0;    //删除后可以正常运行

        /// <summary>
        /// 组区域起始偏移
        /// </summary>
        public long GroupOffset = 0;

        /// <summary>
        /// 组区域数量
        /// </summary>
        public long GroupCount = 0;

        /// <summary>
        /// 删除后可以正常运行
        /// </summary>
        public short[] GroupMeta;


        public long UnkOffset = 0;

        public List<short> UnkData;
        #endregion



        #region IOFunctions组
        public void Read(string FilePath) => this.Read(new BinaryReader(new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)));

        public void Write(string SavePath) => this.Write(new BinaryWriter(new FileStream(SavePath, FileMode.Create)));

        public void Read(BinaryReader br)
        {
            #region Initialize
            this.Version = br.ReadInt16();
            this.FileSize = br.ReadInt64();
            this.TerrainID = br.ReadInt32();

            //  Pos
            this.Vector1 = new Vector32(br);
            this.Vector2 = new Vector32(br);

            //  Cell
            //读取边界，这里的数据可能是区块
            //比例关系  1区块 = 64坐标系单位  (1 cell = 64 pos)
            this.Xmin = br.ReadInt16();   //terrain-start-x
            this.Xmax = br.ReadInt16();
            this.Ymin = br.ReadInt16();   //terrain-start-y
            this.Ymax = br.ReadInt16();
            this.XRange = br.ReadInt16();  //count-x
            this.YRange = br.ReadInt16();  //count-y

            var Color1 = br.ReadInt32();
            var Color2 = br.ReadInt32();
            var Color3 = br.ReadInt32();
            if (Color1 != 0) Console.WriteLine("#cred# Color1不为0: " + Color1);
            if (Color2 != 112) Console.WriteLine("#cred# Color2不为112: " + Color2);
            if (Color3 != 0) Console.WriteLine("#cred# Color3不为0: " + Color3);


            Console.WriteLine($"#cblue# 区域范围 {this.Vector1} ~ {this.Vector2}");
            Console.WriteLine($"Sector  [{ this.Xmin },{ this.Ymin }] ~ [{ this.Xmax },{ this.Ymax }]  ({this.XRange},{this.YRange})");

            long MaxIndex = br.ReadInt64();       //最大区块索引
            this.Height1_Offset = br.ReadInt64(); //区域位置信息偏移1
            this.Height2_Count = br.ReadInt64();  //区域位置2 对象数量
            this.Height2_Offset = br.ReadInt64(); //区域位置2 信息偏移
            this.Unk4 = br.ReadInt64();           //地形 7430 不为0
            this.GroupOffset = br.ReadInt64();    //组信息位置偏移
            this.GroupCount = br.ReadInt64();     //
            this.UnkOffset = br.ReadInt64();      //未知区域偏移
            #endregion

            #region 处理单元格区域
            this.AreaList = new TerrainCell[this.XRange * this.YRange];
            for (int i = 0; i < this.AreaList.Length; i++)
            {
                var CurArea = this.AreaList[i] = new TerrainCell();
                CurArea.Read(br);
            }

            //对于类型3，Param2 指类型分区的数量    而其他类型则常为0，未知作用
            Console.WriteLine($"类型1 { AreaList.Where(a => a.Type == CellType.Unk1).Count() }    类型2 { AreaList.Where(a => a.Type == CellType.Unk2).Count() }");
            Console.WriteLine($"类型3 { AreaList.Where(a => a.Type == CellType.Unk3).Count() }    类型4 { AreaList.Where(a => a.Type == CellType.Unk4).Count() }");
            #endregion

            #region 高度区域偏移
            this.Heights1 = new List<short>();
            this.Heights2 = new List<HeightParam>();

            br.BaseStream.Position = this.Height1_Offset + 2;
            while (br.BaseStream.Position < this.Height2_Offset + 2) this.Heights1.Add(br.ReadInt16());

            while (br.BaseStream.Position < this.GroupOffset + 2) this.Heights2.Add(new HeightParam(br.ReadInt16(), br.ReadInt16()));


            Console.WriteLine($"unk4: {Unk4}  Heights: {this.Heights1.Count},{this.Heights2.Count}  GroupOffset:{GroupOffset} UnkOffset: {this.UnkOffset}");
            #endregion


            #region 未知集合
            br.BaseStream.Position = this.GroupOffset + 2;

            this.GroupMeta = new short[this.GroupCount];
            for (int i = 0; i < this.GroupMeta.Length; i++) this.GroupMeta[i] = br.ReadInt16();
            #endregion

            #region 未知集合2
            if (this.GroupOffset != this.UnkOffset)
            {
                //throw new Exception("模式未支持");

                System.Diagnostics.Trace.WriteLine($"{this.GroupOffset} ({this.GroupCount})   " + this.UnkOffset);
                System.Diagnostics.Trace.WriteLine(br.BaseStream.Position + "  " + br.BaseStream.Length);
            }
            #endregion

            br.Close();
            br.Dispose();
        }

        public void Write(BinaryWriter bw)
        {
            #region Initialize
            bw.Write(this.Version);
            bw.Write(this.FileSize);
            bw.Write(this.TerrainID);
            this.Vector1.Write(bw);
            this.Vector2.Write(bw);

            //写入边界
            bw.Write(this.Xmin);
            bw.Write(this.Xmax);
            bw.Write(this.Ymin);
            bw.Write(this.Ymax);
            bw.Write(this.XRange);
            bw.Write(this.YRange);

            bw.Write(0);
            bw.Write(112);
            bw.Write(0);

            //MaxIndex
            bw.Write((long)(this.AreaList.Max(a => (a.Type == CellType.Unk1 || a.Type == CellType.Unk2) ? a.AreaIdx : 0) + 1));

            //偏移数据到最后重写
            bw.Write(0L);   //Height1_Offset
            bw.Write((long)this.Heights2.Count);
            bw.Write(0L);   //Height2_Offset
            bw.Write(this.Unk4);
            bw.Write(0L);
            bw.Write((long)(this.GroupMeta?.Length ?? 0));
            bw.Write(0L);
            #endregion

            #region 处理单元格区域
            foreach (var CurArea in this.AreaList)
            {
                CurArea.Write(bw);
            }
            #endregion

            // 高度集合
            var heigthOffset1 = bw.BaseStream.Position;
            this.Heights1.ForEach(o => bw.Write(o));

            var heigthOffset2 = bw.BaseStream.Position;
            this.Heights2.ForEach(o =>
            {
                bw.Write(o.Min);
                bw.Write(o.Max);
            });

            // 未知集合
            var groupOffset = bw.BaseStream.Position;
            foreach (var c in this.GroupMeta) bw.Write(c);
     

            #region 最后处理
            //重写长度
            bw.BaseStream.Position = 2;
            bw.Write(bw.BaseStream.Length - 2);

            //heigthOffset1
            bw.BaseStream.Position = 0x3A;
            bw.Write(heigthOffset1 - 2);

            //heigthOffset2
            bw.BaseStream.Position = 0x4A;
            bw.Write(heigthOffset2 - 2);

            //groupOffset
            bw.BaseStream.Position = 0x5A;
            bw.Write(groupOffset - 2);

            //groupOffset2
            bw.BaseStream.Position = 0x6A;
            bw.Write(groupOffset - 2);

            bw.Flush();
            bw.Close();
            bw.Dispose();
            #endregion
        }

        /// <summary>
        /// 存储数据
        /// </summary>
        /// <param name="FilePath"></param>
        public void Save(string FilePath)
        {
            BinaryWriter bw = new(new MemoryStream());
            this.Write(bw);

            bw.BaseStream.Position = 2;
            bw.Write(this.FileSize = bw.BaseStream.Length - 2);

            Console.WriteLine("数据合并完成，开始执行最后封包。");

            BinaryWriter fw = new(new FileStream(FilePath, FileMode.Create));
            fw.Write(bw.BaseStream.ToBytes());
            fw.Close();

            Console.WriteLine("执行全部结束");
        }
        #endregion




        #region Functions
        /// <summary>
        /// 输出测试
        /// </summary>
        /// <param name="Path"></param>
        public void OutputTest(string Path)
        {
            XmlInfo xi = new();

            #region Terrain Main
            var Xe = xi.CreateElement("terrain");
            Xe.SetAttribute("id", this.TerrainID.ToString());
            Xe.SetAttribute("Vector1", Vector1.ToString());
            Xe.SetAttribute("Vector2", Vector2.ToString());
            Xe.SetAttribute("Xmin", Xmin.ToString());
            Xe.SetAttribute("Xmax", Xmax.ToString());
            Xe.SetAttribute("Ymin", Ymin.ToString());
            Xe.SetAttribute("Ymax", Ymax.ToString());
            Xe.SetAttribute("XRange", XRange.ToString());
            Xe.SetAttribute("YRange", YRange.ToString());
            Xe.SetAttribute("Unk4", Unk4.ToString());
            xi.AppendChild(Xe);

            #endregion

            #region AreaList
            var AreaCollection = xi.CreateElement("Area");
            Xe.AppendChild(AreaCollection);
            for (int i = 0; i < AreaList.Length; i++)
            {
                var TagetX = i / YRange + 1;
                var TagetY = i % YRange + 1;

                var Area = AreaList[i];

                var AreaNode = xi.CreateElement("area");
                AreaNode.SetAttribute("check", $"{TagetX},{TagetY}");

                AreaNode.SetAttribute("type", ((int)Area.Type).ToString());
                AreaNode.SetAttribute("index", Area.AreaIdx.ToString());
                AreaNode.SetAttribute("param2", Area.Param2.ToString());

                AreaCollection.AppendChild(AreaNode);
            }
            #endregion
                   

            #region Z1 Value
            var ZCollection = xi.CreateElement("Z1");
            ZCollection.SetAttribute("count", Heights1.Count.ToString());
            Xe.AppendChild(ZCollection);

            for (int i = 0; i < Heights1.Count; i++)
            {
                var height = Heights1[i];
                var UnkNode1 = xi.CreateElement("case");

                UnkNode1.SetAttribute("index", (i + 1).ToString());
                UnkNode1.SetAttribute("value", height.ToString());

                ZCollection.AppendChild(UnkNode1);
            }
            #endregion

            #region Z2 Value
            var ZCollection2 = xi.CreateElement("Z2");
            ZCollection2.SetAttribute("count", Heights2.Count.ToString());
            Xe.AppendChild(ZCollection2);

            for (int i = 0; i < Heights2.Count; i++)
            {
                var height = Heights2[i];
                var UnkNode1 = xi.CreateElement("case");

                UnkNode1.SetAttribute("index", (i + 1).ToString());
                UnkNode1.SetAttribute("min", height.Min.ToString());
                UnkNode1.SetAttribute("max", height.Max.ToString());

                ZCollection2.AppendChild(UnkNode1);
            }
            #endregion


            #region Group
            var GroupCollection = xi.CreateElement("Group");
            GroupCollection.SetAttribute("count", this.GroupMeta.Length.ToString());
            Xe.AppendChild(GroupCollection);
            for (int i = 0; i < this.GroupMeta.Length; i++)
            {
                var CaseNode = xi.CreateElement("case");
                CaseNode.SetAttribute("index",i.ToString());
                CaseNode.SetAttribute("value", this.GroupMeta[i].ToString());
                GroupCollection.AppendChild(CaseNode);
            }
            #endregion

            xi.Save(Path, true);
        }

        /// <summary>
        /// 输入测试
        /// </summary>
        /// <param name="Path"></param>
        public void InputTest(string Path)
        {
            #region Initialize
            this.Heights1 = new List<short>();
            this.Heights2 = new List<HeightParam>();

            var MetaList = new List<short>();

            XmlDocument XmlDoc = new();
            XmlDoc.Load(Path);

            var Xe = XmlDoc.SelectSingleNode("table/terrain");
			#endregion


			#region Terrain Main
			this.TerrainID = int.Parse(Xe.Attributes["id"].Value);
			this.Vector1 = new Vector32(Xe.Attributes["Vector1"].Value);
			this.Vector2 = new Vector32(Xe.Attributes["Vector2"].Value);
			this.Xmin = short.Parse(Xe.Attributes["Xmin"].Value);
            this.Ymin = short.Parse(Xe.Attributes["Ymin"].Value);
            this.Xmax = Xe.Attributes["Xmax"] is null ? (short)0 : short.Parse(Xe.Attributes["Xmax"].Value);
			this.Ymax = Xe.Attributes["Ymax"] is null ? (short)0 : short.Parse(Xe.Attributes["Ymax"].Value);
			this.XRange = short.Parse(Xe.Attributes["XRange"].Value);
			this.YRange = short.Parse(Xe.Attributes["YRange"].Value);
			this.Unk4 = int.Parse(Xe.Attributes["Unk4"].Value);


            //校验信息
            short CheckX = (short)(this.Xmin + this.XRange - 1);
            short CheckY = (short)(this.Ymin + this.YRange - 1);

            if (this.Xmax == 0) this.Xmax = CheckX;
            else if (this.Xmax != CheckX) throw new ArgumentException($"X不符合 ({this.Xmax} != {CheckX})");

            if (this.Ymax == 0) this.Ymax = CheckY;
            else if (this.Ymax != CheckY) throw new ArgumentException($"Y不符合 ({this.Ymax} != {CheckY})");
            #endregion

            #region AreaList
            List<TerrainCell> TerrainArea = new();
			foreach (var Area in Xe.SelectNodes("./Area/node()").OfType<XmlElement>())
			{
				if (Area.Name == "break") break;
				var CurArea = new TerrainCell()
				{
					Type = (CellType)int.Parse(Area.Attributes["type"].Value),
					AreaIdx = int.Parse(Area.Attributes["index"].Value),
					Param2 = int.Parse(Area.Attributes["param2"].Value),
				};

				TerrainArea.Add(CurArea);

				//if (TerrainArea.Count == XRange * YRange)
				//	break;
			}

			//测试
			bool AutoCreate = true;
			int CurrentIndex = 0;
			foreach (var Area in TerrainArea)
			{
				if (Area.Type == CellType.Unk1)
				{
					if (AutoCreate) Area.AreaIdx = CurrentIndex;
					else if (CurrentIndex != Area.AreaIdx)
						System.Diagnostics.Debug.WriteLine($"Area计算错误   {CurrentIndex} ≠ {Area.AreaIdx}");

					CurrentIndex += 1;
				}
				else if (Area.Type == CellType.Unk2)
				{
					if (AutoCreate) Area.AreaIdx = CurrentIndex;
					else if (CurrentIndex != Area.AreaIdx)
						System.Diagnostics.Debug.WriteLine($"Area计算错误   {CurrentIndex} ≠ {Area.AreaIdx}");

					Area.AreaIdx = CurrentIndex;

					CurrentIndex += 64;
				}
				else if (Area.Type == CellType.Unk3) continue;
				else if (Area.Type == CellType.Unk4) continue;
			}

			this.AreaList = TerrainArea.ToArray();
			#endregion


			#region Z1 Value
			foreach (var ZCase in Xe.SelectNodes("./Z1/node()").OfType<XmlElement>())
            {
                if (ZCase.Name == "break") break;

                this.Heights1.Add(short.Parse(ZCase.Attributes["value"].Value));
            }
            #endregion

            #region Z2 Value
            foreach (var ZCase in Xe.SelectNodes("./Z2/node()").OfType<XmlElement>())
            {
                if (ZCase.Name == "break") break;

                this.Heights2.Add(
                    new HeightParam(
                        short.Parse(ZCase.Attributes["min"].Value),
                        short.Parse(ZCase.Attributes["max"].Value)
                        ));
            }
            #endregion


            #region Group
            foreach (XmlElement Group in Xe.SelectNodes("./Group/node()").OfType<XmlElement>())
            {
                MetaList.Add(short.Parse(Group.Attributes["value"].Value));
            }

            this.GroupMeta = MetaList.ToArray();
            #endregion
        }
        #endregion
    }
}
