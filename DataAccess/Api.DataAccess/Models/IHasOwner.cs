using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.DataAccess.Models
{
	public interface IHasOwner
	{
		int Owner { get; }
	}
}
