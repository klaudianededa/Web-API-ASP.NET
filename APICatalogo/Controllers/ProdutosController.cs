using APICatalogo.Context; //banco de dados
using APICatalogo.Models; //banco de dados
using Microsoft.AspNetCore.Mvc; //recursos de controladores e retornos HTTP
using Microsoft.EntityFrameworkCore; //comandos de banco de dados

namespace APICatalogo.Controllers //diz que essa classe pertence à pasta de Controladores do projeto APICatalogo
{
    [Route("[controller]")] //define a URL base desse controlador - substituida pelo nome da classe
    [ApiController] //avisa ao ASP.NET que isso é uma API
    public class ProdutosController : ControllerBase
    {

        //injeção de dependência:
        private readonly AppDbContext _context; //var privada, somente leitura - _var indica que é variável privada global da classe

        public ProdutosController(AppDbContext context) //construtor
        {
            //injeção de dependencia acontecendo:
            _context = context;
        }
        //====================================================================
        //Obtendo todos os produtos
        [HttpGet] //mapeia para o verbo GET

        //ActionResult permite retornar códigos de status HTTP 
        //IEnumerable<Produto> significa que ele vai devolver uma lista genérica de produtos
        public ActionResult<IEnumerable<Produto>> Get()
        {

            var produtos = _context.Produtos.ToList(); //executa um SELECT * FROM Produtos e transforma o resultado em uma lista C#
            //Nota técnica: O método .ToList() do Entity Framework nunca retorna null. Se o banco estiver vazio, ele retorna uma lista vazia []. Portanto, essa checagem de null não vai ser ativada na prática, mas o conceito de verificar se há dados está correto!
            if (produtos is null) //se a lista for nula
            {
                return NotFound("Produtos não encontrados"); //Se não encontrar, devolve o Status 404 Not Found
            }
            return produtos; //Se tudo der certo, o ASP.NET empacota a lista em JSON e devolve automaticamente com o Status 200 OK
        }

        //====================================================================
        //Obtendo um produto específico pelo ID
        [HttpGet("{id:int}", Name = "ObterProduto")] //define que a rota espera um int no final | name: apelido interno para a rota
        public ActionResult<Produto> Get(int id) //o id da URL vem p essa variável
        {
            var produto = _context.Produtos.FirstOrDefault(p => p.ProdutoId == id); //LINQ primeiro produto onde ProdutoId seja igual o id da URL, se não retorna null
            if (produto is null)
            {
                return NotFound("Produto não encontrado"); //se for nulo devolve 404 Not Found
            }
            return produto; // achou e devolve em JSON (202 ok)
        }


        //====================================================================
        //Criando Dados
        [HttpPost] // mapeia para o verbo POST
        public ActionResult Post(Produto produto) //: O ASP.NET pega o JSON que o cliente enviou no corpo da requisição (body) e converte para o objeto Produto
        {
            if (produto is null) //se o cliente não enviou nada no body
            {
                return BadRequest(); //devolve 400 Bad Request
            }
            _context.Produtos.Add(produto); //Informa ao Entity Framework que existe um novo objeto para ser inserido
            _context.SaveChanges(); //Vai de fato no MySQL e executa o comando INSERT INTO. O banco gera o novo ID, e o EF Core atualiza o seu objeto produto com esse novo ID instantaneamente.

            //CreatedAtRouteResult: Retorna 201 Created (Padrão ouro do REST para POST). Ele faz três coisas:
            //Chama a rota apelidada de "ObterProduto" (que criamos no GET)
            //Passa o novo id gerado para montar a URL (Ex: /produtos/6). Essa URL vai no cabeçalho (Header) Location da resposta.
            //Devolve no corpo da resposta o JSON do próprio produto recém-criado.
            return new CreatedAtRouteResult("ObterProduto",
                new { id = produto.ProdutoId }, produto);
        }

        //====================================================================
        //Atualizando Dados
        [HttpPut("{id:int}")] //mapeia para o verbo PUT e espera o id na URL
        public ActionResult Put(int id, Produto produto)//Recebe o ID da URL e o objeto completo vindo do corpo JSON.
        {
            if (id != produto.ProdutoId) //Se o cliente tentar mandar atualizar o produto ID 5 na URL, mas no JSON ele mandar os dados com ProdutoId = 9
            {
                return BadRequest(); //devolve 400 Bad Request para evitar invasões ou falhas nos dados
            }

            _context.Entry(produto).State = EntityState.Modified; //pega o objeto produto e marca o status dele como modificado
            _context.SaveChanges(); //O EF Core vê o status "Modificado" e gera um UPDATE Produtos SET ... no banco de dados

            return Ok(produto); //Se atualizou com sucesso, retorna o Status 200 OK mostrando como o produto ficou
        }




        [HttpDelete("{id:int}")] //mapeia para o verbo DELETE e espera o id na URL
        public ActionResult Delete(int id) //recebe o ID da URL
        {
            var produto = _context.Produtos.FirstOrDefault(p=> p.ProdutoId == id); //primeiro ou null, produtoid igual o que passar aqui 
            //outra alternativa:
            //var produto = _context.Produtos.Find(id);

            if (produto is null) //se nao encontrar o produto
            {
                return NotFound("Produto não localizado..."); //retorna 404
            }
            _context.Produtos.Remove(produto); //encontrou o id, remove o produto
            _context.SaveChanges(); //salva as alterações

            return Ok(produto); //excluiu e retorna 200 ok mostrando o produto 
        }

    }
}
