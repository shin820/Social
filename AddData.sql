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
  ( 1,'Facebook Message','1',1,10000),
  ( 1,'Facebook Visitor Post','2',2,10000),
  ( 1,'Facebook Wall Post','3',3,10000),
  ( 1,'Twitter Tweet','4',4,10000),
  ( 1,'Twitter Direct Message','5',5,10000),
  ( 4,'New','0',1,10000),
  ( 4,'Pending Internal','1',2,10000),
  ( 4,'Pending External','2',3,10000),
  ( 4,'On Hold','3',4,10000),
  ( 4,'Closed','4',5,10000),
  ( 5,'Low','0',1,10000),
  ( 5,'Normal','1',2,10000),
  ( 5,'High','2',3,10000),
  ( 5,'Urgent','3',4,10000)
GO
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
  (1,4,2,'4',10000,1),
  (1,2,1,'@Me',10000,2),
  (2,4,2,'4',10000,3),
  (2,3,1,'@My Department',10000,4),
  (3,4,2,'4',10000,5),
  (3,2,1,'@My Department Member',10000,6),
  (3,3,1,'Blank',10000,7),
  (4,4,2,'4',10000,8),
  (5,2,1,'Blank',10000,9),
  (5,3,1,'Blank',10000,10)
  GO