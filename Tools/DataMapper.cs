using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DbAccess.Tools
{
	public class DataMapper<T> where T : class 
	{
		private static readonly DataMapper<T> _instance = new DataMapper<T>();
		PropertyInfo[] properties;

		private DataMapper()
		{
			properties = typeof(T).GetProperties();
		}
		static DataMapper() { }

		public static DataMapper<T> Instance { get { return _instance; } }

		public T MapToObject(IDataReader reader)
		{
			IEnumerable<string> columns = reader.GetSchemaTable()
				.Rows.Cast<DataRow>().Select(
				c => c["ColumnName"].ToString().ToLower()).ToList();

			T obj = Activator.CreateInstance<T>();

			foreach(PropertyInfo pinfo in properties)
			{
				if(columns.Contains(pinfo.Name.ToLower()))
				{
					if(reader[pinfo.Name] != DBNull.Value)
					{
						if(reader[pinfo.Name].GetType() == typeof(decimal))
						{
							pinfo.SetValue(obj, reader.GetDouble(pinfo.Name));
						}
						else
						{
							pinfo.SetValue(obj, 
								(reader.GetValue(reader.GetOrdinal(pinfo.Name)) ?? null),
								null);
						}
					}
				}
			}

			return obj;
		}			
	}

	public static class DataHelper
	{
		//extension methods
		public static double GetDouble(this IDataReader reader, string columnName) 
		{
			double dbl = 0;
			double.TryParse(reader[columnName].ToString(), out dbl);
			return dbl;
		}

		public static double GetDouble(this IDataReader reader, int columnIndex)
		{
			double dbl = 0;
			double.TryParse(reader[columnIndex].ToString(), out dbl);
			return dbl;
		}

		public static T GetParamValue<T>(this IDbDataParameter[] dbParams,  
			string paramName)
		{
			foreach (IDbDataParameter param in dbParams)
			{
				if (param.ParameterName.ToLower() == paramName.ToLower())
				{
					return (T)Convert.ChangeType(param.Value, typeof(T));
				}
			}
			return default(T);
		}
	}
}
