using Verse;

namespace RimWorld
{
	public class CompArt : ThingComp
	{
		private string authorNameInt;

		private string titleInt;

		private TaleReference taleRef;

		public string AuthorName
		{
			get
			{
				if (this.authorNameInt.NullOrEmpty())
				{
					return "UnknownLower".Translate();
				}
				return this.authorNameInt;
			}
		}

		public string Title
		{
			get
			{
				if (this.titleInt.NullOrEmpty())
				{
					Log.Error("CompArt got title but it wasn't configured.");
					this.titleInt = "Error";
				}
				return this.titleInt;
			}
		}

		public TaleReference TaleRef
		{
			get
			{
				return this.taleRef;
			}
		}

		public bool CanShowArt
		{
			get
			{
				if (this.Props.mustBeFullGrave)
				{
					Building_Grave building_Grave = base.parent as Building_Grave;
					if (building_Grave != null && building_Grave.HasCorpse)
					{
						goto IL_002f;
					}
					return false;
				}
				goto IL_002f;
				IL_002f:
				QualityCategory qualityCategory = default(QualityCategory);
				if (!((Thing)base.parent).TryGetQuality(out qualityCategory))
				{
					return true;
				}
				return (int)qualityCategory >= (int)this.Props.minQualityForArtistic;
			}
		}

		public bool Active
		{
			get
			{
				return this.taleRef != null;
			}
		}

		public CompProperties_Art Props
		{
			get
			{
				return (CompProperties_Art)base.props;
			}
		}

		public void InitializeArt(ArtGenerationContext source)
		{
			this.InitializeArt(null, source);
		}

		public void InitializeArt(Thing relatedThing)
		{
			this.InitializeArt(relatedThing, ArtGenerationContext.Colony);
		}

		private void InitializeArt(Thing relatedThing, ArtGenerationContext source)
		{
			if (this.taleRef != null)
			{
				this.taleRef.ReferenceDestroyed();
				this.taleRef = null;
			}
			if (this.CanShowArt)
			{
				if (Current.ProgramState == ProgramState.Playing)
				{
					if (relatedThing != null)
					{
						this.taleRef = Find.TaleManager.GetRandomTaleReferenceForArtConcerning(relatedThing);
					}
					else
					{
						this.taleRef = Find.TaleManager.GetRandomTaleReferenceForArt(source);
					}
				}
				else
				{
					this.taleRef = TaleReference.Taleless;
				}
				this.titleInt = this.GenerateTitle();
			}
			else
			{
				this.titleInt = null;
				this.taleRef = null;
			}
		}

		public void JustCreatedBy(Pawn pawn)
		{
			if (this.CanShowArt)
			{
				this.authorNameInt = pawn.Name.ToStringFull;
			}
		}

		public void Clear()
		{
			this.authorNameInt = null;
			this.titleInt = null;
			if (this.taleRef != null)
			{
				this.taleRef.ReferenceDestroyed();
				this.taleRef = null;
			}
		}

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look<string>(ref this.authorNameInt, "authorName", (string)null, false);
			Scribe_Values.Look<string>(ref this.titleInt, "title", (string)null, false);
			Scribe_Deep.Look<TaleReference>(ref this.taleRef, "taleRef", new object[0]);
		}

		public override string CompInspectStringExtra()
		{
			if (!this.Active)
			{
				return null;
			}
			string text = "Author".Translate() + ": " + this.AuthorName;
			string text2 = text;
			return text2 + "\n" + "Title".Translate() + ": " + this.Title;
		}

		public override void PostDestroy(DestroyMode mode, Map previousMap)
		{
			base.PostDestroy(mode, previousMap);
			if (this.taleRef != null)
			{
				this.taleRef.ReferenceDestroyed();
				this.taleRef = null;
			}
		}

		public override string GetDescriptionPart()
		{
			if (!this.Active)
			{
				return null;
			}
			string empty = string.Empty;
			empty += this.Title;
			empty += "\n\n";
			empty += this.GenerateImageDescription();
			empty += "\n\n";
			return empty + "Author".Translate() + ": " + this.AuthorName;
		}

		public override bool AllowStackWith(Thing other)
		{
			if (this.Active)
			{
				return false;
			}
			return true;
		}

		public string GenerateImageDescription()
		{
			if (this.taleRef == null)
			{
				Log.Error("Did CompArt.GenerateImageDescription without initializing art: " + base.parent);
				this.InitializeArt(ArtGenerationContext.Outsider);
			}
			return this.taleRef.GenerateText(TextGenerationPurpose.ArtDescription, this.Props.descriptionMaker.RulesPlusIncludes);
		}

		private string GenerateTitle()
		{
			if (this.taleRef == null)
			{
				Log.Error("Did CompArt.GenerateTitle without initializing art: " + base.parent);
				this.InitializeArt(ArtGenerationContext.Outsider);
			}
			return GenText.CapitalizeAsTitle(this.taleRef.GenerateText(TextGenerationPurpose.ArtName, this.Props.nameMaker.RulesPlusIncludes));
		}
	}
}
