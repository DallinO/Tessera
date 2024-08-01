use Guardian;

create table player (
	id int primary key not null,
	username varchar not null,
	nickname varchar,
	[password] varchar not null,
	email varchar not null,
	join_date datetime not null
);

select * from player;