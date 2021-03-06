USE [RaceGameDB]
GO
/****** Object:  Table [dbo].[GamePlays]    Script Date: 11/10/2013 17:08:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GamePlays](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Section] [int] NOT NULL,
	[VelocityFreeway] [int] NOT NULL,
	[VelocityTollway] [int] NOT NULL,
	[PriceSubject] [int] NOT NULL,
	[PriceRandom] [int] NOT NULL,
	[Account] [int] NOT NULL,
	[TimeLeft] [real] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
