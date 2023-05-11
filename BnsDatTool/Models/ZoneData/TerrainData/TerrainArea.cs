using System.IO;

namespace Xylia.bns.Modules.DataFormat.ZoneData.TerrainData
{
	public class TerrainCell
	{
		#region Functions
		public void Read(BinaryReader reader)
		{
			this.Type = (CellType)reader.ReadInt32();
			this.AreaIdx = reader.ReadInt32();
			this.Param2 = reader.ReadInt32();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write((int)this.Type);
			writer.Write(this.AreaIdx);
			writer.Write(this.Param2);
		}
		#endregion

		#region Fields
		public CellType Type;
		public int AreaIdx;
		public int Param2;
		#endregion
	}

	public enum CellType
	{
		None,
		Unk1, //1 单元格
		Unk2, //2
		Unk3, //3 删除后缺失入场点，导致无法进入地图
		Unk4,
	}

	/// <summary>
	/// 向量
	/// </summary>
	public struct Vector32
	{
		#region Constructor
		public Vector32(BinaryReader reader)
		{
			this.X = reader.ReadInt16();
			this.Y = reader.ReadInt16();
			this.Z = reader.ReadInt16();
		}

		public Vector32(string Value)
		{
			var group = Value.Split(',');

			this.X = short.Parse(group[0]);
			this.Y = short.Parse(group[1]);
			this.Z = short.Parse(group[2]);
		}
		#endregion

		#region Fields
		public short X;
		public short Y;
		public short Z;
		#endregion


		#region Functions
		public override string ToString() => $"{this.X},{this.Y},{this.Z}";

		/// <summary>
		/// 存储数据
		/// </summary>
		/// <param name="writer"></param>
		public void Write(BinaryWriter writer)
		{
			writer.Write(this.X);
			writer.Write(this.Y);
			writer.Write(this.Z);
		}
		#endregion
	}
}
