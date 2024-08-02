using GPA.Common.DTOs;
using System.Text;

namespace GPA.Utils.Database
{
    public class SearchHelper
    {
        public static string ConvertSearchToString(RequestFilterDto filter)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(filter.Search ?? string.Empty));
        }
    }
}
