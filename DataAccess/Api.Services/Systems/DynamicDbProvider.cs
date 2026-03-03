using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Services.Systems
{
	public class DynamicDbProvider
	{
		private string _connectionString;

		// The background worker reads this
		public string CurrentConnectionString => _connectionString;

		public void UpdateConnectionString(string newConn)
		{
			_connectionString = newConn;
		}
	}
}
