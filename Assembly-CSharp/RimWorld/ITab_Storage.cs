using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class ITab_Storage : ITab
	{
		private Vector2 scrollPosition;

		private static readonly Vector2 WinSize = new Vector2(300f, 480f);

		protected virtual IStoreSettingsParent SelStoreSettingsParent
		{
			get
			{
				Thing thing = base.SelObject as Thing;
				if (thing != null)
				{
					IStoreSettingsParent thingOrThingCompStoreSettingsParent = this.GetThingOrThingCompStoreSettingsParent(thing);
					if (thingOrThingCompStoreSettingsParent != null)
					{
						return thingOrThingCompStoreSettingsParent;
					}
					return null;
				}
				return base.SelObject as IStoreSettingsParent;
			}
		}

		public override bool IsVisible
		{
			get
			{
				return this.SelStoreSettingsParent.StorageTabVisible;
			}
		}

		protected virtual bool IsPrioritySettingVisible
		{
			get
			{
				return true;
			}
		}

		private float TopAreaHeight
		{
			get
			{
				return (float)((!this.IsPrioritySettingVisible) ? 20 : 35);
			}
		}

		public ITab_Storage()
		{
			base.size = ITab_Storage.WinSize;
			base.labelKey = "TabStorage";
			base.tutorTag = "Storage";
		}

		protected override void FillTab()
		{
			IStoreSettingsParent selStoreSettingsParent = this.SelStoreSettingsParent;
			StorageSettings settings = selStoreSettingsParent.GetStoreSettings();
			Vector2 winSize = ITab_Storage.WinSize;
			float x = winSize.x;
			Vector2 winSize2 = ITab_Storage.WinSize;
			Rect position = new Rect(0f, 0f, x, winSize2.y).ContractedBy(10f);
			GUI.BeginGroup(position);
			if (this.IsPrioritySettingVisible)
			{
				Text.Font = GameFont.Small;
				Rect rect = new Rect(0f, 0f, 160f, (float)(this.TopAreaHeight - 6.0));
				if (Widgets.ButtonText(rect, "Priority".Translate() + ": " + settings.Priority.Label(), true, false, true))
				{
					List<FloatMenuOption> list = new List<FloatMenuOption>();
					IEnumerator enumerator = Enum.GetValues(typeof(StoragePriority)).GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							StoragePriority storagePriority = (StoragePriority)enumerator.Current;
							if (storagePriority != 0)
							{
								StoragePriority localPr = storagePriority;
								list.Add(new FloatMenuOption(localPr.Label().CapitalizeFirst(), delegate
								{
									settings.Priority = localPr;
								}, MenuOptionPriority.Default, null, null, 0f, null, null));
							}
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = (enumerator as IDisposable)) != null)
						{
							disposable.Dispose();
						}
					}
					Find.WindowStack.Add(new FloatMenu(list));
				}
				UIHighlighter.HighlightOpportunity(rect, "StoragePriority");
			}
			ThingFilter parentFilter = null;
			if (selStoreSettingsParent.GetParentStoreSettings() != null)
			{
				parentFilter = selStoreSettingsParent.GetParentStoreSettings().filter;
			}
			Rect rect2 = new Rect(0f, this.TopAreaHeight, position.width, position.height - this.TopAreaHeight);
			ThingFilterUI.DoThingFilterConfigWindow(rect2, ref this.scrollPosition, settings.filter, parentFilter, 8, (IEnumerable<ThingDef>)null, (IEnumerable<SpecialThingFilterDef>)null, (List<ThingDef>)null);
			PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.StorageTab, KnowledgeAmount.FrameDisplayed);
			GUI.EndGroup();
		}

		protected IStoreSettingsParent GetThingOrThingCompStoreSettingsParent(Thing t)
		{
			IStoreSettingsParent storeSettingsParent = t as IStoreSettingsParent;
			if (storeSettingsParent != null)
			{
				return storeSettingsParent;
			}
			ThingWithComps thingWithComps = t as ThingWithComps;
			if (thingWithComps != null)
			{
				List<ThingComp> allComps = thingWithComps.AllComps;
				for (int i = 0; i < allComps.Count; i++)
				{
					storeSettingsParent = (allComps[i] as IStoreSettingsParent);
					if (storeSettingsParent != null)
					{
						return storeSettingsParent;
					}
				}
			}
			return null;
		}
	}
}
