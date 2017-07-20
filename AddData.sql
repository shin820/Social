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
(1,4,'To',10000),
(1,4,'Replied Agents',10000),
(1,4,'Last Replied Agent',10000),
(1,4,'Last Message Sent by',10000),
(1,4,'Agent Assignee Status',10000),
(1,4,'Tags',10000),
(1,1,'From',10000),
(1,1,'Notes',10000),
(1,1,'Logs',10000),
(1,1,'Messages',10000),
(1,3,'Last Message Sent',10000),
(1,3,'Created',10000),
(1,3,'Last Modified',10000),
(1,2,'Handling Time',10000),
(1,2,'Total Messages',10000),
(1,2,'Conversation ID',10000)


insert into [Social].[dbo].[t_Social_ConversationFieldOption]
  (
       [FieldId]
      ,[Name]
      ,[Value]
      ,[Index]
      ,[SiteId]
  )
  values
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