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
CREATE TABLE [t_Social_Conversation100014500](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Source] [smallint] NOT NULL,
	[OriginalId] [nvarchar](200) NULL,
	[IfRead] [bit] DEFAULT(0) NOT NULL,
	[LastMessageSentTime] [datetime] NOT NULL,
	[LastMessageSenderId] [int] NOT NULL,
	[LastRepliedAgentId] [int] NULL,
	[AgentId] [int] NULL,
	[DepartmentId] [int] NULL,
	[Status] [smallint] NOT NULL,
	[Subject] [nvarchar](200) NOT NULL,
	[Priority] [smallint] NOT NULL,
	[Note] [nvarchar](2000) NULL,
	[IsDeleted] [bit] DEFAULT(0) NOT NULL,
	[IsHidden] [bit] DEFAULT(0) NOT NULL,
	[CreatedTime] [datetime] DEFAULT(getdate()) NOT NULL,
	[ModifiedTime] [datetime] NULL,
	CONSTRAINT [PK_t_Social_Conversation100014500] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE TABLE [t_Social_ConversationField](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[IfSystem] [bit] NOT NULL,
	[DataType] [smallint] NOT NULL,
	[Name] [nvarchar](max) NULL,
	[SiteId] [int] NOT NULL,
 CONSTRAINT [PK_t_Social_ConversationField] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
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
CREATE TABLE [t_Social_ConversationLog100014500](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ConversationId] [int] NOT NULL,
	[Type] [smallint] NOT NULL,
	[Content] [nvarchar](500) NULL,
	[CreatedTime] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL
	CONSTRAINT [PK_t_Social_ConversationLog100014500] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [t_Social_ConversationLog100014500]  WITH CHECK ADD  CONSTRAINT [FK_t_Social_ConversationLog100014500_t_Social_Conversation100014500_ConversationId] FOREIGN KEY([ConversationId])
REFERENCES [t_Social_Conversation100014500] ([Id])
GO
ALTER TABLE [t_Social_ConversationLog100014500] CHECK CONSTRAINT [FK_t_Social_ConversationLog100014500_t_Social_Conversation100014500_ConversationId]
GO
CREATE TABLE [t_Social_Filter](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](200) NOT NULL,
	[Index] [int] DEFAULT(0) NOT NULL,
	[IfPublic] [bit] NOT NULL,
	[Type] [smallint] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedTime] [datetime] NOT NULL,
	[SiteId] [int] NOT NULL,
	[LogicalExpression] [nvarchar](200) NULL,
 CONSTRAINT [PK_t_Social_Filter] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
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
CREATE TABLE [t_Social_Message100014500](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ConversationId] [int] NOT NULL,
	[Source] [smallint] NOT NULL,
	[OriginalId] [nvarchar](200) NOT NULL,
	[OriginalLink] [nvarchar](500) NULL,
	[ParentId] [int] NULL,
	[SendTime] [datetime] NOT NULL,
	[SenderId] [int] NOT NULL,
	[ReceiverId] [int] NULL,
	[Content] [nvarchar](2000) NULL,
	[Story] [nvarchar](500) NULL,
	[IsDeleted] [bit] DEFAULT(0) NOT NULL,
	[QuoteTweetId] [nvarchar](200) NULL,
	[SendAgentId] [int] NULL,
	CONSTRAINT [PK_t_Social_Message100014500] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [t_Social_Message100014500]  WITH CHECK ADD  CONSTRAINT [FK_t_Social_Message100014500_t_Social_Conversation100014500_ConversationId] FOREIGN KEY([ConversationId])
REFERENCES [t_Social_Conversation100014500] ([Id])
GO
ALTER TABLE [t_Social_Message100014500] CHECK CONSTRAINT [FK_t_Social_Message100014500_t_Social_Conversation100014500_ConversationId]
GO
CREATE INDEX IX_t_Social_Message100014500_OriginalId_Source ON t_Social_Message100014500([OriginalId],[Source] ASC)
GO
CREATE INDEX IX_t_Social_Message100014500_SenderId ON t_Social_Message100014500([SenderId] ASC)
GO
CREATE INDEX IX_t_Social_Message100014500_ReceiverId ON t_Social_Message100014500([ReceiverId] ASC)
GO
CREATE TABLE [t_Social_MessageAttachment100014500](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[MessageId] [int] NOT NULL,
	[OriginalId] [nvarchar](200) NULL,
	[OriginalLink] [nvarchar](500) NULL,
	[Type] [smallint] NOT NULL,
	[Name] [nvarchar](200) NULL,
	[MimeType] [nvarchar](100) NULL,
	[Size] [bigint] NOT NULL,
	[Url] [nvarchar](500) NULL,
	[PreviewUrl] [nvarchar](500) NULL,
	[RawData] [image] NULL,
	CONSTRAINT [PK_t_Social_MessageAttachment100014500] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [t_Social_MessageAttachment100014500]  WITH CHECK ADD  CONSTRAINT [FK_t_Social_MessageAttachment100014500_t_Social_Message100014500_MessageId] FOREIGN KEY([MessageId])
REFERENCES [t_Social_Message100014500] ([Id])
GO
ALTER TABLE [t_Social_MessageAttachment100014500] CHECK CONSTRAINT [FK_t_Social_MessageAttachment100014500_t_Social_Message100014500_MessageId]
GO
CREATE TABLE [t_Social_TwitterAuth](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AuthorizationId] [nvarchar](50) NOT NULL,
	[AuthorizationKey] [nvarchar](200) NOT NULL,
	[AuthorizationSecret] [nvarchar](200) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
INSERT INTO  [t_Social_ConversationField]
(
	[IfSystem]
	,[DataType]
	,[Name]
	,[SiteId]
)
VALUES
(1,4,'Source',100014500),
(1,4,'Agent Assignee',100014500),
(1,4,'Department Assignee',100014500),
(1,4,'Status',100014500),
(1,4,'Priority',100014500),
(1,4,'Social Accounts',100014500),
(1,4,'Replied Agents',100014500),
(1,4,'Last Replied Agent',100014500),
(1,1,'Last Message Sent by',100014500),
(1,4,'Agent Assignee Status',100014500),
(1,4,'Department Assignee Status',100014500),
(1,1,'Social Users',100014500),
(1,1,'Comment/Messages',100014500),
(1,3,'Last Message Sent',100014500),
(1,3,'Created',100014500),
(1,3,'Last Modified',100014500),
(1,2,'Time to Last Message',100014500),
(1,2,'Total Messages',100014500),
(1,2,'Conversation ID',100014500)
GO
INSERT INTO [t_Social_ConversationFieldOption]
(
	[FieldId]
	,[Name]
	,[Value]
	,[Index]
	,[SiteId]
)
VALUES
( (SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND [Name] = 'Source' AND [DataType] = 4),'Facebook Message','1',1,100014500),
( (SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND [Name] = 'Source' AND [DataType] = 4),'Facebook Visitor Post','2',2,100014500),
( (SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND [Name] = 'Source' AND [DataType] = 4),'Facebook Wall Post','3',3,100014500),
( (SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND [Name] = 'Source' AND [DataType] = 4),'Twitter Tweet','4',4,100014500),
( (SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND [Name] = 'Source' AND [DataType] = 4),'Twitter Direct Message','5',5,100014500),
( (SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND [Name] = 'Status' AND [DataType] = 4),'New','0',1,100014500),
( (SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND [Name] = 'Status' AND [DataType] = 4),'Pending Internal','1',2,100014500),
( (SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND [Name] = 'Status' AND [DataType] = 4),'Pending External','2',3,100014500),
( (SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND [Name] = 'Status' AND [DataType] = 4),'On Hold','3',4,100014500),
( (SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND [Name] = 'Status' AND [DataType] = 4),'Closed','4',5,100014500),
( (SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND [Name] = 'Priority' AND [DataType] = 4),'Low','0',1,100014500),
( (SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND [Name] = 'Priority' AND [DataType] = 4),'Normal','1',2,100014500),
( (SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND [Name] = 'Priority' AND [DataType] = 4),'High','2',3,100014500),
( (SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND [Name] = 'Priority' AND [DataType] = 4),'Urgent','3',4,100014500)
GO
INSERT INTO [t_Social_Filter]
(
	[Name]
	,[Index]
	,[IfPublic]
	,[Type]
	,[CreatedBy]
	,[CreatedTime]
	,[SiteId]
)
VALUES
('My Open', 1, 1,1,0,getdate(),100014500 ),
('My Department''s Open', 2, 1,1,0,getdate(),100014500 ),
('My Offline Colleagues'' Open', 3, 1,1,0,getdate(),100014500 ),
('All Open', 4, 1,1,0,getdate(),100014500 ),
('Unassigned', 5, 1,1,0,getdate(),100014500 ),
('All', 6, 1,1,0,getdate(),100014500 )
GO
INSERT INTO [t_Social_FilterCondition]
(
	[FilterId]
	,[FieldId]
	,[MatchType]
	,[Value]
	,[SiteId]
	,[Index]
)
VALUES
((SELECT [Id] FROM [t_Social_Filter] WHERE [SiteId] = 100014500 AND [Name] = 'My Open' AND [IfPublic] = 1),(SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND Name='Status'),2,'4',100014500,1),
((SELECT [Id] FROM [t_Social_Filter] WHERE [SiteId] = 100014500 AND [Name] = 'My Open' AND [IfPublic] = 1),(SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND Name='Agent Assignee'),1,'@Me',100014500,2),
((SELECT [Id] FROM [t_Social_Filter] WHERE [SiteId] = 100014500 AND [Name] = 'My Department''s Open' AND [IfPublic] = 1),(SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND Name='Status'),2,'4',100014500,3),
((SELECT [Id] FROM [t_Social_Filter] WHERE [SiteId] = 100014500 AND [Name] = 'My Department''s Open' AND [IfPublic] = 1),(SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND Name='Department Assignee'),1,'@My Department',100014500,4),
((SELECT [Id] FROM [t_Social_Filter] WHERE [SiteId] = 100014500 AND [Name] = 'My Offline Colleagues'' Open' AND [IfPublic] = 1),(SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND Name='Status'),2,'4',100014500,5),
((SELECT [Id] FROM [t_Social_Filter] WHERE [SiteId] = 100014500 AND [Name] = 'My Offline Colleagues'' Open' AND [IfPublic] = 1),(SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND Name='Agent Assignee'),1,'@My Department Member',100014500,6),
((SELECT [Id] FROM [t_Social_Filter] WHERE [SiteId] = 100014500 AND [Name] = 'My Offline Colleagues'' Open' AND [IfPublic] = 1),(SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND Name='Department Assignee'),1,'Blank',100014500,7),
((SELECT [Id] FROM [t_Social_Filter] WHERE [SiteId] = 100014500 AND [Name] = 'All Open' AND [IfPublic] = 1),(SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND Name='Status'),2,'4',100014500,8),
((SELECT [Id] FROM [t_Social_Filter] WHERE [SiteId] = 100014500 AND [Name] = 'Unassigned' AND [IfPublic] = 1),(SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND Name='Agent Assignee'),1,'Blank',100014500,9),
((SELECT [Id] FROM [t_Social_Filter] WHERE [SiteId] = 100014500 AND [Name] = 'Unassigned' AND [IfPublic] = 1),(SELECT [Id] FROM [t_Social_ConversationField] WHERE [SiteId] = 100014500 AND Name='Department Assignee'),1,'Blank',100014500,10)
GO

