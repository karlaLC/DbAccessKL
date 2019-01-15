using DbAccess.Tools;
using System;
using System.Collections.Generic;
using System.Data;

namespace DbAccess.DbAdapter
{
	public class DbAdapter : IDbAdapter
	{
			public IDbConnection Conn { get; private set; }
			public IDbCommand Cmd { get; private set; }

			public DbAdapter(IDbCommand command, IDbConnection conn)  
			{
				Cmd = command;
				Conn = conn;
			}
			
			// GET - ExecuteReader: Sends the CommandText to the Connection and builds a SqlDataReader.
			public List<T> LoadObject<T>(string storedProcedure, IDbDataParameter[] parameters = null) where T: class 
			{
				List<T> list = new List<T>();

				using (IDbConnection conn = Conn)
				using (IDbCommand cmd = Cmd)
				{
					if (conn.State != ConnectionState.Open)
						conn.Open();

					cmd.Connection = conn;
					cmd.CommandTimeout = 5000;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = storedProcedure;
					if(parameters != null)
					{
						foreach (IDbDataAdapter parameter in parameters)
							cmd.Parameters.Add(parameter);
					}
					IDataReader reader = cmd.ExecuteReader();
					while(reader.Read())
					{
						//TODO: Map to object 
						list.Add(DataMapper<T>.Instance.MapToObject(reader));
					}
				}

				return list;
			}

			// POST-PUT-DELETE - ExecuteNonQuery: Executes a T-SQL statement against the connection and returns the rows affected.
			public int ExecuteQuery(string storedProcedure, 
				IDbDataParameter[] parameters, 
				Action<IDbDataParameter[]> returnParameters = null) //this is a delegate...this one for when you want it to return something, like an id. Action says i'll take inputs and return void (the other one's called fnk, takes input and returns value)
			{
				using (IDbConnection conn = Conn)
				using (IDbCommand cmd = Cmd)
				{
					if (conn.State != ConnectionState.Open)
						conn.Open();

					cmd.Connection = conn;
					cmd.CommandTimeout = 5000;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = storedProcedure;
					foreach (IDbDataParameter parameter in parameters)
						cmd.Parameters.Add(parameter);

					int returnValue = cmd.ExecuteNonQuery();

					if(returnParameters != null)
					{
						returnParameters(parameters); //simplified version would be returnParameters?.Invoke(parameters);
					}

					return returnValue;
				}
			}
			
			//ExecuteScalar: Executes the query, and returns the first column of the first row in the result set returned by the query.
			//Additional columns or rows are ignored.
			public T ExecuteDbScalar<T>(string storedProcedure, IDbDataParameter[] parameters = null)  
			{
				using (IDbConnection conn = Conn)
				using (IDbCommand cmd = Cmd)
				{
					if (conn.State != ConnectionState.Open)
						conn.Open();

					cmd.Connection = conn;
					cmd.CommandTimeout = 5000;
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = storedProcedure;
					foreach (IDbDataParameter parameter in parameters)
						cmd.Parameters.Add(parameter);

					object obj = cmd.ExecuteScalar(); 
					return (T)Convert.ChangeType(obj, typeof(T));
				}
			}		
	}
}