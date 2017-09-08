SET NOEXEC OFF

USE ETLTest
GO

DECLARE @DBName varchar(MAX)

SELECT @DBName = DB_NAME()

IF @DBName <> 'ETLTest'
BEGIN
	PRINT 'Incorrect database. This test data generator will only run on ETLTest'
	SET NOEXEC ON
END

IF OBJECT_ID('TestTableSize') IS NOT NULL DROP TABLE dbo.TestTableSize

CREATE TABLE dbo.TestTableSize
(
	MyKeyField VARCHAR(10) NOT NULL,
	MyDate1 DATETIME NOT NULL,
	MyDate2 DATETIME NOT NULL,
	MyDate3 DATETIME NOT NULL,
	MyDate4 DATETIME NOT NULL,
	MyDate5 DATETIME NOT NULL
)

GO


DECLARE @RowCount INT
DECLARE @RowString VARCHAR(10)
DECLARE @Random INT
DECLARE @Upper INT
DECLARE @Lower INT
DECLARE @InsertDate DATETIME

SET @Lower = -730
SET @Upper = -1
SET @RowCount = 0

WHILE @RowCount < 100000
BEGIN
	SET @RowString = CAST(@RowCount AS VARCHAR(10))
	SELECT @Random = ROUND(((@Upper - @Lower -1) * RAND() + @Lower), 0)
	SET @InsertDate = DATEADD(dd, @Random, GETDATE())
	
	INSERT INTO TestTableSize
		(MyKeyField
		,MyDate1
		,MyDate2
		,MyDate3
		,MyDate4
		,MyDate5)
	VALUES
		(REPLICATE('0', 10 - DATALENGTH(@RowString)) + @RowString
		, @InsertDate
		,DATEADD(dd, 1, @InsertDate)
		,DATEADD(dd, 2, @InsertDate)
		,DATEADD(dd, 3, @InsertDate)
		,DATEADD(dd, 4, @InsertDate))

	SET @RowCount = @RowCount + 1
END

GO

IF OBJECT_ID('ResultingRecord') IS NOT NULL DROP TABLE dbo.ResultingRecord

CREATE TABLE dbo.ResultingRecord
(
	Id VARCHAR(10) NOT NULL,
	MinimumDate DATETIME NOT NULL
)

GO
