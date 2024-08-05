﻿using GPA.Common.DTOs.Inventory;

namespace GPA.Utils
{
    public class DetailedDateUtil
    {
        public static DateOnly? GetDetailedDate(DetailedDate? date)
        {
            if (date is null)
            {
                return null;
            }

            return new DateOnly(date.Year, date.Month, date.Day);
        }
    }
}
