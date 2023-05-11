namespace Xylia.bns.Modules.DataFormat.Analyse.Condition
{
	public class ValidCondition	: Condition
	{
		public override bool Invalid => string.IsNullOrWhiteSpace(this.TargetAlias);
	}
}
