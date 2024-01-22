SET
    search_path TO public;

create table person (
    id serial primary key,
    name text not null
);

create table chat_message (
    id serial primary key,
    message_text text not null,
    created_at timestamp not null default now(),
    user_name text not null
);

create table chat_message_image (
    id serial primary key,
    chat_message_id integer not null,
    image_data text not null,
    file_name text not null,
    foreign key (chat_message_id) references chat_message(id)
);