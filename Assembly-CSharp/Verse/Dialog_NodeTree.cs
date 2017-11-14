using RimWorld;
using System;
using UnityEngine;

namespace Verse
{
	public class Dialog_NodeTree : Window
	{
		private Vector2 scrollPosition;

		protected string title;

		protected DiaNode curNode;

		public Action closeAction;

		private float makeInteractiveAtTime;

		public Color screenFillColor = Color.clear;

		private const float InteractivityDelay = 0.5f;

		private const float TitleHeight = 36f;

		private float optTotalHeight;

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(620f, 480f);
			}
		}

		private bool InteractiveNow
		{
			get
			{
				return Time.realtimeSinceStartup >= this.makeInteractiveAtTime;
			}
		}

		public Dialog_NodeTree(DiaNode nodeRoot, bool delayInteractivity = false, bool radioMode = false, string title = null)
		{
			this.title = title;
			this.GotoNode(nodeRoot);
			base.forcePause = true;
			base.absorbInputAroundWindow = true;
			base.closeOnEscapeKey = false;
			if (delayInteractivity)
			{
				this.makeInteractiveAtTime = (float)(Time.realtimeSinceStartup + 0.5);
			}
			base.soundAppear = SoundDefOf.CommsWindow_Open;
			base.soundClose = SoundDefOf.CommsWindow_Close;
			if (radioMode)
			{
				base.soundAmbient = SoundDefOf.RadioComms_Ambience;
			}
		}

		public override void PreClose()
		{
			base.PreClose();
			this.curNode.PreClose();
		}

		public override void PostClose()
		{
			base.PostClose();
			if (this.closeAction != null)
			{
				this.closeAction();
			}
		}

		public override void WindowOnGUI()
		{
			if (this.screenFillColor != Color.clear)
			{
				GUI.color = this.screenFillColor;
				GUI.DrawTexture(new Rect(0f, 0f, (float)UI.screenWidth, (float)UI.screenHeight), BaseContent.WhiteTex);
				GUI.color = Color.white;
			}
			base.WindowOnGUI();
		}

		public override void DoWindowContents(Rect inRect)
		{
			Rect rect = inRect.AtZero();
			if (this.title != null)
			{
				Rect rect2 = rect;
				rect2.height = 36f;
				rect.yMin += 53f;
				Widgets.DrawTitleBG(rect2);
				rect2.xMin += 9f;
				rect2.yMin += 5f;
				Widgets.Label(rect2, this.title);
			}
			this.DrawNode(rect);
		}

		protected void DrawNode(Rect rect)
		{
			GUI.BeginGroup(rect);
			Text.Font = GameFont.Small;
			Rect outRect = new Rect(0f, 0f, rect.width, rect.height - this.optTotalHeight);
			float width = (float)(rect.width - 16.0);
			Rect rect2 = new Rect(0f, 0f, width, Text.CalcHeight(this.curNode.text, width));
			Widgets.BeginScrollView(outRect, ref this.scrollPosition, rect2, true);
			Widgets.Label(rect2, this.curNode.text);
			Widgets.EndScrollView();
			float num = rect.height - this.optTotalHeight;
			float num2 = 0f;
			for (int i = 0; i < this.curNode.options.Count; i++)
			{
				Rect rect3 = new Rect(15f, num, (float)(rect.width - 30.0), 999f);
				float num3 = this.curNode.options[i].OptOnGUI(rect3, this.InteractiveNow);
				num = (float)(num + (num3 + 7.0));
				num2 = (float)(num2 + (num3 + 7.0));
			}
			if (Event.current.type == EventType.Layout)
			{
				this.optTotalHeight = num2;
			}
			GUI.EndGroup();
		}

		public void GotoNode(DiaNode node)
		{
			foreach (DiaOption option in node.options)
			{
				option.dialog = this;
			}
			this.curNode = node;
		}
	}
}
