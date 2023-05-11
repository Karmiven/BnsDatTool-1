using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Xylia.CustomException;

namespace Xylia.bns.Read
{
	/// <summary>
	/// 类型
	/// </summary>
	public enum DatType
	{
		local64 = 0,
		local = 1,

		xml = 2,
		xml64 = 4,

		config = 8,
		config64 = 16,
	}

	/// <summary>
	/// dat文件集合
	/// </summary>
	public class DataPathes
	{
		#region Constructor
		private readonly Dictionary<DatType, List<FileInfo>> DataPathMenu;

		public DataPathes(string Folder)
		{
			this.DataPathMenu = new();
			this.InitPara(Folder);
		}
		#endregion


		#region Functions
		private void InitPara(string Folder)
		{
			List<FileInfo> files = new();

			var DirInfo = new DirectoryInfo(Folder);
			files.AddRange(DirInfo.GetFiles("*.dat", SearchOption.AllDirectories));
			files.AddRange(DirInfo.GetFiles("*.bin", SearchOption.AllDirectories));


			foreach (var file in files)
			{
				DatType datType = DatType.xml;
				switch (Path.GetFileNameWithoutExtension(file.Name).ToLower())
				{
					case "xml":
					case "datafile":
						datType = DatType.xml;
						break;

					case "xml64":
					case "datafile64":
						datType = DatType.xml64;
						break;

					case "config":
						datType = DatType.config; break;

					case "config64":
						datType = DatType.config64; break;

					case "local":
					case "localfile":
						datType = DatType.local; break;

					case "local64":
					case "localfile64":
						datType = DatType.local64; break;

					default: continue;
				}


				//add
				if (!DataPathMenu.ContainsKey(datType))
					DataPathMenu.Add(datType, new());

				DataPathMenu[datType].Add(file);
			}
		}

		/// <summary>
		/// 获取文件
		/// </summary>
		/// <param name="Type"></param>
		/// <param name="SelectBin"></param>
		/// <returns></returns>
		/// <exception cref="ReadException"></exception>
		public List<FileInfo> GetFiles(DatType Type, bool? SelectBin)
		{
			var result = new List<FileInfo>();
			if (Type == DatType.xml || Type == DatType.xml64)
			{
				if (DataPathMenu.ContainsKey(DatType.xml64)) result.AddRange(DataPathMenu[DatType.xml64]);
				if (DataPathMenu.ContainsKey(DatType.xml)) result.AddRange(DataPathMenu[DatType.xml]);
			}
			else if (Type == DatType.config || Type == DatType.config64)
			{
				if (DataPathMenu.ContainsKey(DatType.config64)) result.AddRange(DataPathMenu[DatType.config64]);
				if (DataPathMenu.ContainsKey(DatType.config)) result.AddRange(DataPathMenu[DatType.config]);
			}
			else if (Type == DatType.local || Type == DatType.local64)
			{
				if (DataPathMenu.ContainsKey(DatType.local64)) result.AddRange(DataPathMenu[DatType.local64]);
				if (DataPathMenu.ContainsKey(DatType.local)) result.AddRange(DataPathMenu[DatType.local]);
			}

			//True 只筛选bin, False 只筛选dat
			if (SelectBin == true) return result.Where(r => r.Extension == ".bin").ToList();
			if (SelectBin == false) return result.Where(r => r.Extension == ".dat").ToList();

			return result;
		}
		#endregion
	}


	public static class Extension
	{
		public static bool Judge64Bit(this FileInfo FileInfo) => FileInfo.Name.Judge64Bit();

		public static bool Judge64Bit(this string FilePath)
		{
			if (string.IsNullOrWhiteSpace(FilePath))
				return false;

			return Path.GetFileNameWithoutExtension(FilePath).Contains("64");
		}


		public static bool Has32bit(this IEnumerable<FileInfo> files) => files.FirstOrDefault(f => !f.Judge64Bit()) != null;

		public static bool Has64bit(this IEnumerable<FileInfo> files) => files.FirstOrDefault(f => f.Judge64Bit()) != null;


		public static IEnumerable<FileInfo> GetFiles(this IEnumerable<FileInfo> files, bool? is64 = null)
		{
			if (is64 is null) return files;
			else if (is64.Value) return files.Where(f => Judge64Bit(f));
			else return files.Where(f => !Judge64Bit(f));
		}



		/// <summary>
		/// 获取对应区域
		/// </summary>
		/// <param name="Folder"></param>
		/// <returns></returns>
		public static string GetRegion(this string Folder)
		{
			try
			{
				foreach (var item in new DirectoryInfo(Folder).GetFiles("local*.dat", SearchOption.AllDirectories))
				{
					string SubPath = Path.GetDirectoryName(Path.GetDirectoryName(item.FullName));
					string MainPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(item.FullName)));

					return ReplaceRegion(SubPath.Replace(MainPath + @"\", ""));
				}
			}
			catch
			{

			}

			return "未知或未能确定";
		}

		/// <summary>
		/// 替换区域性特征文本
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string ReplaceRegion(string str)
		{
			List<KeyValuePair<string, string>> Region = new();
			Region.Add(new KeyValuePair<string, string>("CHINESES", "中国大陆"));
			Region.Add(new KeyValuePair<string, string>("CHINESET", "中国台湾"));
			Region.Add(new KeyValuePair<string, string>("Korean", "韩国"));

			Region.ForEach(r => str = Regex.Replace(str, r.Key, r.Value, RegexOptions.IgnoreCase));
			return str;
		}
	}
}