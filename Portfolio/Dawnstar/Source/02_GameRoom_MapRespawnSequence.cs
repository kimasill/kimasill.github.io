// Server/Server/Game/Room/GameRoom_Sequence.cs

public partial class GameRoom : TaskQueue
{
    public void HandleRespawn(Player player, RespawnType respawnType)
    {
        if (player?.Room == null)
            return;

        LeaveGame(player.Id);

        if (respawnType == RespawnType.Spot)
            TryMoveToNearestPortal(player);

        RestorePlayerStateAfterRespawn(player);
        EnterGame(player, randPos: false);
    }

    private void TryMoveToNearestPortal(Player player)
    {
        if (!DataManager.MapDict.TryGetValue(player.MapInfo.TemplateId, out MapData mapData)
            || mapData.portals == null || mapData.portals.Count == 0)
            return;

        int portalId = GetNearestPortalId(mapData, player.CellPos);
        if (portalId != 0)
            UpdatePlayerMapInfo(player, mapData, portalId);
    }

    private void RestorePlayerStateAfterRespawn(Player player)
    {
        player.Stat.Hp = player.Stat.MaxHp;
        player.PosInfo.State = CreatureState.Idle;
        player.PosInfo.MoveDir = MoveDir.Down;
    }

    private int GetNearestPortalId(MapData mapData, Vector2Int cellPos)
    {
        if (mapData?.portals == null || mapData.portals.Count == 0)
            return 0;

        double bestDistance = double.MaxValue;
        int bestPortalId = 0;

        foreach (var portal in mapData.portals)
        {
            var portalCell = new Vector2Int((int)portal.posX, (int)portal.posY);
            double distance = (cellPos - portalCell).magnitude;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestPortalId = portal.id;
            }
        }

        return bestPortalId;
    }

    public async void HandleMapChanged(Player player, int portalId)
    {
        if (player == null)
            return;

        if (!DataManager.MapDict.TryGetValue(player.MapInfo.TemplateId, out MapData currentMap))
            return;

        int destinationMapId = 0;
        int destinationPortalId = 0;
        foreach (var portal in currentMap.portals)
        {
            if (portal.id == portalId)
            {
                destinationMapId = portal.mapId;
                destinationPortalId = portal.destination;
                break;
            }
        }

        if (!DataManager.MapDict.TryGetValue(destinationMapId, out MapData nextMap))
            return;

        bool createRoomIfMissing = nextMap.type == MapType.Dungeon;
        GameRoom destinationRoom = await GameLogic.Instance.GetRoom(destinationMapId, createRoomIfMissing);
        HandleMapChanged(player, nextMap, destinationPortalId, destinationRoom);
    }

    public void HandleMapChanged(Player player, MapData map, int destPortalId, GameRoom destinationRoom)
    {
        if (player == null || destinationRoom == null)
            return;

        LeaveGame(player.Id, save: false);
        UpdatePlayerMapInfo(player, map, destPortalId);

        MapDb mapDb = new MapDb()
        {
            PlayerDbId = player.PlayerDbId,
            TemplateId = map.id,
            Scene = map.name,
            MapName = map.name
        };
        DbTransaction.SavePlayerMap(player, mapDb);

        player.Room = destinationRoom;
        destinationRoom.Enqueue(destinationRoom.EnterGame, player, false);
    }

    public void UpdatePlayerMapInfo(Player player, MapData map, int destPortalId)
    {
        if (player == null || map == null || map.portals == null)
            return;

        PortalData portalData = map.portals.FirstOrDefault(p => p != null && p.id == destPortalId);
        if (portalData == null)
            return;

        player.CellPos = new Vector2Int((int)portalData.posX, (int)portalData.posY);
        player.PosInfo.State = CreatureState.Idle;
        player.MapInfo.TemplateId = map.id;
        player.MapInfo.MapName = map.name;
        player.MapInfo.Scene = map.name;
        player.MapInfo.PortalId = portalData.id;
        player.Session.UpdateMapChests(player, map.id);
        player.Session.UpdateMapInteractions(player, map.id);
    }

    public void HandleEnterDungeon(Player player, int mapId)
    {
        if (player == null)
            return;

        if (player.Session.CurrentParty == null)
            player.Session.CreateParty();

        PartyMatchingSystem.Instance.EnterMap(player.Session.CurrentParty, mapId);
    }

    public void HandleMatching(Player player, int mapId, AdmitType admitType)
    {
        if (player == null)
            return;

        switch (admitType)
        {
            case AdmitType.Matching:
                PartyMatchingSystem.Instance.Register(player.Session, mapId);
                break;
            case AdmitType.Cancel:
                PartyMatchingSystem.Instance.Unregister(player.Session, mapId);
                break;
        }
    }
}
