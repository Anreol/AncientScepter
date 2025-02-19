﻿using AncientScepterSkills.Content.ModCompatibility;
using EntityStates.Commando.CommandoWeapon;
using R2API;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace AncientScepterSkills.Content.Skills
{
    public class CommandoGrenade2 : ClonedScepterSkill
    {
        private static GameObject projReplacer;
        public override SkillDef skillDefToClone { get; protected set; }

        public override string oldDescToken { get; protected set; }
        public override string newDescToken { get; protected set; }
        public override string overrideStr => "\n<color=#d299ff>SCEPTER: Half damage and knockback; throw eight at once.</color>";

        public override string exclusiveToBodyName => "CommandoBody";
        public override SkillSlot targetSlot => SkillSlot.Special;
        public override int targetVariantIndex => 1;

        internal override void Setup()
        {
            var oldDef = LegacyResourcesAPI.Load<SkillDef>("SkillDefs/CommandoBody/ThrowGrenade");
            skillDefToClone = CloneSkillDef(oldDef);

            var nametoken = "ANCIENTSCEPTER_COMMANDO_GRENADENAME";
            newDescToken = "ANCIENTSCEPTER_COMMANDO_GRENADEDESC";
            oldDescToken = oldDef.skillDescriptionToken;
            var namestr = "Carpet Bomb";
            LanguageAPI.Add(nametoken, namestr);

            skillDefToClone.skillName = $"{oldDef.skillName}Scepter";
            (skillDefToClone as ScriptableObject).name = skillDefToClone.skillName;
            skillDefToClone.skillNameToken = nametoken;
            skillDefToClone.skillDescriptionToken = newDescToken;
            skillDefToClone.icon = Assets.SpriteAssets.CommandoGrenade2;

            ContentAddition.AddSkillDef(skillDefToClone);

            projReplacer = Resources.Load<GameObject>("prefabs/projectiles/CommandoGrenadeProjectile").InstantiateClone("CIScepCommandoGrenade");
            var pie = projReplacer.GetComponent<ProjectileImpactExplosion>();
            pie.blastDamageCoefficient *= 0.5f;
            pie.bonusBlastForce *= 0.5f;

            ContentAddition.AddProjectile(projReplacer);

            if (BetterUICompat.compatBetterUI)
            {
                doBetterUI();
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining | System.Runtime.CompilerServices.MethodImplOptions.NoOptimization)]
        internal void doBetterUI()
        {
            BetterUI.ProcCoefficientCatalog.AddSkill(skillDefToClone.skillName, BetterUI.ProcCoefficientCatalog.GetProcCoefficientInfo("ThrowGrenade"));
        }

        internal override void LoadBehavior()
        {
            On.EntityStates.GenericProjectileBaseState.FireProjectile += On_FireFMJFire;
        }

        internal override void UnloadBehavior()
        {
            On.EntityStates.GenericProjectileBaseState.FireProjectile -= On_FireFMJFire;
        }

        private void On_FireFMJFire(On.EntityStates.GenericProjectileBaseState.orig_FireProjectile orig, global::EntityStates.GenericProjectileBaseState self)
        {
            var cc = self.outer.commonComponents;
            bool isBoosted = self is ThrowGrenade
                && Util.HasEffectiveAuthority(self.outer.networkIdentity)
                && cc.characterBody.skillLocator.GetSkill(targetSlot)?.skillDef == skillDefToClone;
            if (isBoosted) self.projectilePrefab = projReplacer;
            orig(self);
            if (isBoosted)
            {
                for (var i = 0; i < 7; i++)
                {
                    Ray r;
                    if (cc.inputBank) r = new Ray(cc.inputBank.aimOrigin, cc.inputBank.aimDirection);
                    else r = new Ray(cc.transform.position, cc.transform.forward);
                    r.direction = Util.ApplySpread(r.direction, self.minSpread + 7f, self.maxSpread + 15f, 1f, 1f, 0f, self.projectilePitchBonus);
                    ProjectileManager.instance.FireProjectile(
                        self.projectilePrefab,
                        r.origin, Util.QuaternionSafeLookRotation(r.direction),
                        self.outer.gameObject,
                        self.damageStat * self.damageCoefficient,
                        self.force,
                        Util.CheckRoll(self.critStat, cc.characterBody.master),
                        DamageColorIndex.Default, null, -1f);
                }
            }
        }
    }
}