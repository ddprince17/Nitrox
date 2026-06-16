using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Helper;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxClient.MonoBehaviours;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync;

public sealed class EquippedItemInitialSyncProcessor : InitialSyncProcessor
{
    public EquippedItemInitialSyncProcessor()
    {
        AddDependency<PlayerInitialSyncProcessor>();
        AddDependency<RemotePlayerInitialSyncProcessor>();
        AddDependency<GlobalRootInitialSyncProcessor>();
    }

    public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
    {
        int totalEquippedItemsDone = 0;

        using (PacketSuppressor<EntitySpawnedByClient>.Suppress())
        {
            foreach (KeyValuePair<string, NitroxId> equippedItem in packet.EquippedItems)
            {
                string slot = equippedItem.Key;
                NitroxId id = equippedItem.Value;

                waitScreenItem.SetProgress(totalEquippedItemsDone, packet.EquippedItems.Count);

                if (!NitroxEntity.TryGetObjectFrom(id, out GameObject gameObject))
                {
                    // The equipped item entity failed to spawn (SpawnBatchAsync is best-effort and logs+continues
                    // on spawner errors); skip it instead of throwing and aborting the entire initial sync.
                    Log.Warn($"Equipped item {id} for slot {slot} was not spawned; skipping.");
                    totalEquippedItemsDone++;
                    continue;
                }
                Pickupable pickupable = gameObject.RequireComponent<Pickupable>();

                GameObject player = Player.mainObject;
                Optional<Equipment> opEquipment = EquipmentHelper.FindEquipmentComponent(player);

                if (opEquipment.HasValue)
                {
                    Equipment equipment = opEquipment.Value;
                    InventoryItem inventoryItem = new(pickupable);
                    inventoryItem.container = equipment;
                    inventoryItem.item.Reparent(equipment.tr);

                    Dictionary<string, InventoryItem> itemsBySlot = equipment.equipment;
                    itemsBySlot[slot] = inventoryItem;

                    equipment.UpdateCount(pickupable.GetTechType(), true);
                    Equipment.SendEquipmentEvent(pickupable, 0, player, slot);
                    equipment.NotifyEquip(slot, inventoryItem);
                }
                else
                {
                    Log.Info($"Could not find equipment type for {gameObject.name}");
                }

                totalEquippedItemsDone++;
                yield return null;
            }
        }

        Log.Info($"Recieved initial sync with {totalEquippedItemsDone} pieces of equipped items");
    }
}
