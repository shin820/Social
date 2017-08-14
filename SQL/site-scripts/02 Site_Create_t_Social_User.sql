SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [t_Social_User](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OriginalId] [nvarchar](200) NOT NULL,
	[OriginalLink] [nvarchar](500) NULL,
	[Source] [smallint] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Email] [nvarchar](200) NULL,
	[Avatar] [nvarchar](200) NULL,
	[SiteId] [int] NOT NULL,
	[ScreenName] [nvarchar](200) NULL,
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