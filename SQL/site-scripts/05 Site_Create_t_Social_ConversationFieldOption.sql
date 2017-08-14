SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [t_Social_ConversationFieldOption](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FieldId] [int] NOT NULL,
	[Name] [nvarchar](200) NULL,
	[Value] [nvarchar](200) NULL,
	[Index] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
 CONSTRAINT [PK_t_Social_ConversationFieldOption] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [t_Social_ConversationFieldOption]  WITH CHECK ADD  CONSTRAINT [FK_t_Social_ConversationFieldOption_t_Social_ConversationField_FieldId] FOREIGN KEY([FieldId])
REFERENCES [t_Social_ConversationField] ([Id])
GO
ALTER TABLE [t_Social_ConversationFieldOption] CHECK CONSTRAINT [FK_t_Social_ConversationFieldOption_t_Social_ConversationField_FieldId]
GO