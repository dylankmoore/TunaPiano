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
app.MapPost("/api/songs", async (TunaPianoDbContext db, songCreationDTO creationDTO) =>
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
app.MapDelete("/api/songs/{id}", (TunaPianoDbContext db, int id) =>
{
    var deleteSong = db.Songs.SingleOrDefault(s => s.Id == id);
    if (deleteSong == null)
    {
        return Results.NotFound();
    }
    db.Songs.Remove(deleteSong);
    db.SaveChanges();
    return Results.NoContent();
});

//UPDATE A SONG
app.MapPut("/api/songs/{id}", (TunaPianoDbContext db, UpdateSongDTO updateDTO, int id) =>
{
    var updateSong = db.Songs.Find(id);
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
    var songs = db.Songs.Select(song => new SongDTO
    {
        Id = song.Id,
        Title = song.Title,
        ArtistId = song.ArtistId, 
        Album = song.Album,
        Length = song.Length
    }).ToList();

    return Results.Ok(songs);
});

//GET SONG DETAILS
app.MapGet("/api/songs/{id}", (TunaPianoDbContext db, int id) =>
{
    var songDetails = db.Songs
        .Include(s => s.Artist)
        .Include(s => s.Genres)
        .FirstOrDefault(s => s.Id == id);

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

app.Run();