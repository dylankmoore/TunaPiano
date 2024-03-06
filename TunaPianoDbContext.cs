using Microsoft.EntityFrameworkCore;
using TunaPiano.Models;
using System.Runtime.CompilerServices;

public class TunaPianoDbContext : DbContext
{

    public DbSet<Artist> Artists { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Song> Songs { get; set; }
    public TunaPianoDbContext(DbContextOptions<TunaPianoDbContext> context) : base(context)
    {

    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // seed data with artists
        modelBuilder.Entity<Artist>().HasData(new Artist[]
        {
         new Artist {Id = 1, Name = "Linda Ronstadt", Age = 77, Bio = "Linda Ronstadt is one of the top-selling female rock artists of all time, and a trailblazer in successfully testing the boundaries of popular music across Rock, Pop, Latin, Folk, Country, Jazz, and Broadway." },
         new Artist {Id = 2, Name = "Carly Rae Jepsen", Age = 38, Bio = "Carly Rae Jepsen is a Canadian singer-songwriter and actress." },
         new Artist {Id = 3, Name = "Kate Bush", Age = 65, Bio = "Kate Bush is an English singer, songwriter, record producer and dancer. In 1978, at the age of 19, she topped the UK Singles Chart for four weeks with her debut single \"Wuthering Heights\", becoming the first female artist to achieve a UK number one with a self-written song." },
         new Artist {Id = 4, Name = "Dolly Parton", Age = 78, Bio = "Dolly Rebecca Parton is an American singer-songwriter, actress, and philanthropist, known primarily for her decades-long career in country music. " },
         new Artist {Id = 5, Name = "Lauryn Hill", Age = 48, Bio = "Lauryn Noelle Hill is an American rapper, singer, songwriter, and record producer. Hill is regarded as one of the greatest rappers of all time, as well as one of the most influential musicians of her generation."},
        });

        modelBuilder.Entity<Genre>().HasData(new Genre[]
         {
             new Genre {Id = 1, Description = "Folk Rock"},
             new Genre {Id = 2, Description = "Pop"},
             new Genre {Id = 3, Description = "Art Rock"},
             new Genre {Id = 4, Description = "Country"},
             new Genre {Id = 5, Description = "R&B"},
         });

        modelBuilder.Entity<Song>().HasData(new Song[]
         {
             new Song {Id = 1, Title = "Blue Bayou", ArtistId = 1, Album = "Simple Dreams", Length = 357},
             new Song {Id = 2, Title = "Western Wind", ArtistId = 2, Album = "The Loneliest Time", Length = 346},
             new Song {Id = 3, Title = "The Big Sky", ArtistId = 3, Album = "Hounds of Love", Length = 441},
             new Song {Id = 4, Title = "9 to 5", ArtistId = 4, Album = "9 to 5 and Odd Jobs", Length = 243},
             new Song {Id = 5, Title = "Ex-Factor", ArtistId = 5, Album = "The Miseducation of Lauryn Hill", Length = 527},
        });

    }
}