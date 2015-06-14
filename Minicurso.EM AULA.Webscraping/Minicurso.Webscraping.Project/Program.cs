using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//IMPORTAMOS a biblioteca do HTML AGILITY PACK
using HtmlAgilityPack;
using Minicurso.Webscraping.Project.database;


namespace Minicurso.Webscraping.Project
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("--- INICIANDO WEB SCRAPING ---");
            //pegamos o url BASE para reutilizar nas etapas do nosso
            //codigo
            var urlBase = "http://marvel.com/";
            var urlConteudo = "comics/list/623/get_started_with_free_issues?&options%5Boffset%5D=0&totalcount=42";
            //a partir do HTML Agility, usamos a classe para um
            //pedido HTTP
            var pedidoHttp = new HtmlWeb();

            //Fazemos o download do HTML
            var html = pedidoHttp.Load(urlBase + urlConteudo);
            
            //pegamos o body do nosso documento;
            var body = html.DocumentNode.SelectSingleNode("//body");
            
            //pegamos a div mãe que se encontra no body (MAE ONDE ESTÁ A LISTALISTA)
            var conteudo = body.SelectSingleNode("//*[@id='comicsListing']");
            //Pegamos as div mae
            var divs = conteudo.SelectNodes("div");
            
            //fomos no SQL VIEWR criamos o database no localdb
            //criamos nossa base e table
            //fomos na classe de contexto (Banco.Context)
            //conferimos o NOME DA MINHA CLASSE PRA SABER QUAL USAR
            //APÓS colocar o nome dele, DEMOS CTRL  . 
            //PARAA IMPORTAR A NAMESPACE NO PROJETO
            var conexao = new marveldbEntities1();
            
            
            //ENTRAMOS em cada item da div
            try
            {
                foreach (var div in divs)
                {
                    //pegamos a url do item
                    //fomos no tag <a>  
                    //em atributes, pegamos o valor do HRF
                    //E LIMPAMOS os espaços em branoc com TRIM
                    var url = div.SelectSingleNode("div[2]/h5/a")
                        .Attributes["href"].Value.Trim();
                    Console.WriteLine("[ {0} ]", url);
                    //novo pedido, fiz um redirect (uma nova solicitação)
                    var htmlItem = pedidoHttp.Load(urlBase + url);

                    //pegamos o BODY do item
                    var bodyItem = htmlItem.DocumentNode.SelectSingleNode("//body");

                    //jogamos STRING LOCONA pra pegar o VALOR do atributo
                    //GENERICO (DOS CARAs) e retiramos os espacos 
                    var nodeBase = bodyItem.SelectSingleNode("//*[@id='comics-issuedetail']/section[1]/div[4]/div/div");



                    var urlImagem = nodeBase
                        .SelectSingleNode("div[1]/div[1]/a")
                        .Attributes["data-dpop-image-detail"].Value.Trim();

                    var titulo = nodeBase.SelectSingleNode("div[2]/h1")
                        .InnerText
                        .Trim();
                    Console.WriteLine("[ {0} ]", titulo);

                    //PEGA NODEBASE que contem o HTML daquela div da lista
                    //fomos na div que contem OS TEXTOS (PODEM ESTAR COM TAGS,
                    //MAS ELE IGNORA e pego texto por conta do INNER HTML)
                    //usamos o SPLIT para dividir em LINHAS (QUE HTML RETORNA)
                    //split divide a string em partes a partir de um caracter ('char')
                    //divimos por dois pontos (pois no padrao está "Descricao":"Valor")
                    //USAMOS o MÉTODO DE EXTENSÃO DO C# (LINQ) e falamos que queremos
                    //todos itens que forem diferentes de vazio (' " "  ')
                    //usamo o select para selecionar os itens divididos por dois pontos
                    //usamos o select novamente e definimos que QUEREMOS os ELEMENTO 1 
                    //(VALOR) da nossa lista
                    //em seguida usamos o tolist para transformar tudo em um
                    var detalhes = nodeBase
                        .SelectSingleNode("div[2]/div[1]")
                        .InnerText
                        .Split('\n')
                        .Where(item => item.Trim() != string.Empty)
                        .Select(item => item.Trim().Split(':'))
                        .Select(item => item.ElementAtOrDefault(1))
                        .ToList();
                    //pegamos o elemento de cada posicao
                    //caso nao tenha o elemento ele retorna o valor
                    //padrao da variavel (no caso da string é null)
                    var dataPublicacao = detalhes.ElementAtOrDefault(0);
                    var escritor = detalhes.ElementAtOrDefault(1);
                    var desenhista = detalhes.ElementAtOrDefault(2);

                    var descricao = nodeBase.SelectSingleNode("div[2]/div[2]").InnerText.Trim();

                    var revistinha = new REVISTINHA();
                    revistinha.titulo = titulo;

                    revistinha.publicacao = dataPublicacao;
                    revistinha.escritor = escritor;
                    revistinha.desenhista = desenhista;
                    revistinha.imagem = urlImagem;
                    revistinha.descricao = descricao;
                    conexao.REVISTINHA.Add(revistinha);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            conexao.SaveChanges();
            Console.Read();

        
        }
    }
}
