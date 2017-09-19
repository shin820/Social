BEGIN
		DECLARE @sql varchar(max)
		SET @sql=
		'
		INSERT INTO  [t_Social_ConversationField]
		(
			[IfSystem]
			,[DataType]
			,[Name]
		)
		VALUES
		(1,4,''Source''),
		(1,4,''Agent Assignee''),
		(1,4,''Department Assignee''),
		(1,4,''Status''),
		(1,4,''Priority''),
		(1,4,''Social Accounts''),
		(1,4,''Replied Agents''),
		(1,4,''Last Replied Agent''),
		(1,1,''Last Message Sent by''),
		(1,4,''Agent Assignee Status''),
		(1,4,''Department Assignee Status''),
		(1,1,''Social Users''),
		(1,1,''Comment/Messages''),
		(1,3,''Last Message Sent''),
		(1,3,''Created''),
		(1,3,''Last Modified''),
		(1,2,''Time to Last Message''),
		(1,2,''Total Messages''),
		(1,2,''Conversation ID'')
		'
	EXEC(@sql)
	END