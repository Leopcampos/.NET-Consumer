using Consumer.Pedidos.Consumers;
using Consumer.Pedidos.Settings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<MessageBrokerSettings>(builder.Configuration.GetSection("MessageBrokerSettings"));
builder.Services.Configure<CheckoutSettings>(builder.Configuration.GetSection("CheckoutSettings"));
builder.Services.AddHostedService<PedidosConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
