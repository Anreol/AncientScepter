﻿using AncientScepterSkills.Content.ModCompatibility;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace AncientScepterSkills.Content.Skills
{
    public class MercEvisProjectile2 : ClonedScepterSkill
    {
        public override SkillDef skillDefToClone { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Charges four times faster. Hold and fire up to four charges at once.</color>";

        public override string exclusiveToBodyName => "MercBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void Setup()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/MercBody/MercBodyEvisProjectile");
            skillDefToClone = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_MERC_EVISPROJNAME";
            newDescToken = "ANCIENTSCEPTER_MERC_EVISPROJDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Gale-Force";
            LanguageAPI.Add(nametoken, namestr);

            skillDefToClone.skillName = $"{oldDef.skillName}Scepter";
            (skillDefToClone as ScriptableObject).name = skillDefToClone.skillName;
            skillDefToClone.skillNameToken = nametoken;
            skillDefToClone.skillDescriptionToken = newDescToken;
            skillDefToClone.icon = Assets.SpriteAssets.MercEvis2Projectile;
            skillDefToClone.baseMaxStock *= 4;
            skillDefToClone.baseRechargeInterval /= 4f;

            ContentAddition.AddSkillDef(skillDefToClone);

            if (BetterUICompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(skillDefToClone.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("MercBodyEvisProjectile"));
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.GenericProjectileBaseState.OnEnter += On_FireFMJEnter;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.GenericProjectileBaseState.OnEnter -= On_FireFMJEnter;
        }

        private void On_FireFMJEnter(On.EntityStates.GenericProjectileBaseState.orig_OnEnter orig, global::EntityStates.GenericProjectileBaseState self)
        {
            orig(self);
            if (!(self is global::EntityStates.Merc.Weapon.ThrowEvisProjectile) || self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef != skillDefToClone) return;
            if (!self.outer.commonComponents.skillLocator?.special) return;
            var fireCount = self.outer.commonComponents.skillLocator.special.stock;
            self.outer.commonComponents.skillLocator.special.stock = 0;
            for (var i = 0; i < fireCount; i++)
            {
                self.FireProjectile();
                self.DoFireEffects();
            }
        }
    }
}