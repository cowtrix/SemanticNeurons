using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SemanticNeurons
{
	public class WeightedSet<T>
	{
		private long m_weightSum;
		public readonly ConcurrentDictionary<T, uint> Weights = new ConcurrentDictionary<T, uint>();

		public WeightedSet(IEnumerable<T> values)
		{
			foreach(var v in values)
			{
				Weights[v] = 1;
			}
			m_weightSum = (long)values.Count();
		}

		public void Normalize(uint resolution = 100)
		{
			var min = Weights.Min(x => x.Value);
			var max = Weights.Max(x => x.Value);

			lock(Weights)
			{
				foreach(var w in Weights.Keys.ToList())
				{
					var norm = (Weights[w] - min) / (float)(max - min);
					norm *= resolution;
					Weights[w] = Math.Max(1, (uint)norm);
				}
				m_weightSum = Weights.Sum(x => x.Value);
			}
		}

		public void AddWeight(T key, int weightDelta = 1)
		{
			if(!Weights.ContainsKey(key))
			{
				throw new KeyNotFoundException(key?.ToString());
			}
			Interlocked.Add(ref m_weightSum, weightDelta);
			Weights.AddOrUpdate(key, _ => (uint)weightDelta, (_, x) => x + weightDelta > 0 ? (uint)(x + weightDelta) : 0);
		}

		public T GetRandom(Random rnd)
		{
			if(m_weightSum <= 0)
			{
				throw new Exception("This should never happen");
			}
			var val = rnd.NextDouble() * m_weightSum;
			uint count = 0;
			T key = default;
			foreach(var kvp in Weights)
			{
				key = kvp.Key;
				var weight = kvp.Value;
				count += weight;
				if (count > val)
				{
					return key;
				}
			}
			return key;
		}
	}

}
