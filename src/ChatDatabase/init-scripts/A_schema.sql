SET
    search_path TO public;

create table chat_message (
    id uuid primary key,
    message_text text not null,
    created_at timestamp not null default now(),
    user_name text not null,
    lamport_clock integer not null,
    vector_clock jsonb not null
);

create table chat_message_image (
    id uuid primary key,
    chat_message_id uuid not null,
    foreign key (chat_message_id) references chat_message(id)
);

create table image_location (
    id uuid primary key,
    chat_message_image_id uuid not null,
    service_identifier numeric not null
);