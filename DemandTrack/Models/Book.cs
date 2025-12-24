namespace DemandTrack.Models
{
    public class Book
    {
        public int Id { get; set; }

        public string Isbn { get; set; } = null!;
        public string Title { get; set; } = null!;
    }
}
