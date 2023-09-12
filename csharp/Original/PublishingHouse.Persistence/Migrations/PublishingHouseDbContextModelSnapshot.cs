﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PublishingHouse.Persistence;

#nullable disable

namespace PublishingHouse.Persistence.Migrations
{
    [DbContext(typeof(PublishingHouseDbContext))]
    partial class PublishingHouseDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("PublishingHouse.Persistence.Authors.AuthorEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Authors", (string)null);
                });

            modelBuilder.Entity("PublishingHouse.Persistence.Books.BookEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AuthorId")
                        .HasColumnType("uuid");

                    b.Property<string>("BindingType")
                        .HasColumnType("text");

                    b.Property<int>("CurrentState")
                        .HasColumnType("integer");

                    b.Property<int>("Edition")
                        .HasColumnType("integer");

                    b.Property<string>("Genre")
                        .HasColumnType("text");

                    b.Property<string>("ISBN")
                        .HasColumnType("text");

                    b.Property<int?>("NumberOfIllustrations")
                        .HasColumnType("integer");

                    b.Property<DateOnly?>("PublicationDate")
                        .HasColumnType("date");

                    b.Property<Guid>("PublisherId")
                        .HasColumnType("uuid");

                    b.Property<string>("Summary")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("TotalPages")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("PublisherId");

                    b.ToTable("Books", (string)null);
                });

            modelBuilder.Entity("PublishingHouse.Persistence.Languages.LanguageEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Languages", (string)null);
                });

            modelBuilder.Entity("PublishingHouse.Persistence.Publishers.PublisherEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Publishers", (string)null);

                    b.HasData(
                        new
                        {
                            Id = new Guid("c528d322-17eb-47ba-bccf-6cb61d340f09"),
                            Name = "Readers Digest"
                        });
                });

            modelBuilder.Entity("PublishingHouse.Persistence.Reviewers.ReviewerEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("BookEntityId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("BookEntityId");

                    b.ToTable("Reviewers", (string)null);
                });

            modelBuilder.Entity("PublishingHouse.Persistence.Translators.TranslatorEntity", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Translators", (string)null);
                });

            modelBuilder.Entity("PublishingHouse.Persistence.Books.BookEntity", b =>
                {
                    b.HasOne("PublishingHouse.Persistence.Authors.AuthorEntity", "Author")
                        .WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PublishingHouse.Persistence.Publishers.PublisherEntity", "Publisher")
                        .WithMany()
                        .HasForeignKey("PublisherId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("PublishingHouse.Persistence.Books.ValueObjects.CommitteeApproval", "CommitteeApproval", b1 =>
                        {
                            b1.Property<Guid>("BookEntityId")
                                .HasColumnType("uuid");

                            b1.Property<string>("Feedback")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<bool>("IsApproved")
                                .HasColumnType("boolean");

                            b1.HasKey("BookEntityId");

                            b1.ToTable("Books");

                            b1.WithOwner()
                                .HasForeignKey("BookEntityId");
                        });

                    b.OwnsMany("PublishingHouse.Persistence.Books.Entities.ChapterEntity", "Chapters", b1 =>
                        {
                            b1.Property<int>("Number")
                                .HasColumnType("integer");

                            b1.Property<Guid>("BookId")
                                .HasColumnType("uuid");

                            b1.Property<string>("Content")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.Property<string>("Title")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.HasKey("Number", "BookId");

                            b1.HasIndex("BookId");

                            b1.ToTable("BookChapters", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("BookId");
                        });

                    b.OwnsMany("PublishingHouse.Persistence.Books.Entities.Format", "Formats", b1 =>
                        {
                            b1.Property<string>("FormatType")
                                .HasColumnType("text");

                            b1.Property<Guid>("BookId")
                                .HasColumnType("uuid");

                            b1.Property<int>("SoldCopies")
                                .HasColumnType("integer");

                            b1.Property<int>("TotalCopies")
                                .HasColumnType("integer");

                            b1.HasKey("FormatType", "BookId");

                            b1.HasIndex("BookId");

                            b1.ToTable("BookFormats", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("BookId");
                        });

                    b.OwnsMany("PublishingHouse.Persistence.Books.ValueObjects.Translation", "Translations", b1 =>
                        {
                            b1.Property<Guid>("BookId")
                                .HasColumnType("uuid");

                            b1.Property<Guid>("LanguageId")
                                .HasColumnType("uuid");

                            b1.Property<Guid>("TranslatorId")
                                .HasColumnType("uuid");

                            b1.HasKey("BookId", "LanguageId");

                            b1.HasIndex("LanguageId");

                            b1.HasIndex("TranslatorId");

                            b1.ToTable("BookTranslations", (string)null);

                            b1.WithOwner()
                                .HasForeignKey("BookId");

                            b1.HasOne("PublishingHouse.Persistence.Languages.LanguageEntity", "Language")
                                .WithMany()
                                .HasForeignKey("LanguageId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.HasOne("PublishingHouse.Persistence.Translators.TranslatorEntity", "Translator")
                                .WithMany()
                                .HasForeignKey("TranslatorId")
                                .OnDelete(DeleteBehavior.Cascade)
                                .IsRequired();

                            b1.Navigation("Language");

                            b1.Navigation("Translator");
                        });

                    b.Navigation("Author");

                    b.Navigation("Chapters");

                    b.Navigation("CommitteeApproval");

                    b.Navigation("Formats");

                    b.Navigation("Publisher");

                    b.Navigation("Translations");
                });

            modelBuilder.Entity("PublishingHouse.Persistence.Reviewers.ReviewerEntity", b =>
                {
                    b.HasOne("PublishingHouse.Persistence.Books.BookEntity", null)
                        .WithMany("Reviewers")
                        .HasForeignKey("BookEntityId");
                });

            modelBuilder.Entity("PublishingHouse.Persistence.Books.BookEntity", b =>
                {
                    b.Navigation("Reviewers");
                });
#pragma warning restore 612, 618
        }
    }
}
