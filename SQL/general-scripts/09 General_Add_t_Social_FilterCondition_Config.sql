IF OBJECT_ID('t_Social_FilterCondition_Config') IS NOT NULL
AND NOT EXISTS (SELECT * FROM [t_Social_FilterCondition_Config])
BEGIN
	INSERT INTO [t_Social_FilterCondition_Config]
	(
		[FilterName]
		,[FieldName]
		,[MatchType]
		,[Value]
		,[Index]
	)
	VALUES
	('My Open','Status',2,'4',1),
	('My Open','Agent Assignee',1,'@Me',2),
	('My Departments'' Open','Status',2,'4',3),
	('My Departments'' Open','Department Assignee',1,'@My Department',4),
	('My Offline Department Members'' Open','Status',2,'4',5),
	('My Offline Department Members'' Open','Agent Assignee',1,'@My Department Member',6),
	('My Offline Department Members'' Open','Agent Assignee Status',1,'2',7),
	('All Open','Status',2,'4',8),
	('Unassigned','Agent Assignee',1,'Blank',9),
	('Unassigned','Department Assignee',1,'Blank',10)
END