using QLNS.Models;

namespace QLNS.Services
{
    public class LogService
    {
        private readonly FaceIdHrmsContext _db;

        public LogService(FaceIdHrmsContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Ghi log hệ thống
        /// </summary>
        public async Task LogAsync(int? userId, string action, string description)
        {
            var log = new SystemLog
            {
                UserId = userId,
                Action = action,
                Description = description,
                CreatedAt = DateTime.Now
            };

            _db.SystemLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}
