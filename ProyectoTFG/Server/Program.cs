using Microsoft.AspNetCore.ResponseCompression;
using ProyectoTFG.Server.Models.Interfaces;
using ProyectoTFG.Server.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Inyectamos mongoDb
builder.Services.AddScoped<IAccesoDatos, MongoDBService>();

byte[] _bytesFirma = System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:firmaJWT"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                 .AddJwtBearer(
                        (JwtBearerOptions opciones) => {
                            opciones.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                            {
                                ValidateIssuer = true, //<--- validar si el jwt ha sido generado por mi servidor (claim registrado "issuer")
                                ValidateLifetime = true, //<-- validar la fecha de expiracion del jwt (claim registrado "exp")
                                ValidateIssuerSigningKey = true, //<--- valildar si el jwt ha sido firmado por mi servidor
                                //configuro firma q tengo q comprobar y nombre del servidor a comprobar...
                                ValidIssuer = builder.Configuration["JWT:issuer"],
                                ValidateAudience = false, //<--- obligado pq por defecto SIEMPRE intenta acceder a que subdominios son validos para los jwt (audience)
                                IssuerSigningKey = new SymmetricSecurityKey(_bytesFirma)
                            };
                        }
                   )
                 .AddGoogle(
                        (GoogleOptions options) =>
                        {
                            // opciones de configuracion del cliente oauth2 que va a usar nuestro servidor para
                            // conectarse a google y solicitarle codigo de cliente, urls, jwt para uso de apis, ...
                            options.ClientId = builder.Configuration["Google:client_id"];
                            options.ClientSecret = builder.Configuration["Google:client_secret"];
                        }
                 );

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
