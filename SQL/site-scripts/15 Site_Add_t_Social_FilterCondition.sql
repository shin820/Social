BEGIN
    --DECLARE @siteDbId INT
	DECLARE @siteFrom INT
	DECLARE @siteTo INT

	--SET @siteDbId=21;
	SET @siteFrom=(@siteDbId-1)*500
	SET @siteTo=@siteFrom+500

	DECLARE @siteId INT
	SET @siteId=@siteFrom

	WHILE @siteId < @siteTo
	BEGIN
	    DECLARE @siteIdStr nvarchar(100)
		SET @siteIdStr=CONVERT(nvarchar(100),@siteId)
		DECLARE @sql varchar(max)
		DECLARE @filterId int;

		SET @sql=
		'
		IF OBJECT_ID(''t_Social_FilterCondition'') IS NOT NULL
		AND OBJECT_ID(''t_Social_Filter'') IS NOT NULL
		AND OBJECT_ID(''t_Social_ConversationField'') IS NOT NULL
		AND NOT EXISTS (SELECT * FROM [t_Social_FilterCondition] WHERE SiteId='+@siteIdStr+')
		BEGIN
		INSERT INTO [t_Social_FilterCondition]
		(
			[FilterId]
			,[FieldId]
			,[MatchType]
			,[Value]
			,[SiteId]
			,[Index]
		)
		SELECT 
		(SELECT [Id] AS [FilterId] FROM [t_Social_Filter] WHERE [SiteId] = '+@siteIdStr+' AND [Name] = p.FilterName)
		,(SELECT [Id] AS [FieldId] FROM [t_Social_ConversationField] WHERE [SiteId] = '+@siteIdStr+' AND [Name] = p.FieldName)
		,[MatchType]
		,[Value]
		,'+@siteIdStr+' AS [SiteId]
		,[Index]
		FROM [Comm100.General].[dbo].[t_Social_FilterCondition_Config] p
		END
		'
	EXEC(@sql)
	SET @siteId=@siteId+1
	END
END