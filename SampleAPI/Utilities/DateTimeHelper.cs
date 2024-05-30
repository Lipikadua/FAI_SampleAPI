namespace SampleAPI.Utilities
{
    public static class DateTimeHelper
    {
        public static DateTime CalculateStartDate(int numberOfDays)
        {
            var currentDate = DateTime.Today;
            var startDate = currentDate;

            // Subtract weekends and holidays from the number of days
            for (int i = 0; i < numberOfDays; i++)
            {
                // Exclude weekends (Saturday and Sunday)
                while (startDate.DayOfWeek == DayOfWeek.Saturday || startDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    startDate = startDate.AddDays(-1);
                }

                // Check for holidays (replace this with actual holiday logic from an API or database)
                // considered holidays as weekends
                while (IsHoliday(startDate))
                {
                    startDate = startDate.AddDays(-1);
                }

                startDate = startDate.AddDays(-1);
            }

            return startDate;
        }

        public static bool IsHoliday(DateTime date)
        {

            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

    }
}