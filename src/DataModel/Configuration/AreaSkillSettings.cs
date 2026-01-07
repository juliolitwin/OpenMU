// <copyright file="AreaSkillSettings.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.DataModel.Configuration;

using MUnique.OpenMU.Annotations;

/// <summary>
/// Settings for area skills.
/// </summary>
[Cloneable]
public partial class AreaSkillSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether to use a frustum to filter potential targets.
    /// </summary>
    public bool UseFrustumFilter { get; set; }

    /// <summary>
    /// Gets or sets the width of the frustum at the start.
    /// </summary>
    public float FrustumStartWidth { get; set; }

    /// <summary>
    /// Gets or sets the width of the frustum at the end.
    /// </summary>
    public float FrustumEndWidth { get; set; }

    /// <summary>
    /// Gets or sets the distance.
    /// </summary>
    public float FrustumDistance { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to consider the target area coordinate to filter potential targets.
    /// </summary>
    public bool UseTargetAreaFilter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a square target area is used instead of a circular one.
    /// </summary>
    public bool UseTargetAreaSquare { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the target area center should be the caster position.
    /// </summary>
    public bool UseCasterAsCenter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the target area center must be within skill range of the caster.
    /// </summary>
    public bool RequireTargetAreaCenterInRange { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to enforce euclidean range checks when selecting targets.
    /// </summary>
    public bool UseEuclideanRange { get; set; }

    /// <summary>
    /// Gets or sets the target area diameter.
    /// </summary>
    public float TargetAreaDiameter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use deferred hits,
    /// for skills which take a while to visually arrive at the target.
    /// </summary>
    public bool UseDeferredHits { get; set; }

    /// <summary>
    /// Gets or sets the delay per one distance, when <see cref="UseDeferredHits"/> is active.
    /// </summary>
    public TimeSpan DelayPerOneDistance { get; set; }

    /// <summary>
    /// Gets or sets the delay between hits.
    /// </summary>
    public TimeSpan DelayBetweenHits { get; set; }

    /// <summary>
    /// Gets or sets the fixed delay which is applied to each hit.
    /// </summary>
    public TimeSpan DelayPerHit { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of hits per target.
    /// </summary>
    public int MinimumNumberOfHitsPerTarget { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of hits per target.
    /// </summary>
    public int MaximumNumberOfHitsPerTarget { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of hits per attack.
    /// </summary>
    public int MaximumNumberOfHitsPerAttack { get; set; }

    /// <summary>
    /// Gets or sets the number of targets which are always hit when random target selection is enabled.
    /// </summary>
    public int GuaranteedTargets { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of targets to consider for random selection.
    /// A value of 0 disables random target selection.
    /// </summary>
    public int MaximumTargets { get; set; }

    /// <summary>
    /// Gets or sets the chance between 0 and 1 to hit targets beyond <see cref="GuaranteedTargets"/>
    /// when random target selection is enabled.
    /// </summary>
    public float AdditionalTargetChance { get; set; } = 1.0f;

    /// <summary>
    /// Gets or sets the number of additional hits which are only applied to the explicit target.
    /// </summary>
    public int ExplicitTargetAdditionalHits { get; set; }

    /// <summary>
    /// Gets or sets the delay for additional hits on the explicit target.
    /// </summary>
    public TimeSpan ExplicitTargetAdditionalHitDelay { get; set; }

    /// <summary>
    /// Gets or sets the hit chance per distance multiplier.
    /// E.g. when set to 0.9 and the target is 5 steps away,
    /// the chance to hit is 5^0.9 = 0.59.
    /// </summary>
    public float HitChancePerDistanceMultiplier { get; set; }

    /// <summary>
    /// Gets or sets the number of projectiles/arrows that are fired.
    /// When greater than 1, the projectiles are evenly distributed within the frustum.
    /// Each target can only be hit by projectiles whose paths cross the target's position.
    /// Default is 1 (single projectile).
    /// </summary>
    public int ProjectileCount { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        return "Area Skill Settings";
    }
}
