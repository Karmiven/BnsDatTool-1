
using Xylia.Attribute;

namespace Xylia.bns.Modules.DataFormat.Analyse.Condition
{
	public class Exist : ValidCondition
	{
		#region Constructor
		public Exist(bool Flag) => this.Flag = Flag;

		/// <summary>
		/// 是否存在
		/// </summary>
		public bool Flag = true;
		#endregion


		#region Functions
		protected override bool IsMeet(IHash Hash, bool ExistTarget) => (Flag && ExistTarget) || (!Flag && !ExistTarget);
		#endregion
	}
}
