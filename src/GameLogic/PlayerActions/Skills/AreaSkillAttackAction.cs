// <copyright file="AreaSkillAttackAction.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Skills;

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.Views;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Pathfinding;

/// <summary>
/// Action to attack with a skill which inflicts damage to an area of the current map of the player.
/// </summary>
public class AreaSkillAttackAction
{
    private const int UndefinedTarget = 0xFFFF;
    private const short ElectricSpikeSkillNumber = 65;
    private const short StunSkillNumber = 67;
    private const short CastleSiegeMapNumber = 30;
    private const short LandOfTrialsMapNumber = 31;
    private const int ElectricSpikePartyRange = 10;
    private const float ElectricSpikePartyHealthLossFactor = 0.2f;
    private const float ElectricSpikePartyManaLossFactor = 0.05f;

    private static readonly ConcurrentDictionary<AreaSkillSettings, FrustumBasedTargetFilter> FrustumFilters = new();

    /// <summary>
    /// Performs the skill by the player at the specified area. Additionally, to the target area, a target object can be specified.
    /// </summary>
    /// <param name="player">The player.</param>
    /// <param name="extraTargetId">The extra target identifier.</param>
    /// <param name="skillId">The skill identifier.</param>
    /// <param name="targetAreaCenter">The coordinates of the center of the target area.</param>
    /// <param name="rotation">The rotation in which the player is looking. It's not really relevant for the hitted objects yet, but for some directed skills in the future it might be.</param>
    /// <param name="hitImplicitlyForExplicitSkill">If set to <c>true</c>, hit implicitly for <see cref="SkillType.AreaSkillExplicitHits"/>.</param>
    public async ValueTask AttackAsync(Player player, ushort extraTargetId, ushort skillId, Point targetAreaCenter, byte rotation, bool hitImplicitlyForExplicitSkill = false)
    {
        var skillEntry = player.SkillList?.GetSkill(skillId);
        var skill = skillEntry?.Skill;
        if (skill is null || skill.SkillType == SkillType.PassiveBoost)
        {
            return;
        }

        if (!await player.TryConsumeForSkillAsync(skill).ConfigureAwait(false))
        {
            return;
        }

        if (skill.SkillType is SkillType.AreaSkillAutomaticHits or SkillType.AreaSkillExplicitTarget or SkillType.Buff
            || (skill.SkillType is SkillType.AreaSkillExplicitHits && hitImplicitlyForExplicitSkill))
        {
            // todo: delayed automatic hits, like evil spirit, flame, triple shot... when hitImplicitlyForExplicitSkill = true.

            await this.PerformAutomaticHitsAsync(player, extraTargetId, targetAreaCenter, skillEntry!, skill, rotation).ConfigureAwait(false);
        }

        await player.ForEachWorldObserverAsync<IShowAreaSkillAnimationPlugIn>(p => p.ShowAreaSkillAnimationAsync(player, skill, targetAreaCenter, rotation), true).ConfigureAwait(false);
    }

    private static bool AreaSkillSettingsAreDefault([NotNullWhen(true)] AreaSkillSettings? settings)
    {
        if (settings is null)
        {
            return true;
        }

        return !settings.UseDeferredHits
               && settings.DelayPerOneDistance <= TimeSpan.Zero
               && settings.MinimumNumberOfHitsPerTarget == 1
               && settings.MaximumNumberOfHitsPerTarget == 1
               && settings.MaximumNumberOfHitsPerAttack == 0
               && settings.DelayPerHit <= TimeSpan.Zero
               && settings.RandomDelayPerHitMinimum <= TimeSpan.Zero
               && settings.RandomDelayPerHitMaximum <= TimeSpan.Zero
               && Math.Abs(settings.HitChancePerDistanceMultiplier - 1.0) <= 0.00001f
               && settings.GuaranteedTargets == 0
               && settings.MaximumTargets == 0
               && Math.Abs(settings.AdditionalTargetChance - 1.0) <= 0.00001f
               && settings.ProjectileCount <= 1
               && settings.ExplicitTargetAdditionalHits == 0
               && settings.ExplicitTargetAdditionalHitDelay <= TimeSpan.Zero
               && !settings.UseCasterAsCenter;
    }

    private static IEnumerable<IAttackable> GetTargets(Player player, Point targetAreaCenter, Skill skill, byte rotation, ushort extraTargetId)
    {
        var isExtraTargetDefined = extraTargetId != UndefinedTarget;
        var extraTarget = isExtraTargetDefined ? player.GetObject(extraTargetId) as IAttackable : null;

        if (skill.SkillType == SkillType.AreaSkillExplicitTarget)
        {
            if (extraTarget is not null
                && extraTarget.CheckSkillTargetRestrictions(player, skill)
                && IsTargetInSkillRange(player, extraTarget.Position, skill, 2)
                && !extraTarget.IsAtSafezone())
            {
                yield return extraTarget;
            }

            yield break;
        }

        // Include the explicit extra target if it's valid and in range
        if (extraTarget is not null
            && extraTarget.CheckSkillTargetRestrictions(player, skill)
            && IsTargetInSkillRange(player, extraTarget.Position, skill, 2)
            && !extraTarget.IsAtSafezone())
        {
            yield return extraTarget;
        }

        foreach (var target in GetTargetsInRange(player, targetAreaCenter, skill, rotation))
        {
            // Skip the extra target if we already yielded it
            if (target.Id == extraTargetId)
            {
                continue;
            }

            yield return target;
        }
    }

    private static IEnumerable<IAttackable> GetTargetsInRange(Player player, Point targetAreaCenter, Skill skill, byte rotation)
    {
        var areaSkillSettings = skill.AreaSkillSettings;
        var center = areaSkillSettings?.UseCasterAsCenter == true ? player.Position : targetAreaCenter;
        var range = skill.Range;
        var targetsInRange = player.CurrentMap?
                    .GetAttackablesInRange(center, range)
                    .Where(a => a != player)
                    .Where(a => !a.IsAtSafezone())
            ?? [];

        if (areaSkillSettings?.UseEuclideanRange == true)
        {
            targetsInRange = targetsInRange.Where(a => IsWithinEuclideanRange(center, a.Position, range));
        }

        if (areaSkillSettings is { UseFrustumFilter: true })
        {
            var filter = FrustumFilters.GetOrAdd(areaSkillSettings, static s => new FrustumBasedTargetFilter(s.FrustumStartWidth, s.FrustumEndWidth, s.FrustumDistance, s.ProjectileCount > 0 ? s.ProjectileCount : 1));
            targetsInRange = targetsInRange.Where(a => filter.IsTargetWithinBounds(player, a, rotation));
        }

        if (areaSkillSettings is { UseTargetAreaFilter: true })
        {
            var halfDiameter = areaSkillSettings.TargetAreaDiameter * 0.5f;
            if (areaSkillSettings.UseTargetAreaSquare)
            {
                targetsInRange = targetsInRange.Where(a => Math.Abs(a.Position.X - center.X) <= halfDiameter && Math.Abs(a.Position.Y - center.Y) <= halfDiameter);
            }
            else
            {
                targetsInRange = targetsInRange.Where(a => IsWithinEuclideanRange(center, a.Position, halfDiameter));
            }
        }

        targetsInRange = targetsInRange.Where(target => IsValidAreaSkillTarget(player, skill, target));

        return targetsInRange;
    }

    private static bool IsValidAreaSkillTarget(Player player, Skill skill, IAttackable target)
    {
        var restrictionTarget = target is Monster { SummonedBy: not null } summoned
            ? summoned.SummonedBy
            : target;

        if (!player.GameContext.Configuration.AreaSkillHitsPlayer && restrictionTarget is Player)
        {
            return false;
        }

        return restrictionTarget.CheckSkillTargetRestrictions(player, skill);
    }

    private static bool IsTargetInSkillRange(Player player, Point targetPosition, Skill skill, int rangeBuffer)
    {
        var range = skill.Range + rangeBuffer;
        if (skill.AreaSkillSettings?.UseEuclideanRange == true)
        {
            return IsWithinEuclideanRange(player.Position, targetPosition, range);
        }

        return player.IsInRange(targetPosition, range);
    }

    private static bool IsWithinEuclideanRange(Point origin, Point target, double range)
    {
        if (range < 0)
        {
            return false;
        }

        return Math.Floor(origin.EuclideanDistanceTo(target)) <= range;
    }

    private async ValueTask PerformAutomaticHitsAsync(Player player, ushort extraTargetId, Point targetAreaCenter, SkillEntry skillEntry, Skill skill, byte rotation)
    {
        if (player.Attributes is not { } attributes)
        {
            return;
        }

        if (attributes[Stats.IsStunned] > 0)
        {
            player.Logger.LogWarning("Probably Hacker - player {player} is attacking in stunned state", player);
            return;
        }

        if (attributes[Stats.IsAsleep] > 0)
        {
            player.Logger.LogWarning("Probably Hacker - player {player} is attacking in sleep state", player);
            return;
        }

        if (player.IsAtSafezone())
        {
            player.Logger.LogWarning("Probably Hacker - player {player} is attacking from safezone", player);
            return;
        }

        if (player.Attributes[Stats.AmmunitionConsumptionRate] > player.Attributes[Stats.AmmunitionAmount])
        {
            return;
        }

        var baseSkillNumber = skillEntry.GetBaseSkill().Number;
        if (baseSkillNumber == StunSkillNumber)
        {
            var mapNumber = player.CurrentMap?.Definition.Number;
            if (mapNumber != CastleSiegeMapNumber && mapNumber != LandOfTrialsMapNumber)
            {
                return;
            }
        }

        var areaSkillSettings = skill.AreaSkillSettings;
        if (areaSkillSettings?.RequireTargetAreaCenterInRange == true)
        {
            var center = areaSkillSettings.UseCasterAsCenter ? player.Position : targetAreaCenter;
            if (!IsWithinEuclideanRange(player.Position, center, skill.Range))
            {
                return;
            }
        }

        bool isCombo = false;
        if (player.ComboState is { } comboState)
        {
            isCombo = await comboState.RegisterSkillAsync(skill).ConfigureAwait(false);
        }

        IAttackable? extraTarget = null;
        var anyHit = false;
        var targets = GetTargets(player, targetAreaCenter, skill, rotation, extraTargetId);
        if (areaSkillSettings is null || AreaSkillSettingsAreDefault(areaSkillSettings))
        {
            // Just hit all targets once.
            foreach (var target in targets)
            {
                if (target.Id == extraTargetId)
                {
                    extraTarget = target;
                }

                await this.ApplySkillAsync(player, skillEntry, target, targetAreaCenter, isCombo).ConfigureAwait(false);
                anyHit = true;
            }
        }
        else
        {
            (extraTarget, anyHit) = await this.AttackTargetsAsync(player, extraTargetId, targetAreaCenter, skillEntry, areaSkillSettings, targets, rotation, isCombo).ConfigureAwait(false);
        }

        if (anyHit && baseSkillNumber == ElectricSpikeSkillNumber)
        {
            ApplyElectricSpikePartyPenalty(player);
        }

        if (isCombo)
        {
            await player.ForEachWorldObserverAsync<IShowSkillAnimationPlugIn>(p => p.ShowComboAnimationAsync(player, extraTarget), true).ConfigureAwait(false);
        }
    }

    private async Task<(IAttackable? ExtraTarget, bool AnyHit)> AttackTargetsAsync(Player player, ushort extraTargetId, Point targetAreaCenter, SkillEntry skillEntry, AreaSkillSettings areaSkillSettings, IEnumerable<IAttackable> targets, byte rotation, bool isCombo)
    {
        IAttackable? extraTarget = null;
        var anyHit = false;
        var attackCount = 0;
        var maxAttacks = areaSkillSettings.MaximumNumberOfHitsPerAttack == 0 ? int.MaxValue : areaSkillSettings.MaximumNumberOfHitsPerAttack;
        var currentDelay = TimeSpan.Zero;
        var delayPerHit = areaSkillSettings.DelayPerHit;
        if (delayPerHit < TimeSpan.Zero)
        {
            delayPerHit = TimeSpan.Zero;
        }

        var randomDelayMinimum = areaSkillSettings.RandomDelayPerHitMinimum;
        if (randomDelayMinimum < TimeSpan.Zero)
        {
            randomDelayMinimum = TimeSpan.Zero;
        }

        var randomDelayMaximum = areaSkillSettings.RandomDelayPerHitMaximum;
        if (randomDelayMaximum < TimeSpan.Zero)
        {
            randomDelayMaximum = TimeSpan.Zero;
        }

        if (randomDelayMaximum < randomDelayMinimum)
        {
            randomDelayMaximum = randomDelayMinimum;
        }

        var explicitTargetAdditionalHits = areaSkillSettings.ExplicitTargetAdditionalHits;
        if (explicitTargetAdditionalHits < 0)
        {
            explicitTargetAdditionalHits = 0;
        }

        var explicitTargetAdditionalDelay = areaSkillSettings.ExplicitTargetAdditionalHitDelay;
        if (explicitTargetAdditionalDelay < TimeSpan.Zero)
        {
            explicitTargetAdditionalDelay = TimeSpan.Zero;
        }

        // Order targets by distance to process nearest targets first
        var orderedTargets = targets.ToList();
        FrustumBasedTargetFilter? filter = null;
        var projectileCount = 1;
        var attackRounds = areaSkillSettings.MaximumNumberOfHitsPerTarget;

        if (areaSkillSettings is { UseFrustumFilter: true, ProjectileCount: > 1 })
        {
            orderedTargets.Sort((a, b) => player.GetDistanceTo(a).CompareTo(player.GetDistanceTo(b)));
            filter = FrustumFilters.GetOrAdd(areaSkillSettings, static s => new FrustumBasedTargetFilter(s.FrustumStartWidth, s.FrustumEndWidth, s.FrustumDistance, s.ProjectileCount));
            projectileCount = areaSkillSettings.ProjectileCount;
            attackRounds = 1; // One attack round per projectile

            extraTarget = orderedTargets.FirstOrDefault(t => t.Id == extraTargetId);
            if (extraTarget is not null)
            {
                // In this case we just calculate the angle on server side, so that lags
                // or desynced positions may not have such a big impact
                var angle = (float)player.Position.GetAngleDegreeTo(extraTarget.Position);
                rotation = (byte)((angle / 360.0f) * 256.0f);
            }
        }

        orderedTargets = ApplyRandomTargetSelection(orderedTargets, areaSkillSettings);

        // Process each projectile separately
        for (int projectileIndex = 0; projectileIndex < projectileCount; projectileIndex++)
        {
            if (attackCount >= maxAttacks)
            {
                break;
            }

            for (int attackRound = 0; attackRound < attackRounds; attackRound++)
            {
                if (attackCount >= maxAttacks)
                {
                    break;
                }

                foreach (var target in orderedTargets)
                {
                    if (attackCount >= maxAttacks)
                    {
                        break;
                    }

                    if (target.Id == extraTargetId)
                    {
                        extraTarget = target;
                    }

                    // Skip targets that have died in previous rounds
                    if (!target.IsAlive)
                    {
                        continue;
                    }

                    // For multiple projectiles, check if this specific projectile can hit the target
                    if (filter != null && !filter.IsTargetWithinBounds(player, target, rotation, projectileIndex))
                    {
                        continue; // This projectile cannot hit this target
                    }

                    var hitChance = attackRound < areaSkillSettings.MinimumNumberOfHitsPerTarget
                        ? 1.0
                        : Math.Min(areaSkillSettings.HitChancePerDistanceMultiplier, Math.Pow(areaSkillSettings.HitChancePerDistanceMultiplier, player.GetDistanceTo(target)));
                    if (hitChance < 1.0 && !Rand.NextRandomBool(hitChance))
                    {
                        continue;
                    }

                    var distanceDelay = areaSkillSettings.DelayPerOneDistance * player.GetDistanceTo(target);
                    var randomDelay = TimeSpan.Zero;
                    if (randomDelayMaximum > TimeSpan.Zero)
                    {
                        var minMs = (int)Math.Round(randomDelayMinimum.TotalMilliseconds);
                        var maxMs = (int)Math.Round(randomDelayMaximum.TotalMilliseconds);
                        if (maxMs > minMs)
                        {
                            randomDelay = TimeSpan.FromMilliseconds(Rand.NextInt(minMs, maxMs + 1));
                        }
                        else if (minMs > 0)
                        {
                            randomDelay = TimeSpan.FromMilliseconds(minMs);
                        }
                    }

                    var attackDelay = currentDelay + distanceDelay + delayPerHit + randomDelay;
                    attackCount++;

                    if (attackDelay == TimeSpan.Zero)
                    {
                        // Check if target is still alive and in valid state before attacking
                        if (!target.IsAtSafezone() && target.IsActive())
                        {
                            await this.ApplySkillAsync(player, skillEntry, target, targetAreaCenter, isCombo).ConfigureAwait(false);
                            anyHit = true;
                        }
                    }
                    else
                    {
                        // The most pragmatic approach is just spawning a Task for each hit.
                        // We have to see, how this works out in terms of performance.
                        anyHit = true;
                        _ = Task.Run(async () =>
                        {
                            await Task.Delay(attackDelay).ConfigureAwait(false);
                            if (!target.IsAtSafezone() && target.IsActive())
                            {
                                await this.ApplySkillAsync(player, skillEntry, target, targetAreaCenter, isCombo).ConfigureAwait(false);
                            }
                        });
                    }

                    if (explicitTargetAdditionalHits > 0 && target.Id == extraTargetId)
                    {
                        for (int i = 0; i < explicitTargetAdditionalHits; i++)
                        {
                            if (explicitTargetAdditionalDelay == TimeSpan.Zero)
                            {
                                if (!target.IsAtSafezone() && target.IsActive())
                                {
                                    await this.ApplySkillAsync(player, skillEntry, target, targetAreaCenter, isCombo).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                anyHit = true;
                                _ = Task.Run(async () =>
                                {
                                    await Task.Delay(explicitTargetAdditionalDelay).ConfigureAwait(false);
                                    if (!target.IsAtSafezone() && target.IsActive())
                                    {
                                        await this.ApplySkillAsync(player, skillEntry, target, targetAreaCenter, isCombo).ConfigureAwait(false);
                                    }
                                });
                            }
                        }
                    }
                }

                currentDelay += areaSkillSettings.DelayBetweenHits;
            }
        }

        return (extraTarget, anyHit);
    }

    private static void ApplyElectricSpikePartyPenalty(Player player)
    {
        if (player.Party is null || player.CurrentMap is null)
        {
            return;
        }

        foreach (var partyMember in player.Party.PartyList.OfType<Player>())
        {
            if (partyMember == player || partyMember.CurrentMap != player.CurrentMap)
            {
                continue;
            }

            if (Math.Floor(player.GetDistanceTo(partyMember.Position)) >= ElectricSpikePartyRange)
            {
                continue;
            }

            if (partyMember.Attributes is not { } attributes)
            {
                continue;
            }

            var healthLoss = attributes[Stats.CurrentHealth] * ElectricSpikePartyHealthLossFactor;
            var manaLoss = attributes[Stats.CurrentMana] * ElectricSpikePartyManaLossFactor;
            attributes[Stats.CurrentHealth] = Math.Max(0, attributes[Stats.CurrentHealth] - healthLoss);
            attributes[Stats.CurrentMana] = Math.Max(0, attributes[Stats.CurrentMana] - manaLoss);
        }
    }

    private static List<IAttackable> ApplyRandomTargetSelection(List<IAttackable> orderedTargets, AreaSkillSettings areaSkillSettings)
    {
        var maximumTargets = areaSkillSettings.MaximumTargets;
        var guaranteedTargets = areaSkillSettings.GuaranteedTargets;
        var additionalChance = areaSkillSettings.AdditionalTargetChance;

        if (maximumTargets <= 0
            && guaranteedTargets <= 0
            && additionalChance >= 1.0f)
        {
            return orderedTargets;
        }

        if (maximumTargets <= 0)
        {
            maximumTargets = orderedTargets.Count;
        }

        if (guaranteedTargets < 0)
        {
            guaranteedTargets = 0;
        }

        if (maximumTargets < guaranteedTargets)
        {
            maximumTargets = guaranteedTargets;
        }

        if (additionalChance < 0)
        {
            additionalChance = 0;
        }
        else if (additionalChance > 1)
        {
            additionalChance = 1;
        }

        var selectedTargets = new List<IAttackable>(Math.Min(maximumTargets, orderedTargets.Count));
        for (int i = 0; i < orderedTargets.Count && i < maximumTargets; i++)
        {
            var shouldHit = i < guaranteedTargets;
            if (!shouldHit && Rand.NextRandomBool(additionalChance))
            {
                shouldHit = true;
            }

            if (shouldHit)
            {
                selectedTargets.Add(orderedTargets[i]);
            }
        }

        return selectedTargets;
    }

    private async ValueTask ApplySkillAsync(Player player, SkillEntry skillEntry, IAttackable target, Point targetAreaCenter, bool isCombo)
    {
        skillEntry.ThrowNotInitializedProperty(skillEntry.Skill is null, nameof(skillEntry.Skill));

        if (skillEntry.Skill.SkillType == SkillType.Buff)
        {
            await target.ApplyMagicEffectAsync(player, skillEntry).ConfigureAwait(false);
            return;
        }

        var hitInfo = await target.AttackByAsync(player, skillEntry, isCombo).ConfigureAwait(false);
        await target.TryApplyElementalEffectsAsync(player, skillEntry).ConfigureAwait(false);
        var baseSkill = skillEntry.GetBaseSkill();

        if (player.GameContext.PlugInManager.GetStrategy<short, IAreaSkillPlugIn>(baseSkill.Number) is { } strategy)
        {
            await strategy.AfterTargetGotAttackedAsync(player, target, skillEntry, targetAreaCenter, hitInfo).ConfigureAwait(false);
        }
    }
}
