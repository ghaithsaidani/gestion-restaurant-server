using Microsoft.EntityFrameworkCore;
using Restaurant_Backend.Models.DbModels;
using Restaurant_Backend.Models.RealTimeCommunication;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddSignalR();

// Dependancy Injection Of DbContext Class
builder.Services.AddDbContext<ApiDbContext>(option => option.UseSqlServer(builder.Configuration.GetConnectionString("DevConnection")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

var app = builder.Build();

app.UseCors("CorsPolicy");

app.MapHub<UpdateHub>("/api/updateHub");


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

app.Run();
