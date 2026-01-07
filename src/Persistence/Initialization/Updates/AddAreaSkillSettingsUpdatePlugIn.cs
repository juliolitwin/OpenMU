// <copyright file="AddAreaSkillSettingsUpdatePlugIn.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Updates;

using System.Runtime.InteropServices;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.Persistence.Initialization.Skills;
using MUnique.OpenMU.PlugIns;

/// <summary>
/// This adds the items required to enter the kalima map.
/// </summary>
[PlugIn(PlugInName, PlugInDescription)]
[Guid("D01DA745-BF72-40C4-BD90-D2D637AEDF99")]
public class AddAreaSkillSettingsUpdatePlugIn : UpdatePlugInBase
{
    /// <summary>
    /// The plug in name.
    /// </summary>
    internal const string PlugInName = "Add Area Skill Settings";

    /// <summary>
    /// The plug in description.
    /// </summary>
    internal const string PlugInDescription = "Adds the new area skill settings for skills like evil spirit, etc. to make them work properly again.";

    /// <inheritdoc />
    public override UpdateVersion Version => UpdateVersion.AddAreaSkillSettings;

    /// <inheritdoc />
    public override string DataInitializationKey => VersionSeasonSix.DataInitialization.Id;

    /// <inheritdoc />
    public override string Name => PlugInName;

    /// <inheritdoc />
    public override string Description => PlugInDescription;

    /// <inheritdoc />
    public override bool IsMandatory => false;

    /// <inheritdoc />
    public override DateTime CreatedAt => new(2024, 10, 25, 19, 0, 0, DateTimeKind.Utc);

    /// <inheritdoc />
    protected override async ValueTask ApplyAsync(IContext context, GameConfiguration gameConfiguration)
    {
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.Flame, false, default, default, default, true, TimeSpan.Zero, TimeSpan.FromMilliseconds(500), 0, 2, default, 0.5f, targetAreaDiameter: 2, useTargetAreaFilter: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.Twister, true, 1.5f, 1.5f, 4f, true, TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(1000), 0, 2, default, 0.7f, useCasterAsCenter: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.EvilSpirit, false, default, default, default, true, TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(1000), 0, 2, default, 0.7f, newRange: 7);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.AquaBeam, true, 1.5f, 1.5f, 8f, useCasterAsCenter: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.Cometfall, false, default, default, default, targetAreaDiameter: 2, useTargetAreaFilter: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.TripleShot, true, 1f, 4.5f, 7f, true, TimeSpan.FromMilliseconds(50), maximumHitsPerTarget: 3, maximumHitsPerAttack: 3, projectileCount: 3, useCasterAsCenter: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.Decay, false, default, default, default, useTargetAreaFilter: true, targetAreaDiameter: 8, randomDelayPerHitMaximum: TimeSpan.FromMilliseconds(500));
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.IceStorm, false, default, default, default, useTargetAreaFilter: true, targetAreaDiameter: 8);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.Penetration, true, 1.1f, 1.2f, 8f, useDeferredHits: true, delayPerOneDistance: TimeSpan.FromMilliseconds(50), useCasterAsCenter: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.FireSlash, true, 1.5f, 2, 2, useCasterAsCenter: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.PowerSlash, true, 1.0f, 6.0f, 6.0f, guaranteedTargets: 5, maximumTargets: 10, additionalTargetChance: 0.5f, useCasterAsCenter: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.Earthshake, false, 0, 0, 0, newRange: 5, useCasterAsCenter: true, useEuclideanRange: true, delayPerHit: TimeSpan.FromMilliseconds(500));
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.ElectricSpike, true, 1.5f, 1.5f, 12f, useCasterAsCenter: true, delayPerHit: TimeSpan.FromMilliseconds(500));
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.ForceWave, true, 1f, 1f, 4f, useCasterAsCenter: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.Stun, true, 1.5f, 1.5f, 4f, newRange: 4, skillTypeOverride: SkillType.Buff, useCasterAsCenter: true, useEuclideanRange: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.DrainLife, false, 0, 0, 0, skillTypeOverride: null, delayPerHit: TimeSpan.FromMilliseconds(700));
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.ChainLightning, false, 0, 0, 0, skillTypeOverride: null, delayPerHit: TimeSpan.FromMilliseconds(200));
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.Explosion223, false, 0, 0, 0, useTargetAreaFilter: true, targetAreaDiameter: 4, useTargetAreaSquare: true, requireTargetAreaCenterInRange: true, delayPerHit: TimeSpan.FromMilliseconds(1000));
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.Requiem, false, 0, 0, 0, useTargetAreaFilter: true, targetAreaDiameter: 4, useTargetAreaSquare: true, requireTargetAreaCenterInRange: true, delayPerHit: TimeSpan.FromMilliseconds(1000));
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.Pollution, false, 0, 0, 0, useTargetAreaFilter: true, targetAreaDiameter: 6, guaranteedTargets: 4, maximumTargets: 8, additionalTargetChance: 0.5f, useTargetAreaSquare: true, requireTargetAreaCenterInRange: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.LightningShock, false, 0, 0, 0, maximumHitsPerAttack: 12, guaranteedTargets: 5, maximumTargets: 12, additionalTargetChance: 0.5f, useCasterAsCenter: true, useEuclideanRange: true, delayPerHit: TimeSpan.FromMilliseconds(250));
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.StrikeofDestruction, false, 0, 0, 0, useTargetAreaFilter: true, targetAreaDiameter: 6, guaranteedTargets: 4, maximumTargets: 8, additionalTargetChance: 0.5f, useTargetAreaSquare: true, requireTargetAreaCenterInRange: true, delayPerHit: TimeSpan.FromMilliseconds(500));
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.PlasmaStorm, false, 0, 0, 0, maximumHitsPerAttack: 5, useCasterAsCenter: true, useEuclideanRange: true, delayPerHit: TimeSpan.FromMilliseconds(300));
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.TwistingSlash, false, 0, 0, 0, newRange: 3, useCasterAsCenter: true, useEuclideanRange: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.RagefulBlow, false, 0, 0, 0, newRange: 4, useCasterAsCenter: true, useEuclideanRange: true, delayPerHit: TimeSpan.FromMilliseconds(500));
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.DragonRoar, false, 0, 0, 0, minimumHitsPerTarget: 4, maximumHitsPerTarget: 4, guaranteedTargets: 4, maximumTargets: 8, additionalTargetChance: 0.5f, useTargetAreaFilter: true, targetAreaDiameter: 6, useTargetAreaSquare: true, requireTargetAreaCenterInRange: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.PhoenixShot, false, 0, 0, 0, skillTypeOverride: SkillType.AreaSkillAutomaticHits, minimumHitsPerTarget: 4, maximumHitsPerTarget: 4, guaranteedTargets: 4, maximumTargets: 8, additionalTargetChance: 0.5f, useTargetAreaFilter: true, targetAreaDiameter: 4, useTargetAreaSquare: true, requireTargetAreaCenterInRange: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.MultiShot, true, 1f, 6f, 7f, projectileCount: 5, useCasterAsCenter: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.FlameStrike, true, 5f, 2f, 4f, minimumHitsPerTarget: 2, maximumHitsPerTarget: 2, guaranteedTargets: 8, additionalTargetChance: 0.5f, useCasterAsCenter: true);
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.GiganticStorm, false, 0, 0, 0, useTargetAreaFilter: true, targetAreaDiameter: 12, guaranteedTargets: 8, maximumTargets: 12, additionalTargetChance: 0.5f, useTargetAreaSquare: true, requireTargetAreaCenterInRange: true, delayPerHit: TimeSpan.FromMilliseconds(400));
        this.AddAreaSkillSettings(gameConfiguration, context, SkillNumber.ChaoticDiseier, true, 1.5f, 1.5f, 6f, guaranteedTargets: 8, additionalTargetChance: 0.5f, delayPerHit: TimeSpan.FromMilliseconds(200), explicitTargetAdditionalHits: 1, explicitTargetAdditionalHitDelay: TimeSpan.FromMilliseconds(300), useCasterAsCenter: true);

        if (gameConfiguration.Skills.FirstOrDefault(s => s.Number == (short)SkillNumber.FireScream) is { } fireScream)
        {
            fireScream.SkillType = SkillType.DirectHit;
            fireScream.AreaSkillSettings = null;
        }

        foreach (var skill in gameConfiguration.Skills.Where(s => s.MasterDefinition?.ReplacedSkill?.Number == (short)SkillNumber.FireScream))
        {
            skill.SkillType = SkillType.DirectHit;
            skill.AreaSkillSettings = null;
        }

        // Fix master skills as well:
        foreach (var skill in gameConfiguration.Skills.OrderBy(s => s.Number))
        {
            var replacedSkill = skill.MasterDefinition?.ReplacedSkill;
            if (replacedSkill?.AreaSkillSettings is not { } areaSkillSettings)
            {
                continue;
            }

            skill.AreaSkillSettings = context.CreateNew<AreaSkillSettings>();
            var id = skill.AreaSkillSettings.GetId();
            skill.AreaSkillSettings.AssignValuesOf(areaSkillSettings, gameConfiguration);
            skill.AreaSkillSettings.SetGuid(id);
        }
    }

    private void AddAreaSkillSettings(
        GameConfiguration gameConfiguration,
        IContext context,
        SkillNumber skillNumber,
        bool useFrustumFilter,
        float frustumStartWidth,
        float frustumEndWidth,
        float frustumDistance,
        bool useDeferredHits = false,
        TimeSpan delayPerOneDistance = default,
        TimeSpan delayBetweenHits = default,
        int minimumHitsPerTarget = 1,
        int maximumHitsPerTarget = 1,
        int maximumHitsPerAttack = default,
        float hitChancePerDistanceMultiplier = 1.0f,
        bool useTargetAreaFilter = false,
        float targetAreaDiameter = default,
        int projectileCount = 1,
        short? newRange = null,
        SkillType? skillTypeOverride = SkillType.AreaSkillAutomaticHits,
        int guaranteedTargets = 0,
        int maximumTargets = 0,
        float additionalTargetChance = 1.0f,
        bool useCasterAsCenter = false,
        bool useTargetAreaSquare = false,
        bool requireTargetAreaCenterInRange = false,
        bool useEuclideanRange = false,
        TimeSpan delayPerHit = default,
        TimeSpan randomDelayPerHitMinimum = default,
        TimeSpan randomDelayPerHitMaximum = default,
        int explicitTargetAdditionalHits = 0,
        TimeSpan explicitTargetAdditionalHitDelay = default)
    {
        var skill = gameConfiguration.Skills.First(s => s.Number == (short)skillNumber);
        var areaSkillSettings = context.CreateNew<AreaSkillSettings>();
        skill.AreaSkillSettings = areaSkillSettings;
        if (skillTypeOverride.HasValue)
        {
            skill.SkillType = skillTypeOverride.Value;
        }

        if (newRange.HasValue)
        {
            skill.Range = newRange.Value;
        }

        areaSkillSettings.UseFrustumFilter = useFrustumFilter;
        areaSkillSettings.FrustumStartWidth = frustumStartWidth;
        areaSkillSettings.FrustumEndWidth = frustumEndWidth;
        areaSkillSettings.FrustumDistance = frustumDistance;
        areaSkillSettings.UseTargetAreaFilter = useTargetAreaFilter;
        areaSkillSettings.TargetAreaDiameter = targetAreaDiameter;
        areaSkillSettings.UseTargetAreaSquare = useTargetAreaSquare;
        areaSkillSettings.UseDeferredHits = useDeferredHits;
        areaSkillSettings.DelayPerOneDistance = delayPerOneDistance;
        areaSkillSettings.DelayBetweenHits = delayBetweenHits;
        areaSkillSettings.DelayPerHit = delayPerHit;
        areaSkillSettings.RandomDelayPerHitMinimum = randomDelayPerHitMinimum;
        areaSkillSettings.RandomDelayPerHitMaximum = randomDelayPerHitMaximum;
        areaSkillSettings.MinimumNumberOfHitsPerTarget = minimumHitsPerTarget;
        areaSkillSettings.MaximumNumberOfHitsPerTarget = maximumHitsPerTarget;
        areaSkillSettings.MaximumNumberOfHitsPerAttack = maximumHitsPerAttack;
        areaSkillSettings.HitChancePerDistanceMultiplier = hitChancePerDistanceMultiplier;
        areaSkillSettings.ProjectileCount = projectileCount;
        areaSkillSettings.GuaranteedTargets = guaranteedTargets;
        areaSkillSettings.MaximumTargets = maximumTargets;
        areaSkillSettings.AdditionalTargetChance = additionalTargetChance;
        areaSkillSettings.UseCasterAsCenter = useCasterAsCenter;
        areaSkillSettings.RequireTargetAreaCenterInRange = requireTargetAreaCenterInRange;
        areaSkillSettings.UseEuclideanRange = useEuclideanRange;
        areaSkillSettings.ExplicitTargetAdditionalHits = explicitTargetAdditionalHits;
        areaSkillSettings.ExplicitTargetAdditionalHitDelay = explicitTargetAdditionalHitDelay;
    }
}
