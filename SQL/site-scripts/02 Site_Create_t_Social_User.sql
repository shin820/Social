SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [t_Social_User](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OriginalId] [nvarchar](256) NOT NULL,
	[OriginalLink] [nvarchar](512) NULL DEFAULT(''),
	[Source] [smallint] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Email] [nvarchar](256) NULL DEFAULT(''),
	[Avatar] [nvarchar](256) NULL DEFAULT(''),
	[SiteId] [int] NOT NULL,
	[ScreenName] [nvarchar](256) NULL DEFAULT(''),
	[Type] [smallint] NOT NULL,
	[IsDeleted] [bit] DEFAULT(0) NOT NULL,
 CONSTRAINT [PK_t_Social_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [t_Social_Account]  WITH CHECK ADD  CONSTRAINT [FK_t_Social_Account_t_Social_User_Id] FOREIGN KEY([Id])
REFERENCES [t_Social_User] ([Id])
GO
ALTER TABLE [t_Social_Account] CHECK CONSTRAINT [FK_t_Social_Account_t_Social_User_Id]
GO