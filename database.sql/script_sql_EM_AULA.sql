CREATE TABLE [dbo].[REVISTINHA] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [titulo]     VARCHAR (MAX) NULL,
    [escritor]   VARCHAR (MAX) NULL,
    [desenhista] VARCHAR (MAX) NULL,
    [publicacao] VARCHAR (MAX) NULL,
    [imagem]     VARCHAR (MAX) NULL,
    [descricao]  VARCHAR (MAX) NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);

