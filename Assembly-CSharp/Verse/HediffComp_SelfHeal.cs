namespace Verse
{
	public class HediffComp_SelfHeal : HediffComp
	{
		public int ticksSinceHeal;

		public HediffCompProperties_SelfHeal Props
		{
			get
			{
				return (HediffCompProperties_SelfHeal)base.props;
			}
		}

		public override void CompExposeData()
		{
			Scribe_Values.Look<int>(ref this.ticksSinceHeal, "ticksSinceHeal", 0, false);
		}

		public override void CompPostTick(ref float severityAdjustment)
		{
			this.ticksSinceHeal++;
			if (this.ticksSinceHeal > this.Props.healIntervalTicksStanding)
			{
				severityAdjustment -= 1f;
				this.ticksSinceHeal = 0;
			}
		}
	}
}
