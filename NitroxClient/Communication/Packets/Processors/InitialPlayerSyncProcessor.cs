using System;
using System.Collections;
using System.Collections.Generic;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class InitialPlayerSyncProcessor(IEnumerable<IInitialSyncProcessor> processors) : IClientPacketProcessor<InitialPlayerSync>
{
    private readonly HashSet<Type> alreadyRan = [];
    private readonly HashSet<IInitialSyncProcessor> processors = processors.ToSet();

    private int cumulativeProcessorsRan;
    private bool initialSyncFailed;

    private WaitScreen.ManualWaitItem loadingMultiplayerWaitItem;
    private InitialPlayerSync packet;
    private int processorsRanLastCycle;
    private WaitScreen.ManualWaitItem subWaitScreenItem;

    public Task Process(ClientProcessorContext context, InitialPlayerSync packet)
    {
        this.packet = packet;

        loadingMultiplayerWaitItem = WaitScreen.Add(Language.main.Get("Nitrox_SyncingWorld"));
        Log.InGame(Language.main.Get("Nitrox_SyncingWorld"));

        cumulativeProcessorsRan = 0;
        Multiplayer.Main.StartCoroutine(ProcessInitialSyncPacket(context));
        return Task.CompletedTask;
    }

    private IEnumerator ProcessInitialSyncPacket(ClientProcessorContext context)
    {
        bool moreProcessorsToRun;
        do
        {
            yield return Multiplayer.Main.StartCoroutine(RunPendingProcessors());

            if (initialSyncFailed)
            {
                WaitScreen.Remove(loadingMultiplayerWaitItem);
                Log.Error("Initial sync aborted because a sync processor threw; the client state is incomplete. Please rejoin the server.");
                Log.InGame("Failed to synchronize with the server. Please rejoin.");
                yield break;
            }

            moreProcessorsToRun = alreadyRan.Count < processors.Count;
            if (moreProcessorsToRun && processorsRanLastCycle == 0)
            {
                throw new Exception($"Detected circular dependencies in initial packet sync between: {GetRemainingProcessorsText()}");
            }
        } while (moreProcessorsToRun);

        WaitScreen.Remove(loadingMultiplayerWaitItem);
        Multiplayer.Main.InitialSyncCompleted = true;

        // When the player finishes loading, we can take back his invincibility
        Player.main.liveMixin.invincible = false;
        Player.main.UnfreezeStats();

        context.Send(new PlayerSyncFinished());
    }

    private IEnumerator RunPendingProcessors()
    {
        processorsRanLastCycle = 0;

        foreach (IInitialSyncProcessor processor in processors)
        {
            if (!IsWaitingToRun(processor.GetType()) || !HasDependenciesSatisfied(processor))
            {
                continue;
            }

            loadingMultiplayerWaitItem.SetProgress(cumulativeProcessorsRan, processors.Count);

            Log.Info($"Running {processor.GetType()}");
            subWaitScreenItem = WaitScreen.Add($"Running {processor.GetType().Name}");

            // Drive the processor's coroutine manually so we can capture exceptions: `yield return StartCoroutine(...)`
            // swallows them (Unity does not propagate coroutine exceptions to the parent), which previously left a
            // failed/partial step marked as "ran" so its dependents executed against incomplete state. We only mark the
            // processor as ran after it completes cleanly; any failure aborts the whole sync (a partial join is unrecoverable).
            IEnumerator enumerator = processor.Process(packet, subWaitScreenItem);
            while (true)
            {
                object current;
                try
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    current = enumerator.Current;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Initial sync processor {processor.GetType().Name} threw; aborting initial sync");
                    WaitScreen.Remove(subWaitScreenItem);
                    initialSyncFailed = true;
                    yield break;
                }
                yield return current;
            }

            WaitScreen.Remove(subWaitScreenItem);

            alreadyRan.Add(processor.GetType());
            processorsRanLastCycle++;
            cumulativeProcessorsRan++;
        }
    }

    private bool HasDependenciesSatisfied(IInitialSyncProcessor processor)
    {
        foreach (Type dependentType in processor.DependentProcessors)
        {
            if (IsWaitingToRun(dependentType))
            {
                return false;
            }
        }

        return true;
    }

    private bool IsWaitingToRun(Type processor)
    {
        return !alreadyRan.Contains(processor);
    }

    private string GetRemainingProcessorsText()
    {
        string remaining = "";

        foreach (IInitialSyncProcessor processor in processors)
        {
            if (IsWaitingToRun(processor.GetType()))
            {
                remaining += $" {processor.GetType()}";
            }
        }

        return remaining;
    }
}
