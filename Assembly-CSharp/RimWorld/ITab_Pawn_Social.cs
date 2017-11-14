using System;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ITab_Pawn_Social : ITab
	{
		public const float Width = 540f;

		public override bool IsVisible
		{
			get
			{
				return this.SelPawnForSocialInfo.RaceProps.IsFlesh;
			}
		}

		private Pawn SelPawnForSocialInfo
		{
			get
			{
				if (base.SelPawn != null)
				{
					return base.SelPawn;
				}
				Corpse corpse = base.SelThing as Corpse;
				if (corpse != null)
				{
					return corpse.InnerPawn;
				}
				throw new InvalidOperationException("Social tab on non-pawn non-corpse " + base.SelThing);
			}
		}

		public ITab_Pawn_Social()
		{
			base.size = new Vector2(540f, 510f);
			base.labelKey = "TabSocial";
			base.tutorTag = "Social";
		}

		protected override void FillTab()
		{
			SocialCardUtility.DrawSocialCard(new Rect(0f, 0f, base.size.x, base.size.y), this.SelPawnForSocialInfo);
		}
	}
}
