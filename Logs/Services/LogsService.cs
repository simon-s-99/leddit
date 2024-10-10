using Logs.Repository;
using LedditModels;
using Logs.DTOs;

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

        public Log AddLog(AddLogDTO log)
        {
            Log newLog = new Log
            {
                Body = log.Body,
                CreatedDate = log.CreatedDate,
            };

            _context.Logs.Add(newLog);
            _context.SaveChanges();

            return newLog;
        }

    }
}
