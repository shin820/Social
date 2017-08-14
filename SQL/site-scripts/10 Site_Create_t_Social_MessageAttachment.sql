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
		IF NOT EXISTS (SELECT name FROM sysobjects WHERE type=''U'' AND name=''t_Social_MessageAttachment'+@siteIdStr+''')
		BEGIN
			CREATE TABLE [t_Social_MessageAttachment'+@siteIdStr+'](
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
				[SiteId] [int] NOT NULL,
			 CONSTRAINT [PK_t_Social_MessageAttachment'+@siteIdStr+'] PRIMARY KEY CLUSTERED 
			(
				[Id] ASC
			)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
			) ON [PRIMARY]

			ALTER TABLE [t_Social_MessageAttachment'+@siteIdStr+']  WITH CHECK ADD  CONSTRAINT [FK_t_Social_MessageAttachment'+@siteIdStr+'_t_Social_Message'+@siteIdStr+'_MessageId] FOREIGN KEY([MessageId])
REFERENCES [t_Social_Message'+@siteIdStr+'] ([Id])
			ALTER TABLE [t_Social_MessageAttachment'+@siteIdStr+'] CHECK CONSTRAINT [FK_t_Social_MessageAttachment'+@siteIdStr+'_t_Social_Message'+@siteIdStr+'_MessageId]
		END
		'
	EXEC(@sql)
	SET @siteId=@siteId+1
	END
END