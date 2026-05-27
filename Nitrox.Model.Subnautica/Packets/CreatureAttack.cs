using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class CreatureAttack : Packet
{
    public NitroxId CreatureId { get; }
    public SessionId TargetSessionId { get; }
    public float Damage { get; }

    public CreatureAttack(NitroxId creatureId, SessionId targetSessionId, float damage)
    {
        CreatureId = creatureId;
        TargetSessionId = targetSessionId;
        Damage = damage;
    }
}
