using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;
using HarmonyLib;
using HarmonyMod;

namespace GetOnTheGround
{
   public class GetOnTheGroundComp : ThingComp
    {
		public bool shouldStand = true;

		public bool isProne
		{
			get
			{
				return ((!shouldStand) && dad.jobs.posture == PawnPosture.LayingOnGroundFaceUp);
			}
		}

		public Pawn dad
		{
			get
			{
				return (Pawn)this.parent;
			}
		}

		public override void CompTick()
		{
			if (dad != null)
			{
				if (dad.Faction == Faction.OfPlayer)
				{
					if (dad.Drafted)
					{
						if (!shouldStand && !isProne)
						{
							if ((dad.jobs?.curJob?.def ?? null) == JobDefOf.Goto | (dad.jobs?.curJob?.def ?? null) == JobDefOf.Wait | (dad.jobs?.curJob?.def ?? null) == JobDefOf.Wait_Combat)
							{
								dad.jobs.posture = PawnPosture.LayingOnGroundFaceUp;
							}
						}

						
					}
				}
			}

			

			
			
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
			if (dad != null)
			{
				if (dad.Drafted)
				{
					if (shouldStand)
					{
						yield return new Command_Action()
						{

							defaultLabel = "crawl",
							defaultDesc = "crawl or go prone. Or whatever the fuck you call it",

							icon = ContentFinder<Texture2D>.Get("goprone", true),

							action = delegate { shouldStand = false; }


						};
					}
					if (!shouldStand)
					{
						yield return new Command_Action()
						{

							defaultLabel = "get up",
							defaultDesc = "stop crawling",

							icon = ContentFinder<Texture2D>.Get("goup", true),

							action = delegate { shouldStand = true; dad.jobs.posture = PawnPosture.Standing; }


						};
					}
				}
			}

				
			
		}
    }
	

	public class StatPart_Crawling : StatPart
	{
		public override void TransformValue(StatRequest req, ref float val)
		{
			if (!(req.Thing is Pawn))
			{
				return;
			}

			if (req.Thing is Pawn p)
			{
				if (HasComp((Pawn)req.Thing))
				{
					if (ActuallyProne((Pawn)req.Thing))
					{
						val *= 0.30f;
						val *= Math.Max((p.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation)), 0.5f);
					}


				}
			}


		}

		public override string ExplanationPart(StatRequest req)
		{
			if (!(req.Thing is Pawn))
			{
				return "";
			}

			if (HasComp((Pawn)req.Thing))
			{
				if (ActuallyProne((Pawn)req.Thing))
				{
					return "Prone:  * 0.3x" + "\n" + " Manipulation effect when prone " + Math.Max((((Pawn)(req.Thing)).health.capacities.GetLevel(PawnCapacityDefOf.Manipulation)), 0.5f).ToString() + "x";
				}
				else
				{
					return "";
				}
				
			}
			else
			{
				return "";
			}
		}

		public bool HasComp(Pawn p)
		{
			if (p.Faction != Faction.OfPlayer)
			{
				return false;
			}

			if (p.TryGetComp<GetOnTheGroundComp>() != null)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool ActuallyProne(Pawn p)
		{
			if (HasComp(p))
			{
				return p.TryGetComp<GetOnTheGroundComp>().isProne;
			}
			else
			{
				return false;
			}
		}
	}
	[StaticConstructorOnStartup]
	public class FuckPatches
	{
		static FuckPatches()
		{
			StatDefOf.MoveSpeed.parts.Add(new StatPart_Crawling());

			List<ThingDef> humanoids = DefDatabase<ThingDef>.AllDefs.Where(x => x.race != null && x.race.Humanlike && !(x.comps?.Any(y => y.compClass == typeof(GetOnTheGroundComp)) ?? false)).ToList();

			foreach (ThingDef human in humanoids)
			{
				if (human.comps == null)
				{
					human.comps = new List<CompProperties>();
				}

				human.comps.Add(new CompProperties { compClass = typeof(GetOnTheGroundComp) });

				Log.Message(human.label.Colorize(Color.green));
			}
		}
	}
}
