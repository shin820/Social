USE Social
GO
INSERT INTO  [Social].[dbo].[t_Social_ConversationField]
(
	   [IfSystem]
      ,[DataType]
      ,[Name]
      ,[SiteId]
)
VALUES
(1,4,'Source',10000),
(1,4,'Agent Assignee',10000),
(1,4,'Department Assignee',10000),
(1,4,'Status',10000),
(1,4,'Priority',10000),
(1,4,'Social Accounts',10000),
(1,4,'Replied Agents',10000),
(1,4,'Last Replied Agent',10000),
(1,1,'Last Message Sent by',10000),
(1,4,'Agent Assignee Status',10000),
(1,4,'Department Assignee Status',10000),
(1,1,'Social Users',10000),
(1,1,'Comment/Messages',10000),
(1,3,'Last Message Sent',10000),
(1,3,'Created',10000),
(1,3,'Last Modified',10000),
(1,2,'Time to Last Message',10000),
(1,2,'Total Messages',10000),
(1,2,'Conversation ID',10000)
GO
INSERT INTO [Social].[dbo].[t_Social_ConversationFieldOption]
  (
       [FieldId]
      ,[Name]
      ,[Value]
      ,[Index]
      ,[SiteId]
  )
VALUES
  ( (SELECT [Id] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Source' AND [DataType] = 4),'Facebook Message','1',1,(SELECT [SiteId] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Source' AND [DataType] = 4)),
  ( (SELECT [Id] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Source' AND [DataType] = 4),'Facebook Visitor Post','2',2,(SELECT [SiteId] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Source' AND [DataType] = 4)),
  ( (SELECT [Id] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Source' AND [DataType] = 4),'Facebook Wall Post','3',3,(SELECT [SiteId] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Source' AND [DataType] = 4)),
  ( (SELECT [Id] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Source' AND [DataType] = 4),'Twitter Tweet','4',4,(SELECT [SiteId] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Source' AND [DataType] = 4)),
  ( (SELECT [Id] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Source' AND [DataType] = 4),'Twitter Direct Message','5',5,(SELECT [SiteId] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Source' AND [DataType] = 4)),
  ( (SELECT [Id] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Status' AND [DataType] = 4),'New','0',1,(SELECT [SiteId] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Status' AND [DataType] = 4)),
  ( (SELECT [Id] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Status' AND [DataType] = 4),'Pending Internal','1',2,(SELECT [SiteId] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Status' AND [DataType] = 4)),
  ( (SELECT [Id] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Status' AND [DataType] = 4),'Pending External','2',3,(SELECT [SiteId] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Status' AND [DataType] = 4)),
  ( (SELECT [Id] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Status' AND [DataType] = 4),'On Hold','3',4,(SELECT [SiteId] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Status' AND [DataType] = 4)),
  ( (SELECT [Id] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Status' AND [DataType] = 4),'Closed','4',5,(SELECT [SiteId] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Status' AND [DataType] = 4)),
  ( (SELECT [Id] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Priority' AND [DataType] = 4),'Low','0',1,(SELECT [SiteId] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Priority' AND [DataType] = 4)),
  ( (SELECT [Id] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Priority' AND [DataType] = 4),'Normal','1',2,(SELECT [SiteId] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Priority' AND [DataType] = 4)),
  ( (SELECT [Id] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Priority' AND [DataType] = 4),'High','2',3,(SELECT [SiteId] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Priority' AND [DataType] = 4)),
  ( (SELECT [Id] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Priority' AND [DataType] = 4),'Urgent','3',4,(SELECT [SiteId] FROM [Social].[dbo].[t_Social_ConversationField] WHERE [Name] = 'Priority' AND [DataType] = 4))
GO

if NOT exists(SELECT * FROM [Social].[dbo].[t_Social_Filter] WHERE [SiteId]=10000)
INSERT INTO [Social].[dbo].[t_Social_Filter]
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
  ('My Open', 1, 1,1,1,getdate(),10000 ),
  ('My Department''s Open', 2, 1,1,1,getdate(),10000 ),
  ('My Offline Colleagues'' Open', 3, 1,1,1,getdate(),10000 ),
  ('All Open', 4, 1,1,1,getdate(),10000 ),
  ('Unassigned', 5, 1,1,1,getdate(),10000 ),
  ('All', 6, 1,1,1,getdate(),10000 )
GO
INSERT INTO [Social].[dbo].[t_Social_FilterCondition]
  (
       [FilterId]
      ,[FieldId]
      ,[MatchType]
      ,[Value]
      ,[SiteId]
      ,[Index]
  )
  VALUES
  ((SELECT [Id] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'My Open' AND [IfPublic] = 1),4,2,'4',(SELECT [SiteId] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'My Open' AND [IfPublic] = 1),1),
  ((SELECT [Id] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'My Open' AND [IfPublic] = 1),2,1,'@Me',(SELECT [SiteId] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'My Open' AND [IfPublic] = 1),2),
  ((SELECT [Id] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'My Department''s Open' AND [IfPublic] = 1),4,2,'4',(SELECT [SiteId] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'My Department''s Open' AND [IfPublic] = 1),3),
  ((SELECT [Id] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'My Department''s Open' AND [IfPublic] = 1),3,1,'@My Department',(SELECT [SiteId] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'My Department''s Open' AND [IfPublic] = 1),4),
  ((SELECT [Id] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'My Offline Colleagues'' Open' AND [IfPublic] = 1),4,2,'4',(SELECT [SiteId] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'My Offline Colleagues'' Open' AND [IfPublic] = 1),5),
  ((SELECT [Id] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'My Offline Colleagues'' Open' AND [IfPublic] = 1),2,1,'@My Department Member',(SELECT [SiteId] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'My Offline Colleagues'' Open' AND [IfPublic] = 1),6),
  ((SELECT [Id] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'My Offline Colleagues'' Open' AND [IfPublic] = 1),3,1,'Blank',(SELECT [SiteId] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'My Offline Colleagues'' Open' AND [IfPublic] = 1),7),
  ((SELECT [Id] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'All Open' AND [IfPublic] = 1),4,2,'4',(SELECT [SiteId] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'All Open' AND [IfPublic] = 1),8),
  ((SELECT [Id] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'Unassigned' AND [IfPublic] = 1),2,1,'Blank',(SELECT [SiteId] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'Unassigned' AND [IfPublic] = 1),9),
  ((SELECT [Id] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'Unassigned' AND [IfPublic] = 1),3,1,'Blank',(SELECT [SiteId] FROM [Social].[dbo].[t_Social_Filter] WHERE [Name] = 'Unassigned' AND [IfPublic] = 1),10)
  GO