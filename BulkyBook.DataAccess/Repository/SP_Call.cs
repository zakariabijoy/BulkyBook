using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BulkyBook.DataAccess.Repository
{
    public class SP_Call:ISP_Call
    {
        private readonly ApplicationDbContext _db;
        private static string ConnectionString;

        public SP_Call( ApplicationDbContext db)
        {
            _db = db;
            ConnectionString = _db.Database.GetDbConnection().ConnectionString;
        }
        public void Dispose()
        {
            _db.Dispose();
        }

        public T Single<T>(string procedureName, DynamicParameters param = null)
        {
            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();
            return (T) Convert.ChangeType(
                sqlConnection.ExecuteScalar<T>(procedureName, param, commandType: CommandType.StoredProcedure),
                typeof(T));
        }

        public void Execute(string procedureName, DynamicParameters param = null)
        {
            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();
            sqlConnection.Execute(procedureName, param, commandType: CommandType.StoredProcedure);
        }

        public T OneRecord<T>(string procedureName, DynamicParameters param = null)
        {
            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();
            var value= sqlConnection.Query<T>(procedureName, param, commandType: CommandType.StoredProcedure);
            return (T) Convert.ChangeType(value.FirstOrDefault(), typeof(T));
        }

        public IEnumerable<T> List<T>(string procedureName, DynamicParameters param = null)
        {
            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();
            return sqlConnection.Query<T>(procedureName, param, commandType: CommandType.StoredProcedure);
        }

        public Tuple<IEnumerable<T1>, IEnumerable<T2>> List<T1, T2>(string procedureName, DynamicParameters param = null)
        {
            using SqlConnection sqlConnection = new SqlConnection(ConnectionString);
            sqlConnection.Open();
            var result = sqlConnection.QueryMultiple(procedureName, param, commandType: CommandType.StoredProcedure);
            var item1 = result.Read<T1>().ToList();
            var item2 = result.Read<T2>().ToList();

            if (item1 !=null && item2 != null)
            {
                return   new Tuple<IEnumerable<T1>, IEnumerable<T2>>(item1,item2);
            }

            return  new Tuple<IEnumerable<T1>, IEnumerable<T2>>(new List<T1>(),new List<T2>() );
        }
    }
}
