namespace DemandTrack.Models
{
    public enum DemandStatus
    {
        Draft = 0,
        Open = 1,
        PartiallyFulfilled = 2,
        Fulfilled = 3,
        Cancelled = 4
    }

    public enum DemandItemStatus
    {
        Pending = 0,
        Partial = 1,
        Fulfilled = 2
    }
}
