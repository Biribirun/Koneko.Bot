using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace Koneko.Bot.Db
{
    public class DbConnection
    {
        private string ConnectionString => ConfigurationManager.AppSettings["LiteDbConnectionString"];
        public LiteDB.LiteRepository Repository
            => new LiteDB.LiteRepository(ConnectionString);
    }
}
