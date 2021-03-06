using RimWorld.Planet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class AlertsReadout
	{
		private List<Alert> activeAlerts = new List<Alert>(16);

		private int curAlertIndex;

		private float lastFinalY;

		private int mouseoverAlertIndex = -1;

		private readonly List<Alert> AllAlerts = new List<Alert>();

		private const int StartTickDelay = 600;

		public const float AlertListWidth = 164f;

		private static int AlertCycleLength = 20;

		private readonly List<AlertPriority> PriosInDrawOrder;

		public AlertsReadout()
		{
			this.AllAlerts.Clear();
			foreach (Type item2 in typeof(Alert).AllLeafSubclasses())
			{
				this.AllAlerts.Add((Alert)Activator.CreateInstance(item2));
			}
			if (this.PriosInDrawOrder == null)
			{
				this.PriosInDrawOrder = new List<AlertPriority>();
				IEnumerator enumerator2 = Enum.GetValues(typeof(AlertPriority)).GetEnumerator();
				try
				{
					while (enumerator2.MoveNext())
					{
						AlertPriority item = (AlertPriority)enumerator2.Current;
						this.PriosInDrawOrder.Add(item);
					}
				}
				finally
				{
					IDisposable disposable;
					if ((disposable = (enumerator2 as IDisposable)) != null)
					{
						disposable.Dispose();
					}
				}
				this.PriosInDrawOrder.Reverse();
			}
		}

		public void AlertsReadoutUpdate()
		{
			if (Mathf.Max(Find.TickManager.TicksGame, Find.TutorialState.endTick) >= 600)
			{
				if (Find.Storyteller.def.disableAlerts)
				{
					this.activeAlerts.Clear();
				}
				else
				{
					this.curAlertIndex++;
					if (this.curAlertIndex >= AlertsReadout.AlertCycleLength)
					{
						this.curAlertIndex = 0;
					}
					for (int i = this.curAlertIndex; i < this.AllAlerts.Count; i += AlertsReadout.AlertCycleLength)
					{
						Alert alert = this.AllAlerts[i];
						try
						{
							if (alert.Active)
							{
								if (!this.activeAlerts.Contains(alert))
								{
									this.activeAlerts.Add(alert);
									alert.Notify_Started();
								}
							}
							else
							{
								int num = 0;
								while (num < this.activeAlerts.Count)
								{
									if (this.activeAlerts[num] != alert)
									{
										num++;
										continue;
									}
									this.activeAlerts.RemoveAt(num);
									break;
								}
							}
						}
						catch (Exception ex)
						{
							Log.ErrorOnce("Exception processing alert " + alert.ToString() + ": " + ex.ToString(), 743575);
							if (this.activeAlerts.Contains(alert))
							{
								this.activeAlerts.Remove(alert);
							}
						}
					}
					for (int num2 = this.activeAlerts.Count - 1; num2 >= 0; num2--)
					{
						Alert alert2 = this.activeAlerts[num2];
						try
						{
							this.activeAlerts[num2].AlertActiveUpdate();
						}
						catch (Exception ex2)
						{
							Log.ErrorOnce("Exception updating alert " + alert2.ToString() + ": " + ex2.ToString(), 743575);
							this.activeAlerts.RemoveAt(num2);
						}
					}
					if (this.mouseoverAlertIndex >= 0 && this.mouseoverAlertIndex < this.activeAlerts.Count)
					{
						AlertReport report = this.activeAlerts[this.mouseoverAlertIndex].GetReport();
						GlobalTargetInfo culprit = report.culprit;
						if (culprit.IsValid && culprit.IsMapTarget && Find.VisibleMap != null && culprit.Map == Find.VisibleMap)
						{
							GenDraw.DrawArrowPointingAt(((TargetInfo)culprit).CenterVector3, false);
						}
					}
					this.mouseoverAlertIndex = -1;
				}
			}
		}

		public void AlertsReadoutOnGUI()
		{
			if (Event.current.type != EventType.Layout && this.activeAlerts.Count != 0)
			{
				Alert alert = null;
				AlertPriority alertPriority = AlertPriority.Critical;
				bool flag = false;
				float num = (float)(Find.LetterStack.LastTopY - (float)this.activeAlerts.Count * 28.0);
				Rect rect = new Rect((float)((float)UI.screenWidth - 154.0), num, 154f, this.lastFinalY - num);
				float num2 = GenUI.BackgroundDarkAlphaForText();
				if (num2 > 0.0010000000474974513)
				{
					GUI.color = new Color(1f, 1f, 1f, num2);
					Widgets.DrawShadowAround(rect);
					GUI.color = Color.white;
				}
				float num3 = num;
				if (num3 < 0.0)
				{
					num3 = 0f;
				}
				for (int i = 0; i < this.PriosInDrawOrder.Count; i++)
				{
					AlertPriority alertPriority2 = this.PriosInDrawOrder[i];
					for (int j = 0; j < this.activeAlerts.Count; j++)
					{
						Alert alert2 = this.activeAlerts[j];
						if (alert2.Priority == alertPriority2)
						{
							if (!flag)
							{
								alertPriority = alertPriority2;
								flag = true;
							}
							Rect rect2 = alert2.DrawAt(num3, alertPriority2 != alertPriority);
							if (Mouse.IsOver(rect2))
							{
								alert = alert2;
								this.mouseoverAlertIndex = j;
							}
							num3 += rect2.height;
						}
					}
				}
				this.lastFinalY = num3;
				UIHighlighter.HighlightOpportunity(rect, "Alerts");
				if (alert != null)
				{
					alert.DrawInfoPane();
					PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Alerts, KnowledgeAmount.FrameDisplayed);
				}
			}
		}
	}
}
