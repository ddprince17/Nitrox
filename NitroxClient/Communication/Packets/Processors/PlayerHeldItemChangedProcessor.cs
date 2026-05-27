using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PlayerHeldItemChangedProcessor : IClientPacketProcessor<PlayerHeldItemChanged>
{
    private readonly PlayerManager playerManager;
    private int defaultLayer;
    private int viewModelLayer;

    public PlayerHeldItemChangedProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;

        if (NitroxEnvironment.IsNormal)
        {
            SetupLayers();
        }
    }

    public Task Process(ClientProcessorContext context, PlayerHeldItemChanged packet)
    {
        if (!playerManager.TryFind(packet.SessionId, out RemotePlayer player))
        {
            Log.Warn($"[{nameof(PlayerHeldItemChangedProcessor)}] Could not find player with session id: {packet.SessionId}.");
            return Task.CompletedTask;
        }
        if (!NitroxEntity.TryGetObjectFrom(packet.ItemId, out GameObject item))
        {
            Log.Warn($"[{nameof(PlayerHeldItemChangedProcessor)}] Could not find entity with id: {packet.ItemId}.");
            return Task.CompletedTask;
        }

        Pickupable pickupable = item.GetComponent<Pickupable>();
        if (!pickupable)
        {
            Log.Warn($"[{nameof(PlayerHeldItemChangedProcessor)}] Entity with id: {packet.ItemId} is not a pickupable.");
            return Task.CompletedTask;
        }

        ItemsContainer inventory = player.Inventory;
        PlayerTool tool = item.GetComponent<PlayerTool>();

        // Copied from QuickSlots
        switch (packet.Type)
        {
            case PlayerHeldItemChanged.ChangeType.DRAW_AS_TOOL:
                if (!tool)
                {
                    Log.Warn($"[{nameof(PlayerHeldItemChangedProcessor)}] Entity with id: {packet.ItemId} is not a player tool.");
                    return Task.CompletedTask;
                }

                ModelPlug.PlugIntoSocket(tool, player.ItemAttachPoint);
                Utils.SetLayerRecursively(item, viewModelLayer);
                foreach (Animator componentsInChild in tool.GetComponentsInChildren<Animator>())
                {
                    componentsInChild.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                }
                if (tool.mainCollider)
                {
                    tool.mainCollider.enabled = false;
                }
                if (tool.TryGetComponent(out Rigidbody toolRigidbody))
                {
                    toolRigidbody.isKinematic = true;
                }
                if (tool.TryGetComponent(out Floater floater))
                {
                    floater.collider.enabled = false;
                }
                item.SetActive(true);
                tool.SetHandIKTargetsEnabled(true);
                SafeAnimator.SetBool(player.ArmsController.GetComponent<Animator>(), $"holding_{tool.animToolName}", true);
                player.AnimationController["using_tool_first"] = packet.IsFirstTime != null;

                if (item.TryGetComponent(out FPModel fpModelDraw)) //FPModel needs to be updated
                {
                    fpModelDraw.OnEquip(null, null);
                }
                break;

            case PlayerHeldItemChanged.ChangeType.HOLSTER_AS_TOOL:
                if (!tool)
                {
                    Log.Warn($"[{nameof(PlayerHeldItemChangedProcessor)}] Entity with id: {packet.ItemId} is not a player tool.");
                    return Task.CompletedTask;
                }

                item.SetActive(false);
                Utils.SetLayerRecursively(item, defaultLayer);
                if (tool.mainCollider)
                {
                    tool.mainCollider.enabled = true;
                }
                if (tool.TryGetComponent(out toolRigidbody))
                {
                    toolRigidbody.isKinematic = false;
                }
                if (tool.TryGetComponent(out floater))
                {
                    floater.collider.enabled = true;
                }
                if (pickupable.inventoryItem != null)
                {
                    pickupable.inventoryItem.item.Reparent(inventory.tr);
                }
                else
                {
                    Log.Warn($"[{nameof(PlayerHeldItemChangedProcessor)}] Pickupable with id: {packet.ItemId} has no inventory item while holstering tool.");
                }
                foreach (Animator componentsInChild in tool.GetComponentsInChildren<Animator>())
                {
                    componentsInChild.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                }
                SafeAnimator.SetBool(player.ArmsController.GetComponent<Animator>(), $"holding_{tool.animToolName}", false);
                player.AnimationController["using_tool_first"] = false;

                if (item.TryGetComponent(out FPModel fpModelHolster)) //FPModel needs to be updated
                {
                    fpModelHolster.OnUnequip(null, null);
                }

                break;

            case PlayerHeldItemChanged.ChangeType.DRAW_AS_ITEM:
                if (pickupable.inventoryItem == null)
                {
                    Log.Warn($"[{nameof(PlayerHeldItemChangedProcessor)}] Pickupable with id: {packet.ItemId} has no inventory item while drawing item.");
                    return Task.CompletedTask;
                }

                pickupable.inventoryItem.item.Reparent(player.ItemAttachPoint);
                pickupable.inventoryItem.item.SetVisible(true);
                Utils.SetLayerRecursively(pickupable.inventoryItem.item.gameObject, viewModelLayer);
                break;

            case PlayerHeldItemChanged.ChangeType.HOLSTER_AS_ITEM:
                if (pickupable.inventoryItem == null)
                {
                    Log.Warn($"[{nameof(PlayerHeldItemChangedProcessor)}] Pickupable with id: {packet.ItemId} has no inventory item while holstering item.");
                    return Task.CompletedTask;
                }

                pickupable.inventoryItem.item.Reparent(inventory.tr);
                pickupable.inventoryItem.item.SetVisible(false);
                Utils.SetLayerRecursively(pickupable.inventoryItem.item.gameObject, defaultLayer);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(packet.Type));
        }
        return Task.CompletedTask;
    }

    private void SetupLayers()
    {
        defaultLayer = LayerMask.NameToLayer("Default");
        viewModelLayer = LayerMask.NameToLayer("Viewmodel");
    }
}
