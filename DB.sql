USE [TEC]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](50) NOT NULL,
	[IPAddress] [nvarchar](50) NULL,
	[Longitude] [decimal](10, 3) NULL,
	[Latitude] [decimal](10, 3) NULL,
	[Timestamp] [datetime] NULL,
	[ActionPageID] [int] NULL,
	[IsBlocked] [bit] NOT NULL,
	[OTP] [nvarchar](50) NULL,
	[RetryCount] [int] NOT NULL,
	[VerifiedTimestamp] [datetime] NULL,
	[MobileNo] [nvarchar](50) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_IsBlocked]  DEFAULT ((0)) FOR [IsBlocked]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_RetryCount]  DEFAULT ((0)) FOR [RetryCount]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Hakimuddin
-- Create date: 17-10-2019
-- Description:	Create OTP for the User
-- =============================================
CREATE PROCEDURE [dbo].[CREATE_OTP] 
	@USERNAME NVARCHAR(50), 
	@OTP NVARCHAR(50),
	@IPADDRESS NVARCHAR(50) = NULL,
	@LATITUDE DECIMAL(10,3) = NULL,
	@LONGITUDE DECIMAL (10,3) = NULL,
	@ACTIONPAGEID NVARCHAR(50) = NULL,
	@MOBILENO NVARCHAR(50) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ISBLOCKED BIT;
	DECLARE @ID INT;

	SELECT	@ID = ID,
			@ISBLOCKED = ISBLOCKED,
			@MOBILENO = MOBILENO
			FROM Users 
			WHERE Username = @Username;

	IF @ID IS NULL
	BEGIN
		RAISERROR('USER DOES NOT EXISTS',16,1);
		RETURN;
	END
	ELSE IF @ISBLOCKED = 1
	BEGIN
		RAISERROR('USER BLOCKED',16,1);
		RETURN;
	END
	ELSE
	BEGIN
		UPDATE [dbo].[Users]
		   SET 
			   [IPAddress] = @IPADDRESS
			  ,[Longitude] = @LONGITUDE
			  ,[Latitude] = @LATITUDE
			  ,[Timestamp] = CURRENT_TIMESTAMP
			  ,[ActionPageID] = @ACTIONPAGEID		 
			  ,[OTP] = @OTP
			  ,[RetryCount] = 0
		 WHERE [USERNAME] =@USERNAME;
	END
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Hakimuddin
-- Create date: 17-10-2019
-- Description:	To Get the OTP of User for Resending
-- =============================================
CREATE PROCEDURE [dbo].[Get_OTP] 	
	@Username NVARCHAR(50), 
	@OTP NVARCHAR(50) OUTPUT ,
	@MOBILENO NVARCHAR(50) OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @ISBLOCKED BIT;
	DECLARE @ID INT;

	SELECT	@ID = ID, 
			@OTP = OTP, 
			@ISBLOCKED = ISBLOCKED ,
			@MOBILENO = MOBILENO
			FROM Users 
			WHERE Username = @Username;

	IF @ID IS NULL
	BEGIN
		RAISERROR('USER DOES NOT EXISTS',16,1);
		RETURN;
	END
	ELSE 
	BEGIN
		IF @ISBLOCKED IS NOT NULL AND @ISBLOCKED = 1
		BEGIN
			SET @OTP = NULL;
			SET @MOBILENO = NULL;

			RAISERROR('USER BLOCKED',16,1);
			RETURN;
		END			
	END	
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Hakimuddin
-- Create date: 17-10-2019
-- Description:	Verify OTP
-- =============================================
CREATE PROCEDURE [dbo].[Verify_OTP] 	
	@Username NVARCHAR(50),
	@OTP NVARCHAR(50)	
AS
BEGIN	
	SET NOCOUNT ON;

	DECLARE @ISBLOCKED BIT;
	DECLARE @ID INT;
	DECLARE @RETRYCOUNT INT;
	DECLARE @SAVEDOTP NVARCHAR(50);
	DECLARE @TIMESTAMP DATETIME;

	SELECT	@ID = ID,
			@SAVEDOTP = OTP, 
			@ISBLOCKED = ISBLOCKED,
			@RETRYCOUNT = RETRYCOUNT,
			@TIMESTAMP = [TIMESTAMP]
			FROM Users 
			WHERE Username = @Username;

	IF @ID IS NULL
	BEGIN
		RAISERROR('USER DOES NOT EXISTS',16,1);
		RETURN 0;
	END
	ELSE 
	BEGIN
		IF @ISBLOCKED IS NOT NULL AND @ISBLOCKED = 1
		BEGIN
			SET @OTP = NULL;
			RAISERROR('USER BLOCKED',16,1);
			RETURN 0;
		END
		ELSE IF @OTP <> @SAVEDOTP
		BEGIN		
			SET @RETRYCOUNT = @RETRYCOUNT + 1;
			
			IF @RETRYCOUNT = 3
			BEGIN
				SET @ISBLOCKED = 1;
			END
			
			UPDATE USERS SET IsBlocked = @ISBLOCKED, RETRYCOUNT = @RETRYCOUNT WHERE USERNAME = @Username;

			RAISERROR('INVALID OTP',16,1);
			RETURN 0; 
		END
		ELSE IF  DATEDIFF(MINUTE,@TIMESTAMP,CURRENT_TIMESTAMP) > 15
		BEGIN
			RAISERROR('OTP EXPIRED',16,1);
			RETURN 0;
		END
		ELSE IF @OTP = @SAVEDOTP AND DATEDIFF(MINUTE,@TIMESTAMP,CURRENT_TIMESTAMP) <= 15
		BEGIN
			RETURN 1;
		END
	END	
  
END
GO
