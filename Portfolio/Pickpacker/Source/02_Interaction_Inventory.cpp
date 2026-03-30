// Source:
// - Source/Blaster/Components/InteractionComponent.cpp
// - Source/Blaster/Components/PlayerInventoryComponent.cpp

void UInteractionComponent::UpdateTarget()
{
	ACharacter* OwnerCharacter = Cast<ACharacter>(GetOwner());
	if (!OwnerCharacter || !OwnerCharacter->IsLocallyControlled()) return;

	UCameraComponent* Camera = OwnerCharacter->FindComponentByClass<UCameraComponent>();
	const FVector CameraLocation = Camera
		? Camera->GetComponentLocation()
		: OwnerCharacter->GetActorLocation() + OwnerCharacter->GetActorRotation().RotateVector(TraceStartOffset);
	const FVector Direction = Camera ? Camera->GetForwardVector() : OwnerCharacter->GetActorForwardVector();
	const FVector Start = CameraLocation;
	const FVector End = CameraLocation + Direction * InteractionDistance;

	FCollisionQueryParams InteractionParams(SCENE_QUERY_STAT(InteractionTargetTrace), false);
	InteractionParams.AddIgnoredActor(OwnerCharacter);

	if (IsValid(CarriedParcel))
	{
		// Ignore the carried parcel so shelves and interactables stay targetable.
		InteractionParams.AddIgnoredActor(CarriedParcel);
		if (AActor* Carrier = CarriedParcel->GetAttachParentActor())
		{
			InteractionParams.AddIgnoredActor(Carrier);
		}
	}

	FHitResult InteractionHit;
	bool bInteractionTrace = GetWorld() &&
		GetWorld()->LineTraceSingleByChannel(InteractionHit, Start, End, TraceChannel, InteractionParams);

	if (!bInteractionTrace && GetWorld())
	{
		FHitResult SweepHit;
		const FCollisionShape Capsule = FCollisionShape::MakeCapsule(30.0f, 30.0f);
		if (GetWorld()->SweepSingleByChannel(SweepHit, Start, End, FQuat::Identity, TraceChannel, Capsule, InteractionParams))
		{
			InteractionHit = SweepHit;
			bInteractionTrace = true;
		}
	}

	// When carrying a parcel, search shelves on a separate channel and
	// keep the current target if distances are close to prevent flicker.
	AShelfActor* BestShelfFromTrace = nullptr;
	if (IsValid(CarriedParcel) && GetWorld())
	{
		TArray<FHitResult> ShelfHits;
		FCollisionQueryParams ShelfParams = InteractionParams;
		if (GetWorld()->LineTraceMultiByChannel(ShelfHits, Start, End, ECC_GameTraceChannel4, ShelfParams))
		{
			TMap<AShelfActor*, float> ShelfToDistance;
			for (const FHitResult& Hit : ShelfHits)
			{
				AShelfActor* Shelf = Cast<AShelfActor>(Hit.GetActor());
				if (!Shelf) continue;

				ShelfToDistance.Add(Shelf, FVector::Dist(Start, Hit.ImpactPoint));
			}

			// ... surface height sorting + hysteresis logic ...
		}
	}
}

bool UPlayerInventoryComponent::CollectItem(AParcelActor* Item)
{
	if (!Item || !Item->IsItem() || IsInventoryFull() || CollectedItems.Contains(Item))
	{
		return false;
	}

	if (!GetOwner() || !GetOwner()->HasAuthority())
	{
		Server_CollectItem(Item);
		return true;
	}

	CollectedItems.Add(Item);
	Item->SetActorHiddenInGame(true);
	if (UStaticMeshComponent* MeshComp = Cast<UStaticMeshComponent>(Item->GetRootComponent()))
	{
		MeshComp->SetCollisionEnabled(ECollisionEnabled::NoCollision);
		MeshComp->SetSimulatePhysics(false);
	}

	OnItemCollected.Broadcast(Item, CollectedItems.Num());
	UpdateInventoryUI();
	return true;
}

bool UPlayerInventoryComponent::PutCarriedParcelIntoInventory(int32 SlotIndex)
{
	if (!GetOwner() || !GetOwner()->HasAuthority())
	{
		Server_PutCarriedParcelIntoInventory(SlotIndex);
		return true;
	}

	ACharacter* OwnerCharacter = Cast<ACharacter>(GetOwner());
	ABlasterCharacter* BlasterCharacter = Cast<ABlasterCharacter>(OwnerCharacter);
	UInteractionComponent* InteractionComp = BlasterCharacter ? BlasterCharacter->GetInteractionComponent() : nullptr;
	AParcelActor* CarriedParcel = InteractionComp ? InteractionComp->GetCarriedParcel() : nullptr;

	if (!CarriedParcel || !CarriedParcel->IsItem())
	{
		return false;
	}

	int32 TargetSlot = SlotIndex;
	if (TargetSlot < 0 || TargetSlot >= MaxInventorySize)
	{
		for (int32 i = 0; i < MaxInventorySize; ++i)
		{
			if (!CollectedItems.IsValidIndex(i) || CollectedItems[i] == nullptr)
			{
				TargetSlot = i;
				break;
			}
		}
	}

	if (TargetSlot < 0 || TargetSlot >= MaxInventorySize) return false;
	if (CollectedItems.IsValidIndex(TargetSlot) && CollectedItems[TargetSlot] != nullptr) return false;

	while (CollectedItems.Num() <= TargetSlot)
	{
		CollectedItems.Add(nullptr);
	}

	CarriedParcel->RequestDrop(FVector::ZeroVector);
	CollectedItems[TargetSlot] = CarriedParcel;

	CarriedParcel->SetActorHiddenInGame(true);
	if (UStaticMeshComponent* MeshComp = CarriedParcel->GetParcelMesh())
	{
		MeshComp->SetCollisionEnabled(ECollisionEnabled::NoCollision);
		MeshComp->SetSimulatePhysics(false);
	}

	InteractionComp->SetCarriedParcel(nullptr);
	OnItemCollected.Broadcast(CarriedParcel, CollectedItems.Num());
	UpdateInventoryUI();
	return true;
}

void UPlayerInventoryComponent::OnRep_CollectedItems()
{
	UpdateInventoryUI();
}
