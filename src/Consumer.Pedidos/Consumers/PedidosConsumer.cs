using Consumer.Pedidos.Models;
using Consumer.Pedidos.Repositories;
using Consumer.Pedidos.Services;
using Consumer.Pedidos.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Consumer.Pedidos.Consumers;

public class PedidosConsumer : BackgroundService
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly MessageBrokerSettings? _messageBrokerSettings;
    private readonly CheckoutService? _checkoutService;
    private readonly PedidoRepository _pedidoRepository;

    private IConnection? _connection;
    private IModel? _model;

    public PedidosConsumer(IServiceProvider? serviceProvider, IOptions<MessageBrokerSettings>? messageBrokerSettings, CheckoutService? checkoutService, PedidoRepository pedidoRepository)
    {
        _serviceProvider = serviceProvider;
        _messageBrokerSettings = messageBrokerSettings.Value;
        _checkoutService = checkoutService;
        _pedidoRepository = pedidoRepository;

        #region Conexão com o servidor da mensageria

        var factory = new ConnectionFactory { Uri = new Uri(_messageBrokerSettings.ConnectionString) };
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_model);

        consumer.Received += async (sender, args) =>
        {
            //lendo o Payload da fila (dados da fila)
            var contentArray = args.Body.ToArray();
            var contentString = Encoding.UTF8.GetString(contentArray);

            //processando os dados da fila
            using (var scope = _serviceProvider.CreateScope())
            {
                //lendo a mensagem da fila e deserializando
                var pedidoModel = JsonConvert.DeserializeObject<PedidosModel>(contentString);
                var detalhesPedido = JsonConvert.DeserializeObject<DetalhesPedido>(pedidoModel.DetalhesPedido);

                //enviando o pedido para pagamento
                var result = await _checkoutService.PostAsync(detalhesPedido);

                //atualizando o banco de dados
                var status = result.Status.Equals("success") ? 2 : result.Status.Equals("error") ? 3 : 0;
                _pedidoRepository.Update(detalhesPedido.Id.Value, result.TransactionId.Value, status);

                //remover a mensagem da fila
                //_model.BasicAck(args.DeliveryTag, false);
            }
        };

        _model.BasicConsume(_messageBrokerSettings.QueueName, false, consumer);
    }
}