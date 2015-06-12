CREATE TABLE [dbo].[TB_MARVEL] (
    [Id]                   INT  IDENTITY (1, 1) NOT NULL,
	[TITULO]			   VARCHAR (200) NULL,
	[PRECO]                VARCHAR (200) NULL,
	[ESCRITOR]             VARCHAR (200) NULL,
	[IMAGEM]               VARCHAR (200) NULL,
	[DESCRICAO]            VARCHAR (MAX) NULL,
    [DATA_PUBLICACAO]      VARCHAR (200) NULL,
    [ADICIONADO_IN_MARVEL] VARCHAR (200) NULL,
    [RATING]               VARCHAR (200) NULL,
    
    [ARTISTA_CAPA]         VARCHAR (200) NULL,
   
    [IMPRECAO]             VARCHAR (200) NULL,
    [FORMATO]              VARCHAR (200) NULL,
   
    
    [DT_ATUALIZACAO]       DATETIME   NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

