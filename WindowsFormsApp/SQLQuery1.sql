create table Users
(
Id int primary key identity,
UserLogin nvarchar(max),
UserPassword nvarchar(max)
);
create table Contacts
(
Id int primary key identity,
IdContact int,
ContactName nvarchar(max),
IdUser int
);
create table UserChat
(
Id int primary key identity,
IdUser int,
IdChat int
);
create table Chats
(
Id int primary key identity
);
create table ChatMessages
(
Id int primary key identity,
IdChat int,
MessageText nvarchar(max)
);

alter table Contacts add foreign key(IdContact) references Users(Id);
alter table Contacts add foreign key(IdUser) references Users(Id);
alter table UserChat add foreign key(IdUser) references Users(Id);
alter table UserChat add foreign key(IdChat) references Chats(Id);
alter table ChatMessages add foreign key(IdChat) references Chats(Id);

insert into Users values('User1', 'qwerty'),('User2', 'ytrewq');
insert into Contacts values(2, 'Contact1', 1),(1, 'MyContact', 2);