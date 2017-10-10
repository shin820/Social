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
		SET @sql=
		'
		IF OBJECT_ID(''t_Social_ConversationField'') IS NOT NULL
		AND NOT EXISTS (SELECT * FROM [t_Social_ConversationField] WHERE SiteId='+@siteIdStr+')
		BEGIN
		INSERT INTO  [t_Social_ConversationField]
		(
			[IfSystem]
			,[DataType]
			,[Name]
			,[SiteId]
		)
		SELECT
			[IfSystem]
			,[DataType]
			,[Name]
			,'+@siteIdStr+' AS [SiteId]
	    FROM [Comm100.General].[dbo].[t_Social_ConversationField_Config]
		END
		'
	EXEC(@sql)
	SET @siteId=@siteId+1
	END
END