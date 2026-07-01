// ============================================================
// BirthdayPortal.Data/Repositories/PartyVoteRepository.cs
// Place in: BirthdayPortal.Data → Repositories folder
// ============================================================

using CelebrateHub.Data;
using CelebrateHub.Data.Models;
using CelebrateHub.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CelebrateHub.Data.Repositories
{
    public class PartyVoteRepository : IPartyVoteRepository
    {
        private readonly AppDbContext _db;
        public PartyVoteRepository(AppDbContext db) => _db = db;

        public async Task<PartyVote?> GetVoteAsync(int partyEventId, int voterEmployeeId)
            => await _db.PartyVotes
                        .FirstOrDefaultAsync(v =>
                            v.PartyEventId == partyEventId &&
                            v.VoterEmployeeId == voterEmployeeId);

        public async Task<IEnumerable<PartyVote>> GetVotesByEventAsync(int partyEventId)
            => await _db.PartyVotes
                        .Include(v => v.VoterEmployee)
                        .Where(v => v.PartyEventId == partyEventId)
                        .OrderByDescending(v => v.VotedOn)
                        .ToListAsync();

        public async Task<PartyVote> CastVoteAsync(PartyVote vote)
        {
            _db.PartyVotes.Add(vote);
            await _db.SaveChangesAsync();
            return vote;
        }

        public async Task UpdateVoteAsync(PartyVote vote)
        {
            vote.UpdatedOn = DateTime.UtcNow;
            _db.PartyVotes.Update(vote);
            await _db.SaveChangesAsync();
        }
    }
}