using Framework.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.EntityFramework
{
    public class ShardingTableInterceptor : IDbCommandInterceptor
    {
        private static List<string> ShardingBySiteTables = new List<string>();

        private Func<int?> _siteIdFunc = () => null;

        public Func<int?> SiteIdFunc
        {
            get
            {
                return _siteIdFunc;
            }
            set
            {
                _siteIdFunc = value;
            }
        }

        static ShardingTableInterceptor()
        {
            var entities = typeof(ShardingTableInterceptor).Assembly.GetExportedTypes().Where(t => t.IsAssignableFrom(typeof(Entity)));

            foreach (var entity in entities)
            {
                if (entity is IShardingBySiteId)
                {
                    var tableAttribute = entity.GetAttribute<TableAttribute>();
                    if (tableAttribute != null && !string.IsNullOrWhiteSpace(tableAttribute.Name))
                    {
                        ShardingBySiteTables.Add(tableAttribute.Name);
                    }
                }
            }
        }

        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            ReplaceTableName(command);
        }

        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            ReplaceTableName(command);
        }

        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            ReplaceTableName(command);
        }

        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            ReplaceTableName(command);
        }

        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            ReplaceTableName(command);
        }

        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            ReplaceTableName(command);
        }

        public void ReplaceTableName(DbCommand command)
        {
            int? siteId = SiteIdFunc();
            if (siteId == null)
            {
                return;
            }

            foreach (var shardingTable in ShardingBySiteTables)
            {
                string commandText = command.CommandText.ToLower();

                bool hasShardingTable = commandText.Contains(shardingTable);
                bool isReplaced = commandText.Contains(shardingTable + siteId);

                if (hasShardingTable && !isReplaced)
                {
                    command.CommandText = command.CommandText.Replace(shardingTable, shardingTable + siteId.ToString());
                }
            }
        }
    }
}
