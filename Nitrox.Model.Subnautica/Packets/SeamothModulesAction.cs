using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.Packets
{
    [Serializable]
    public class SeamothModulesAction : Packet
    {
        public NitroxTechType TechType { get; }
        public int SlotID { get; }
        public NitroxId Id { get; }
        public NitroxVector3 Forward { get; }
        public NitroxQuaternion Rotation { get; }

        // The firer's module charge (held-button charge). Slot charging is local input only and never networked, so
        // observers must use these transmitted values rather than their always-zero local copy of the remote SeaMoth.
        public float Charge { get; }
        public float ChargeScalar { get; }

        public SeamothModulesAction(NitroxTechType techType, int slotID, NitroxId id, NitroxVector3 forward, NitroxQuaternion rotation, float charge, float chargeScalar)
        {
            TechType = techType;
            SlotID = slotID;
            Id = id;
            Forward = forward;
            Rotation = rotation;
            Charge = charge;
            ChargeScalar = chargeScalar;
        }
    }
}
