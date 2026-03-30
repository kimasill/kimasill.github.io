// Server/Server/Game/Interactions/, GameRoom_Quest.cs, ClientSession_preGame.cs

public class Interaction : GameObject
{
    public List<Vector2Int> Cells { get; set; } = new List<Vector2Int>();

    public static Interaction CreateInteraction(InteractionData data)
    {
        if (data == null)
            return null;

        Interaction interaction = null;
        // 맵 데이터 → Door / Trigger
        switch (data.interactionType)
        {
            case InteractionType.Door:
                interaction = new Door((DoorData)data);
                break;
            case InteractionType.Trigger:
                interaction = new Trigger((TriggerData)data);
                break;
            default:
                return null;
        }

        interaction.ObjectType = GameObjectType.Interaction;
        interaction.Id = EntityRegistry.Instance.GenerateId(GameObjectType.Interaction);
        return interaction;
    }

    public virtual void OnTriggerEnter(int id) { }
    public virtual void OnInteraction() { }
}

internal class Door : Interaction
{
    public bool IsOpen { get; set; }
    public List<int> KeyItems { get; set; } = new List<int>();
    public Dictionary<int, bool> Triggers { get; set; } = new Dictionary<int, bool>();

    public override void OnInteraction()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (Room?.Map == null)
            return;

        foreach (var cellPos in Cells)
            Room.Map.SetCollision(cellPos, false);
        IsOpen = true;
    }

    public void Close()
    {
        if (Room?.Map == null)
            return;

        foreach (var cellPos in Cells)
            Room.Map.SetCollision(cellPos, true);
        IsOpen = false;
    }

    public override void OnTriggerEnter(int id)
    {
        if (Triggers.ContainsKey(id))
            Triggers[id] = !Triggers[id];
    }
}

internal class Trigger : Interaction
{
    public bool IsActivated { get; set; }
    public List<int> ActivationItems { get; set; } = new List<int>();
    public Dictionary<int, bool> Conditions { get; set; } = new Dictionary<int, bool>();

    public override void OnInteraction()
    {
        if (IsActivated) Deactivate();
        else Activate();
    }

    public void Activate()
    {
        foreach (var key in Conditions.Keys.ToList())
            Conditions[key] = true;
        IsActivated = true;
    }

    public void Deactivate()
    {
        foreach (var key in Conditions.Keys.ToList())
            Conditions[key] = false;
        IsActivated = false;
    }
}

public partial class GameRoom : TaskQueue
{
    public void HandleDoorInteraction(Player player, int doorId)
    {
        if (player?.Room == null)
            return;

        if (!(Map.GetInteraction(doorId) is Door door))
            return;

        bool success = true;

        if (!door.IsOpen)
        {
            foreach (var trigger in door.Triggers.Values)
            {
                if (!trigger)
                {
                    success = false;
                    break;
                }
            }

            if (success && door.KeyItems.Count > 0)
            {
                foreach (var keyId in door.KeyItems)
                {
                    if (player.Inven.FindByTemplateId(keyId) == null)
                    {
                        success = false;
                        break;
                    }
                }

                if (success)
                {
                    foreach (var keyId in door.KeyItems)
                    {
                        Item key = player.Inven.FindByTemplateId(keyId);
                        if (key == null)
                            continue;

                        DbTransaction.SaveRemovedItemDB(player, new ItemDb()
                        {
                            TemplateId = key.TemplateId,
                            Count = 1,
                            OwnerDbId = player.PlayerDbId,
                            Slot = key.Slot
                        }, player.Room);
                        player.Inven.Remove(key.ItemDbId, 1);
                    }
                }
            }
        }

        var interactionPacket = new S_Interaction()
        {
            Success = success,
            ObjectId = doorId,
            PlayerId = player.Id,
            InteractionType = InteractionType.Door
        };

        if (success)
        {
            door.OnInteraction();
            player.Room.Broadcast(player.CellPos, interactionPacket);
        }
        else
        {
            player.Session.Send(interactionPacket);
        }
    }

    public void HandleTriggerInteraction(Player player, int triggerId)
    {
        if (player?.Room == null)
            return;

        if (!(Map.GetInteraction(triggerId) is Trigger trigger))
            return;

        bool success = true;
        if (trigger.ActivationItems != null && trigger.ActivationItems.Count > 0 && !trigger.IsActivated)
        {
            foreach (var keyId in trigger.ActivationItems)
            {
                if (player.Inven.FindByTemplateId(keyId) == null)
                    success = false;
            }
        }

        var targetIds = new List<int>();
        if (success && trigger.Conditions.Count > 0)
        {
            foreach (int targetInteractionId in trigger.Conditions.Keys)
            {
                targetIds.Add(targetInteractionId);
                Interaction target = Map.GetInteraction(targetInteractionId);
                target?.OnTriggerEnter(triggerId);
            }
        }

        var interactionPacket = new S_Interaction()
        {
            Success = success,
            ObjectId = triggerId,
            PlayerId = player.Id,
            InteractionType = InteractionType.Trigger
        };
        interactionPacket.TargetId.AddRange(targetIds);

        if (success)
        {
            trigger.OnInteraction();
            player.Room.Broadcast(player.CellPos, interactionPacket);
        }
        else
        {
            player.Session.Send(interactionPacket);
        }
    }
}

public partial class ClientSession : PacketSession
{
    public void UpdateMapChests(Player player, int mapId)
    {
        if (MyPlayer?.MapInfo == null)
            return;

        var chestIds = new List<int>();
        if (!DataManager.MapDict.TryGetValue(mapId, out MapData mapData) || mapData.chests == null)
            return;

        using (AppDbContext db = new AppDbContext())
        {
            List<ChestDb> chests = db.Chests
                .Where(c => c.MapDbId == MyPlayer.MapInfo.MapDbId)
                .ToList();

            foreach (var chest in mapData.chests)
            {
                ChestDb chestDb = chests.FirstOrDefault(c => c.ChestId == chest.chestId);
                if (chestDb == null)
                {
                    chestDb = new ChestDb()
                    {
                        TemplateId = chest.templateId,
                        MapDbId = MyPlayer.MapInfo.MapDbId,
                        ChestId = chest.chestId,
                        Opened = false
                    };
                    chestIds.Add(chest.chestId);
                    DbTransaction.UpdateChestDb(player, chestDb);
                }
                else if (!chestDb.Opened)
                {
                    chestIds.Add(chestDb.ChestId);
                }
            }
        }

        MyPlayer.MapInfo.ChestIds.Clear();
        MyPlayer.MapInfo.ChestIds.AddRange(chestIds);
    }

    public void UpdateMapInteractions(Player player, int mapId)
    {
        if (MyPlayer?.MapInfo == null)
            return;

        var interactionIds = new List<int>();

        using (AppDbContext db = new AppDbContext())
        {
            List<InteractionDb> interactions = db.Interactions
                .Where(i => i.MapDbId == MyPlayer.MapInfo.MapDbId)
                .ToList();

            foreach (var interaction in interactions)
            {
                if (interaction.Completed)
                    interactionIds.Add(interaction.TemplateId);
            }
        }

        MyPlayer.MapInfo.InteractionIds.Clear();
        MyPlayer.MapInfo.InteractionIds.AddRange(interactionIds);
    }
}
