using Logs.Repository;
using LedditModels;

namespace Logs.Services
{
    public class LogsService
    {
        private readonly ApplicationDbContext _context;
        private readonly MessageService _messageService;

        public LogsService(ApplicationDbContext context, MessageService messageService)
        {
            _context = context;
            _messageService = messageService;
        }

        public Log AddLog(Log log)
        {
            _context.Logs.Add(log);
            _context.SaveChanges();
            return log;
        }

    }
}
