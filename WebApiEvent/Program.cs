using Microsoft.EntityFrameworkCore;
using WebApiEvent;
using WebApiEvent.CustomMiddleware;
using WebApiEvent.Data;
using WebApiEvent.DataAccess;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Events.Any())
    {
        db.Events.AddRange(SeedData.GetEvents());
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAll");
}
else
    app.UseCors("Prod");

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();