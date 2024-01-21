namespace Consumer.Pedidos.Models;

public class PedidosModel
{
    public Guid? Id { get; set; }
    public DateTime? DataHoraCriacao { get; set; }
    public DetalhesPedido? DetalhesPedido { get; set; }
}

public class DetalhesPedido
{
    public Guid? Id { get; set; }
    public DateTime? DataHora { get; set; }
    public decimal? Valor { get; set; }
    public List<Produto>? Produtos { get; set; }
    public string? Status { get; set; }
    public string? ProtocoloPagamento { get; set; }
    public Guid? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public Guid? CobrancaId { get; set; }
    public Cobranca? Cobranca { get; set; }
}

public class Cliente
{
    public string? Id { get; set; }
    public string? Nome { get; set; }
    public string? Cpf { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
    public string? Complemento { get; set; }
    public string? Numero { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Uf { get; set; }
    public string? Cep { get; set; }
}

public class Cobranca
{
    public Guid? Id { get; set; }
    public string? NumeroCartao { get; set; }
    public string? NomeImpressoNoCartao { get; set; }
    public int? MesValidade { get; set; }
    public int? AnoValidade { get; set; }
    public int? CodigoSeguranca { get; set; }
    public decimal? Valor { get; set; }
}

public class Produto
{
    public Guid? Id { get; set; }
    public string? Nome { get; set; }
    public decimal? Preco { get; set; }
    public int? Quantidade { get; set; }
}    
