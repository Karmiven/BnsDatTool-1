using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;

using Xylia.bns.Util.Sort;
using Xylia.Extension;

namespace Xylia.bns.Modules.DataFormat.Analyse.Server
{
	public static partial class ServerHandle
	{
		public static string PublicOutFolder = @"F:\Build\server\2021_版本库2\server\";

		/// <summary>
		/// 创建服务端序列化文件
		/// </summary>
		/// <param name="Lists">配置文件</param>
		/// <param name="PatheLists">数据文件</param>
		/// <param name="InSameFolder"></param>
		/// <param name="PreserveWhitespace"></param>
		public static void CreateSerData(this List<TableInfo> Lists, List<string> PatheLists, bool InSameFolder, bool PreserveWhitespace)
		{
			int CurCount = 0;
			int Process = 0;

			// 统计错误信息
			var CrashInfo = new List<string>();

			int AutoID = 1;
			new Thread(act =>
			{
				foreach (var p in PatheLists)
				{
					#region 生成存储路径
					string FinalDirPath = Directory.Exists(PublicOutFolder) ? PublicOutFolder : Path.GetDirectoryName(p) + @"\server\";

					//如果指示输出到同一文件夹
					if (InSameFolder) FinalDirPath = Path.GetDirectoryName(p) + @"\server\";

					var xmlDoc = new XmlDocument();
					xmlDoc.Load(p);

					xmlDoc.DocumentElement.SetAttribute("release-side", "server");
					string Module = xmlDoc.DocumentElement.Attributes["release-module"]?.Value;
					string Type = xmlDoc.DocumentElement.Attributes["type"]?.Value;
					#endregion

					#region 找到符合的配置文件
					TableInfo CurTable = null;
					ushort MajorVersion = 0;
					ushort MinorVersion = 0;
					foreach (var List in Lists)
					{
						if (!List.Module.MyEquals(Module) || !List.TypeName.MyEquals(Type))
						{
							Console.WriteLine($"== CRASH == 配置文件（{Type}）与数据类型（{List.Type}）不符，进程退出");
							Console.WriteLine($"== CRASH == 配置文件（{Module}）与数据类型（{List.Module}）不符，进程退出");
							continue;
						}

						CurTable = List;
						xmlDoc.DocumentElement.Attributes["version"]?.Value.GetVersion(out MajorVersion, out MinorVersion);

						break;
					}

					ArgumentNullException.ThrowIfNull(CurTable);
					#endregion



					try
					{
						//#region 获取信息
						////分类输出统计用
						//var Sortes = new Dictionary<string, List<XmlNode>>();
						//var XmlNodes = xmlDoc.DocumentElement.ChildNodes.OfType<XmlNode>().ToList();

						//var RecordTable = CurTable.GetRecordTable(true, false);  //创建 记录器 字典，便于处理时获取
						//var IDRecord = CurTable.Records.Find(r => r.ValueType == Value.VType.TId && r.ApplyServer == ApplyMode.Auto);   //获取id记录器
						//var TypeRecord = CurTable.Records.Find(r => r.ValueType == Value.VType.TType);  //获取type记录器
						//#endregion

						//#region 生成默认值数组
						//var DefaultRecords = new Dictionary<long, List<RecordDef>>();
						//var DefaultSpams = new Dictionary<long, List<RecordDef>>();

						//DefaultRecords.Add(-1, new List<RecordDef>());
						//foreach (var r in CurTable.Records.Where(r => r.Server && !r.Alias.IsNull() && ((r.DefaultInfo?.IsValid ?? false) || (r.ValidCond != null && !r.ValidCond.Invalid))))
						//{
						//	var Types = new List<int>();

						//	if (!r.Filter.Any()) Types.Add(-1);
						//	else r.Filter.ForEach(f => Types.Add(f));

						//	//填充类型属性
						//	Types.ForEach(t =>
						//{
						//	if (!DefaultRecords.ContainsKey(t)) DefaultRecords.Add(t, new List<RecordDef>());

						//	DefaultRecords[t].Add(r);
						//});
						//}

						////合并通用字典 和 专用字典
						//foreach (var r in DefaultRecords.Where(r => r.Key != -1)) DefaultRecords[-1].ToList().ForEach(t => r.Value.Add(t));
						//#endregion

						//#region Initialize
						//string SortPath = "test";

						//var outputs = new List<ObjectOutput>();
						//foreach (var x in XmlNodes)
						//{
						//	if (x.NodeType == XmlNodeType.Element)
						//	{
						//		var obj = new ObjectOutput();
						//		foreach (XmlAttribute c in x.Attributes)
						//			obj.Cells.Add(new OutputCell(c.Name, c.Value));

						//		outputs.Add(obj);
						//	}
						//}

						//outputs.GetOutputs(RecordTable, TypeRecord, IDRecord, ref AutoID);
						//#endregion

						//#region 存储新文件
						////生成输出文件名
						//string FinalPath = FinalDirPath + Path.GetFileNameWithoutExtension(p) + Path.GetExtension(p);
						//var FileEntity = CurTable.CreateXml(MajorVersion, MinorVersion);
						//foreach (var o in outputs)
						//{
						//	var Record = FileEntity.CreateElement("record");
						//	foreach (var cell in o.Cells)
						//	{
						//		//if (cell.CDATA) Record.AppendChild(FileEntity.CreateCData(cell.OutputVal));
						//		//else 

						//		Record.SetAttribute(cell.Alias.Replace(" ", "-"), cell.OutputVal);
						//	}

						//	lock (FileEntity) FileEntity.AppendChild(Record);
						//}

						//FileEntity.Save(FinalPath, true);
						//Console.WriteLine("新文件已存储到 >> " + FinalPath);
						//#endregion
					}
					catch (Exception ee)
					{
						Tip.Message(ee.ToString());
					}

					if (++CurCount == PatheLists.Count)
					{
						Console.WriteLine("完成");
						GC.Collect();
					}
				}

			}).Start();
		}

		/// <summary>
		/// 排序Functions
		/// </summary>
		/// <param name="xmlNode"></param>
		/// <param name="recurse"></param>
		public static void XmlSort(this XmlNode xmlNode, bool recurse = true)
		{
			if (xmlNode.NodeType != XmlNodeType.Element) return;
			if (xmlNode.Attributes.Count > 0)
			{
				XmlElement Element = (XmlElement)xmlNode;

				var attrs = new List<XmlAttribute>();
				foreach (XmlAttribute attr in xmlNode.Attributes) attrs.Add(attr);

				//进行自定义排序
				attrs.Sort(new XmlAttributeSort() { IsConfig = false });

				//原节点移除所有
				Element.Attributes.RemoveAll();

				//设置数值
				attrs.ForEach(a => Element.SetAttribute(a.Name, a.Value));
			}

			//递归
			if (recurse && xmlNode.HasChildNodes)
				xmlNode.ChildNodes.OfType<XmlElement>().ForEach(node => XmlSort(node, true));
		}

		public static void GetVersion(this string Info, out ushort MajorVersion, out ushort MinorVersion)
		{
			MajorVersion = MinorVersion = 0;
			if (Info is null) return;

			var VersionText = Info.Split('.');
			MajorVersion = (ushort)VersionText[0].ToShort();
			MinorVersion = (ushort)VersionText[1].ToShort();
		}
	}
}