using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace KOControls.Core
{
	public static class ReflectionHelper
	{
		public static Type GetEnumerableGenericType(Type type)
		{
			if (type == null) throw new ArgumentNullException();
			foreach (Type interfaceType in type.GetInterfaces())
			{
				if (interfaceType.IsGenericType &&
					interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
				{
					return interfaceType.GetGenericArguments()[0];
				}
			}
			return null;
		}

		public static IList CreateListInstanceWithT(Type T)
		{
			Type l = typeof(List<>);
			Type lt = l.MakeGenericType(T);
			return (IList)Activator.CreateInstance(lt);	
		}
	}
}
