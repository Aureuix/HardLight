using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(Events.DynamicJobAllocationRule))]
public sealed partial class DynamicJobAllocationRuleComponent : Component
{
    /// <summary>
    /// How often to check and adjust job slots (in seconds)
    /// </summary>
    [DataField("checkInterval")]
    public float CheckInterval = 600f; // 10 minutes

    /// <summary>
    /// Time since last check
    /// </summary>
    public float TimeSinceLastCheck = 0f;

    /// <summary>
    /// Contractor job ID
    /// </summary>
    [DataField("contractorJob")]
    public ProtoId<JobPrototype> ContractorJob = "Contractor";

    /// <summary>
    /// Mercenary job ID
    /// </summary>
    [DataField("mercenaryJob")]
    public ProtoId<JobPrototype> MercenaryJob = "Mercenary";

    /// <summary>
    /// Pilot job ID
    /// </summary>
    [DataField("pilotJob")]
    public ProtoId<JobPrototype> PilotJob = "Pilot";

    /// <summary>
    /// Percentage of players that should be contractors (0.25 = 25%)
    /// </summary>
    [DataField("contractorPercentage")]
    public float ContractorPercentage = 0.25f;

    /// <summary>
    /// Percentage of players that should be pilots (0.05 = 5%)
    /// </summary>
    [DataField("pilotPercentage")]
    public float PilotPercentage = 0.05f;

    /// <summary>
    /// Percentage of players that should be mercenaries (0.20 = 20%)
    /// </summary>
    [DataField("mercenaryPercentage")]
    public float MercenaryPercentage = 0.20f;

    /// <summary>
    /// Maximum number of contractor slots
    /// </summary>
    [DataField("contractorCap")]
    public int ContractorCap = 25;

    /// <summary>
    /// Maximum number of pilot slots
    /// </summary>
    [DataField("pilotCap")]
    public int PilotCap = 5;

    /// <summary>
    /// Maximum number of mercenary slots
    /// </summary>
    [DataField("mercenaryCap")]
    public int MercenaryCap = 20;
}
