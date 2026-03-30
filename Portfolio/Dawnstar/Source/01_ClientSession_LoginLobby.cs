// Server/Server/Session/ClientSession_preGame.cs

public partial class ClientSession : PacketSession
{
    public int AccountDbId { get; private set; }
    public List<LobbyPlayerInfo> LobbyPlayers { get; } = new List<LobbyPlayerInfo>();

    public void HandleLogin(C_Login loginPacket)
    {
        if (ServerState != PlayerServerState.ServerStateLogin)
            return;

        if (!int.TryParse(loginPacket.UniqueId, out int tokenValue))
            return;

        LobbyPlayers.Clear();

        using (AppDbContext db = new AppDbContext())
        using (CommonDbContext sharedDb = new CommonDbContext())
        {
            TokenDb token = sharedDb.Tokens
                .Where(t => t.Token == tokenValue)
                .FirstOrDefault();
            if (token == null)
                return;

            AccountDb findAccount = db.Accounts
                .Include(a => a.Players)
                .Where(a => a.AccountName == token.AccountDbId.ToString())
                .FirstOrDefault();

            if (findAccount != null)
            {
                AccountDbId = findAccount.AccountDbId;

                S_Login loginResponse = new S_Login { LoginOk = 1 };
                foreach (PlayerDb player in findAccount.Players)
                {
                    LobbyPlayerInfo summary = ToLobbyPlayerInfo(player);
                    LobbyPlayers.Add(summary);
                    loginResponse.Players.Add(summary);
                }

                // 로그인 성공: 로비 캐릭터 목록 전송 후 세션 → Lobby
                Send(loginResponse);
                ServerState = PlayerServerState.ServerStateLobby;
            }
            else
            {
                AccountDb newAccount = new AccountDb() { AccountName = token.AccountDbId.ToString() };
                db.Accounts.Add(newAccount);
                if (!db.SaveChangesEx())
                    return;

                AccountDbId = newAccount.AccountDbId;
                Send(new S_Login { LoginOk = 1 });
                ServerState = PlayerServerState.ServerStateLobby;
            }
        }
    }

    private static LobbyPlayerInfo ToLobbyPlayerInfo(PlayerDb playerDb)
    {
        IReadOnlyList<int> realizations = (playerDb.Realizations != null && playerDb.Realizations.Count > 0)
            ? playerDb.Realizations
            : new List<int>() { 0, 0, 0, 0 };

        LobbyPlayerInfo result = new LobbyPlayerInfo()
        {
            PlayerDbId = playerDb.PlayerDbId,
            Name = playerDb.PlayerName,
            StatInfo = new StatInfo()
            {
                Level = playerDb.Level,
                Hp = playerDb.Hp,
                Up = playerDb.Up,
                HpRegen = playerDb.HpRegen,
                UpRegen = playerDb.UpRegen,
                MaxHp = playerDb.MaxHp,
                MaxUp = playerDb.MaxUp,
                Attack = playerDb.Attack,
                Defense = playerDb.Defense,
                Accuracy = playerDb.Accuracy,
                Avoid = playerDb.Avoid,
                Speed = playerDb.Speed,
                TotalExp = playerDb.Exp,
                StatPoint = playerDb.StatPoint,
                CriticalChance = playerDb.CriticalChance,
                CriticalDamage = playerDb.CriticalDamage,
                PotionPerformance = playerDb.PotionPerformance,
                MaxPotion = playerDb.MaxPotion
            }
        };
        result.StatInfo.Realizations.Clear();
        result.StatInfo.Realizations.AddRange(realizations);
        return result;
    }

    public void HandleEnterGame(C_EnterGame enterGamePacket)
    {
        if (ServerState != PlayerServerState.ServerStateLobby)
            return;

        LobbyPlayerInfo playerInfo = LobbyPlayers.Find(p => p.Name == enterGamePacket.Name);
        if (playerInfo == null)
            return;

        int mapId = 1;
        bool isFirstLogin = false;
        MyPlayer = EntityRegistry.Instance.Add<Player>();

        MyPlayer.PlayerDbId = playerInfo.PlayerDbId;
        MyPlayer.Info.Name = playerInfo.Name;
        MyPlayer.Info.Position.State = CreatureState.Idle;
        MyPlayer.Info.Position.MoveDir = MoveDir.Down;
        MyPlayer.Info.Position.LookDir = LookDir.LookRight;
        MyPlayer.Stat.MergeFrom(playerInfo.StatInfo);
        if (MyPlayer.Stat.Hp <= 0)
            MyPlayer.Stat.Hp = MyPlayer.Stat.MaxHp;

        S_QuestList questListPacket = new S_QuestList();
        using (AppDbContext db = new AppDbContext())
        {
            List<QuestDb> quests = db.Quests
                .Where(q => q.OwnerDbId == playerInfo.PlayerDbId)
                .ToList();
            foreach (QuestDb questDb in quests)
            {
                Quest quest = Quest.MakeQuest(questDb);
                if (quest != null)
                {
                    MyPlayer.Quest.Add(quest);
                    QuestInfo info = new QuestInfo();
                    info.MergeFrom(quest.Info);
                    questListPacket.Quests.Add(info);
                }
            }
        }
        Send(questListPacket);

        if (MyPlayer.Quest.Quests.Count == 0)
        {
            isFirstLogin = true;
            ServerState = PlayerServerState.ServerStateSingle;
        }

        if (isFirstLogin)
        {
            MyPlayer = SingleGameSetting(MyPlayer);
        }
        else
        {
            using (AppDbContext db = new AppDbContext())
            {
                PlayerDb player = db.Players.FirstOrDefault(p => p.PlayerDbId == playerInfo.PlayerDbId);
                if (player != null)
                {
                    MyPlayer.Info.Position.PosX = player.PosX;
                    MyPlayer.Info.Position.PosY = player.PosY;
                    MyPlayer.MapInfo.MapDbId = player.MapDbId;
                }
            }
        }

        S_ItemList itemListPacket = new S_ItemList();
        using (AppDbContext db = new AppDbContext())
        {
            List<ItemDb> items = db.Items
                .Where(i => i.OwnerDbId == playerInfo.PlayerDbId)
                .ToList();
            foreach (ItemDb itemDb in items)
            {
                Item item = Item.MakeItem(itemDb);
                if (item != null)
                {
                    MyPlayer.Inven.Add(item);
                    ItemInfo info = new ItemInfo();
                    info.MergeFrom(item.Info);
                    itemListPacket.Items.Add(info);
                }
            }
        }
        Send(itemListPacket);

        MyPlayer.Session = this;
        using (AppDbContext db = new AppDbContext())
        {
            MapDb mapDb = db.Maps.Where(m => m.MapDbId == MyPlayer.MapInfo.MapDbId).FirstOrDefault();
            if (mapDb != null)
            {
                MapData mapData = DataManager.MapDict.TryGetValue(mapDb.TemplateId, out mapData) ? mapData : null;
                if (mapData == null)
                    return;
                if (mapData.type == MapType.Dungeon)
                {
                    MyPlayer.PosInfo.PosX = (int)mapData.portals[0].posX;
                    MyPlayer.PosInfo.PosY = (int)mapData.portals[0].posY;
                }
                ChangeServerState(mapDb.TemplateId);
                MyPlayer.MapInfo.TemplateId = mapDb.TemplateId;
                MyPlayer.MapInfo.Scene = mapDb.Scene;
                MyPlayer.MapInfo.MapName = mapDb.MapName;
                mapId = mapDb.TemplateId;
            }
        }

        UpdateMapInteractions(MyPlayer, mapId);
        UpdateMapChests(MyPlayer, mapId);
        MyPlayer.Skill = new Skill(MyPlayer);

        // 퀘스트·인벤·맵 상태 반영 후 룸 큐에 EnterGame
        if (ServerState == PlayerServerState.ServerStateSingle)
        {
            GameLogic.Instance.Enqueue(() =>
            {
                GameRoom room = GameLogic.Instance.Add(mapId);
                room.Enqueue(room.EnterGame, MyPlayer, false);
            });
        }
        else
        {
            ServerState = PlayerServerState.ServerStateGame;
            GameLogic.Instance.Enqueue(() =>
            {
                GameRoom room = GameLogic.Instance.FindByMapId(mapId) ?? GameLogic.Instance.Add(mapId);
                room.Enqueue(room.EnterGame, MyPlayer, false);
            });
        }
    }

    public void HandleCreatePlayer(C_CreatePlayer createPacket)
    {
        if (ServerState != PlayerServerState.ServerStateLobby)
            return;

        using (AppDbContext db = new AppDbContext())
        {
            PlayerDb findPlayer = db.Players
                .Where(p => p.PlayerName == createPacket.Name)
                .FirstOrDefault();

            if (findPlayer != null)
            {
                Send(new S_CreatePlayer());
                return;
            }

            if (!DataManager.StatDict.TryGetValue(1, out StatData stat) || stat == null)
            {
                Send(new S_CreatePlayer());
                return;
            }

            PlayerDb newPlayerDb = new PlayerDb()
            {
                PlayerName = createPacket.Name,
                Level = stat.Level,
                Hp = stat.MaxHp,
                MaxHp = stat.MaxHp,
                Up = stat.MaxUp,
                MaxUp = stat.MaxUp,
                HpRegen = stat.HpRegen,
                UpRegen = stat.UpRegen,
                Attack = stat.Attack,
                Speed = stat.Speed,
                Avoid = stat.Avoid,
                Accuracy = stat.Accuracy,
                CriticalChance = stat.CriticalChance,
                CriticalDamage = stat.CriticalDamage,
                PotionPerformance = 1,
                MaxPotion = 5,
                Exp = 0,
                AccountDbId = AccountDbId
            };

            db.Players.Add(newPlayerDb);
            if (!db.SaveChangesEx())
                return;

            LobbyPlayerInfo lobbyPlayer = new LobbyPlayerInfo()
            {
                PlayerDbId = newPlayerDb.PlayerDbId,
                Name = createPacket.Name,
                StatInfo = new StatInfo()
                {
                    Level = stat.Level,
                    Hp = stat.MaxHp,
                    MaxHp = stat.MaxHp,
                    Up = stat.MaxUp,
                    MaxUp = stat.MaxUp,
                    HpRegen = stat.HpRegen,
                    UpRegen = stat.UpRegen,
                    Attack = stat.Attack,
                    Speed = stat.Speed,
                    PotionPerformance = 0,
                    MaxPotion = 5,
                    TotalExp = 0
                }
            };
            LobbyPlayers.Add(lobbyPlayer);

            S_CreatePlayer newPlayer = new S_CreatePlayer() { Player = new LobbyPlayerInfo() };
            newPlayer.Player.MergeFrom(lobbyPlayer);
            Send(newPlayer);
        }
    }

    public void ChangeServerState(int mapId)
    {
        DataManager.MapDict.TryGetValue(mapId, out MapData mapData);
        if (mapData == null)
            return;
        if (mapData.type == MapType.Quest)
            ServerState = PlayerServerState.ServerStateSingle;
        else
            ServerState = PlayerServerState.ServerStateGame;
    }
}
