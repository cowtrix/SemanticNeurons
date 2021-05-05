using System;
using System.Collections.Generic;
using System.Linq;

namespace SemanticNeurons
{
	public static class TrainerExtensions
	{
		public static float ErrorBound<T, TIn, TOut>(this Trainer<T, TIn, TOut> trainer, IDictionary<TIn, TOut> values) where T : IFunctionObject<TIn, TOut> =>
			values.Sum(d => Math.Abs(trainer.Error(default, d.Key, d.Value)))
			.SafeFloat();

		public static float ErrorSum<T, TIn, TOut>(this Trainer<T, TIn, TOut> trainer, T obj, IDictionary<TIn, TOut> values) where T : IFunctionObject<TIn, TOut> =>
			values.Sum(d => Math.Abs(trainer.Error(obj, d.Key, d.Value)))
			.SafeFloat();

		public static float ErrorAvg<T, TIn, TOut>(this Trainer<T, TIn, TOut> trainer, T obj, IDictionary<TIn, TOut> values) where T : IFunctionObject<TIn, TOut> =>
			values.Average(d => Math.Abs(trainer.Error(obj, d.Key, d.Value)))
			.SafeFloat();
	}

}
