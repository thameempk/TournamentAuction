namespace TournamentAuction.Domain.Entities;

public class Bid
{
    public Guid BidId { get; set; }
    public Guid AuctionId { get; set; }
    public Guid PlayerId { get; set; }
    public Guid TeamId { get; set; }
    public decimal BidAmount { get; set; }
    public DateTime BidTime { get; set; }
}
