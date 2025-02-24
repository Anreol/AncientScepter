﻿using AncientScepterSkills.Content.ModCompatibility;
using EntityStates.Merc;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;

namespace AncientScepterSkills.Content.Skills
{
    public class MercEvis2 : ClonedScepterSkill
    {
        public override SkillDef skillDefToClone { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }

        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Double duration. Kills reset duration." +
            "\nHold down the special button to leave earlier.</color>";

        public override string exclusiveToBodyName => "MercBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 0;

        internal override void Setup()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/MercBody/MercBodyEvis");
            skillDefToClone = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_MERC_EVISNAME";
            newDescToken = "ANCIENTSCEPTER_MERC_EVISDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Massacre";
            LanguageAPI.Add(nametoken, namestr);

            skillDefToClone.skillName = $"{oldDef.skillName}Scepter";
            (skillDefToClone as ScriptableObject).name = skillDefToClone.skillName;
            skillDefToClone.skillNameToken = nametoken;
            skillDefToClone.skillDescriptionToken = newDescToken;
            skillDefToClone.icon = Assets.SpriteAssets.MercEvis2;

            ContentAddition.AddSkillDef(skillDefToClone);

            if (BetterUICompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(skillDefToClone.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("MercBodyEvis"));
        }

        internal override void LoadBehavior()
        {
            GlobalEventManager.onCharacterDeathGlobal += Evt_GEMOnCharacterDeathGlobal;
            On.EntityStates.Merc.Evis.FixedUpdate += On_EvisFixedUpdate;
        }

        internal override void UnloadBehavior()
        {
            GlobalEventManager.onCharacterDeathGlobal -= Evt_GEMOnCharacterDeathGlobal;
            On.EntityStates.Merc.Evis.FixedUpdate -= On_EvisFixedUpdate;
        }

        private void Evt_GEMOnCharacterDeathGlobal(DamageReport rep)
        {
            var attackerState = rep.attackerBody?.GetComponent<EntityStateMachine>()?.state;
            if (attackerState is Evis asEvis && rep.attackerBody.skillLocator.GetSkill(targetSlot)?.skillDef == skillDefToClone
                && Vector3.Distance(rep.attackerBody.transform.position, rep.victim.transform.position) < Evis.maxRadius)
            {
                if (rep.attackerBody.inputBank.skill4.down == false)
                {
                    asEvis.stopwatch = 0f;
                }
                else
                {
                    asEvis.stopwatch = 99f;
                }
            }
        }

        private void On_EvisFixedUpdate(On.EntityStates.Merc.Evis.orig_FixedUpdate orig, Evis self)
        {
            var origDuration = Evis.duration;
            if (self.outer.commonComponents.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == skillDefToClone) Evis.duration *= 2f;
            orig(self);
            Evis.duration = origDuration;
        }
    }
}