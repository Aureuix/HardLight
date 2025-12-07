using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Lizards.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class LizardSegmentsComponent : Component
{
    public string BodyPrototype = "HL_LizardBody";
    public string Body2Prototype = "HL_LizardBody2";
    public string TailPrototype = "HL_LizardTailTip";
}
