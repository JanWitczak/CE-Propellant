using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;
using System;
using System.Linq;

namespace Propellant
{
	public class PropellantMod : Mod
	{
		public static PropellantSettings Settings;
		public PropellantMod(ModContentPack content) : base(content)
		{
			Settings = GetSettings<PropellantSettings>();
		}
		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard settingsMenu = new Listing_Standard();
			settingsMenu.Begin(inRect);
			settingsMenu.Label($"{"SteelFactor".Translate()}: {Settings.SteelFactor}");
			Settings.SteelFactor = (float)Math.Round(settingsMenu.Slider(Settings.SteelFactor, 0.1f, 5.0f), 1);
			settingsMenu.Label($"{"PropellantFactor".Translate()}: {Settings.PropellantFactor}");
			Settings.PropellantFactor = (float)Math.Round(settingsMenu.Slider(Settings.PropellantFactor, 0.1f, 5.0f), 1);
			settingsMenu.Label($"{"OtherFactor".Translate()}: {Settings.OtherFactor}");
			Settings.OtherFactor = (float)Math.Round(settingsMenu.Slider(Settings.OtherFactor, 0.1f, 5.0f), 1);
			settingsMenu.Label($"{"WorkFactor".Translate()}: {Settings.WorkFactor}");
			Settings.WorkFactor = (float)Math.Round(settingsMenu.Slider(Settings.WorkFactor, 0.1f, 5.0f), 1);
			settingsMenu.GapLine(25);
			settingsMenu.Label($"{"MarketValueFactor".Translate()}: {Settings.MarketValueFactor}");
			Settings.MarketValueFactor = (float)Math.Round(settingsMenu.Slider(Settings.MarketValueFactor, 0.1f, 5.0f), 1);
			settingsMenu.GapLine(25);
			settingsMenu.Label($"{"TraderSmallAmmoCountFactor".Translate()}: {Settings.TraderSmallAmmoCountFactor}");
			Settings.TraderSmallAmmoCountFactor = (float)Math.Round(settingsMenu.Slider(Settings.TraderSmallAmmoCountFactor, 0.5f, 5.0f), 1);
			settingsMenu.Label($"{"TraderMediumAmmoCountFactor".Translate()}: {Settings.TraderMediumAmmoCountFactor}");
			Settings.TraderMediumAmmoCountFactor = (float)Math.Round(settingsMenu.Slider(Settings.TraderMediumAmmoCountFactor, 0.5f, 5.0f), 1);
			settingsMenu.Label($"{"TraderHeavyAmmoCountFactor".Translate()}: {Settings.TraderHeavyAmmoCountFactor}");
			Settings.TraderHeavyAmmoCountFactor = (float)Math.Round(settingsMenu.Slider(Settings.TraderHeavyAmmoCountFactor, 0.5f, 5.0f), 1);
			settingsMenu.End();
		}
		public override string SettingsCategory()
		{
			return "CE Propellant";
		}
	}
	public class PropellantSettings : ModSettings
	{
		public float SteelFactor = 1.0f;
		public float PropellantFactor = 0.5f;
		public float OtherFactor = 1.0f;
		public float WorkFactor = 2.0f;
		public float MarketValueFactor = 1.0f;
		public float TraderSmallAmmoCountFactor = 1.0f;
		public float TraderMediumAmmoCountFactor = 1.0f;
		public float TraderHeavyAmmoCountFactor = 1.0f;
		public override void ExposeData()
		{
			Scribe_Values.Look(ref SteelFactor, "SteelFactor", defaultValue: 1.0f);
			Scribe_Values.Look(ref PropellantFactor, "PropellantFactor", defaultValue: 0.5f);
			Scribe_Values.Look(ref OtherFactor, "OtherFactor", defaultValue: 1.0f);
			Scribe_Values.Look(ref WorkFactor, "WorkFactor", defaultValue: 2.0f);
			Scribe_Values.Look(ref MarketValueFactor, "MarketValueFactor", defaultValue: 1.0f);
			Scribe_Values.Look(ref TraderSmallAmmoCountFactor, "TraderSmallAmmoCountFactor", defaultValue: 1.0f);
			Scribe_Values.Look(ref TraderMediumAmmoCountFactor, "TraderMediumAmmoCountFactor", defaultValue: 1.0f);
			Scribe_Values.Look(ref TraderHeavyAmmoCountFactor, "TraderHeavyAmmoCountFactor", defaultValue: 1.0f);
		}
	}
	[StaticConstructorOnStartup]
	static class AmmoProcessor
	{
		static AmmoProcessor()
		{
			ThingDef propellant = DefDatabase<ThingDef>.GetNamed("AIE-Propellant");
			IEnumerable<RecipeDef> recipeDefs = DefDatabase<RecipeDef>.AllDefs;
			foreach (RecipeDef recipeDef in recipeDefs)
			{
				if (recipeDef.ingredients.Any(x => x.IsFixedIngredient && x.FixedIngredient == propellant))
				{
					float baseSteelCount = recipeDef.ingredients.Find(x => x.IsFixedIngredient && x.FixedIngredient == ThingDefOf.Steel).GetBaseCount();
					foreach (IngredientCount ingredient in recipeDef.ingredients)
					{
						if (ingredient.IsFixedIngredient && ingredient.FixedIngredient == ThingDefOf.Steel)
						{
							ingredient.SetBaseCount(ingredient.GetBaseCount() * PropellantMod.Settings.SteelFactor);
						}
						else if (ingredient.IsFixedIngredient && ingredient.FixedIngredient == propellant)
						{
							ingredient.SetBaseCount(baseSteelCount * PropellantMod.Settings.PropellantFactor);
						}
						else
						{
							ingredient.SetBaseCount(ingredient.GetBaseCount() * PropellantMod.Settings.OtherFactor);
						}
					}
					recipeDef.workAmount *= PropellantMod.Settings.WorkFactor;
					recipeDef.ProducedThingDef.BaseMarketValue *= PropellantMod.Settings.MarketValueFactor;
				}
			}
			IEnumerable<TraderKindDef> traderKindDefs = DefDatabase<TraderKindDef>.AllDefs;
			foreach (TraderKindDef traderKindDef in traderKindDefs)
			{
				foreach (StockGenerator stockGenerator in traderKindDef.stockGenerators)
				{
					StockGenerator_Tag stockGeneratorTag = stockGenerator as StockGenerator_Tag;
					if (stockGeneratorTag == null) continue;
					if (stockGeneratorTag.tradeTag == "CE_Ammo")
					{
						stockGeneratorTag.countRange.min = (int)(stockGeneratorTag.countRange.min * PropellantMod.Settings.TraderSmallAmmoCountFactor);
						stockGeneratorTag.countRange.max = (int)(stockGeneratorTag.countRange.max * PropellantMod.Settings.TraderSmallAmmoCountFactor);
					}
					else if (stockGeneratorTag.tradeTag == "CE_MediumAmmo")
					{
						stockGeneratorTag.countRange.min = (int)(stockGeneratorTag.countRange.min * PropellantMod.Settings.TraderMediumAmmoCountFactor);
						stockGeneratorTag.countRange.max = (int)(stockGeneratorTag.countRange.max * PropellantMod.Settings.TraderMediumAmmoCountFactor);
					}
					else if (stockGeneratorTag.tradeTag == "CE_HeavyAmmo")
					{
						stockGeneratorTag.countRange.min = (int)(stockGeneratorTag.countRange.min * PropellantMod.Settings.TraderHeavyAmmoCountFactor);
						stockGeneratorTag.countRange.max = (int)(stockGeneratorTag.countRange.max * PropellantMod.Settings.TraderHeavyAmmoCountFactor);
					}
				}
			}
		}
	}
}