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

    public virtual DbSet<ChatMessageImage> ChatMessageImages { get; set; }

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

        modelBuilder.Entity<ChatMessageImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("chat_message_image_pkey");

            entity.ToTable("chat_message_image");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ChatMessageId).HasColumnName("chat_message_id");

            entity.HasOne(d => d.ChatMessage).WithMany(p => p.ChatMessageImages)
                .HasForeignKey(d => d.ChatMessageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("chat_message_image_chat_message_id_fkey");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
