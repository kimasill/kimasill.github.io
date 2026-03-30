// Server/Server/Game/Room/GameRoom_Quest.cs

public partial class GameRoom : TaskQueue
{
    // DB 퀘스트 행과 메모리 Quest 진행도 동기화
    public void HandleStartQuest(Player player, int questId = 0)
    {
        if (player == null)
            return;

        Quest quest = player.Quest.Get(questId);
        if (quest != null && quest.Progress == 1)
            return;

        using (AppDbContext db = new AppDbContext())
        {
            List<QuestDb> quests = db.Quests
                .Where(q => q.OwnerDbId == player.PlayerDbId && (questId == 0 || q.TemplateId == questId))
                .OrderByDescending(q => q.QuestDbId)
                .ToList();

            QuestDb questDb = quests.FirstOrDefault();
            if (questDb == null)
            {
                if (questId == 0)
                {
                    questDb = new QuestDb()
                    {
                        OwnerDbId = player.PlayerDbId,
                        TemplateId = 1,
                        Progress = 1,
                        Completed = false,
                    };
                }
                else
                {
                    if (!DataManager.QuestDict.TryGetValue(questId, out QuestData questData) || questData == null)
                        return;

                    questDb = new QuestDb()
                    {
                        OwnerDbId = player.PlayerDbId,
                        TemplateId = questData.id,
                        Progress = 1,
                        Completed = false,
                    };
                }
            }
            else
            {
                if (questDb.Completed)
                    return;

                questDb.Progress = 1;
                player.Quest.CurrentQuest = Quest.MakeQuest(questDb);
            }

            DbTransaction.SaveStartQuest(player, questDb, player.Room);
        }
    }

    public void HandleUpdateQuest(Player player, int questId, int progress)
    {
        if (player == null)
            return;

        Quest quest = player.Quest.CurrentQuest;
        if (quest == null || quest.TemplateId != questId)
            return;

        quest.Progress = progress;

        QuestDb questDb = new QuestDb()
        {
            QuestDbId = quest.QuestDbId,
            OwnerDbId = player.PlayerDbId,
            TemplateId = quest.TemplateId,
            Progress = quest.Progress,
            Completed = quest.IsCompleted,
        };

        DbTransaction.UpdateQuestProgress(player, questDb, player.Room);
    }

    public void HandleQuestComplete(Player player, int questId)
    {
        if (player == null)
            return;

        Quest quest = player.Quest.Get(questId);
        if (quest == null || quest.TemplateId != questId)
            return;

        if (!quest.IsCompleted)
            quest.IsCompleted = true;
        quest.Progress = 100;

        QuestDb questDb = new QuestDb()
        {
            OwnerDbId = player.PlayerDbId,
            QuestDbId = quest.QuestDbId,
            TemplateId = quest.TemplateId,
            Progress = quest.Progress,
            Completed = quest.IsCompleted,
        };

        DbTransaction.SaveCompleteQuest(player, questDb, player.Room);

        S_QuestList questListPacket = new S_QuestList();
        foreach (Quest q in player.Quest.Quests.Values)
            questListPacket.Quests.Add(q.Info);

        player.Session.Send(questListPacket);
    }

    public void HandleSelectStat(Player player, int statId)
    {
        if (player == null || player.StatPoint <= 0)
            return;

        if (!DataManager.RealizationDict.TryGetValue(statId, out RealizationData realization) || realization == null)
            return;

        int index = realization.id - 1;
        if (player.Stat.Realizations == null
            || index < 0
            || index >= player.Stat.Realizations.Count)
            return;

        PlayerDb playerDb = new PlayerDb() { PlayerDbId = player.PlayerDbId };

        player.StatPoint--;
        player.Stat.Realizations[index] += 1;
        int investedCount = player.Stat.Realizations[index];
        playerDb.Realizations = player.Stat.Realizations.ToList();

        // 투자 횟수의 약수마다 특수 스탯을 적용 (데이터 테이블의 milestone과 매칭)
        foreach (int milestone in GetDivisors(investedCount))
        {
            foreach (var specialStat in realization.specialStatDatas)
            {
                if (specialStat != null && specialStat.point == milestone)
                    playerDb = ApplySpecialStat(player, playerDb, specialStat);
            }
        }

        playerDb.StatPoint = player.StatPoint;
        DbTransaction.SavePlayerStatDb(player, playerDb, player.Room);
    }

    /// <summary>1..n 약수 나열. Realization milestone과 정수 배수 조건에 사용.</summary>
    private static List<int> GetDivisors(int number)
    {
        var divisors = new List<int>();
        for (int i = 1; i <= number; i++)
        {
            if (number % i == 0)
                divisors.Add(i);
        }
        return divisors;
    }

    private PlayerDb ApplySpecialStat(Player player, PlayerDb playerDb, SpecialStatData specialStatData)
    {
        switch (specialStatData.name)
        {
            case "생명력":
                player.Stat.MaxHp += (int)specialStatData.value;
                playerDb.MaxHp = player.Stat.MaxHp;
                break;
            case "공격력":
                player.Stat.Attack += (int)specialStatData.value;
                playerDb.Attack = player.Stat.Attack;
                break;
            case "방어력":
                player.Stat.Defense += (int)specialStatData.value;
                playerDb.Defense = player.Stat.Defense;
                break;
            case "치명타 확률":
                player.Stat.CriticalChance += (int)specialStatData.value;
                playerDb.CriticalChance = player.Stat.CriticalChance;
                break;
            case "회복제 성능":
                player.Stat.PotionPerformance += (int)specialStatData.value;
                playerDb.PotionPerformance = player.Stat.PotionPerformance;
                break;
        }

        return playerDb;
    }
}
