// Source:
// - Source/Blaster/Components/EscapeProgressComponent.cpp

void UEscapeProgressComponent::SetWorldFlag(const FGameplayTag& Flag, int32 Value)
{
	if (!GetOwner() || !GetOwner()->HasAuthority())
	{
		return;
	}

	if (FWorldFlagEntry* Entry = FindWorldFlagEntry(Flag))
	{
		Entry->Value = Value;
	}
	else
	{
		FWorldFlagEntry NewEntry;
		NewEntry.Flag = Flag;
		NewEntry.Value = Value;
		WorldFlags.Add(MoveTemp(NewEntry));
	}

	// Re-evaluate endings whenever world progression changes.
	EvaluateEndings();
}

void UEscapeProgressComponent::AddAuthorizedPlayer(APlayerState* PlayerState)
{
	if (!GetOwner() || !GetOwner()->HasAuthority() || !PlayerState)
	{
		return;
	}

	const TWeakObjectPtr<APlayerState> Key(PlayerState);
	if (!AuthorizedPlayers.Contains(Key))
	{
		AuthorizedPlayers.Add(Key);
		AuthorizedPlayerCount = AuthorizedPlayers.Num();
		EvaluateEndings();
	}
}

bool UEscapeProgressComponent::StartEndingById(const FName& EndingId)
{
	if (!GetOwner() || !GetOwner()->HasAuthority() || EndingId.IsNone())
	{
		return false;
	}

	for (const UDA_EndingData* EndingData : EndingDataAssets)
	{
		if (EndingData && EndingData->EndingId == EndingId)
		{
			StartEnding(EndingData);
			return true;
		}
	}

	return false;
}

void UEscapeProgressComponent::EvaluateEndings()
{
	if (!GetOwner() || !GetOwner()->HasAuthority())
	{
		return;
	}

	if (!CurrentEndingId.IsNone())
	{
		return;
	}

	for (const UDA_EndingData* EndingData : EndingDataAssets)
	{
		if (!EndingData)
		{
			continue;
		}

		if (IsEndingConditionMet(EndingData))
		{
			StartEnding(EndingData);
			return;
		}
	}
}
