using trustesseApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddAppInfrastructure();


builder.Services.AddServiceExt();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCustomSwagger();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        string swaggerJsonBasePath = string.IsNullOrWhiteSpace(options.RoutePrefix) ? "." : "..";
        options.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/Trustess/swagger.json",
        "Trustess Volunteer Assessment V1");
        options.RoutePrefix = "swagger";
        options.DisplayRequestDuration();
        options.EnableDeepLinking();
        options.EnableTryItOutByDefault();
    });

}


app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");

app.UseRateLimiter();

app.UseAuthentication();
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    await next();
});
app.UseAuthorization();

app.MapControllers();

app.Run();
