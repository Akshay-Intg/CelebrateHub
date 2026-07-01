// ============================================================
using CelebrateHub.Data.Models;

namespace CelebrateHub.Data.Repositories.Interfaces
{
    /// <summary>
    /// Data-access contract for PartyVote entities.
    /// </summary>
    public interface IPartyVoteRepository
    {
        /// <summary>Returns the voter's existing vote for this event, or null.</summary>
        Task<PartyVote?> GetVoteAsync(int partyEventId, int voterEmployeeId);

        Task<IEnumerable<PartyVote>> GetVotesByEventAsync(int partyEventId);

        Task<PartyVote> CastVoteAsync(PartyVote vote);

        /// <summary>Updates VoteType + UpdatedOn on an existing vote record.</summary>
        Task UpdateVoteAsync(PartyVote vote);
    }
}