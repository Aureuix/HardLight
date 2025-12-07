using Content.Shared.Lizards.Components;
using Robust.Shared.Map;

namespace Content.Server.Lizards.Systems;

public sealed class LizardSegmentsSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<LizardSegmentsComponent, ComponentStartup>(OnHeadStartup);
    }

    private void OnHeadStartup(Entity<LizardSegmentsComponent> ent, ref ComponentStartup args)
    {
        var xform = Transform(ent.Owner);

        var body = EntityManager.SpawnEntity(ent.Comp.BodyPrototype, xform.Coordinates);
        var body2 = EntityManager.SpawnEntity(ent.Comp.Body2Prototype, xform.Coordinates);
        var tail = EntityManager.SpawnEntity(ent.Comp.TailPrototype, xform.Coordinates);

        if (TryComp<TrailFollowerComponent>(body, out var bodyFollow))
        {
            bodyFollow.Leader = ent.Owner;
            Dirty(body, bodyFollow);
        }

        if (TryComp<TrailFollowerComponent>(body2, out var body2Follow))
        {
            body2Follow.Leader = body;
            Dirty(body2, body2Follow);
        }

        if (TryComp<TrailFollowerComponent>(tail, out var tailFollow))
        {
            tailFollow.Leader = body2;
            Dirty(tail, tailFollow);
        }
    }
}
