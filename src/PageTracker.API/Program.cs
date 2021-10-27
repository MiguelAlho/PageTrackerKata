using Microsoft.AspNetCore.Mvc;
using PageTracker.API;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton(new Tracker());

var app = builder.Build();

app.MapGet("/counter/visitors", ([FromQuery] string uri, Tracker tracker) =>
{
    if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri? parsed)
        || !parsed.IsAbsoluteUri
        || !(parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps)  
        )
        return Results.BadRequest("uri is invalid");

    return Results.Ok(tracker.GetUniqueVisitorCount(parsed));
});

app.MapPost("/", ([FromBody] Visit visit, Tracker tracker) => {
    if (!visit.Uri.IsAbsoluteUri
        || !(visit.Uri.Scheme == Uri.UriSchemeHttp || visit.Uri.Scheme == Uri.UriSchemeHttps)
        )
        return Results.BadRequest("uri is invalid");
    
    if(string.IsNullOrWhiteSpace(visit.VisitorId))
        return Results.BadRequest("visitorId is invalid");

    tracker.RegisterVisit(visit.Uri, visit.VisitorId);
    return Results.NoContent();
});

app.Run();

internal record Visit(Uri Uri, string VisitorId);

public partial class Program
{
    // Expose the Program class for use with WebApplicationFactory<T>
}