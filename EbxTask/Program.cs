using EbxTask.API.Middleware;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocal",
        builder => builder.WithOrigins("http://localhost:3000")
        .AllowAnyMethod()
        .AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowLocal");
app.UseExceptionMiddleware();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();
