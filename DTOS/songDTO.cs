namespace TunaPiano.DTOS
{
    public class SongDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int ArtistId { get; set; }
        public string Album { get; set; }
        public int Length { get; set; }
    }
}