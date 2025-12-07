using Content.Shared.Lizards.Components;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Robust.Shared.Physics.Components;
using System;
using System.Numerics;
using System.Linq;
using Robust.Shared.Maths;

namespace Content.Server.Lizards.Systems;

public sealed class SpriteTrailServerSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xformSys = default!;

    private readonly Dictionary<EntityUid, Queue<(EntityCoordinates, Angle)>> _buffers = new();
    private readonly Dictionary<EntityUid, bool> _leaderActive = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<TrailLeaderComponent, ComponentStartup>(OnLeaderStartup);
        SubscribeLocalEvent<TrailLeaderComponent, ComponentShutdown>(OnLeaderShutdown);
    }

    private void OnLeaderStartup(Entity<TrailLeaderComponent> ent, ref ComponentStartup args)
    {
        _buffers[ent.Owner] = new Queue<(EntityCoordinates, Angle)>(ent.Comp.BufferSize);
        _leaderActive[ent.Owner] = false;
    }

    private void OnLeaderShutdown(Entity<TrailLeaderComponent> ent, ref ComponentShutdown args)
    {
        _buffers.Remove(ent.Owner);
        _leaderActive.Remove(ent.Owner);
    }

    // Followers don't need a startup subscription here; linking handled elsewhere.

    public override void Update(float frameTime)
    {
        var qLead = EntityQueryEnumerator<TrailLeaderComponent, TransformComponent>();
        while (qLead.MoveNext(out var uid, out var leader, out var xform))
        {
            if (!_buffers.TryGetValue(uid, out var buf))
                continue;
            // Only enqueue if moved a small threshold to reduce stacking
            var coords = xform.Coordinates;
            var rot = xform.LocalRotation;
            var shouldEnqueue = true;
            if (buf.Count > 0)
            {
                var last = buf.Last();
                var lastMap = last.Item1.ToMap(EntityManager, _xformSys);
                var currMap = coords.ToMap(EntityManager, _xformSys);
                if (Vector2.Distance(lastMap.Position, currMap.Position) < 0.02f && Math.Abs((rot - last.Item2).Theta) < 0.01f)
                    shouldEnqueue = false;
            }

            if (shouldEnqueue)
            {
                if (buf.Count >= leader.BufferSize)
                    buf.Dequeue();
                buf.Enqueue((coords, rot));
                _leaderActive[uid] = true;
            }
            else
            {
                // Head is effectively stationary: clear buffer so followers stop immediately
                _leaderActive[uid] = false;
            }
        }

        var qFollow = EntityQueryEnumerator<TrailFollowerComponent, TransformComponent>();
        while (qFollow.MoveNext(out var uid, out var follower, out var xform))
        {
            if (follower.Leader == default || !_buffers.TryGetValue(follower.Leader, out var buf))
                continue;
            var leaderIsActive = _leaderActive.TryGetValue(follower.Leader, out var active) && active;
            // If we don't have enough history yet, fallback to leader's current transform
            (EntityCoordinates, Angle) target;
            if (buf.Count < follower.Delay)
            {
                var lxf = Transform(follower.Leader);
                target = (lxf.Coordinates, lxf.LocalRotation);
            }
            else
            {
                var arr = buf.ToArray();
                target = arr[arr.Length - follower.Delay];
            }
            // Use target position directly (no offset) so segments don't drift diagonally
            var targetWorld = target.Item1.ToMap(EntityManager, _xformSys);
            if (!leaderIsActive || Vector2.Distance(xform.WorldPosition, targetWorld.Position) < 0.001f)
            {
                // Even when stationary, align rotation toward the leader's position
                var dir = targetWorld.Position - xform.WorldPosition;
                // Apply sprite forward offset so sprites aren't sideways
                const float spriteForwardOffsetDeg = -90f;
                var desiredRot = dir.LengthSquared() > 0f ? Angle.FromDegrees((float)(Math.Atan2(dir.Y, dir.X) * 180.0 / Math.PI) + spriteForwardOffsetDeg) : target.Item2;
                _xformSys.SetLocalRotation(uid, desiredRot);
                continue;
            }

            // Smoothing: interpolate current position towards target
            if (follower.SmoothFactor > 0f)
            {
                var currWorld = xform.WorldPosition;
                var newPos = Vector2.Lerp(currWorld, targetWorld.Position, Math.Clamp(follower.SmoothFactor, 0f, 1f));
                _xformSys.SetWorldPosition(uid, newPos);
            }
            else
            {
                _xformSys.SetCoordinates(uid, target.Item1);
            }
            // Smoothly rotate to face the leader/target direction
            var dir2 = targetWorld.Position - _xformSys.GetWorldPosition(uid);
            const float spriteForwardOffsetDeg2 = -90f;
            var desired = dir2.LengthSquared() > 0f ? Angle.FromDegrees((float)(Math.Atan2(dir2.Y, dir2.X) * 180.0 / Math.PI) + spriteForwardOffsetDeg2) : target.Item2;
            if (follower.SmoothFactor > 0f)
            {
                var curr = xform.LocalRotation;
                var newRotTheta = Robust.Shared.Maths.MathHelper.Lerp(curr.Theta, desired.Theta, Math.Clamp(follower.SmoothFactor, 0f, 1f));
                _xformSys.SetLocalRotation(uid, Angle.FromDegrees((float)(newRotTheta * 180.0 / Math.PI)));
            }
            else
            {
                _xformSys.SetLocalRotation(uid, desired);
            }
        }
    }
}
