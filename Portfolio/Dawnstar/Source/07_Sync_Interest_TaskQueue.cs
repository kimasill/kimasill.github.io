// Server/Server/Game/Job/TaskQueue.cs
// 룸마다 작업 큐; ExecuteAll에서 타이머·큐 작업 순차 실행
public class TaskQueue
{
    TaskTimer _timer = new TaskTimer();
    ConcurrentQueue<IJob> _jobQueue = new ConcurrentQueue<IJob>();

    public IJob EnqueueAfter(int tickAfter, Action action) { return EnqueueAfter(tickAfter, new Job(action)); }
    public IJob EnqueueAfter<T1>(int tickAfter, Action<T1> action, T1 t1) { return EnqueueAfter(tickAfter, new Job<T1>(action, t1)); }
    public IJob EnqueueAfter<T1, T2>(int tickAfter, Action<T1, T2> action, T1 t1, T2 t2) { return EnqueueAfter(tickAfter, new Job<T1, T2>(action, t1, t2)); }
    public IJob EnqueueAfter<T1, T2, T3>(int tickAfter, Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { return EnqueueAfter(tickAfter, new Job<T1, T2, T3>(action, t1, t2, t3)); }
    public IJob EnqueueAfter<T1, T2, T3, T4>(int tickAfter, Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4) { return EnqueueAfter(tickAfter, new Job<T1, T2, T3, T4>(action, t1, t2, t3, t4)); }

    public IJob EnqueueAfter(int tickAfter, IJob job)
    {
        _timer.Enqueue(job, tickAfter);
        return job;
    }
    public void Enqueue(Action action) { Enqueue(new Job(action)); }
    public void Enqueue<T1>(Action<T1> action, T1 t1) { Enqueue(new Job<T1>(action, t1)); }
    public void Enqueue<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2) { Enqueue(new Job<T1, T2>(action, t1, t2)); }
    public void Enqueue<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { Enqueue(new Job<T1, T2, T3>(action, t1, t2, t3)); }
    public void Enqueue<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4) { Enqueue(new Job<T1, T2, T3, T4>(action, t1, t2, t3, t4)); }

    public void Enqueue(IJob job)
    {
        _jobQueue.Enqueue(job);
    }
    public void Enqueue(Func<Task> asyncAction)
    {
        Enqueue(new AsyncJob(asyncAction));
    }

    public void ExecuteAll()
    {
        _timer.ExecuteAll();
        while (true)
        {
            IJob job = Pop();
            if (job == null)
                return;
            job.Execute();
        }
    }

    private IJob Pop()
    {
        if (_jobQueue.TryDequeue(out IJob job))
            return job;
        return null;
    }
}

public class AsyncJob : IJob
{
    private readonly Func<Task> _asyncAction;

    public AsyncJob(Func<Task> asyncAction)
    {
        _asyncAction = asyncAction;
    }

    public override void Execute()
    {
        _asyncAction().GetAwaiter().GetResult();
    }
}

// Server/Server/Game/Room/GameRoom.cs (발췌)

public partial class GameRoom : TaskQueue
{
    public const int VisionCells = 20;

    public void Update()
    {
        ExecuteAll();
    }

    public void Broadcast(Vector2Int pos, IMessage packet, Player excludePlayer = null)
    {
        List<Zone> zones = GetAdjacentZone(pos);
        foreach (Player p in zones.SelectMany(z => z.Players))
        {
            if (p == excludePlayer)
                continue;
            int dx = p.CellPos.x - pos.x;
            int dy = p.CellPos.y - pos.y;
            if (Math.Abs(dx) > GameRoom.VisionCells || Math.Abs(dy) > GameRoom.VisionCells)
                continue;
            p.Session.Send(packet);
        }
    }

    public List<Zone> GetAdjacentZone(Vector2Int cellPos, int range = GameRoom.VisionCells)
    {
        HashSet<Zone> zones = new HashSet<Zone>();

        int maxY = cellPos.y + range;
        int minY = cellPos.y - range;
        int maxX = cellPos.x + range;
        int minX = cellPos.x - range;

        if (ZoneCells == 0)
            throw new InvalidOperationException("ZoneCells cannot be zero.");

        Vector2Int topLeft = new Vector2Int(minX, maxY);
        int minIndexY = (Map.MaxY - topLeft.y) / ZoneCells;
        int minIndexX = (topLeft.x - Map.MinX) / ZoneCells;

        Vector2Int bottomRight = new Vector2Int(maxX, minY);
        int maxIndexY = (Map.MaxY - bottomRight.y) / ZoneCells;
        int maxIndexX = (bottomRight.x - Map.MinX) / ZoneCells;

        for (int x = minIndexX; x <= maxIndexX; x++)
        {
            for (int y = minIndexY; y <= maxIndexY; y++)
            {
                Zone zone = GetZone(y, x);
                if (zone == null)
                    continue;
                zones.Add(zone);
            }
        }

        int[] delta = new int[2] { -range, +range };
        foreach (int dy in delta)
        {
            foreach (int dx in delta)
            {
                int y = cellPos.y + dy;
                int x = cellPos.x + dx;
                Zone zone = GetZone(new Vector2Int(x, y));
                if (zone == null)
                    continue;
                zones.Add(zone);
            }
        }
        return zones.ToList();
    }
}

// Server/Server/Game/Room/InterestManagement.cs

// 플레이어 시야: 이전 집합과의 차이만 Spawn/Despawn 패킷
public class InterestManagement
{
    public Player Owner { get; private set; }
    public HashSet<GameObject> PreviousObjects { get; private set; } = new HashSet<GameObject>();

    public InterestManagement(Player owner)
    {
        Owner = owner;
    }

    public void Refresh()
    {
        PreviousObjects.Clear();
    }

    public HashSet<GameObject> GatherObjects()
    {
        if (Owner == null || Owner.Room == null)
            return null;

        HashSet<GameObject> objects = new HashSet<GameObject>();
        Vector2Int cellPos = Owner.CellPos;
        List<Zone> zones = Owner.Room.GetAdjacentZone(cellPos);

        foreach (Zone zone in zones)
        {
            if (zone == null)
                continue;

            foreach (Player player in zone.Players)
            {
                if (player == null)
                    continue;
                int dx = player.CellPos.x - cellPos.x;
                int dy = player.CellPos.y - cellPos.y;
                if (Math.Abs(dx) > GameRoom.VisionCells || Math.Abs(dy) > GameRoom.VisionCells)
                    continue;
                objects.Add(player);
            }

            foreach (Monster monster in zone.Monsters)
            {
                if (monster == null)
                    continue;
                int dx = monster.CellPos.x - cellPos.x;
                int dy = monster.CellPos.y - cellPos.y;
                if (Math.Abs(dx) > GameRoom.VisionCells || Math.Abs(dy) > GameRoom.VisionCells)
                    continue;
                objects.Add(monster);
            }

            foreach (Projectile projectile in zone.Projectiles)
            {
                if (projectile == null)
                    continue;
                int dx = projectile.CellPos.x - cellPos.x;
                int dy = projectile.CellPos.y - cellPos.y;
                if (Math.Abs(dx) > GameRoom.VisionCells || Math.Abs(dy) > GameRoom.VisionCells)
                    continue;
                objects.Add(projectile);
            }

            foreach (Magic magic in zone.Magics)
            {
                if (magic == null)
                    continue;
                int dx = magic.CellPos.x - cellPos.x;
                int dy = magic.CellPos.y - cellPos.y;
                if (Math.Abs(dx) > GameRoom.VisionCells || Math.Abs(dy) > GameRoom.VisionCells)
                    continue;
                objects.Add(magic);
            }
        }
        return objects;
    }

    public void Update()
    {
        if (Owner == null || Owner.Room == null)
            return;

        HashSet<GameObject> currentObjects = GatherObjects();

        List<GameObject> added = currentObjects.Except(PreviousObjects).ToList();
        if (added.Count > 0)
        {
            S_Spawn spawnPacket = new S_Spawn();
            foreach (GameObject obj in added)
            {
                if (obj == Owner)
                    continue;
                else if (obj.ObjectType == GameObjectType.Player)
                {
                    Player player = obj as Player;
                    if (player.Session == null)
                        continue;
                    // 프로젝트: 시야에 잡힌 타 플레이어 장비 정보를 룸 큐에서 지연 동기화
                    Owner.Room.EnqueueAfter(100, Owner.Room.HandleEquippedItemList, Owner, player, true);
                }
                ObjectInfo info = new ObjectInfo();
                info.MergeFrom(obj.Info);
                spawnPacket.Objects.Add(info);
            }
            Owner.Session.Send(spawnPacket);
        }

        List<GameObject> removed = PreviousObjects.Except(currentObjects).ToList();
        if (removed.Count > 0)
        {
            S_Despawn despawnPacket = new S_Despawn();
            foreach (GameObject obj in removed)
                despawnPacket.ObjectId.Add(obj.Id);
            Owner.Session.Send(despawnPacket);
        }
        PreviousObjects = currentObjects;
        // 프로젝트: 시야 갱신 주기(틱) 예약 — 맵·부하에 맞게 조정 가능
        Owner.Room.EnqueueAfter(500, Update);
    }
}
