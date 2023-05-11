namespace Xylia.bns.Modules.DataFormat.ZoneData.TerrainData
{
	/// <summary>
	/// 高度参数
	/// </summary>
	public class HeightParam
	{
		public short Min;

		public short Max;


		public HeightParam(short Min, short Max)
		{
			this.Min = Min;
			this.Max = Max;
		}
	}
}
