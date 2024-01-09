SET
    search_path TO public;

create table person (
    id serial primary key,
    name text not null
);

create table message (
    id serial primary key,
    message text not null,
    created_at timestamp not null default now()
);