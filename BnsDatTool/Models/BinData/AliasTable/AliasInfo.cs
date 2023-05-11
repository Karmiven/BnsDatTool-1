using System;
using System.Collections.Generic;

namespace Xylia.bns.Modules.DataFormat.Bin.Entity.BDAT.AliasTable
{
	/// <summary>
	/// 别名信息（需要从InfoCase转换）
	/// </summary>
	public sealed class AliasInfo
	{ 
		#region Constructor
		public AliasInfo()
		{

		}

		public AliasInfo(uint MainID, uint Variation, string CompleteInfo)
		{
			this.MainID = MainID;
			this.Variation = Variation;
			this.CompleteText = CompleteInfo;
		}

		public AliasInfo(uint MainID, uint Variation, string ParentTable, string Info)
		{
			this.MainID = MainID;
			this.Variation = Variation;

			this.ParentTable = ParentTable;
			this.Alias = Info;
		}
		#endregion

		#region Fields
		public uint MainID;

		public uint Variation;

		/// <summary>
		/// 归属表
		/// </summary>
		public string ParentTable;

		/// <summary>
		/// 文本
		/// </summary>
		public string Alias;

		/// <summary>
		/// 完整文本
		/// </summary>
		public string CompleteText
		{
			get => ParentTable + ":" + Alias;
			set
			{
				if (!value.Contains(':')) throw new Exception("别名缓存区必须以列表+数据别名方式存储");
				else
				{
					var tmp = value.Split(':');

					ParentTable = tmp[0];
					Alias = tmp[1];
				}
			}
		}


		public override string ToString() => $"{CompleteText}";
		#endregion
	}
}