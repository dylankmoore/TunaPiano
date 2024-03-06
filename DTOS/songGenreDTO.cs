using TunaPiano.Models;

namespace TunaPiano.DTOS
{
    public class SongGenreDTO
    {
        public int SongId { get; set; }
        public Song Song { get; set; }
        public int GenreId { get; set; }
        public Genre Genre { get; set; }
    }
}
