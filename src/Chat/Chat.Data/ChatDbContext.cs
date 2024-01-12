using System;
using System.Collections.Generic;
using Chat.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Chat.Data;

public partial class ChatDbContext : DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ChatMessage> ChatMessages { get; set; }

    public virtual DbSet<Person> People { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("chat_message_pkey");

            entity.ToTable("chat_message");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.MessageText).HasColumnName("message_text");
            entity.Property(e => e.UserName).HasColumnName("user_name");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("person_pkey");

            entity.ToTable("person");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
