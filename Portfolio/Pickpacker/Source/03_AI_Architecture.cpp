// Source:
// - Source/Blaster/AI/PPAIControllerBase.cpp
// - Source/Blaster/AI/PPPatrolBoundsLibrary.cpp
// - Source/Blaster/AI/UPPSightPerceptionComponent.cpp

bool APPAIControllerBase::RunBehaviorTreeWithBlackboard(UBehaviorTree* BTAsset, UBlackboardData* BBOverride)
{
	if (!BTAsset)
	{
		UE_LOG(LogTemp, Warning, TEXT("[PPAIControllerBase] RunBehaviorTreeWithBlackboard: BehaviorTree is null"));
		return false;
	}

	UBlackboardData* BBToUse = BBOverride ? BBOverride : BTAsset->BlackboardAsset.Get();
	if (!BBToUse)
	{
		UE_LOG(LogTemp, Warning, TEXT("[PPAIControllerBase] No BlackboardData available"));
		return false;
	}

	UBlackboardComponent* BlackboardComp = nullptr;
	if (!UseBlackboard(BBToUse, BlackboardComp))
	{
		UE_LOG(LogTemp, Error, TEXT("[PPAIControllerBase] UseBlackboard failed"));
		return false;
	}

	const bool bStarted = RunBehaviorTree(BTAsset);
	bPPBehaviorTreeRunning = bStarted;
	return bStarted;
}

FVector PPPatrolBoundsLibrary::GenerateRandomPatrolPointHorizontal(
	const FVector& Center,
	const FVector& Extent,
	float MinPatrolRadius,
	float MaxPatrolRadius)
{
	FVector RandomPoint;
	RandomPoint.X = FMath::RandRange(Center.X - Extent.X, Center.X + Extent.X);
	RandomPoint.Y = FMath::RandRange(Center.Y - Extent.Y, Center.Y + Extent.Y);
	RandomPoint.Z = Center.Z;

	FVector ToPoint = RandomPoint - Center;
	ToPoint.Z = 0.0f;
	const float Distance = ToPoint.Size();

	if (Distance < MinPatrolRadius)
	{
		ToPoint = ToPoint.GetSafeNormal() * MinPatrolRadius;
		RandomPoint = Center + ToPoint;
		RandomPoint.Z = Center.Z;
	}
	else if (Distance > MaxPatrolRadius)
	{
		ToPoint = ToPoint.GetSafeNormal() * MaxPatrolRadius;
		RandomPoint = Center + ToPoint;
		RandomPoint.Z = Center.Z;
	}

	return RandomPoint;
}

void UPPSightPerceptionComponent::ApplySightParameters(
	float SightRadius,
	float PeripheralVisionAngleDegrees,
	float LoseSightRadiusMultiplier)
{
	if (!SightConfig)
	{
		return;
	}

	SightConfig->SightRadius = SightRadius;
	SightConfig->LoseSightRadius = SightRadius * LoseSightRadiusMultiplier;
	SightConfig->PeripheralVisionAngleDegrees = PeripheralVisionAngleDegrees;
	SightConfig->SetMaxAge(2.0f);
	SightConfig->DetectionByAffiliation.bDetectEnemies = true;
	SightConfig->DetectionByAffiliation.bDetectFriendlies = true;
	SightConfig->DetectionByAffiliation.bDetectNeutrals = true;

	ConfigureSense(*SightConfig);
	SetDominantSense(UAISense_Sight::StaticClass());
}
