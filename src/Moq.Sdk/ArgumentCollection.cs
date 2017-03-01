using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Moq.Sdk
{
	class ArgumentCollection : IArgumentCollection
	{
        List<ParameterInfo> infos;
        List<object> values;

		public ArgumentCollection(object[] values, ParameterInfo[] infos)
		{
			this.infos = infos.ToList();
			this.values = values.ToList();
		}

		public ArgumentCollection(IEnumerable<object> values, IEnumerable<ParameterInfo> infos)
		{
			this.infos = infos.ToList();
			this.values = values.ToList();
		}

		public object this[int index]
		{
			get => values[index];
			set => values[index] = value;
		}

		public object this[string name]
		{
			get => values[IndexOf(name)];
            set => values[IndexOf(name)] = value;
		}

		public int Count => infos.Count;

		public bool Contains(string name) => IndexOf(name) != -1;

		public IEnumerator<object> GetEnumerator() => values.GetEnumerator();

		public ParameterInfo GetInfo(string name) => infos[IndexOf(name)];

		public ParameterInfo GetInfo(int index) => infos[index];

		public string NameOf(int index) => infos[index].Name;

		public int IndexOf(string name)
		{
			for (var i = 0; i < infos.Count; ++i)
			{
				if (infos[i].Name == name)
					return i;
			}

			return -1;
		}

		IEnumerator IEnumerable.GetEnumerator() => values.GetEnumerator();
	}
}
