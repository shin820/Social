using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Framework.Core.UnitTest.EntityFramework
{
    public class ShardingTableInterceptorTest
    {
        [Fact]
        public void ShouldInitializeShardingTables()
        {
            ShardingTableInterceptor interceptor = new ShardingTableInterceptor(new[] { Assembly.GetExecutingAssembly() });
            List<string> shardingTables = interceptor.GetShardingBySiteTables();

            Assert.Equal(2, shardingTables.Count);
            Assert.True(shardingTables.Contains("t_Test_Table1"));
            Assert.True(shardingTables.Contains("t_Test_Table2"));
        }

        [Fact]
        public void ShouldAddSiteIdToTableName()
        {
            ShardingTableInterceptor interceptor = new ShardingTableInterceptor(new[] { Assembly.GetExecutingAssembly() });
            string sql = "select * from t_Test_Table1";

            Assert.Equal("select * from t_Test_Table110000", interceptor.GetSql(10000, sql));
        }

        [Fact]
        public void ShouldNotAddSiteIdToSimilarTableName()
        {
            ShardingTableInterceptor interceptor = new ShardingTableInterceptor(new[] { Assembly.GetExecutingAssembly() });
            string sql = "select * from t_Test_Table1Ex";

            Assert.Equal("select * from t_Test_Table1Ex", interceptor.GetSql(10000, sql));
        }

        [Fact]
        public void ShouldAddSiteIdWithMulitpleTableNames()
        {
            ShardingTableInterceptor interceptor = new ShardingTableInterceptor(new[] { Assembly.GetExecutingAssembly() });
            string sql = "select * from [t_Test_Table1] join t_Test_Table1Ex join t_Test_Table2";

            Assert.Equal("select * from [t_Test_Table110000] join t_Test_Table1Ex join t_Test_Table210000", interceptor.GetSql(10000, sql));
        }

        [Fact]
        public void ShouldIgnoreCase()
        {
            ShardingTableInterceptor interceptor = new ShardingTableInterceptor(new[] { Assembly.GetExecutingAssembly() });
            string sql = "select * from t_test_table1 join t_test_table2";

            Assert.Equal("select * from t_Test_Table110000 join t_Test_Table210000", interceptor.GetSql(10000, sql));
        }

        [Table("t_Test_Table1")]
        public class TestTable1 : Entity, IShardingBySiteId
        {
        }

        [Table("t_Test_Table2")]
        public class TestTable2 : Entity, IShardingBySiteId
        {
        }
    }
}
