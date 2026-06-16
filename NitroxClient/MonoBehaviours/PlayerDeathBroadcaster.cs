using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Gui.InGame;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class PlayerDeathBroadcaster : MonoBehaviour
{
    private LocalPlayer localPlayer;

    public void Awake()
    {
        localPlayer = this.Resolve<LocalPlayer>();

        Player.main.playerDeathEvent.AddHandler(this, OnPlayerDeath);
    }

    private void OnPlayerDeath(Player player)
    {
        if (localPlayer.MarkDeathPointsWithBeacon)
        {
            DeathBeacon.SpawnDeathBeacon(player.transform.position.ToDto(), localPlayer.PlayerName);
        }
        // Death is broadcast from Player_OnKill_Patch.Postfix instead: this handler (playerDeathEvent) does not fire in
        // permadeath/hardcore mode, where Player.ResetPlayerOnDeath ends the game before triggering the event.
    }

    public void OnDestroy()
    {
        Player.main.playerDeathEvent.RemoveHandler(this, OnPlayerDeath);
    }
}
