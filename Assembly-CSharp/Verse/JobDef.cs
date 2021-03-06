using RimWorld;
using System;
using System.Collections.Generic;

namespace Verse
{
	public class JobDef : Def
	{
		public Type driverClass;

		[MustTranslate]
		public string reportString = "Doing something.";

		public bool playerInterruptible = true;

		public CheckJobOverrideOnDamageMode checkOverrideOnDamage = CheckJobOverrideOnDamageMode.Always;

		public bool alwaysShowWeapon;

		public bool neverShowWeapon;

		public bool suspendable = true;

		public bool casualInterruptible = true;

		public bool collideWithPawns;

		public bool isIdle;

		public TaleDef taleOnCompletion;

		public bool makeTargetPrisoner;

		public int joyDuration = 4000;

		public int joyMaxParticipants = 1;

		public float joyGainRate = 1f;

		public SkillDef joySkill;

		public float joyXpPerTick;

		public JoyKindDef joyKind;

		public Rot4 faceDir = Rot4.Invalid;

		public override IEnumerable<string> ConfigErrors()
		{
			using (IEnumerator<string> enumerator = base.ConfigErrors().GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					string e = enumerator.Current;
					yield return e;
					/*Error: Unable to find new state assignment for yield return*/;
				}
			}
			if (this.joySkill == null)
				yield break;
			if (this.joyXpPerTick != 0.0)
				yield break;
			yield return "funSkill is not null but funXpPerTick is zero";
			/*Error: Unable to find new state assignment for yield return*/;
			IL_0101:
			/*Error near IL_0102: Unexpected return in MoveNext()*/;
		}
	}
}
