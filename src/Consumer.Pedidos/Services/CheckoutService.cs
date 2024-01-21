using Consumer.Pedidos.Models;
using Consumer.Pedidos.Settings;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Consumer.Pedidos.Services
{
    /// <summary>
    /// Serviços para integração com a API de pagamentos
    /// </summary>
    public class CheckoutService
    {
        private readonly IOptions<CheckoutSettings>? _checkoutSettings;

        public CheckoutService(IOptions<CheckoutSettings>? checkoutSettings)
        {
            _checkoutSettings = checkoutSettings;
        }

        public async Task<CheckoutResponse> PostAsync(PedidosModel model)
        {
            //realizando a autenticação
            var auth = await CreateAuthAsync();

            //acessando o endpoint da api de pagamento para realizar o checkout
            using (var httpClient = new HttpClient())
            {
                //configurando o token de autenticação obtido para acessar o endpoint
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

                //montando os dados que serão enviados para a API
                var checkoutRequest = new CheckoutRequest
                {
                    Total = model.DetalhesPedido.Valor,
                    Customer = new CustomerRequest
                    {
                        Name = model.DetalhesPedido.Cliente.Nome,
                        Email = model.DetalhesPedido.Cliente.Email,
                        Cpf = model.DetalhesPedido.Cliente.Cpf
                    },
                    CreditCard = new CreditcardRequest
                    {
                        CardNumber = model.DetalhesPedido.Cobranca.NumeroCartao,
                        CardholderName = model.DetalhesPedido.Cobranca.NomeImpressoNoCartao,
                        ExpirationMonth = model.DetalhesPedido.Cobranca.MesValidade,
                        ExpirationYear = model.DetalhesPedido.Cobranca.AnoValidade,
                        SecurityCode = model.DetalhesPedido.Cobranca.CodigoSeguranca
                    }
                };

                //fazendo a requisição para executar o pagamento
                var content = new StringContent(JsonConvert.SerializeObject(checkoutRequest),
                    Encoding.UTF8, "application/json");

                var result = await httpClient.PostAsync(_checkoutSettings.Value.ApiUrl + "/checkout", content);

                var builder = new StringBuilder();
                using (var response = result.Content)
                {
                    Task<string> task = response.ReadAsStringAsync();
                    builder.Append(task.Result);
                }

                return JsonConvert.DeserializeObject<CheckoutResponse>(builder.ToString());
            }
        }

        private async Task<AuthResponse> CreateAuthAsync()
        {
            using (var httpClient = new HttpClient())
            {
                var authRequest = new AuthRequest
                {
                    Client = _checkoutSettings.Value.Client,
                    Password = _checkoutSettings.Value.Password
                };

                var content = new StringContent(JsonConvert.SerializeObject(authRequest),
                    Encoding.UTF8, "application/json");

                var result = await httpClient.PostAsync(_checkoutSettings.Value.ApiUrl + "/auth", content);

                var builder = new StringBuilder();
                using (var response = result.Content)
                {
                    Task<string> task = response.ReadAsStringAsync();
                    builder.Append(task.Result);
                }

                return JsonConvert.DeserializeObject<AuthResponse>(builder.ToString());
            }
        }
    }

    public class AuthRequest
    {
        public string? Client { get; set; }
        public string? Password { get; set; }
    }

    public class AuthResponse
    {
        public string? Status { get; set; }
        public string? Client { get; set; }
        public string? AccessToken { get; set; }
    }

    public class CheckoutRequest
    {
        public decimal? Total { get; set; }
        public CustomerRequest? Customer { get; set; }
        public CreditcardRequest? CreditCard { get; set; }
    }

    public class CustomerRequest
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Cpf { get; set; }
    }

    public class CreditcardRequest
    {
        public string? CardNumber { get; set; }
        public string? CardholderName { get; set; }
        public int? ExpirationMonth { get; set; }
        public int? ExpirationYear { get; set; }
        public int? SecurityCode { get; set; }
    }

    public class CheckoutResponse
    {
        public Guid? TransactionId { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
        public OrderResponse? Order { get; set; }
        public int? Total { get; set; }
    }
    public class OrderResponse
    {
        public string? Id { get; set; }
        public string? Date { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerCpf { get; set; }
    }

}