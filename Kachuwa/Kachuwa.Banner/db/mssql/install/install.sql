
CREATE TABLE dbo.BannerKey
(
	BannerKeyId													int primary key identity(1,1),
	Name														nvarchar(256) not null,
	Description													nvarchar(500) null,
	IsActive													bit NOT NULL Default(1),
    IsDeleted													bit NOT NULL Default(0),
    AddedOn														datetime NOT NULL Default(GETUTCDATE()),
    AddedBy														national character varying(256)
);

CREATE TABLE dbo.Banner(
	BannerId													int primary key IDENTITY(1,1) NOT NULL,
	KeyId														int NULL,
	IsVideo														bit default(0),
	VideoLink													nvarchar(256),
	EmbeddedVideoLink											nvarchar(1000),
	Image														nvarchar(256) NOT NULL,
	HeadingText													nvarchar(256) NULL,
	Content														nvarchar(500) NULL,	
	HeadingTextAnimation										nvarchar(250) NULL,
	HeadingContentAnimation										nvarchar(250) NULL,
	LinkAnimation												nvarchar(250) NULL,
	BannerAnimation												nvarchar(250) NULL,
	BannerContentPositionClass									nvarchar(250) NULL,
	BannerContentColor											nvarchar(250) NULL,
	BannerLinkColor												nvarchar(250) NULL,
	BannerImagePosition											nvarchar(250) NULL,
	BannerBackgroundColor										nvarchar(250) NULL,
	BannerHeadingColor											nvarchar(250) NULL,
	Link														nvarchar(256) NULL,
	IsTemporary													bit default(0) not null,
	ExpiredOn													datetime not null Default(GETUTCDATE()),
	IsActive													bit NOT NULL Default(1),
    IsDeleted													bit NOT NULL Default(0),
    AddedOn														datetime NOT NULL Default(GETUTCDATE()),
    AddedBy														national character varying(256)
);
CREATE TABLE dbo.BannerSetting
(
	BannerSettingId												int primary key identity(1,1),
	KeyId														int references BannerKey,
	ImageHeight													int not null default(650),
	ImageWidth													int not null default(1920)
);