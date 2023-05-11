namespace Xylia.bns.Modules.DataFormat.ZoneData
{
	public class ZoneTest
	{
		public string Alias;

		public string ZoneType2;

		public override int GetHashCode()
		{
			return Alias.GetHashCode() + ZoneType2.GetHashCode();
		}
	}
}
