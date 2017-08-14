SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [t_Social_Account](
	[Id] [int] NOT NULL,
	[Token] [nvarchar](500) NOT NULL,
	[TokenSecret] [nvarchar](500) NULL,
	[IfEnable] [bit] DEFAULT(1) NOT NULL,
	[IfConvertMessageToConversation] [bit] NOT NULL,
	[IfConvertVisitorPostToConversation] [bit] NOT NULL,
	[IfConvertWallPostToConversation] [bit] NOT NULL,
	[IfConvertTweetToConversation] [bit] NOT NULL,
	[FacebookPageCategory] [nvarchar](200) NULL,
	[FacebookSignInAs] [nvarchar](200) NULL,
	[CreatedTime] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[ConversationDepartmentId] [int] NULL,
	[ConversationAgentId] [int] NULL,
	[ConversationPriority] [smallint] NULL,
	[SiteId] [int] NOT NULL,
	[IsDeleted] [bit] DEFAULT(0) NOT NULL,
 CONSTRAINT [PK_t_Social_Account] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] 
GO