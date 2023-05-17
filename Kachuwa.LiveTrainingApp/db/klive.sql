CREATE TABLE dbo.Server
(
    ServerId												int IDENTITY(1,1) NOT NULL primary key,    
    DisplayName                                                 nvarchar(256) not null,
    MachineName                                                nvarchar(256) not null,
    IPAddress                                                nvarchar(256) not null,
	UserName												nvarchar(256) not null,			
	Password                                                nvarchar(256) not null,
	ServerType													nvarchar(10) not null,--api,cdn,auth,Web,live,
	RTMPAddress													nvarchar(256) not null,
    IsActive						                            bit default(1),
	AddedBy							                            nvarchar(256),
	AddedOn							                            datetime default(getdate()),
	UpDatedBy						                            nvarchar(256),
	UpdatedOn						                            datetime default(getdate()),
	IsDeleted						                            bit default(0)
    
);

CREATE TABLE dbo.ServerStorage
(
	ServerStorageId												int IDENTITY(1,1) NOT NULL primary key,
	ServerId													int not null default(0),
	TempDirectory                                               nvarchar(256) not null,
    RootDirectory                                               nvarchar(256) not null,
	IsRootDirectoryFull											bit default(0),
	MinimumSpace												int not null default(1),--in gb
	UseOrder													int not null,
	IsActive						                            bit default(1),
	AddedBy							                            nvarchar(256),
	AddedOn							                            datetime default(getdate()),
	UpDatedBy						                            nvarchar(256),
	UpdatedOn						                            datetime default(getdate()),
	IsDeleted						                            bit default(0)

);


create table dbo.Stream
(
	StreamId													bigint not null primary key identity(1,1),
	StreamKey													nvarchar(256) not null,
	VideoId														nvarchar(500) not null,
	Title														nvarchar(256) not null,
	Tags														nvarchar(500),
	Description													nvarchar(max),
	IsScheduled													bit default(0),
	ScheduledAt													datetime,
	IsOnAir														bit default(0),--is live running?
	OnAiredAt													datetime default(getdate()),
	IsFinished													bit default(0),
	FinishedAt													datetime default(getdate()),
	Rtmp														nvarchar(256) not null,
	ViewCount													bigint not null default(0),
	LiveViewerCount												bigint not null default(0),
	RecordVideo													bit not null default(0),
	CoverImage													nvarchar(256),
	Likes														bigint not null default(0),
	Dislikes													bigint not null default(0),
	DelayInSecond												int not null default(0),	
	StreamedBy													bigint not null default(0),
	 AllowLiveChat												bit default(0),
	 AllowQuestionaire											bit default(0),
	IsActive						                            bit default(1),
	AddedBy							                            nvarchar(256),
	AddedOn							                            datetime default(getdate()),
	UpDatedBy						                            nvarchar(256),
	UpdatedOn						                            datetime default(getdate()),
	IsDeleted						                            bit default(0)


);
create table dbo.StreamStatus
(
	StreamStatusId												bigint not null primary key identity(1,1),
	StreamKey													nvarchar(256) not null,
	RecievingSignal												bit default(0),
	ErrorMessage												nvarchar(max),
	IncomingIpAddress											nvarchar(256),
	LastChecked													datetime not null default(getdate())

)
create table dbo.LiveChat
(
	LiveChatId												bigint not null primary key identity(1,1),
	StreamId													bigint not null default(0),
	Message														nvarchar(500),
	SenderId													bigint not null default(0),
	AddedBy							                            nvarchar(256),
	AddedOn							                            datetime default(getdate()),
	UpDatedBy						                            nvarchar(256),
	UpdatedOn						                            datetime default(getdate()),
	IsDeleted						                            bit default(0)
)
create table dbo.StreamComment
(
	StreamCommentId												bigint not null primary key identity(1,1),
	StreamId													bigint not null default(0),
	VideoId														nvarchar(500) not null,
	Comment														nvarchar(500),
	ParentCommentId												bigint not null default(0),
	CommenterId													bigint not null default(0),
	IsActive						                            bit default(1),
	AddedBy							                            nvarchar(256),
	AddedOn							                            datetime default(getdate()),
	UpDatedBy						                            nvarchar(256),
	UpdatedOn						                            datetime default(getdate()),
	IsDeleted						                            bit default(0)
);
create table dbo.UserAudio
(
	UserAudioId													bigint not null primary key identity(1,1),
	StreamId													bigint not null default(0),
	FilePath													nvarchar(500),
	FileContent													nvarchar(max),
	IsFileBased													bit default(1) not null,
	QueueNumber													int not null default(1),
	IsFinishedRecording											bit not null default(0),
	SenderId													bigint not null default(0),
	IsActive						                            bit default(1),
	AddedBy							                            nvarchar(256),
	AddedOn							                            datetime default(getdate()),
	UpDatedBy						                            nvarchar(256),
	UpdatedOn						                            datetime default(getdate()),
	IsDeleted						                            bit default(0)
);


create TABLE dbo.LiveEncodingFormat
(
	LiveEncodingFormatId									bigint not null primary key identity(1,1),
	Name														nvarchar(500) not null,--144p
	Display														nvarchar(500) not null,	
	Commands													nvarchar(max) not null,
	FlagShip													nvarchar(5) not null default('v'),--sd hd fhd 2k
	IsActive						                            bit default(1),
	AddedBy							                            nvarchar(256),
	AddedOn							                            datetime default(getdate()),
	UpDatedBy						                            nvarchar(256),
	UpdatedOn						                            datetime default(getdate()),
	IsDeleted						                            bit default(0)
);

create table dbo.LiveEncoding
(
	LiveEncodingId												bigint not null primary key identity(1,1),
	Name														nvarchar(256) not null,--hls dash 
	DisplayName													nvarchar(256) not null,
	Remarks														nvarchar(500),
	InputCommand												nvarchar(max),
	OtherCommand												nvarchar(max),
	OutputCommand												nvarchar(max),
	Support2k													bit default(0),
	Support4k													bit default(0),
	Support8k													bit default(0),
	IsDefault													bit default(0),
	IsActive						                            bit default(1),	
	AddedBy							                            nvarchar(256),
	AddedOn							                            datetime default(getdate()),
	UpDatedBy						                            nvarchar(256),
	UpdatedOn						                            datetime default(getdate()),
	IsDeleted						                            bit default(0)
);

CREATE TABLE  dbo.UserStreamSetting
(
	UserStreamSettingId							bigint not null primary key identity(1,1),
	UserId									bigint not null default(0),
	LiveEncodingId				bigint not null default(0),
	LiveEncodingFormatIds		nvarchar(256) not null
);



Create table dbo.Subtitle
(
	SubtitleId												int primary key identity(1,1),
	VideoId													nvarchar(256) not null,
	FilePath												nvarchar(256) not null,
	[Language]												nvarchar(10) not null,
	Kind													nvarchar(256),
	SRCLang													nvarchar(256),
	IsActive                                				bit NOT NULL Default(1),
    IsDeleted                               				bit NOT NULL Default(0),
    AddedOn                                 				datetime NOT NULL Default(GETUTCDATE()),
    AddedBy                                 				national character varying(256) not null

);

CREATE TABLE dbo.Genre
(
	GenreId													int primary key identity(1,1),
	GenreName												nvarchar(256) not null,
	IsActive                                				bit NOT NULL Default(1),
    IsDeleted                               				bit NOT NULL Default(0),
    AddedOn                                 				datetime NOT NULL Default(GETUTCDATE()),
    AddedBy                                 				national character varying(256) not null
);
--icon must be made in design
CREATE TABLE MovieType(
	MovieTypeId												int primary key identity(1,1),
	Name													nvarchar(20) not null,
	Icon													nvarchar(500) null,
	Description												nvarchar(256),
	IsActive												bit not null default(1),
	IsDeleted												bit not null default(0),
	AddedOn													datetime not null default(getutcdate()),
	AddedBy													national character varying(256) not null
	);

CREATE TABLE dbo.Movie
(
	MovieId													int primary key identity(1,1),
	MovieName												national character varying(256) not null,	
	ImageUrl												nvarchar(500),
	Description												national character varying(1000),
	GenreIds												national character varying(256),
	Genres													national character varying(1000),	
	MotionPictureRatingId									int ,
	Duration												int not null,
	IsInternational											bit default(0),
	Country													int,--national character varying(256),
	Language												national character varying(256),
	AudioLanguage											national character varying(256),
	MovieType												int not null,
	Rating													int,
	GlobalReleaseDate										datetime not null,
	ReleaseDate												datetime not null,
	Directors												national character varying(1000),
	Producers												national character varying(1000),
	Stars													national character varying(1000),
	PreviewLink												national character varying(1000),
	Keywords												national character varying(1000),
	IsActive                                				bit NOT NULL Default(1),
    IsDeleted                               				bit NOT NULL Default(0),
    AddedOn                                 				datetime NOT NULL Default(GETUTCDATE()),
    AddedBy                                 				national character varying(256) not null
	);
CREATE TABLE dbo.MovieImage
(
	MovieImageId											int primary key identity(1,1),
	MovieId													int references dbo.Movie,
	Image													nvarchar(256) not null,
	IsBanner												bit default(0),
	DisplayOrder											int
);

CREATE TABLE dbo.Program
(
	ProgramId													int primary key identity(1,1),
	Name												national character varying(256) not null,	
	ImageUrl												nvarchar(500),
	Description												national character varying(1000),
	GenreIds												national character varying(256),
	Genres													national character varying(1000),	
	MotionPictureRating										national character varying(256),
	Duration												int not null,
	IsInternational											bit default(0),
	Country													int,--national character varying(256),
	Language												national character varying(256),
	AudioLanguage											national character varying(256),	
	Rating													int,	
	ReleaseDate												datetime not null,
	Directors												national character varying(1000),
	Producers												national character varying(1000),
	Stars													national character varying(1000),
	PreviewLink												national character varying(1000),
	Keywords												national character varying(1000),
	IsActive                                				bit NOT NULL Default(1),
    IsDeleted                               				bit NOT NULL Default(0),
    AddedOn                                 				datetime NOT NULL Default(GETUTCDATE()),
    AddedBy                                 				national character varying(256) not null
);
create table dbo.Season
(
	SeasonId												int primary key identity(1,1),
	Name													national character varying(256) not null,
	ProgramId												int not null default(0),
	DisplayOrder											int not null default(0),
	IsActive                                				bit NOT NULL Default(1),
    IsDeleted                               				bit NOT NULL Default(0),
    AddedOn                                 				datetime NOT NULL Default(GETUTCDATE()),
    AddedBy                                 				national character varying(256) not null	
);
CREATE TABLE dbo.ProgramImage
(
	ProgramImageId											int primary key identity(1,1),
	ProgramId													int references dbo.Movie,
	Image													nvarchar(256) not null,
	IsBanner												bit default(0),
	DisplayOrder											int
);
CREATE TABLE dbo.Episode (
	Episode													int primary key identity(1,1),
	ProgramId													int not null,
	SeasonId												int not null default(0),
	HasSeason												bit not null default(0),
	Name													national character varying(256) not null,
	Code													national character varying(256) not null,	
	Duration												int not null,
	ReleaseDate												datetime not null,
	IsActive                                				bit NOT NULL Default(1),
    IsDeleted                               				bit NOT NULL Default(0),
    AddedOn                                 				datetime NOT NULL Default(GETUTCDATE()),
    AddedBy                                 				national character varying(256) not null
 );
truncate table dbo.LiveEncodingFormat
insert into dbo.LiveEncodingFormat Select 'X144P','X144P','-b:v:0 3150k  -c:v libx264  -filter:v:0 "scale=144:-1" -rc:v vbr_hq -pix_fmt yuv420p -profile:v main -level 4.1 -strict_gop 1 -rc-lookahead 32 -no-scenecut 1 -forced-idr 1  -b:a:0 128k -map 0:v -map 0:a:0 ','144p',1,'Binod Tamang',getdate(),'',getdate(),0
insert into dbo.LiveEncodingFormat Select 'X360P','X360P','-b:v:0 3150k  -c:v libx264  -filter:v:0 "scale=320:-1" -rc:v vbr_hq -pix_fmt yuv420p -profile:v main -level 4.1 -strict_gop 1 -rc-lookahead 32 -no-scenecut 1 -forced-idr 1  -b:a:0 128k -map 0:v -map 0:a:0 ','360p',1,'Binod Tamang',getdate(),'',getdate(),0
insert into dbo.LiveEncodingFormat Select 'X480P','X480P','-b:v:0 3150k  -c:v libx264  -filter:v:0 "scale=480:-1" -rc:v vbr_hq -pix_fmt yuv420p -profile:v main -level 4.1 -strict_gop 1 -rc-lookahead 32 -no-scenecut 1 -forced-idr 1  -b:a:0 128k -map 0:v -map 0:a:0 ','480p',1,'Binod Tamang',getdate(),'',getdate(),0
insert into dbo.LiveEncodingFormat Select 'X720P','X720P','-b:v:0 3150k  -c:v libx264  -filter:v:0 "scale=720:-1" -rc:v vbr_hq -pix_fmt yuv420p -profile:v main -level 4.1 -strict_gop 1 -rc-lookahead 32 -no-scenecut 1 -forced-idr 1  -b:a:0 128k -map 0:v -map 0:a:0 ','hd',1,'Binod Tamang',getdate(),'',getdate(),0
insert into dbo.LiveEncodingFormat Select 'X1080P','X1080P','-b:v:0 5250k  -c:v libx264  -filter:v:0 "scale=1080:-1" -rc:v vbr_hq -pix_fmt yuv420p -profile:v main -level 4.1 -strict_gop 1 -rc-lookahead 32 -no-scenecut 1 -forced-idr 1  -b:a:0 128k -map 0:v -map 0:a:0 ','fhd',1,'Binod Tamang',getdate(),'',getdate(),0
insert into dbo.LiveEncodingFormat Select 'X2K','X2K','-b:v:0 6200k  -c:v libx264  -filter:v:0 "scale=2048:-1" -rc:v vbr_hq -pix_fmt yuv420p -profile:v main -level 4.1 -strict_gop 1 -rc-lookahead 32 -no-scenecut 1 -forced-idr 1  -b:a:0 128k -map 0:v -map 0:a:0 ','2k',1,'Binod Tamang',getdate(),'',getdate(),0
insert into dbo.LiveEncodingFormat Select 'XQHD','XQHD','-b:v:0 7000k  -c:v libx264  -filter:v:0 "scale=2560:-1" -rc:v vbr_hq -pix_fmt yuv420p -profile:v main -level 4.1 -strict_gop 1 -rc-lookahead 32 -no-scenecut 1 -forced-idr 1  -b:a:0 128k -map 0:v -map 0:a:0 ','qhd',1,'Binod Tamang',getdate(),'',getdate(),0
insert into dbo.LiveEncodingFormat Select 'X4K','X4K','-b:v:0 8000k  -c:v libx264  -filter:v:0 "scale=3840:-1" -rc:v vbr_hq -pix_fmt yuv420p -profile:v main -level 4.1 -strict_gop 1 -rc-lookahead 32 -no-scenecut 1 -forced-idr 1  -b:a:0 128k -map 0:v -map 0:a:0 ','4k',1,'Binod Tamang',getdate(),'',getdate(),0
insert into dbo.LiveEncodingFormat Select 'X8K','X8K','-b:v:0 10000k  -c:v libx264  -filter:v:0 "scale=7680:-1" -rc:v vbr_hq -pix_fmt yuv420p -profile:v main -level 4.1 -strict_gop 1 -rc-lookahead 32 -no-scenecut 1 -forced-idr 1  -b:a:0 128k -map 0:v -map 0:a:0 ','8k',1,'Binod Tamang',getdate(),'',getdate(),0

insert into LiveEncoding select 'Default HLS','HLS','till 1080','','','',0,0,0,1,1,'Binod Tamang',getdate(),'',getdate(),0

declare @serverId int=0
insert into [dbo].[Server](DisplayName,MachineName,IPAddress,UserName,Password,ServerType,RTMPAddress,IsActive,AddedBy)
values ('Online Kachhya','OK-WIN1','202.51.74.238','Administrator','password','LIVE','rtmp://202.51.74.238/living',1,'BinodTamang')
select @serverId=SCOPE_IDENTITY()

insert into [dbo].[ServerStorage](ServerId,TempDirectory,RootDirectory,IsRootDirectoryFull,MinimumSpace,UseOrder,IsActive,AddedBy)
values(@serverId,'e:\ltemp','e:\live',0,1,0,1,'Binod Tamang');

----FOR LOCAL
--truncate table [dbo].[Server]
--truncate table [dbo].[ServerStorage]
--declare @serverId int=0
--insert into [dbo].[Server](DisplayName,MachineName,IPAddress,UserName,Password,ServerType,RTMPAddress,IsActive,AddedBy)
--values ('Online Kachhya','Kachuwa1','202.51.74.238','Administrator','password','LIVE','rtmp://localhost/living',1,'BinodTamang')
--select @serverId=SCOPE_IDENTITY()

--insert into [dbo].[ServerStorage](ServerId,TempDirectory,RootDirectory,IsRootDirectoryFull,MinimumSpace,UseOrder,IsActive,AddedBy)
--values(@serverId,'d:\temp','d:\live',0,1,0,1,'Binod Tamang');
