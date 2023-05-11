using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Xylia.Extension;

namespace Xylia.bns.Util
{
	public static class BNSFileHelper
	{
		#region SeriFile
		public static string SeriSourceFolder = @"F:\Build\server\2021_版本库2";

		private static string Seri_GetFileName(string TableName) => $"{TableName.RemoveSuffixString("Data")}Data*.xml";

		public static FileInfo[] GetFiles(string TableName, string MainFoloer = null) => Seri_DataList(TableName, MainFoloer ?? SeriSourceFolder);

		public static FileInfo[] Seri_DataList(string TableName, params string[] DirInfos) => Seri_DataList(TableName, DirInfos.Select(dir => new DirectoryInfo(dir)).ToArray());

		public static FileInfo[] Seri_DataList(string TableName, params DirectoryInfo[] DirInfos)
		{
			var result = DirInfos.SelectMany(dir =>
			{
				//只有物品类型是特殊的
				if (TableName.MyEquals("item"))
				{
					List<FileInfo> tmp = new();

					tmp.AddRange(dir.GetFiles(Seri_GetFileName("Item"), SearchOption.AllDirectories));
					tmp.AddRange(dir.GetFiles(Seri_GetFileName("Accessory"), SearchOption.AllDirectories));
					tmp.AddRange(dir.GetFiles(Seri_GetFileName("Gem"), SearchOption.AllDirectories));
					tmp.AddRange(dir.GetFiles(Seri_GetFileName("Grocery"), SearchOption.AllDirectories));
					tmp.AddRange(dir.GetFiles(Seri_GetFileName("Costume"), SearchOption.AllDirectories));
					tmp.AddRange(dir.GetFiles(Seri_GetFileName("Weapon"), SearchOption.AllDirectories));

					return tmp.ToArray();
				}

				return dir.GetFiles(Seri_GetFileName(TableName), SearchOption.AllDirectories);
			})
			   .Where(data => !data.DirectoryName.MyEndsWith("\\server"))     //排除掉生成的服务端数据
			   .ToArray();

			//调试用
			Trace.WriteLine(TableName + " => " + result.Aggregate(string.Empty, (sum, now) => sum + now.FullName.Replace(now.DirectoryName + "\\", null) + ", "));
			return result;
		}



		///// <summary>
		///// 从指定文件夹中获取序列化的原始数据
		///// </summary>
		///// <param name="TableName"></param>
		///// <param name="Folder"></param>
		///// <returns></returns>
		//public static List<SeriSourceData> Seri_GetSourceData(string TableName, string Folder = null)
		//{
		//	//获取数据文件
		//	var DataList = Seri_DataList(TableName, Folder);
		//	if (DataList is null || !DataList.Any())
		//	{
		//		Trace.WriteLine("没有找到数据源文件 " + TableName);
		//		return null;
		//	}

		//	return DataList.Select(f => new SeriSourceData(f.FullName)).ToList();
		//}

		//public static List<SeriSourceData> Seri_GetSourceData_Select(string TableName, string SeletedPath = null)
		//{
		//	List<string> SourceFiles = new();
		//	List<string> SourceFolder = new();

		//	//遍历拆分路径
		//	foreach (var Path in (SeletedPath + "|").Split('|').Where(path => !string.IsNullOrWhiteSpace(path)))
		//	{
		//		//加载数据
		//		if (!File.Exists(Path)) Console.WriteLine("反序列化数据路径不存在 " + Path);
		//		else
		//		{
		//			SourceFiles.Add(Path);

		//			var CurrentDir = new FileInfo(Path).Directory.FullName;
		//			if (!SourceFolder.Contains(CurrentDir)) SourceFolder.Add(CurrentDir);
		//		}
		//	}

		//	//如果只有一个目录，则执行自动提醒
		//	if (SourceFolder.Count == 1)
		//	{
		//		var AutoGet = Seri_DataList(TableName, SourceFolder.ToArray());
		//		if (AutoGet.Length != SourceFiles.Count)
		//		{
		//			Console.WriteLine("[debug] 可能缺失数据文件");
		//		}
		//	}

		//	//返回结果
		//	return SourceFiles.Select(f => new SeriSourceData(f)).ToList();
		//}
		#endregion


		#region	XmlData
		public static string XmlDataFolder = @"D:\剑灵 Private\BnS_CN\contents\Local\NCSoft\data\Export\xml64\files";
		#endregion

		#region AliasTable
		public static string SaveFolder = @"D:\剑灵 Private\BnS_CN\contents\Local\NCSoft\data\Export\xml64\headc";

		//public static string GetAliasTableOutPath(SeriListTableInfo ListInfo) => GetAliasTableOutPath(ListInfo.TableInfo.Type);

		public static string GetAliasTableOutPath(string TableName) => $@"{SaveFolder}\{TableName.RemoveSuffixString("Data").ToLower()}.xml";
		#endregion
	}
}