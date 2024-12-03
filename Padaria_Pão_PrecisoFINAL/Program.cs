using System;
using System.IO;

struct Produto
{
    public string Nome;
    public DateTime Fabricacao;
    public DateTime Validade; 
    public int Quantidade;
    public double Preco;
    public int DiasValidade; 

    public bool EstaVencido()
    {
        return DateTime.Now > Fabricacao.AddDays(DiasValidade);
    }
}

struct Venda
{
    public string Produto;
    public int Quantidade;
    public string FormaPagamento;
    public double ValorTotal;
    public DateTime Data;
}

class Program
{
    static Produto[] estoque = new Produto[100];
    static Venda[] vendas = new Venda[1000];
    static int totalProdutos = 0;
    static int totalVendas = 0;

    static void Main()
    {
        CarregarEstoque();
        int opcao;

        do
        {
            Console.Clear();
            Console.WriteLine("===== MENU PRINCIPAL =====");
            Console.WriteLine("1. Registrar Venda");
            Console.WriteLine("2. Gerenciar Estoque");
            Console.WriteLine("3. Gerar Relatórios de Vendas (Texto)");
            Console.WriteLine("4. Listar Vendas do Dia");
            Console.WriteLine("5. Salvar Estoque em Formato Binário");
            Console.WriteLine("6. Salvar Estoque em Formato Texto");
            Console.WriteLine("7. Sair");
            Console.Write("Escolha uma opção: ");

            if (!ValidarEntrada(1, 7, out opcao))
            {
                break;
            }

            switch (opcao)
            {
                case 1:
                    RegistrarVenda();
                    break;
                case 2:
                    GerenciarEstoque();
                    break;
                case 3:
                    GerarRelatorios();
                    break;
                case 4:
                    ListarVendas();
                    break;
                case 5:
                    RegistrarNovoDia(); 
                    break;
                case 6:
                    SalvarEstoqueTexto(); 
                    break;
                case 7:
                    SalvarEstoqueTexto();  
                    Console.WriteLine("Saindo...");
                    break;
                default:
                    Console.WriteLine("Opção inválida!");
                    break;
            }
        } while (opcao != 7); 
    }

    static void RegistrarNovoDia()
    {
        string nomeArquivo = $"estoque_{DateTime.Now:yyyyMMdd}.bin";

        using (FileStream fs = new FileStream(nomeArquivo, FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            for (int i = 0; i < totalProdutos; i++)
            {
                writer.Write(estoque[i].Nome);
                writer.Write(estoque[i].Fabricacao.ToString());
                writer.Write(estoque[i].Validade.ToString());
                writer.Write(estoque[i].Quantidade);
                writer.Write(estoque[i].Preco);
                writer.Write(estoque[i].DiasValidade);
            }
        }

        Console.WriteLine($"Dia registrado com sucesso! Estoque salvo em {nomeArquivo}");
        Console.ReadKey();
    }

    static void RegistrarVenda()
    {
        Console.Clear();
        Console.WriteLine("===== REGISTRAR VENDA =====");
        Console.Write("Informe o nome do produto: ");
        string nomeProduto = Console.ReadLine();

        int index = BuscarProduto(nomeProduto);
        if (index == -1)
        {
            Console.WriteLine("Produto não encontrado!");
            Console.ReadKey();
            return;
        }

        Produto produto = estoque[index];
        Console.Write("Quantidade vendida: ");
        int quantidade = int.Parse(Console.ReadLine());

        if (quantidade > produto.Quantidade)
        {
            Console.WriteLine("Estoque insuficiente!");
            Console.ReadKey();
            return;
        }

        Console.Write("Forma de pagamento (Dinheiro/Cartão): ");
        string formaPagamento = Console.ReadLine();

        double valorTotal = produto.Preco * quantidade;

        vendas[totalVendas++] = new Venda
        {
            Produto = produto.Nome,
            Quantidade = quantidade,
            FormaPagamento = formaPagamento,
            ValorTotal = valorTotal,
            Data = DateTime.Now
        };

        produto.Quantidade -= quantidade;
        estoque[index] = produto;


        SalvarVendas();

        Console.WriteLine($"Venda registrada com sucesso! Valor total: R$ {valorTotal:F2}");
        Console.ReadKey();
    }

    static void ListarVendas()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"===== HISTÓRICO DE VENDAS DE HOJE ===== ({DateTime.Now:yyyy-MM-dd})");


            var vendasDoDia = vendas.Take(totalVendas).Where(v => v.Data.Date == DateTime.Now.Date).ToList();

            if (vendasDoDia.Count == 0)
            {
                Console.WriteLine("Nenhuma venda foi registrada hoje.");
                Console.WriteLine("----------------------------");
            }
            else
            {
                double totalCaixa = 0;
                foreach (var venda in vendasDoDia)
                {
                    Console.WriteLine($"Produto: {venda.Produto}");
                    Console.WriteLine($"Quantidade: {venda.Quantidade}");
                    Console.WriteLine($"Forma de Pagamento: {venda.FormaPagamento}");
                    Console.WriteLine($"Valor Total: R$ {venda.ValorTotal:F2}");
                    Console.WriteLine("----------------------------");
                    totalCaixa += venda.ValorTotal;
                }

                Console.WriteLine($"Total Arrecadado Hoje: R$ {totalCaixa:F2}");
            }

            Console.WriteLine("\nDigite [0] para voltar ao menu principal.");
            string input = Console.ReadLine();
            if (input == "0")
                break;
        }
    }

    static void SalvarVendas()
    {
        using (StreamWriter writer = new StreamWriter("vendas.txt", true))
        {
            for (int i = 0; i < totalVendas; i++)
            {

                string dataHora = vendas[i].Data.ToString("yyyy-MM-dd HH:mm:ss");


                writer.WriteLine($"Data: {dataHora}, Produto: {vendas[i].Produto}, Quantidade: {vendas[i].Quantidade}, Forma de Pagamento: {vendas[i].FormaPagamento}, Valor Total: R$ {vendas[i].ValorTotal:F2}");
            }
        }
    }

    static void GerenciarEstoque()
    {
        Console.Clear();
        Console.WriteLine("===== GERENCIAR ESTOQUE =====");
        Console.WriteLine("1. Listar Produtos");
        Console.WriteLine("2. Adicionar Produto");
        Console.WriteLine("3. Verificar Produtos Vencidos");
        Console.Write("Escolha uma opção: ");
        int opcao = int.Parse(Console.ReadLine());

        switch (opcao)
        {
            case 1:
                ListarProdutos();
                break;
            case 2:
                AdicionarProduto();
                break;
            case 3:
                VerificarProdutosVencidos();
                break;
            default:
                Console.WriteLine("Opção inválida!");
                break;
        }

        Console.ReadKey();
    }

    static void ListarProdutos()
    {
        Console.Clear();
        Console.WriteLine("===== LISTA DE PRODUTOS =====");

        if (totalProdutos == 0)
        {
            Console.WriteLine("O estoque está vazio.");
            Console.ReadKey();
            return;
        }

        OrdenarEstoque();
        for (int i = 0; i < totalProdutos; i++)
        {
            Produto p = estoque[i];
            Console.WriteLine($"Nome: {p.Nome}, Fabricado: {p.Fabricacao:yyyy-MM-dd}, Vence em: {p.Fabricacao.AddDays(p.DiasValidade):yyyy-MM-dd}, Quantidade: {p.Quantidade}, Preço: R$ {p.Preco:F2}");
        }
        Console.ReadKey();
    }

    static void AdicionarProduto()
    {
        if (totalProdutos >= estoque.Length)
        {
            Console.WriteLine("Estoque cheio! Não é possível adicionar mais produtos.");
            return;
        }

        Console.Clear();
        Console.WriteLine("===== ADICIONAR PRODUTO =====");
        Console.Write("Nome do produto: ");
        string nome = Console.ReadLine();
        Console.Write("Data de Fabricação (yyyy-MM-dd): ");
        DateTime fabricacao = DateTime.Parse(Console.ReadLine());
        Console.Write("Dias de validade a partir da fabricação: ");
        int diasValidade = int.Parse(Console.ReadLine());
        Console.Write("Quantidade: ");
        int quantidade = int.Parse(Console.ReadLine());
        Console.Write("Preço unitário: ");
        double preco = double.Parse(Console.ReadLine());

        estoque[totalProdutos++] = new Produto
        {
            Nome = nome,
            Fabricacao = fabricacao,
            Validade = fabricacao.AddDays(diasValidade),
            Quantidade = quantidade,
            Preco = preco,
            DiasValidade = diasValidade
        };

        SalvarEstoqueBinario();

        Console.WriteLine("Produto adicionado com sucesso!");
    }
    static void VerificarProdutosVencidos()
    {
        Console.Clear();
        Console.WriteLine("===== PRODUTOS VENCIDOS =====");

        if (totalProdutos == 0)
        {
            Console.WriteLine("O estoque está vazio. Não há produtos vencidos.");
            Console.ReadKey();
            return;
        }

        bool encontrouVencidos = false;
        for (int i = 0; i < totalProdutos; i++)
        {
            Produto produto = estoque[i];
            if (produto.EstaVencido())
            {
                Console.WriteLine($"Produto: {produto.Nome}, Fabricado em: {produto.Fabricacao:yyyy-MM-dd}, Venceu em: {produto.Fabricacao.AddDays(produto.DiasValidade):yyyy-MM-dd}");
                encontrouVencidos = true;
            }
        }

        if (!encontrouVencidos)
        {
            Console.WriteLine("Não há produtos vencidos.");
        }

        Console.ReadKey();
    }

    static void GerarRelatorios()
    {
        Console.Clear();
        Console.WriteLine("===== RELATÓRIOS =====");

        if (totalVendas == 0)
        {
            Console.WriteLine("Nenhuma venda foi registrada.");
            Console.ReadKey();
            return;
        }

        string dataRelatorio = DateTime.Now.ToString("yyyyMMdd");
        string relatorio = $"RelatorioVendasTotais_{dataRelatorio}.txt";

        using (StreamWriter writer = new StreamWriter(relatorio))
        {
            double totalCaixa = 0;
            writer.WriteLine($"Relatório gerado em: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine("------------------------------------------------------------");

            for (int i = 0; i < totalVendas; i++)
            {
                Venda venda = vendas[i];
                // Formatando a data e hora para o formato desejado
                string dataHoraVenda = venda.Data.ToString("yyyy-MM-dd HH:mm:ss");

                writer.WriteLine($"Data e Hora: {dataHoraVenda}, Produto: {venda.Produto}, Quantidade: {venda.Quantidade}, Valor: R$ {venda.ValorTotal:F2}, Forma de Pagamento: {venda.FormaPagamento}");
                totalCaixa += venda.ValorTotal;
            }
            writer.WriteLine($"------------------------------------------------------------");
            writer.WriteLine($"Total do Caixa: R$ {totalCaixa:F2}");
        }

        Console.WriteLine($"Relatório gerado em '{relatorio}'");
        Console.ReadKey();
    }


    static int BuscarProduto(string nome)
    {
        OrdenarEstoque();
        int inicio = 0;
        int fim = totalProdutos - 1;

        while (inicio <= fim)
        {
            int meio = (inicio + fim) / 2;
            int comparacao = string.Compare(estoque[meio].Nome, nome, StringComparison.OrdinalIgnoreCase);

            if (comparacao == 0)
                return meio;
            else if (comparacao < 0)
                inicio = meio + 1;
            else
                fim = meio - 1;
        }

        return -1;
    }

    static void OrdenarEstoque(int n = -1)
    {
        if (n == -1) n = totalProdutos;

        if (n == 1) return;

        for (int i = 0; i < n - 1; i++)
        {
            if (string.Compare(estoque[i].Nome, estoque[i + 1].Nome, StringComparison.OrdinalIgnoreCase) > 0)
            {
                Produto temp = estoque[i];
                estoque[i] = estoque[i + 1];
                estoque[i + 1] = temp;
            }
        }

        OrdenarEstoque(n - 1);
    }

    static bool ValidarEntrada(int min, int max, out int opcao)
    {
        while (true)
        {
            string input = Console.ReadLine();
            if (int.TryParse(input, out opcao) && opcao >= min && opcao <= max)
            {
                return true;
            }
            else
            {
                Console.WriteLine("Entrada inválida! Pressione qualquer tecla para tentar novamente ou '0' para voltar ao menu.");
                string sair = Console.ReadLine();
                if (sair == "0")
                {
                    return false;
                }
                Console.Write($"Digite uma opção válida entre {min} e {max}: ");
            }
        }
    }

    static void CarregarEstoque()
    {
        if (File.Exists("estoque.bin"))
        {
            using (FileStream fs = new FileStream("estoque.bin", FileMode.Open))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    estoque[totalProdutos++] = new Produto
                    {
                        Nome = reader.ReadString(),
                        Fabricacao = DateTime.Parse(reader.ReadString()),
                        Validade = DateTime.Parse(reader.ReadString()),
                        Quantidade = reader.ReadInt32(),
                        Preco = reader.ReadDouble(),
                        DiasValidade = reader.ReadInt32()
                    };
                }
            }
        }
    }

    static void SalvarEstoqueTexto()
    {
        
        Array.Sort(estoque, 0, totalProdutos, Comparer<Produto>.Create((x, y) => string.Compare(x.Nome, y.Nome, StringComparison.OrdinalIgnoreCase)));

        using (StreamWriter writer = new StreamWriter("estoque.txt", false)) 
        {
            for (int i = 0; i < totalProdutos; i++)
            {
                Produto produto = estoque[i];
               
                string dataFabricacao = produto.Fabricacao.ToString("yyyy-MM-dd");
                string dataValidade = produto.Validade.ToString("yyyy-MM-dd");

               
                writer.WriteLine($"Nome: {produto.Nome}, Fabricado em: {dataFabricacao}, Vence em: {dataValidade}, Quantidade: {produto.Quantidade}, Preço: R$ {produto.Preco:F2}, Dias de Validade: {produto.DiasValidade}");
            }
        }
        Console.WriteLine("Estoque salvo no formato de texto com sucesso!");
    }


    static void SalvarEstoqueBinario()
    {
        using (FileStream fs = new FileStream("estoque.bin", FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            for (int i = 0; i < totalProdutos; i++)
            {
                writer.Write(estoque[i].Nome);
                writer.Write(estoque[i].Fabricacao.ToString());
                writer.Write(estoque[i].Validade.ToString());
                writer.Write(estoque[i].Quantidade);
                writer.Write(estoque[i].Preco);
                writer.Write(estoque[i].DiasValidade);
            }
        }
        Console.WriteLine("Estoque salvo no formato binário com sucesso!");
    }
}

