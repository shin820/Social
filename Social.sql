USE [Social]
GO
/****** Object:  Table [dbo].[t_Social_Account]    Script Date: 2017/7/17 9:37:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_Social_Account](
	[Id] [int] NOT NULL,
	[Token] [nvarchar](max) NOT NULL,
	[TokenSecret] [nvarchar](max) NULL,
	[IfEnable] [bit] NOT NULL,
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
 CONSTRAINT [PK_dbo.t_Social_Account] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_Social_Conversation]    Script Date: 2017/7/17 9:37:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_Social_Conversation](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Source] [smallint] NOT NULL,
	[OriginalId] [nvarchar](200) NULL,
	[IfRead] [bit] NOT NULL,
	[LastMessageSentTime] [datetime] NOT NULL,
	[LastMessageSenderId] [int] NOT NULL,
	[LastRepliedAgentId] [int] NULL,
	[AgentId] [int] NULL,
	[DepartmentId] [int] NULL,
	[Status] [smallint] NOT NULL,
	[Subject] [nvarchar](200) NOT NULL,
	[Priority] [smallint] NOT NULL,
	[Note] [nvarchar](2000) NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsHidden] [bit] NOT NULL,
	[SiteId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.t_Social_Conversation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_Social_ConversationField]    Script Date: 2017/7/17 9:37:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_Social_ConversationField](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IfSystem] [bit] NOT NULL,
	[DataType] [smallint] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[SiteId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.t_Social_ConversationField] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_Social_ConversationFieldOption]    Script Date: 2017/7/17 9:37:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_Social_ConversationFieldOption](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FieldId] [int] NOT NULL,
	[Name] [nvarchar](200) NULL,
	[Value] [nvarchar](200) NULL,
	[Index] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.t_Social_ConversationFieldOption] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_Social_ConversationLog]    Script Date: 2017/7/17 9:37:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_Social_ConversationLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ConversationId] [int] NOT NULL,
	[Type] [smallint] NOT NULL,
	[Content] [nvarchar](500) NULL,
	[CreatedTime] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[SiteId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.t_Social_ConversationLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_Social_FacebookWebHookRawData]    Script Date: 2017/7/17 9:37:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_Social_FacebookWebHookRawData](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Data] [nvarchar](max) NULL,
	[CreatedTime] [datetime] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_dbo.t_Social_FacebookWebHookRawData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_Social_Filter]    Script Date: 2017/7/17 9:37:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_Social_Filter](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Index] [int] NOT NULL,
	[IfPublic] [bit] NOT NULL,
	[ConditionRuleTriggerType] [smallint] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedTime] [datetime] NOT NULL,
	[SiteId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.t_Social_Filter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_Social_FilterCondition]    Script Date: 2017/7/17 9:37:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_Social_FilterCondition](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FilterId] [int] NOT NULL,
	[FieldId] [int] NOT NULL,
	[MatchType] [smallint] NOT NULL,
	[Value] [nvarchar](200) NULL,
	[SiteId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.t_Social_FilterCondition] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_Social_Message]    Script Date: 2017/7/17 9:37:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_Social_Message](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ConversationId] [int] NOT NULL,
	[Source] [smallint] NOT NULL,
	[OriginalId] [nvarchar](200) NOT NULL,
	[OriginalLink] [nvarchar](500) NULL,
	[ParentId] [int] NULL,
	[SendTime] [datetime] NOT NULL,
	[SenderId] [int] NOT NULL,
	[ReceiverId] [int] NULL,
	[Content] [nvarchar](max) NULL,
	[Story] [nvarchar](500) NULL,
	[IsDeleted] [bit] NOT NULL,
	[SiteId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.t_Social_Message] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_Social_MessageAttachment]    Script Date: 2017/7/17 9:37:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_Social_MessageAttachment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MessageId] [int] NOT NULL,
	[OriginalId] [nvarchar](200) NULL,
	[OriginalLink] [nvarchar](500) NULL,
	[Type] [smallint] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[MimeType] [nvarchar](max) NULL,
	[Size] [bigint] NOT NULL,
	[Url] [nvarchar](max) NULL,
	[PreviewUrl] [nvarchar](max) NULL,
	[SiteId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.t_Social_MessageAttachment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[t_Social_User]    Script Date: 2017/7/17 9:37:50 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[t_Social_User](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OriginalId] [nvarchar](200) NOT NULL,
	[OriginalLink] [nvarchar](500) NULL,
	[Type] [smallint] NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Email] [nvarchar](200) NULL,
	[Avatar] [nvarchar](200) NULL,
	[SiteId] [int] NOT NULL,
 CONSTRAINT [PK_dbo.t_Social_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[t_Social_Account]  WITH CHECK ADD  CONSTRAINT [FK_dbo.t_Social_Account_dbo.t_Social_User_Id] FOREIGN KEY([Id])
REFERENCES [dbo].[t_Social_User] ([Id])
GO
ALTER TABLE [dbo].[t_Social_Account] CHECK CONSTRAINT [FK_dbo.t_Social_Account_dbo.t_Social_User_Id]
GO
ALTER TABLE [dbo].[t_Social_Conversation]  WITH CHECK ADD  CONSTRAINT [FK_dbo.t_Social_Conversation_dbo.t_Social_User_LastMessageSenderId] FOREIGN KEY([LastMessageSenderId])
REFERENCES [dbo].[t_Social_User] ([Id])
GO
ALTER TABLE [dbo].[t_Social_Conversation] CHECK CONSTRAINT [FK_dbo.t_Social_Conversation_dbo.t_Social_User_LastMessageSenderId]
GO
ALTER TABLE [dbo].[t_Social_ConversationFieldOption]  WITH CHECK ADD  CONSTRAINT [FK_dbo.t_Social_ConversationFieldOption_dbo.t_Social_ConversationField_FieldId] FOREIGN KEY([FieldId])
REFERENCES [dbo].[t_Social_ConversationField] ([Id])
GO
ALTER TABLE [dbo].[t_Social_ConversationFieldOption] CHECK CONSTRAINT [FK_dbo.t_Social_ConversationFieldOption_dbo.t_Social_ConversationField_FieldId]
GO
ALTER TABLE [dbo].[t_Social_ConversationLog]  WITH CHECK ADD  CONSTRAINT [FK_dbo.t_Social_ConversationLog_dbo.t_Social_Conversation_ConversationId] FOREIGN KEY([ConversationId])
REFERENCES [dbo].[t_Social_Conversation] ([Id])
GO
ALTER TABLE [dbo].[t_Social_ConversationLog] CHECK CONSTRAINT [FK_dbo.t_Social_ConversationLog_dbo.t_Social_Conversation_ConversationId]
GO
ALTER TABLE [dbo].[t_Social_FilterCondition]  WITH CHECK ADD  CONSTRAINT [FK_dbo.t_Social_FilterCondition_dbo.t_Social_ConversationField_FieldId] FOREIGN KEY([FieldId])
REFERENCES [dbo].[t_Social_ConversationField] ([Id])
GO
ALTER TABLE [dbo].[t_Social_FilterCondition] CHECK CONSTRAINT [FK_dbo.t_Social_FilterCondition_dbo.t_Social_ConversationField_FieldId]
GO
ALTER TABLE [dbo].[t_Social_FilterCondition]  WITH CHECK ADD  CONSTRAINT [FK_dbo.t_Social_FilterCondition_dbo.t_Social_Filter_FilterId] FOREIGN KEY([FilterId])
REFERENCES [dbo].[t_Social_Filter] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[t_Social_FilterCondition] CHECK CONSTRAINT [FK_dbo.t_Social_FilterCondition_dbo.t_Social_Filter_FilterId]
GO
ALTER TABLE [dbo].[t_Social_Message]  WITH CHECK ADD  CONSTRAINT [FK_dbo.t_Social_Message_dbo.t_Social_Conversation_ConversationId] FOREIGN KEY([ConversationId])
REFERENCES [dbo].[t_Social_Conversation] ([Id])
GO
ALTER TABLE [dbo].[t_Social_Message] CHECK CONSTRAINT [FK_dbo.t_Social_Message_dbo.t_Social_Conversation_ConversationId]
GO
ALTER TABLE [dbo].[t_Social_Message]  WITH CHECK ADD  CONSTRAINT [FK_dbo.t_Social_Message_dbo.t_Social_Message_ParentId] FOREIGN KEY([ParentId])
REFERENCES [dbo].[t_Social_Message] ([Id])
GO
ALTER TABLE [dbo].[t_Social_Message] CHECK CONSTRAINT [FK_dbo.t_Social_Message_dbo.t_Social_Message_ParentId]
GO
ALTER TABLE [dbo].[t_Social_Message]  WITH CHECK ADD  CONSTRAINT [FK_dbo.t_Social_Message_dbo.t_Social_User_ReceiverId] FOREIGN KEY([ReceiverId])
REFERENCES [dbo].[t_Social_User] ([Id])
GO
ALTER TABLE [dbo].[t_Social_Message] CHECK CONSTRAINT [FK_dbo.t_Social_Message_dbo.t_Social_User_ReceiverId]
GO
ALTER TABLE [dbo].[t_Social_Message]  WITH CHECK ADD  CONSTRAINT [FK_dbo.t_Social_Message_dbo.t_Social_User_SenderId] FOREIGN KEY([SenderId])
REFERENCES [dbo].[t_Social_User] ([Id])
GO
ALTER TABLE [dbo].[t_Social_Message] CHECK CONSTRAINT [FK_dbo.t_Social_Message_dbo.t_Social_User_SenderId]
GO
ALTER TABLE [dbo].[t_Social_MessageAttachment]  WITH CHECK ADD  CONSTRAINT [FK_dbo.t_Social_MessageAttachment_dbo.t_Social_Message_MessageId] FOREIGN KEY([MessageId])
REFERENCES [dbo].[t_Social_Message] ([Id])
GO
ALTER TABLE [dbo].[t_Social_MessageAttachment] CHECK CONSTRAINT [FK_dbo.t_Social_MessageAttachment_dbo.t_Social_Message_MessageId]
GO
ALTER TABLE [t_Social_Message] ADD QuoteTweetId NVARCHAR(200)
GO
ALTER TABLE [t_Social_Account] ADD IsDeleted BIT NOT NULL DEFAULT(0)
GO
CREATE TABLE t_Social_TwitterAuth
(
	[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[AuthorizationId] [nvarchar](50) NOT NULL,
	[AuthorizationKey] [nvarchar](200) NOT NULL,
	[AuthorizationSecret] [nvarchar](200) NOT NULL
)
GO
ALTER TABLE [dbo].[t_Social_FilterCondition] Add [Index] INT NOT NULL DEFAULT(0)
GO
sp_rename 't_Social_Filter.ConditionRuleTriggerType', 'Type', 'COLUMN'
GO
ALTER TABLE [dbo].[t_Social_Conversation] ADD CreatedTime DATETIME NOT NULL DEFAULT(CURRENT_TIMESTAMP)
GO
ALTER TABLE [dbo].[t_Social_Conversation] ADD ModifiedTime DATETIME NULL
GO
ALTER TABLE [dbo].[t_Social_Filter] ADD LogicalExpression NVARCHAR(200) NULL
GO
ALTER TABLE [dbo].[t_Social_Message] ADD SendAgentId INT NULL
GO
ALTER TABLE [dbo].[t_Social_User] ADD ScreenName NVARCHAR(200)
GO
sp_rename 't_Social_User.Type', 'Source', 'COLUMN'
GO
ALTER TABLE [dbo].[t_Social_User] ADD [Type] SMALLINT NOT NULL DEFAULT(0)
GO
ALTER TABLE [dbo].[t_Social_User] ADD IsDeleted bit not null default(0)
GO