// Source:
// - Source/Blaster/GameMode/PickpackerGameMode.cpp
// - Source/Blaster/GameState/PickpackerGameState.cpp

void APickpackerGameMode::BeginPlay()
{
	Super::BeginPlay();

	// Force-load streamed gameplay levels before match start so
	// packaged builds do not fail actor references during startup.
	if (HasAuthority() && StreamingLevelNamesToLoadAtStart.Num() > 0)
	{
		if (UWorld* World = GetWorld())
		{
			for (ULevelStreaming* StreamingLevel : World->GetStreamingLevels())
			{
				if (!StreamingLevel) continue;

				const FString LevelName = StreamingLevel->GetWorldAssetPackageFName().ToString();
				const FString LevelShortName = FPaths::GetBaseFilename(LevelName);
				for (const FName& ToLoad : StreamingLevelNamesToLoadAtStart)
				{
					if (LevelShortName.Equals(ToLoad.ToString(), ESearchCase::IgnoreCase) ||
						LevelName.Contains(ToLoad.ToString(), ESearchCase::IgnoreCase))
					{
						StreamingLevel->SetShouldBeLoaded(true);
						StreamingLevel->SetShouldBeVisible(true);
						break;
					}
				}
			}

			UGameplayStatics::FlushLevelStreaming(World);
		}
	}
}

void APickpackerGameMode::StartOrderSystem()
{
	if (!HasAuthority() || bOrderSystemInitialized || !OrderWaveData)
	{
		return;
	}

	bOrderSystemInitialized = true;
	ActiveOrders.Reset();
	CurrentOrderWaveIndex = INDEX_NONE;
	CurrentWaveRepeatIndex = 0;

	if (CanSpawnNewOrders())
	{
		if (const FParcelOrderWave* FirstWave = OrderWaveData->GetWave(0))
		{
			const float InitialDelay = FMath::Max(0.0f, InitialOrderStartDelay);
			ScheduleNextOrderWave(0, 0, InitialDelay);
		}
	}

	if (UWorld* World = GetWorld())
	{
		World->GetTimerManager().SetTimer(
			OrderSystemTimerHandle,
			this,
			&APickpackerGameMode::TickOrderSystem,
			FMath::Max(0.25f, OrderUpdateInterval),
			true
		);
	}
}

bool APickpackerGameMode::TryFulfillOrders(AParcelActor* Parcel)
{
	if (!HasAuthority() || !Parcel)
	{
		return false;
	}

	const bool bParcelPackaged = Parcel->IsPackaged();
	TMap<FGameplayTag, int32> ConsumedUnitsByTag;
	bool bAppliedToAnyOrder = false;

	for (FActiveOrderState& Order : ActiveOrders)
	{
		if (Order.bCompleted || Order.bFailed) continue;
		if (Order.bRequirePackaged && !bParcelPackaged) continue;

		const FGameplayTag TagToMatch = Order.RequiredParcelTag;
		const int32 TotalMatchingUnits = TagToMatch.IsValid()
			? Parcel->GetContentUnitsForTag(TagToMatch)
			: Parcel->GetContentUnitTotal();
		const int32 AlreadyConsumed = TagToMatch.IsValid() ? ConsumedUnitsByTag.FindOrAdd(TagToMatch, 0) : 0;
		const int32 AvailableUnits = FMath::Max(0, TotalMatchingUnits - AlreadyConsumed);

		if (AvailableUnits <= 0) continue;

		const int32 RemainingNeed = Order.RequiredQuantity - Order.SubmittedQuantity;
		const int32 UnitsToApply = FMath::Min(RemainingNeed, AvailableUnits);
		if (UnitsToApply <= 0) continue;

		Order.SubmittedQuantity += UnitsToApply;
		if (TagToMatch.IsValid())
		{
			ConsumedUnitsByTag.FindOrAdd(TagToMatch, 0) += UnitsToApply;
		}

		bAppliedToAnyOrder = true;

		if (Order.SubmittedQuantity >= Order.RequiredQuantity)
		{
			Order.bCompleted = true;
			Order.ResolutionTime = GetWorld() ? GetWorld()->GetTimeSeconds() : 0.0f;
			HandleOrderSuccess(Order);
		}
	}

	if (bAppliedToAnyOrder)
	{
		SyncOrdersToGameState();
		return true;
	}

	return false;
}

void APickpackerGameState::ApplyCreditDelta(int32 Delta, const FString& Reason)
{
	if (!HasAuthority() || Delta == 0)
	{
		return;
	}

	const int32 OldCredits = TeamCredits;
	TeamCredits = FMath::Max(0, TeamCredits + Delta);

	FCreditTransaction Transaction;
	Transaction.TransactionId = FGuid::NewGuid();
	Transaction.Delta = Delta;
	Transaction.BalanceAfter = TeamCredits;
	Transaction.Timestamp = GetWorld() ? GetWorld()->GetTimeSeconds() : 0.0f;
	Transaction.Reason = FText::FromString(Reason);
	CreditHistory.Add(Transaction);

	OnCreditsChanged.Broadcast(TeamCredits, TeamCredits - OldCredits);
}
