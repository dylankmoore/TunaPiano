using TunaPiano.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using TunaPiano.DTOS;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core
builder.Services.AddNpgsql<TunaPianoDbContext>(builder.Configuration["TunaPianoDbConnectionString"]);

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//CREATE A SONG
app.MapPost("/api/songs", async (TunaPianoDbContext db, SongDTO creationDTO) =>
{
    var song = new Song
    {
        Title = creationDTO.Title,
        ArtistId = creationDTO.ArtistId,
        Album = creationDTO.Album,
        Length = creationDTO.Length
    };

    db.Songs.Add(song);
    await db.SaveChangesAsync();

    return Results.Created($"/api/songs/{song.Id}", 
    new { song.Id, song.Title, song.ArtistId, song.Album, song.Length });
});

//DELETE A SONG
app.MapDelete("/api/songs/{songId}", (TunaPianoDbContext db, int songId) =>
{
    var deleteSong = db.Songs.SingleOrDefault(s => s.Id == songId);
    if (deleteSong == null)
    {
        return Results.NotFound();
    }
    db.Songs.Remove(deleteSong);
    db.SaveChanges();
    return Results.NoContent();
});

//UPDATE A SONG
app.MapPut("/api/songs/{songId}", (TunaPianoDbContext db, SongDTO updateDTO, int songId) =>
{
    var updateSong = db.Songs.Find(songId);
    if (updateSong == null)
    {
        return Results.NotFound();
    }

    updateSong.Title = updateDTO.Title;
    updateSong.ArtistId = updateDTO.ArtistId;
    updateSong.Album = updateDTO.Album;
    updateSong.Length = updateDTO.Length;

    db.SaveChanges(); 

    var response = new
    {
        id = updateSong.Id,
        title = updateSong.Title,
        artist_id = updateSong.ArtistId,
        album = updateSong.Album,
        length = updateSong.Length
    };

    return Results.Ok(response);
});

//GET ALL SONGS
app.MapGet("/api/songs", (TunaPianoDbContext db) =>
{
    var songs = db.Songs.Select(song => new
    {
        song.Id,
        song.Title,
        song.ArtistId,
        song.Album,
        song.Length
    }).ToList();

    return Results.Ok(songs);
});

//GET SONG DETAILS
app.MapGet("/api/songs/{songId}", (TunaPianoDbContext db, int songId) =>
{
    var songDetails = db.Songs
        .Include(s => s.Artist)
        .Include(s => s.Genres)
        .FirstOrDefault(s => s.Id == songId);

    if (songDetails == null)
    {
        return Results.NotFound();
    }

    var response = new
    {
        id = songDetails.Id,
        title = songDetails.Title,
        artist = new
        {
            id = songDetails.Artist.Id,
            name = songDetails.Artist.Name,
            age = songDetails.Artist.Age,
            bio = songDetails.Artist.Bio
        },
        album = songDetails.Album,
        length = songDetails.Length,
        genres = songDetails.Genres.Select(g => new
        {
            id = g.Id,
            description = g.Description
        }).ToList()
    };

    return Results.Ok(response);
});

//CREATE AN ARTIST
app.MapPost("/api/artists", (TunaPianoDbContext db, TunaPiano.DTOS.ArtistDTO createArtistDTO) =>
{
    var artist = new TunaPiano.Models.Artist
    {
        Name = createArtistDTO.Name,
        Age = createArtistDTO.Age,
        Bio = createArtistDTO.Bio,
        Songs = new List<Song>()
    };

    db.Artists.Add(artist);
    db.SaveChanges();

    var response = new
    {
        id = artist.Id,
        name = artist.Name,
        age = artist.Age,
        bio = artist.Bio
    };

    return Results.Created($"/artists/{artist.Id}", response);
});

//DELETE AN ARTIST
app.MapDelete("/api/artists/{artistId}", (TunaPianoDbContext db, int artistId) =>
{
    var deleteArtist = db.Artists.SingleOrDefault(a => a.Id == artistId);
    if (deleteArtist == null)
    {
        return Results.NotFound();
    }

    db.Artists.Remove(deleteArtist);
    db.SaveChanges();
    return Results.NoContent();
});

//UPDATE AN ARTIST
app.MapPut("/api/artists/{artistId}", (TunaPianoDbContext db, TunaPiano.DTOS.ArtistDTO updateDTO, int artistId) =>
{
    var updateArtist = db.Artists.Find(artistId);
    if (updateArtist == null)
    {
        return Results.NotFound();
    }

    updateArtist.Name = updateDTO.Name;
    updateArtist.Age = updateDTO.Age;
    updateArtist.Bio = updateDTO.Bio;

    db.SaveChanges();

    var response = new
    {
        id = updateArtist.Id,
        name = updateArtist.Name,
        age = updateArtist.Age,
        bio = updateArtist.Bio
    };

    return Results.Ok(response);
});

//GET ALL ARTISTS
app.MapGet("/api/artists", (TunaPianoDbContext db) =>
{
    var artists = db.Artists.Select(artist => new
    {
        artist.Id,
        artist.Name,
        artist.Age,
        artist.Bio
    }).ToList();

    return Results.Ok(artists);
});

//GET ARTIST DETAILS
app.MapGet("/artists/{artistId}", (TunaPianoDbContext db, int artistId) =>
{
    var artistDetails = db.Artists
        .Include(a => a.Songs)
        .FirstOrDefault(a => a.Id == artistId);

    if (artistDetails == null)
    {
        return Results.NotFound();
    }

    var response = new
    {
        id = artistDetails.Id,
        name = artistDetails.Name,
        age = artistDetails.Age,
        bio = artistDetails.Bio,
        song_count = artistDetails.Songs.Count,
        songs = artistDetails.Songs.Select(s => new
        {
            id = s.Id,
            title = s.Title,
            album = s.Album,
            length = s.Length
        }).ToList()
    };

    return Results.Ok(response);
});

//CREATE A GENRE
app.MapPost("/api/genres", async (TunaPianoDbContext db, GenreDTO genreDto) =>
{
     var newGenre = new Genre
    {
        Description = genreDto.Description
    };

    await db.Genres.AddAsync(newGenre);
    await db.SaveChangesAsync();

    var response = new { Id = newGenre.Id, newGenre.Description };

    return Results.Created($"/api/genres/{newGenre.Id}", response);
});

//DELETE A GENRE
app.MapDelete("/api/genres/{genreId}", (TunaPianoDbContext db, int genreId) =>
{
    var deleteGenre = db.Genres.Find(genreId);
    if (deleteGenre == null)
    {
        return Results.NotFound();
    }

    db.Genres.Remove(deleteGenre);
    db.SaveChanges();
    return Results.NoContent();
});

//UPDATE A GENRE
app.MapPut("/api/genres/{genreId}", (TunaPianoDbContext db, GenreDTO updateDTO, int genreId) =>
{
    var updateGenre = db.Genres.Find(genreId);
    if (updateGenre == null)
    {
        return Results.NotFound();
    }

    updateGenre.Description = updateDTO.Description;

    db.SaveChanges();

    var response = new
    {
        id = updateGenre.Id,
        description = updateGenre.Description
    };

    return Results.Ok(response);
});

//GET ALL GENRES
app.MapGet("/api/genres", async (TunaPianoDbContext db) =>
{
    var genres = await db.Genres.Select(genre => new
    {
        Id = genre.Id,
        Description = genre.Description
    }).ToListAsync();

    return Results.Ok(genres);
});

//GET GENRE DETAILS
app.MapGet("/api/genres/{genreId}", (TunaPianoDbContext db, int genreId) =>
{
    var genreDetails = db.Genres
        .Include(g => g.Songs)
        .FirstOrDefault(g => g.Id == genreId);

    if (genreDetails == null)
    {
        return Results.NotFound();
    }

    var response = new
    {
        id = genreDetails.Id,
        description = genreDetails.Description,
        songs = genreDetails.Songs.Select(s => new
        {
            id = s.Id,
            title = s.Title,
            artist_id = s.ArtistId,
            album = s.Album,
            length = s.Length
        }).ToList()
    };

    return Results.Ok(response);
});

app.Run();