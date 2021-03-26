using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Namespace
{
    //Read: https://docs.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors

    public class TaggedQueryDbCommandInterceptor : DbCommandInterceptor
    {
        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result)
        {
            ProcessQuery(command);

            return result;
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            ProcessQuery(command);

            return new ValueTask<InterceptionResult<DbDataReader>>(result);
        }

        private static void ProcessQuery(DbCommand command)
        {
            if (command.CommandText.Contains("tag:option:recompile", StringComparison.Ordinal))
                command.CommandText += Constants.EFTags.Values.OptionRecompile;
        }
    }
}
