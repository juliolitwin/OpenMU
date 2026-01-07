// <copyright file="StunEffectInitializer.cs" company="MUnique">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MUnique.OpenMU.Persistence.Initialization.Skills;

using MUnique.OpenMU.AttributeSystem;
using MUnique.OpenMU.DataModel.Attributes;
using MUnique.OpenMU.DataModel.Configuration;
using MUnique.OpenMU.GameLogic.Attributes;

/// <summary>
/// Initializer for the stun effect.
/// </summary>
public class StunEffectInitializer : InitializerBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StunEffectInitializer"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="gameConfiguration">The game configuration.</param>
    public StunEffectInitializer(IContext context, GameConfiguration gameConfiguration)
        : base(context, gameConfiguration)
    {
    }

    /// <inheritdoc />
    public override void Initialize()
    {
        var magicEffect = this.Context.CreateNew<MagicEffectDefinition>();
        this.GameConfiguration.MagicEffects.Add(magicEffect);
        magicEffect.Number = (short)MagicEffectNumber.Stunned;
        magicEffect.Name = "Stun Effect";
        magicEffect.InformObservers = true;
        magicEffect.SendDuration = false;
        magicEffect.StopByDeath = true;

        magicEffect.Chance = this.Context.CreateNew<PowerUpDefinitionValue>();
        magicEffect.Chance.ConstantValue.Value = 1.0f;
        magicEffect.ChancePvp = this.Context.CreateNew<PowerUpDefinitionValue>();
        magicEffect.ChancePvp.ConstantValue.Value = 1.0f;

        magicEffect.Duration = this.Context.CreateNew<PowerUpDefinitionValue>();
        magicEffect.Duration.ConstantValue.Value = 10;
        magicEffect.DurationPvp = this.Context.CreateNew<PowerUpDefinitionValue>();
        magicEffect.DurationPvp.ConstantValue.Value = 10;

        var stunned = this.Context.CreateNew<PowerUpDefinition>();
        magicEffect.PowerUpDefinitions.Add(stunned);
        stunned.TargetAttribute = Stats.IsStunned.GetPersistent(this.GameConfiguration);
        stunned.Boost = this.Context.CreateNew<PowerUpDefinitionValue>();
        stunned.Boost.ConstantValue.Value = 1;
    }
}
