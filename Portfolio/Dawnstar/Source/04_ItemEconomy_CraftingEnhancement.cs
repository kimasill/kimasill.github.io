// Server/Server/Game/Room/GameRoom_Item.cs

public partial class GameRoom : TaskQueue
{
    // 구매: 골드·슬롯 검증 후 DbTransaction으로 보상·저장
    public void HandleBuyItem(Player player, C_BuyItem buyPacket)
    {
        if (player == null)
            return;

        if (!DataManager.ItemDict.TryGetValue(buyPacket.TemplateId, out ItemData itemData) || itemData == null)
            return;

        int count = buyPacket.Count;
        if (count <= 0)
            return;

        int? slot = player.Inven.GetSlot(buyPacket.TemplateId, count);
        if (slot == null)
            return;

        long totalCost = (long)itemData.price * count;
        if (player.Gold < totalCost)
        {
            player.Session.Send(new S_BuyItem() { TemplateId = itemData.id, Count = 0 });
            return;
        }

        player.Gold -= (int)totalCost;
        DbTransaction.RewardPlayer(player, itemData, count, this);
    }

    public void HandleSellItem(Player player, C_SellItem sellPacket)
    {
        if (player == null)
            return;

        Item item = player.Inven.Get(sellPacket.ItemDbId);
        if (item == null)
            return;

        if (!DataManager.ItemDict.TryGetValue(item.TemplateId, out ItemData itemData) || itemData == null)
            return;

        int sellCount = sellPacket.Count;
        if (sellCount <= 0 || sellCount > item.Count)
            return;

        int unitPrice = itemData.price / 2;
        player.Gold += unitPrice * sellCount;

        ItemDb itemDb = new ItemDb()
        {
            TemplateId = item.TemplateId,
            Count = sellCount,
            OwnerDbId = player.PlayerDbId,
            Slot = item.Slot
        };
        DbTransaction.SaveRemovedItemDB(player, itemDb, this);
    }

    private void UseConsumableItem(Player player, Item item, ItemData itemData)
    {
        foreach (var option in itemData.options)
        {
            if (option.Key == "Heal")
            {
                if (!int.TryParse(option.Value, out int baseHeal))
                    continue;

                int healAmount = baseHeal + baseHeal * player.PotionPerformance / 100;
                player.ChangeHp(player.Hp + healAmount);
                player.Inven.Remove(item.ItemDbId, 1);

                DbTransaction.SaveRemovedItemDB(player, new ItemDb()
                {
                    TemplateId = item.TemplateId,
                    Count = 1,
                    OwnerDbId = player.PlayerDbId,
                    Slot = item.Slot
                }, this);
            }
            else if (option.Key == "Skill")
            {
                if (!int.TryParse(option.Value, out int skillId))
                    continue;

                if (!DataManager.SkillDict.TryGetValue(skillId, out SkillData skillData) || skillData == null)
                    return;

                if (!player.Skill.HandleSkillCool(skillData))
                    return;

                var skillPacket = new S_Skill() { Info = new SkillInfo() };
                skillPacket.ObjectId = player.Id;
                skillPacket.Info.SkillId = skillId;
                Broadcast(player.CellPos, skillPacket);
                player.Skill.StartSkill(player, skillData, target: player);
            }
        }
    }

    public void HandleEnhanceItem(Player player, C_Enhance enhancePacket)
    {
        if (player == null)
            return;

        Item item = player.Inven.Get(enhancePacket.ItemDbId);
        if (item == null)
            return;

        EnhanceData enhanceData = DataManager.EnhanceDict.Values
            .FirstOrDefault(x => x != null && x.rank == item.Rank + 1 && x.itemType == item.ItemType);
        if (enhanceData == null || enhanceData.costData == null)
            return;

        var costItems = new List<ItemDb>();
        foreach (CostData cost in enhanceData.costData)
        {
            Item costItem = player.Inven.FindByTemplateId(cost.templateId);
            if (costItem == null || costItem.Count < cost.count)
            {
                player.Session.Send(new S_SystemNotice() { Message = "강화 재료가 부족합니다." });
                return;
            }

            costItems.Add(new ItemDb()
            {
                ItemDbId = costItem.ItemDbId,
                TemplateId = costItem.TemplateId,
                Count = cost.count,
                OwnerDbId = player.PlayerDbId,
                Slot = costItem.Slot
            });
        }

        foreach (ItemDb cost in costItems)
            DbTransaction.SaveRemovedItemDB(player, cost, this);

        ItemDb newItemDb = ItemLogic.EnhanceItem(player, item, enhanceData);
        if (newItemDb == null)
        {
            player.Session.Send(new S_Enhance() { ItemInfo = item.Info, Success = false });
            return;
        }

        DbTransaction.SaveEnhancedItemDB(player, newItemDb, this);
    }

    public void HandleEnchantItem(Player player, C_Enchant enchantPacket)
    {
        Item targetItem = player.Inven.Get(enchantPacket.TargetId);
        Item materialItem = player.Inven.Get(enchantPacket.MaterialId);
        if (targetItem == null || materialItem == null)
            return;

        if (!DataManager.ItemDict.TryGetValue(targetItem.TemplateId, out ItemData targetItemData)
            || !DataManager.ItemDict.TryGetValue(materialItem.TemplateId, out ItemData materialItemData))
            return;

        if (materialItemData.itemType != ItemType.Consumable)
            return;

        if (!DataManager.EnchantDict.TryGetValue(targetItem.ItemType, out EnchantData enchantData) || enchantData == null)
            return;

        Dictionary<string, string> enchantedOptions = ItemLogic.Enchant(player, targetItem, enchantData);
        if (enchantedOptions == null || enchantedOptions.Count == 0)
            return;

        var options = new Dictionary<string, string>(targetItem.Options);
        foreach (var option in enchantedOptions)
        {
            string newKey = option.Key;
            int suffix = 1;
            while (options.ContainsKey(newKey))
            {
                newKey = $"{option.Key}_{suffix}";
                suffix++;
            }
            options.Add(newKey, option.Value);
        }

        DbTransaction.SaveEnchantItem(player, new ItemDb()
        {
            ItemDbId = targetItem.ItemDbId,
            Enchant = targetItem.Enchant + 1,
            OwnerDbId = player.PlayerDbId,
            Options = options
        }, this);

        DbTransaction.SaveRemovedItemDB(player, new ItemDb()
        {
            ItemDbId = materialItem.ItemDbId,
            TemplateId = materialItem.TemplateId,
            Count = 1,
            OwnerDbId = player.PlayerDbId,
            Slot = materialItem.Slot
        }, this);
    }

    public void HandleMakeItem(Player player, C_MakeItem makeItemPacket)
    {
        if (player == null)
            return;

        if (!DataManager.ItemDict.TryGetValue(makeItemPacket.TemplateId, out ItemData itemData)
            || itemData == null
            || itemData.pieces == null)
            return;

        int craftCount = makeItemPacket.Count;
        if (craftCount <= 0)
            return;

        var requiredRemovals = new List<ItemDb>();
        foreach (var piece in itemData.pieces)
        {
            Item inv = player.Inven.FindByTemplateId(piece.templateId);
            int need = piece.count * craftCount;
            if (inv == null || inv.Count < need)
            {
                player.Session.Send(new S_SystemNotice() { Message = "재료가 부족합니다." });
                return;
            }

            requiredRemovals.Add(new ItemDb()
            {
                TemplateId = inv.TemplateId,
                Count = need,
                OwnerDbId = player.PlayerDbId,
                Slot = inv.Slot
            });
        }

        foreach (var itemDb in requiredRemovals)
            DbTransaction.SaveRemovedItemDB(player, itemDb, this);

        int? emptySlot = player.Inven.GetEmptySlot();
        if (emptySlot == null)
            return;

        DbTransaction.SaveAddItemDB(player, new ItemDb()
        {
            TemplateId = itemData.id,
            Count = craftCount,
            Grade = itemData.grade.ToString(),
            OwnerDbId = player.PlayerDbId,
            Slot = emptySlot.Value
        }, this);
    }
}
