using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace RimWorld
{
	public class InteractionWorker_RecruitAttempt : InteractionWorker
	{
		private const float MinRecruitChance = 0.005f;

		private const float BondRelationChanceFactor = 4f;

		private static readonly SimpleCurve RecruitChanceFactorCurve_Wildness = new SimpleCurve
		{
			{
				new CurvePoint(1f, 0f),
				true
			},
			{
				new CurvePoint(0.5f, 1f),
				true
			},
			{
				new CurvePoint(0f, 2f),
				true
			}
		};

		private static readonly SimpleCurve RecruitChanceFactorCurve_Opinion = new SimpleCurve
		{
			{
				new CurvePoint(-50f, 0f),
				true
			},
			{
				new CurvePoint(50f, 1f),
				true
			},
			{
				new CurvePoint(100f, 2f),
				true
			}
		};

		private static readonly SimpleCurve RecruitChanceFactorCurve_Mood = new SimpleCurve
		{
			{
				new CurvePoint(0f, 0.25f),
				true
			},
			{
				new CurvePoint(0.1f, 0.25f),
				true
			},
			{
				new CurvePoint(0.25f, 1f),
				true
			},
			{
				new CurvePoint(0.5f, 1f),
				true
			},
			{
				new CurvePoint(1f, 1.5f),
				true
			}
		};

		private const int MenagerieThreshold = 10;

		private const float WildManRecruitChanceFactor = 0.25f;

		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks)
		{
			if (!recipient.mindState.CheckStartMentalStateBecauseRecruitAttempted(initiator))
			{
				bool flag = initiator.InspirationDef == InspirationDefOf.InspiredRecruitment && recipient.RaceProps.Humanlike;
				float num = 1f;
				if (flag || DebugSettings.instantRecruit)
				{
					num = 1f;
				}
				else
				{
					num *= ((!recipient.NonHumanlikeOrWildMan()) ? initiator.GetStatValue(StatDefOf.RecruitPrisonerChance, true) : initiator.GetStatValue(StatDefOf.TameAnimalChance, true));
					float num2 = (float)((!recipient.IsWildMan()) ? ((!recipient.RaceProps.Humanlike) ? InteractionWorker_RecruitAttempt.RecruitChanceFactorCurve_Wildness.Evaluate(recipient.RaceProps.wildness) : (1.0 - recipient.RecruitDifficulty(initiator.Faction, true))) : 0.25);
					num *= num2;
					if (!recipient.NonHumanlikeOrWildMan())
					{
						float x = (float)recipient.relations.OpinionOf(initiator);
						num *= InteractionWorker_RecruitAttempt.RecruitChanceFactorCurve_Opinion.Evaluate(x);
						if (recipient.needs.mood != null)
						{
							float curLevel = recipient.needs.mood.CurLevel;
							num *= InteractionWorker_RecruitAttempt.RecruitChanceFactorCurve_Mood.Evaluate(curLevel);
						}
					}
					if (initiator.relations.DirectRelationExists(PawnRelationDefOf.Bond, recipient))
					{
						num = (float)(num * 4.0);
					}
					num = Mathf.Clamp(num, 0.005f, 1f);
				}
				if (Rand.Chance(num))
				{
					InteractionWorker_RecruitAttempt.DoRecruit(initiator, recipient, num, true);
					extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptAccepted);
					if (flag)
					{
						initiator.mindState.inspirationHandler.EndInspiration(InspirationDefOf.InspiredRecruitment);
					}
				}
				else
				{
					string text = (!recipient.NonHumanlikeOrWildMan()) ? "TextMote_RecruitFail".Translate(num.ToStringPercent()) : "TextMote_TameFail".Translate(num.ToStringPercent());
					MoteMaker.ThrowText((initiator.DrawPos + recipient.DrawPos) / 2f, initiator.Map, text, 8f);
					extraSentencePacks.Add(RulePackDefOf.Sentence_RecruitAttemptRejected);
				}
			}
		}

		public static void DoRecruit(Pawn recruiter, Pawn recruitee, float recruitChance, bool useAudiovisualEffects = true)
		{
			string text = recruitee.LabelIndefinite();
			if (recruitee.guest != null)
			{
				recruitee.guest.SetGuestStatus(null, false);
			}
			bool flag = recruitee.Name != null;
			if (recruitee.Faction != recruiter.Faction)
			{
				recruitee.SetFaction(recruiter.Faction, recruiter);
			}
			if (recruitee.RaceProps.Humanlike)
			{
				if (useAudiovisualEffects)
				{
					Find.LetterStack.ReceiveLetter("LetterLabelMessageRecruitSuccess".Translate(), "MessageRecruitSuccess".Translate(recruiter, recruitee, recruitChance.ToStringPercent()), LetterDefOf.PositiveEvent, recruitee, null);
				}
				TaleRecorder.RecordTale(TaleDefOf.Recruited, recruiter, recruitee);
				recruiter.records.Increment(RecordDefOf.PrisonersRecruited);
				recruitee.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.RecruitedMe, recruiter);
			}
			else
			{
				if (useAudiovisualEffects)
				{
					if (!flag)
					{
						Messages.Message("MessageTameAndNameSuccess".Translate(recruiter.LabelShort, text, recruitChance.ToStringPercent(), recruitee.Name.ToStringFull).AdjustedFor(recruitee), recruitee, MessageTypeDefOf.PositiveEvent);
					}
					else
					{
						Messages.Message("MessageTameSuccess".Translate(recruiter.LabelShort, text, recruitChance.ToStringPercent()), recruitee, MessageTypeDefOf.PositiveEvent);
					}
					MoteMaker.ThrowText((recruiter.DrawPos + recruitee.DrawPos) / 2f, recruiter.Map, "TextMote_TameSuccess".Translate(recruitChance.ToStringPercent()), 8f);
				}
				recruiter.records.Increment(RecordDefOf.AnimalsTamed);
				RelationsUtility.TryDevelopBondRelation(recruiter, recruitee, 0.01f);
				float chance = Mathf.Lerp(0.02f, 1f, recruitee.RaceProps.wildness);
				if (Rand.Chance(chance) || recruitee.IsWildMan())
				{
					TaleRecorder.RecordTale(TaleDefOf.TamedAnimal, recruiter, recruitee);
				}
				if (PawnsFinder.AllMapsWorldAndTemporary_Alive.Count((Pawn p) => p.playerSettings != null && p.playerSettings.master == recruiter) >= 10)
				{
					TaleRecorder.RecordTale(TaleDefOf.IncreasedMenagerie, recruiter, recruitee);
				}
			}
			if (recruitee.caller != null)
			{
				recruitee.caller.DoCall();
			}
		}
	}
}
