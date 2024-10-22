using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.SqlServer;

namespace EF_Core_Dz_5
{
    internal class Program
    {
        public static void Main()
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                var events = new List<Event>
                {
                    new Event {Name = "Party"},
                    new Event {Name = "Conference"}
                };

                var guests = new List<Guest>
                {
                    new Guest {Name = "John Doe" },
                    new Guest {Name = "Mark Spin" },
                    new Guest {Name = "Good Day" }
                };

                var eventGuestRelations = new List<EventGuestRelation>
                {
                    new EventGuestRelation { Event = events[0], Guest = guests[0], Role = GuestRole.Attendee },
                    new EventGuestRelation { Event = events[0], Guest = guests[1], Role = GuestRole.Organizer },
                    new EventGuestRelation { Event = events[1], Guest = guests[1], Role = GuestRole.Speaker },
                    new EventGuestRelation { Event = events[0], Guest = guests[2], Role = GuestRole.Vip }
                };

                db.Events.AddRange(events);
                db.Guests.AddRange(guests);
                db.EventGuestRelations.AddRange(eventGuestRelations);
                db.SaveChanges();
            }

            using (ApplicationContext db = new ApplicationContext())
            {
                //var currentGuest = db.Guests.FirstOrDefault(e => e.Id == 3);                           // добавить гостя на событие

                //if (currentGuest != null)
                //{
                //    var currentEvent = db.Events.FirstOrDefault(e => e.Id == 2);
                //    if (currentEvent != null)
                //    {
                //        currentEvent.Guests.Add(currentGuest);
                //        db.SaveChanges();
                //    }

                //}

                //var currentEvent = db.Events.Include(e => e.Guests).FirstOrDefault(e => e.Id == 2);              // все гости на втором событии

                //var eventGuestRelationToUpdate = db.EventGuestRelations.FirstOrDefault(e => e.EventId == 1 && e.GuestId == 1);  // меняем роль
                //if (eventGuestRelationToUpdate != null)
                //{
                //    eventGuestRelationToUpdate.Role = GuestRole.Vip;
                //    db.SaveChanges();
                //}

                //var currentGuest = db.Guests.Include(e => e.Events).FirstOrDefault(e => e.Id == 1); // получаем все события для конкретного гостя

                //var eventGuestRelationToRemove = db.EventGuestRelations.FirstOrDefault(e => e.EventId == 1 && e.GuestId == 1);  // меняем роль
                //if (eventGuestRelationToRemove != null)
                //{
                //    db.EventGuestRelations.Remove(eventGuestRelationToRemove);
                //    db.SaveChanges();                                                             // удаляем гостя
                //}

                var eventsWhereGuestWasSpeaker = db.EventGuestRelations
                    .Where(e => e.GuestId == 2 && e.Role == GuestRole.Speaker)              //   все события где гость спикер
                    .Select(e => e.Event).ToList();

            }

        }
    }

    public enum GuestRole
    {
        Attendee,
        Vip,
        Organizer,
        Speaker

    }

    public class Guest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Event> Events { get; set; }
        public ICollection<EventGuestRelation> EventGuestRelations { get; set; }
    }

    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Guest> Guests { get; set; }
        public ICollection<EventGuestRelation> EventGuestRelations { get; set; }

        public Event()
        {
            Guests = new List<Guest>();
        }
    }

    public class EventGuestRelation
    {
        public int EventId { get; set; }
        public Event Event { get; set; }
        public int GuestId { get; set; }
        public Guest Guest { get; set; }
        public GuestRole Role { get; set; }

    }

    public class ApplicationContext : DbContext
    {
        public DbSet<Event> Events { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<EventGuestRelation> EventGuestRelations { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=localhost;Database=Dz5;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>().HasMany(e => e.Guests).WithMany(e => e.Events).UsingEntity<EventGuestRelation>
                (
                g => g.HasOne(e => e.Guest).WithMany(e => e.EventGuestRelations).HasForeignKey(e => e.GuestId),
                e => e.HasOne(e => e.Event).WithMany(e => e.EventGuestRelations).HasForeignKey(e => e.EventId),

                egr =>
                {
                    egr.Property(e => e.Role).HasDefaultValue(GuestRole.Attendee);
                    egr.HasKey(key => new { key.EventId, key.GuestId });
                    egr.ToTable("EventGuestRelations");
                }
                );

        }

    }



}
