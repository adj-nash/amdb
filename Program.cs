using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MovieDatabase.Data;
using MovieDatabase.Entities;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MovieDbContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddAutoMapper(typeof(Program));

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddCors(options => options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseCors("AllowAll");

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(@"C:\\Users\\alex_\\OneDrive\\Desktop\\dotnet\\MovieDatabase\\Images\\"),
    RequestPath = "/StaticFiles"
});

app.MapControllers();

app.Run();
