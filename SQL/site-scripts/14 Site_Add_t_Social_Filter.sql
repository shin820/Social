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
		SELECT 
		     [Name]
			,[Index]
			,[IfPublic]
			,[Type]
			,[CreatedBy]
			,getdate()
			,'+@siteIdStr+' AS [SiteId]
		FROM [Comm100.General].[dbo].[t_Social_Filter_Config]
		'
	EXEC(@sql)
	SET @siteId=@siteId+1
	END
END