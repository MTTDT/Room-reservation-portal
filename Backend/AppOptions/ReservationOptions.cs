using System.ComponentModel.DataAnnotations;

namespace Backend.AppOptions
{
    public class ReservationOptions
    {
        public const string SectionName = "ReservationRules";
        public int MaxDurationDays {get; set;}
        public int MaxAdvanceReservationDays {get; set; }
    }
}