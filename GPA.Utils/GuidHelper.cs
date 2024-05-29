namespace GPA.Utils
{
    public class GuidHelper
    {
        public static Guid NewSequentialGuid()
        {
            var guidArray = Guid.NewGuid().ToByteArray();

            DateTime baseDate = new DateTime(1900, 1, 1);
            DateTime now = DateTime.Now;

            // Get the days and milliseconds which will be used to build the byte string
            var days = (now - baseDate).Days;
            var milliseconds = (long)(now.TimeOfDay.TotalMilliseconds / 3.333333);

            // Convert to a byte array
            var daysArray = BitConverter.GetBytes(days);
            var msecsArray = BitConverter.GetBytes(milliseconds);

            // Reverse the bytes to match SQL Server ordering
            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            // Copy the bytes into the guid
            Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
            Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

            return new Guid(guidArray);
        }
    }
}
