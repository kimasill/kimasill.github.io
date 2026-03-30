// Server/Server/Session/ClientSession_party.cs, Party.cs, PartyMatchingSystem.cs

public partial class ClientSession : PacketSession
{
    private Party _currentParty;
    public Party CurrentParty => _currentParty;

    public void CreateParty()
    {
        if (_currentParty != null)
            return;

        _currentParty = PartySystem.Instance.CreateParty();
        _currentParty?.AddMember(MyPlayer);
    }

    public void InviteToParty(ClientSession targetSession)
    {
        if (_currentParty == null)
            CreateParty();

        targetSession.JoinParty(_currentParty);
    }

    public void LeaveParty()
    {
        if (_currentParty == null)
            return;

        var membersSnapshot = new List<Player>(_currentParty.Members);
        int partyId = _currentParty.PartyId;

        _currentParty.RemoveMember(MyPlayer);

        if (_currentParty.Members.Count == 0)
            PartySystem.Instance.RemoveParty(partyId);

        var packet = new S_Party { PartyId = partyId };
        if (_currentParty != null && _currentParty.Members.Count > 0)
            packet.PartyMembers.AddRange(_currentParty.Members.Select(m => m.Info));

        foreach (Player member in membersSnapshot)
            member.Session?.Send(packet);

        _currentParty = null;
    }

    public void JoinParty(Party party)
    {
        if (party == null)
            return;

        if (_currentParty != null)
            LeaveParty();

        _currentParty = party;
        if (!_currentParty.AddMember(MyPlayer))
            return;

        var joinPacket = new S_Party
        {
            PartyId = _currentParty.PartyId
        };
        joinPacket.PartyMembers.AddRange(_currentParty.Members.Select(m => m.Info));

        foreach (Player member in _currentParty.Members)
            member.Session?.Send(joinPacket);
    }
}

public class Party
{
    public int PartyId { get; private set; }
    public List<Player> Members { get; } = new List<Player>();
    private const int MaxMembers = 4;

    public Party(int partyId)
    {
        PartyId = partyId;
    }

    public bool AddMember(Player player)
    {
        if (player == null || Members.Count >= MaxMembers || Members.Contains(player))
            return false;

        Members.Add(player);
        return true;
    }

    public void RemoveMember(Player player)
    {
        Members.Remove(player);
    }

    public bool IsFull() => Members.Count >= MaxMembers;
}

public class PartyMatchingSystem
{
    private readonly Dictionary<int, List<ClientSession>> _waitingLists = new Dictionary<int, List<ClientSession>>();

    public void Register(ClientSession session, int mapId)
    {
        if (!_waitingLists.TryGetValue(mapId, out var queue))
        {
            queue = new List<ClientSession>();
            _waitingLists[mapId] = queue;
        }

        if (queue.Contains(session))
            return;

        queue.Add(session);
        TryMatch(mapId);
    }

    public void Unregister(ClientSession session, int mapId)
    {
        if (_waitingLists.TryGetValue(mapId, out var queue))
            queue.Remove(session);
    }

    // 맵별 대기 4명이 모이면 파티 생성 후 EnterMap
    private void TryMatch(int mapId)
    {
        if (!_waitingLists.TryGetValue(mapId, out var queue) || queue.Count < 4)
            return;

        List<ClientSession> matched = queue.GetRange(0, 4);
        queue.RemoveRange(0, 4);

        var newParty = new Party(PartySystem.Instance.CreateParty().PartyId);
        foreach (var session in matched)
            session.JoinParty(newParty);

        EnterMap(newParty, mapId);
    }

    public void EnterMap(Party party, int mapId)
    {
        if (party == null || party.Members.Count == 0)
            return;

        GameRoom room = GameLogic.Instance.Add(mapId);
        if (!DataManager.MapDict.TryGetValue(mapId, out MapData mapData) || mapData == null)
            return;

        PortalData entryPortal = mapData.portals?.FirstOrDefault();
        if (entryPortal == null)
            return;

        foreach (Player member in party.Members)
        {
            if (member?.Session == null)
                continue;

            member.Session.ServerState = PlayerServerState.ServerStateGame;
            GameLogic.Instance.UpdateRoom(member.Room);
            room.Enqueue(member.Room.HandleMapChanged, member, mapData, entryPortal.id, room);
        }
    }
}
