namespace TunaPiano.DTOS
{
    public class UpdateSongDTO
    {
        public string Title { get; set; }
        public int ArtistId { get; set; }
        public string Album { get; set; }
        public int Length { get; set; }
    }
}
