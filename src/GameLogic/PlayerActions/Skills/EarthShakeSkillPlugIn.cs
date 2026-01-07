// <copyright file="EarthShakeSkillPlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.GameLogic.PlayerActions.Skills;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;
using MUnique.OpenMU.GameLogic.PlugIns;
using MUnique.OpenMU.GameLogic.NPC;
using MUnique.OpenMU.Pathfinding;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// Handles the earth shake skill of the dark horse. Pushes the targets away from the attacker.
/// </summary>
[Guid("5D00F012-B0A3-41D6-B2FD-66D37A81615C")]
[PlugIn("Earth shake skill", "Handles the earth shake skill of the dark horse.")]
public class EarthShakeSkillPlugIn : IAreaSkillPlugIn
{
    private const int KnockbackSteps = 3;
    private static readonly HashSet<int> KnockbackBlockedMonsterNumbers =
    [
        131, 132, 133, 134,
        215, 216, 217, 218, 219,
        275,
        277, 278, 283, 286, 287, 288,
        348,
        459, 460, 461, 462,
    ];

    /// <inheritdoc />
    public short Key => 62;

    /// <inheritdoc />
    public async ValueTask AfterTargetGotAttackedAsync(IAttacker attacker, IAttackable target, SkillEntry skillEntry, Point targetAreaCenter, HitInfo? hitInfo)
    {
        if (!target.IsAlive
            || target.Attributes is not { } attributes
            || attributes[Stats.IsFrozen] > 0
            || attributes[Stats.IsStunned] > 0
            || IsKnockbackBlocked(target)
            || target is not IMovable movableTarget
            || target.CurrentMap is not { } currentMap)
        {
            return;
        }

        var startingPoint = attacker.Position;
        var currentTarget = target.Position;
        var direction = startingPoint.GetDirectionTo(currentTarget);
        if (direction == Direction.Undefined)
        {
            direction = (Direction)Rand.NextInt(1, 9);
        }

        for (int step = 0; step < KnockbackSteps; step++)
        {
            var nextTarget = currentTarget.CalculateTargetPoint(direction);
            if (!currentMap.Terrain.WalkMap[nextTarget.X, nextTarget.Y]
                || (target is NonPlayerCharacter && target.CurrentMap.Terrain.SafezoneMap[nextTarget.X, nextTarget.Y]))
            {
                break;
            }

            currentTarget = nextTarget;
        }

        if (currentTarget != target.Position)
        {
            await movableTarget.MoveAsync(currentTarget).ConfigureAwait(false);
        }
    }

    private static bool IsKnockbackBlocked(IAttackable target)
    {
        if (target is NonPlayerCharacter npc)
        {
            if (npc.Definition.ObjectKind != NpcObjectKind.Monster)
            {
                return true;
            }

            if (KnockbackBlockedMonsterNumbers.Contains(npc.Definition.Number))
            {
                return true;
            }
        }

        return false;
    }
}
