namespace GPA.Common.DTOs
{
    public class ResponseDto<TEntity>
    {
        public int Count { get; set; }
        public IEnumerable<TEntity>? Data { get; set; }
    }
}
