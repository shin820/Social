SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [t_Social_FilterCondition](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FilterId] [int] NOT NULL,
	[FieldId] [int] NOT NULL,
	[MatchType] [smallint] NOT NULL,
	[Value] [nvarchar](200) NULL,
	[SiteId] [int] NOT NULL,
	[Index] [int] DEFAULT(0) NOT NULL,
 CONSTRAINT [PK_t_Social_FilterCondition] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [t_Social_FilterCondition]  WITH CHECK ADD  CONSTRAINT [FK_t_Social_FilterCondition_t_Social_ConversationField_FieldId] FOREIGN KEY([FieldId])
REFERENCES [t_Social_ConversationField] ([Id])
GO
ALTER TABLE [t_Social_FilterCondition] CHECK CONSTRAINT [FK_t_Social_FilterCondition_t_Social_ConversationField_FieldId]
GO
ALTER TABLE [t_Social_FilterCondition]  WITH CHECK ADD  CONSTRAINT [FK_t_Social_FilterCondition_t_Social_Filter_FilterId] FOREIGN KEY([FilterId])
REFERENCES [t_Social_Filter] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [t_Social_FilterCondition] CHECK CONSTRAINT [FK_t_Social_FilterCondition_t_Social_Filter_FilterId]
GO