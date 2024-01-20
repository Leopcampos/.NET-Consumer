
using Consumer.Pedidos.Settings;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Diagnostics;
using System.Text;

namespace Consumer.Pedidos.Consumers;

public class PedidosConsumer : BackgroundService
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly MessageBrokerSettings? _messageBrokerSettings;
    private IConnection? _connection;
    private IModel? _model;

    public PedidosConsumer(IServiceProvider? serviceProvider, IOptions<MessageBrokerSettings>? messageBrokerSettings)
    {
        _serviceProvider = serviceProvider;
        _messageBrokerSettings = messageBrokerSettings.Value;

        #region Conexão com o servidor da mensageria

        var factory = new ConnectionFactory
        {
            Uri = new Uri(_messageBrokerSettings.ConnectionString)
        };

        _connection = factory.CreateConnection();
        _model = _connection.CreateModel();
        _model.QueueDeclare(
            queue: _messageBrokerSettings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
            );

        #endregion
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_model);
        consumer.Received += (sender, args) =>
        {
            //lendo o Payload da fila (dados da fila)
            var contentArray = args.Body.ToArray();
            var contentString = Encoding.UTF8.GetString(contentArray);

            //processando os dados da fila
            using (var scope = _serviceProvider.CreateScope())
            {
                //TODO
                Debug.WriteLine(contentString);
            }
        };

        _model.BasicConsume(_messageBrokerSettings.QueueName, false, consumer);

        return Task.CompletedTask;
    }
}