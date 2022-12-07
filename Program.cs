using api.examenfinal.Data;
using api.examenfinal.Models;
using Api.Autenticacion.Jwt;
using Api.Autenticacion.Jwt.Interfaces;
using Api.Autenticacion.Jwt.Models;
using Api.Autenticacion.Jwt.Repositories;
using Api.Autenticacion.Jwt.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddTransient<IUserRepository, UsersInMemoryRepository>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<IAuthenticationService, AuthenticationService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("PostgreSQLConnection");

builder.Services.AddDbContext<ExamenFinalApp>(options =>
    options.UseNpgsql(connectionString));

var info = new OpenApiInfo
{
    Title = "Curso JWT"
};
var security = new OpenApiSecurityScheme()
{
    Name = "Autorización",
    Type = SecuritySchemeType.ApiKey,
    Scheme = "Bearer",
    BearerFormat = "JWT",
    In = ParameterLocation.Header,
    Description = "Introducir token"
};
var requirement = new OpenApiSecurityRequirement()
{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id   = "Bearer"
            }
        },
        new List<string>()
    }
};
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", security);
    options.AddSecurityRequirement(requirement);
});
builder.Services.Configure<TokenSettings>(
    builder.Configuration.GetSection(nameof(TokenSettings)));

builder.Services.AddAuthentication()
    .AddJwtBearer("CURSO-UA", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["TokenSettings:Issuer"],
            ValidAudience = builder.Configuration["TokenSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder
                .Configuration["TokenSettings:Secret"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddAuthenticationSchemes("CURSO-UA")
        .Build();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();

    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();


app.MapPost("/token", [AllowAnonymous] async (IUserService userService,
    IAuthenticationService authenticationService,
    AuthenticationRequest request) =>
{
    var isValidAuthentication = await authenticationService
        .Authenticate(request.Username, request.Password);

    if (isValidAuthentication)
    {
        var user = await userService.GetByCredentials(request.Username, request.Password);

        var token = await authenticationService.GenerateJwt(user);

        return Results.Ok(new { AccessToken = token });
    }

    return Results.Forbid();
});


app.MapPost("/funcionario/", async (Funcionario funcionario, ExamenFinalApp db) =>
{
    db.Funcionario.Add(funcionario);
    await db.SaveChangesAsync();

    return Results.Created($"/cliente/{funcionario.Id}", funcionario);
});

app.MapGet("/funcionario/{Id:int}", async (int Id, ExamenFinalApp db) =>
{
    return await db.Funcionario.FindAsync(Id) is Funcionario funcionario ? Results.Ok(funcionario) : Results.NotFound();
});

app.MapPut("/funcionario/{Id}", async (int Id, Funcionario funcionario, ExamenFinalApp db) =>
{
    var func = await db.Funcionario.FindAsync(Id);

    if (func is null) return Results.NotFound();

    func.Id = funcionario.Id;
    func.Nombres = funcionario.Nombres;
    func.Apellidos = funcionario.Apellidos;
    func.Documento = funcionario.Documento;
    func.Telefono = funcionario.Telefono;
    func.Email = funcionario.Email;
    func.FechaNacimiento = funcionario.FechaNacimiento;
    func.Ciudad = funcionario.Ciudad;
    func.Nacionalidad = funcionario.Nacionalidad;
    func.Cargo = funcionario.Cargo;
    func.Antiguedad = funcionario.Antiguedad;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/funcionario/{Id}", async (int Id, ExamenFinalApp db) =>
{
    if (await db.Funcionario.FindAsync(Id) is Funcionario func)
    {
        db.Funcionario.Remove(func);
        await db.SaveChangesAsync();
        return Results.Ok(func);
    }

    return Results.NotFound();
});



app.Run();

