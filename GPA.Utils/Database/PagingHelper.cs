using GPA.Common.DTOs;
using Microsoft.Data.SqlClient;

namespace GPA.Utils.Database
{
    public class PagingHelper
    {
        /// <summary>
        /// Return the parameters for paging as: @Page and @PageSize
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static (SqlParameter Page, SqlParameter PageSize, SqlParameter Search) GetPagingParameter(RequestFilterDto filter)
        {
            var search = filter.Search is { Length: > 0 } ? filter.Search : "";
            return
            (
                new SqlParameter("@Page", filter.PageSize * Math.Abs(filter.Page - 1)),
                new SqlParameter("@PageSize", filter.PageSize),
                new SqlParameter("@Search", search)
            );
        }
    }
}
