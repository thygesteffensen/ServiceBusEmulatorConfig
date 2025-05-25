using MudBlazor.Services;
using ServiceBusEmulatorConfig.Web.State;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IApplicationState, ApplicationState>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();