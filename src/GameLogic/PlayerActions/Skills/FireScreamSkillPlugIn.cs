// <copyright file="FireScreamSkillPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Skills;

using System.Runtime.InteropServices;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.GameLogic.Views.World;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the fire scream skill of the dark lord, including the explosion effect.
/// </summary>
[PlugIn(nameof(FireScreamSkillPlugIn), "Handles the fire scream skill of the dark lord, including the explosion effect.")]
[Guid("30F0F3B7-6D5B-4E60-9002-0D80D2FBF97B")]
public class FireScreamSkillPlugIn : TargetedSkillPluginBase
{
    private const short FireScreamSkillNumber = 78;
    private const int ExplosionRange = 1;
    private const double ExplosionChance = 0.03;
    private const double ExplosionDamageFactor = 0.1;
    private static readonly TimeSpan ExplosionDelay = TimeSpan.FromMilliseconds(100);

    /// <inheritdoc />
    public override short Key => FireScreamSkillNumber;

    /// <inheritdoc />
    public override async ValueTask PerformSkillAsync(Player player, IAttackable target, ushort skillId)
    {
        using var loggerScope = player.Logger.BeginScope(this.GetType());

        if (target is null || player.Attributes is not { } attributes)
        {
            return;
        }

        if (attributes[Stats.IsStunned] > 0)
        {
            player.Logger.LogWarning($"Probably Hacker - player {player} is attacking in stunned state");
            return;
        }

        if (attributes[Stats.IsAsleep] > 0)
        {
            player.Logger.LogWarning($"Probably Hacker - player {player} is attacking in asleep state");
            return;
        }

        var skillEntry = player.SkillList?.GetSkill(skillId);
        var skill = skillEntry?.Skill;
        if (skill is null || skill.SkillType == SkillType.PassiveBoost)
        {
            return;
        }

        if (player.IsAtSafezone())
        {
            return;
        }

        if (!target.IsActive() || !target.CheckSkillTargetRestrictions(player, skill))
        {
            return;
        }

        if (!player.IsInRange(target.Position, skill.Range + 2))
        {
            if (target is not ISupportWalk { IsWalking: true })
            {
                await player.InvokeViewPlugInAsync<IObjectMovedPlugIn>(p => p.ObjectMovedAsync(target, MoveType.Instant)).ConfigureAwait(false);
            }

            return;
        }

        if (!await player.TryConsumeForSkillAsync(skill).ConfigureAwait(false))
        {
            return;
        }

        var isCombo = false;
        if (skill.SkillType is SkillType.DirectHit or SkillType.CastleSiegeSkill
            && player.ComboState is { } comboState
            && !target.IsAtSafezone()
            && !player.IsAtSafezone()
            && target.IsActive()
            && player.IsActive())
        {
            isCombo = await comboState.RegisterSkillAsync(skill).ConfigureAwait(false);
        }

        var hitInfo = await target.AttackByAsync(player, skillEntry!, isCombo).ConfigureAwait(false);
        player.LastAttackedTarget.SetTarget(target);
        var effectApplied = await target.TryApplyElementalEffectsAsync(player, skillEntry!).ConfigureAwait(false);

        await player.ForEachWorldObserverAsync<IShowSkillAnimationPlugIn>(p => p.ShowSkillAnimationAsync(player, target, skill, effectApplied), true).ConfigureAwait(false);

        if (hitInfo is not { } hit || (hit.HealthDamage + hit.ShieldDamage) == 0 || !Rand.NextRandomBool(ExplosionChance))
        {
            return;
        }

        await ApplyExplosionAsync(player, target, skillEntry).ConfigureAwait(false);
    }

    private static async ValueTask ApplyExplosionAsync(Player player, IAttackable target, SkillEntry skillEntry)
    {
        if (player.CurrentMap is not { } map)
        {
            return;
        }

        foreach (var candidate in map.GetAttackablesInRange(target.Position, ExplosionRange))
        {
            if (candidate == player
                || !candidate.IsAlive
                || candidate.IsAtSafezone())
            {
                continue;
            }

            if (candidate is not (Player or Monster))
            {
                continue;
            }

            if (!candidate.CheckSkillTargetRestrictions(player, skillEntry.Skill!))
            {
                continue;
            }

            if (target.GetDistanceTo(candidate.Position) > ExplosionRange)
            {
                continue;
            }

            _ = Task.Run(async () =>
            {
                await Task.Delay(ExplosionDelay).ConfigureAwait(false);
                await candidate.AttackByAsync(player, skillEntry, false, ExplosionDamageFactor).ConfigureAwait(false);
            });
        }
    }
}
