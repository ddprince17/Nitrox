using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class GrapplingHookMovementProcessor : IClientPacketProcessor<GrapplingHookMovement>
{
    public Task Process(ClientProcessorContext context, GrapplingHookMovement packet)
    {
        if (!NitroxEntity.TryGetComponentFrom(packet.ExosuitId, out Exosuit exosuit))
        {
            Log.Warn($"[{nameof(GrapplingHookMovementProcessor)}] Could not find Exosuit with id: {packet.ExosuitId}.");
            return Task.CompletedTask;
        }

        IExosuitArm arm = packet.ArmSide == Exosuit.Arm.Left ? exosuit.leftArm : exosuit.rightArm;

        if (arm is not ExosuitGrapplingArm grapplingArm)
        {
            Log.Error($"{packet.ArmSide} arm of exosuit {packet.ExosuitId} is not a grappling arm");
            return Task.CompletedTask;
        }

        if (grapplingArm.hook.resting)
        {
            grapplingArm.rope.LaunchHook(35);
        }

        Rigidbody rb = grapplingArm.hook.RequireComponent<Rigidbody>();

        rb.position = packet.Position.ToUnity();
        rb.velocity = packet.Velocity.ToUnity();
        rb.rotation = packet.Rotation.ToUnity();
        return Task.CompletedTask;
    }
}
