using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//importamos a namespace para utilizar as classes para o webscrapping
using HtmlAgilityPack;
using Demo.WebScrapping.Marvel.database;
namespace Demo.WebScrapping.Marvel
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("----- Iniciando WebScraping in Marvel Comics ---- \n");
            //inicializamos o nosso pedido HTTP
            var pedidoHttp = new HtmlWeb();

            //Inicializamos a nossa URL BASE para ser reutilizada
            var urlBase = "http://marvel.com/";

            //pegamos nossa URL onde está a lista dos arquivos
            var html = pedidoHttp.Load(urlBase + "comics/list/623/get_started_with_free_issues?&options%5Boffset%5D=0&totalcount=41");

            //pegamos dentro do nosso HTML, conteudo da TAG BODY;
            var body = html.DocumentNode.SelectSingleNode("//body");

            //Feito isso, pegamos a DIV em que está NOSSOS ITENS
            var divBase = body.SelectSingleNode("//*[@id='comicsListing']");

            //deixamos esta variavel para ser reutilizada
            var titulo = "";
            //abrimos a conexao com nosso banco (SEMPRE FICARÁ ATIVA POIS executaremos como
            //um script)
            var conexao = new marvel_comicsEntities();

            //pegamos todas as DIVS dentro da DIV PRINCIPAL
            var divList = divBase.SelectNodes("div");

            var index = 0;
            foreach (var div in divList)
            {
                try
                {
                    //Primeiro, pegamos a nossa url base, e CONCATENAMOS ao link que ficou
                    //na tag <a>, pegamos atributo SRC
                    // utilizamos o VALUE para pegar seu conteudo
                    //TRIM serve para limpar os campos em branco
                    var urlItem = urlBase + div.SelectSingleNode("div[2]/h5[1]/a[1]")
                            .Attributes["href"]
                            .Value
                            .Trim();

                    //COM a nova URL em mãos, fazemos uma nova requisição, para cada item da  nossa lista
                    html = pedidoHttp.Load(urlItem);
                   
                    //pegamos o BODY e seu conteudo dentro desta NOVA URL
                    body = html.DocumentNode.SelectSingleNode("//body");

                    //assim como na URL anterior, pegamos a DIV MAE dos nossos ITENS
                    //O jeito mais fácil de achar este conteudo é clicando em INSPECIONAR ELEMENTO
                    //CLICAR COM O DIREITO, E Copy XPath
                    divBase = body.SelectSingleNode("//*[@id='comics-issuedetail']/section[1]/div[4]/div/div");

                    //como nos passos anteriores, vamos pegar o link, mas DESTA vez o link
                    //está no ATRIBUTO data-dpop-image-detail
                    var imagem = divBase.SelectSingleNode("div[1]/div[1]/a")
                        .Attributes["data-dpop-image-detail"]
                        .Value
                        .Trim(); 

                    //pegamos o TITULO do nosso ITEM
                    titulo = divBase.SelectSingleNode("div[2]/h1").InnerText;

                    //Exibimos na tela
                    Console.WriteLine("[ {0} - {1}] \n", index++, titulo);
                    

                    //Como nosso conteudo está entre <strong> e texto puro
                    //precisamos formatar a NOSSA MANEIRA para conseguir extrair os dados
                    //Pegamos a div PRINCIPAL, onde se encontra toda aquela descrição.
                    //USAMOS O Split, para SEPARAR o nosso texto, onde existir um caracter '\n'Ele dividirá o texto
                    //USAMOS o Where do C# para informar que queremos apenas as POSIÇÕES que não forem vazias
                    //USAMOS novamente o Select, para DIVIDIR novamente nossO TEXTO, desta vez dividir por ':'
                    //No proximo select, queremos somente o SEGUNDO ITEM de cada posição.
                    //POIS, o A Primeira posição [0] é a descrição e a posição [1] é o valor que queremos
                    //Transformamos tudo em um ARRAY para pegar cada posição nos proximos passos
                    var detalhes = divBase.SelectSingleNode("div[2]/div[1]")
                        .InnerText.Split('\n')
                        .Where(item => item.Trim() != string.Empty)
                        .Select(item => item.Trim().Split(':'))
                        .Select(item => item.ElementAt(1))
                        .ToList();

                    //agora que temos o ARRAY com os VALORES da DESCRICAO DO ITEM
                    //vamos em cada posicao, receber nossos dados
                    //o ElementAtOrDefault Nos dis que CASO NAO tenha aquela POSICAO
                    //ele retornar o DEFAULT do tipo da variavel, em nosso caso NULL
                    var publicacao = detalhes.ElementAtOrDefault(0);
                    var adicionadoInMarvel = detalhes.ElementAtOrDefault(1);
                    var rating = detalhes.ElementAtOrDefault(2);
                    var writer = detalhes.ElementAtOrDefault(3);
                    var coverArtist = detalhes.ElementAtOrDefault(4);

                    //antes de TUDO, pegamos AQUELE NÓ (POIS EM ALGUNS ESTE NÓ NAO EXISTE)
                    //CASO ELE FOR NULO, passamos um valor DE NULO, caso contrario, acessamos a propriedade
                    //INNERText para Poder manipular.

                    //Repare que no FIM DO TEXTO, ele lê a TAG <a> pegando o texto MORE
                    //Para polpar nosso tempo, fazemos um REPLACE. Indicamos que, onde 
                    //existir a palavra "more" ele substitui por uma string vazia
                    //feito isso, subistituimos os \n, \t por strings vazias, e eliminamos
                    //as strings vazias com o TRIM
                    var nodeText = divBase.SelectSingleNode("div[2]/div[2]/p[2]");
                    var textPublication = nodeText == null ? null : nodeText
                        .InnerText.Replace("more", "")
                        .Replace("\n", "")
                        .Replace("\t", "")
                        .Trim();

                    //aqui pegamos os MORE DETAILS DA PAGINA
                    //clicamos em Copy XPath no CHROME, e colamos a SEÇÃO DE Codigo que QUEREMOS
                    //manipular
                    divBase = html.DocumentNode
                        .SelectSingleNode("//*[@id='comics-issuedetail']/section[1]/section/section/dl/dd[2]/div/div/div[1]/div/div[1]");
                    //COMO VIMOS em algumas PAGINAS, Nem sempre a estrutura é a mesma, então FIZEMOS isto
                    //para passar novamente um XPATH correto.
                    if (divBase == null)
                    {
                        divBase = html.DocumentNode
                       .SelectSingleNode("//*[@id='comics-issuedetail']/section[1]/section/section/dl/dd/div/div/div[1]/div[1]/div[1]");
                        
                    }
                                        
                    //TEMOS os ITENS DENTRO da TAG ul;
                    var ulList = divBase.SelectSingleNode("ul");
                    
                    //DENTRO da tag UL, pegamos todos os ITEMS, LIs, onde estão nosso conteudo;
                    //caso ele for nulo, inicializamos com uma lista vazia do tipo do NODE,
                    //apenas para nao extourar erros
                    var liList = ulList == null ? new HtmlNodeCollection(null) : ulList.SelectNodes("li");
                    
                    //ASSIM como o EXEMPLO de cima, formatamos DE UM JEITO MAIS SIMPLÃO
                    //os textos que a gente precisa. 
                    //REPARE DE NOVO!!! aqui, nao precisamos fazer todas aquelas VALIDACOES
                    //porque este texto está formatado de uma forma diferente
                    //inicializamos as variaveis como empty
                    //e criamos um novo loop, dentro deste loop, a gente setou
                    //os valores Default, que já estão no HTML deles.
                    
                    var imprint = string.Empty;
                    var format = string.Empty;
                    var price = string.Empty;
                    foreach (var li in liList)
                    {
                        var item = li.InnerText.Split(':');
                        //Pegamos aqui a POSIÇAO [0] pois é a DESCRIÇÃO do ITEM
                        switch (item[0])
                        {
                            //caso for algum destes itens, ele PEGA O VALOR dessa LI
                            case "Imprint": 
                               imprint =  item.ElementAtOrDefault(1);
                               break;
                            case "Format":
                                 format =  item.ElementAtOrDefault(1);
                                break;
                            case "Price":
                                price =  item.ElementAtOrDefault(1);
                                break;
                        }
                    }

                  
                    

                    //Instanciamos nossa PROPRIEDADE para gravar nossos dados 
                    var marvel = new TB_MARVEL();

                    //PEGAMOS os dados QUE OBTIVEMOS da PAGINA, e SALVAMOS em nosso objeto
                    marvel.TITULO = titulo;
                    marvel.DATA_PUBLICACAO = publicacao;
                    marvel.ADICIONADO_IN_MARVEL = adicionadoInMarvel;
                    marvel.RATING = rating;
                    marvel.ESCRITOR = writer;
                    marvel.ARTISTA_CAPA = coverArtist;
                    marvel.DESCRICAO = textPublication;
                    marvel.IMAGEM = imagem;
                    marvel.IMPRECAO = imprint;
                    marvel.FORMATO = format;
                    marvel.PRECO = price;

                    //AQUI adicionamos a data de atualização PARA AGORA, para manter-mos o controle
                    marvel.DT_ATUALIZACAO = DateTime.Now;

                    //ADICIONAMOS À LISTA EM MEMORIA, para ser adicionada posteriormente
                    //(AINDA NAO INCLUIMOS NADA NO BANCO!!!)
                    conexao.TB_MARVEL.Add(marvel);
                }
                catch (Exception ex)
                {
                    //caso de ALGUM ERRO, ele vai exibir na tela, e continuar o PROCESSO
                    Console.WriteLine("Erro: [{0}] em [{1}]: ", ex.Message, titulo);
                    continue;
                }
            }

            //Após pegar todos os dados, GRAVAMOS os REGISTROS no Banco de dados
            conexao.SaveChanges();

            Console.WriteLine("\n---- Encerrado -  Aperte uma tecla para encerrar! ---- \n");
            Console.ReadKey();

        }
    }
}
