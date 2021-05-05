using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SemanticNeurons
{
	public abstract class Trainer<T, TIn, TOut> 
		where T: IFunctionObject<TIn, TOut>
	{
		public class TrainerOptions
		{
			public int MaximumGenerationLimit { get; set; } = 100;
			public int PerGenerationMutationCount { get; set; } = 64 * 64;
			public int EvolutionCacheSize { get; set; } = 100;
			public int GenerationCacheSize { get; set; } = 10;  // Should be less that the MaximumGenerationLimit
		}

		public readonly Func<T, TIn, TOut, float> Error;
		public readonly IObjectMutator<T> Mutator;
		public readonly TrainerOptions Options;

		public Trainer(Func<T, TIn, TOut, float> errorCalculator, IObjectMutator<T> mutator, TrainerOptions options)
		{
			Error = errorCalculator;
			Mutator = mutator;
			Options = options;
		}

		private LinkedList<T> m_evolutionCache = new LinkedList<T>();
		private List<(TOut, T)> m_generationCache = new List<(TOut, T)>();

		public T TrainForValues(T program, IDictionary<TIn, TOut> desiredValues, out int generationCount)
		{
			if (program == null)
			{
				throw new ArgumentNullException(nameof(program));
			}
			generationCount = 0;
			var bestError = this.ErrorBound(desiredValues) * 100;
			var targetError = this.ErrorBound(desiredValues) / 100;
			var error = this.ErrorSum(program, desiredValues);

			int previousProgramCount = 0;

			while (generationCount < Options.MaximumGenerationLimit && error > targetError)
			{
				generationCount++;
				var newProgram = CreateMutatedGeneration(generationCount, program, desiredValues, ref bestError, Options.PerGenerationMutationCount);
				if (newProgram == null)
				{
					if (m_evolutionCache.Any())
					{
						//Console.WriteLine($"Not luck - going back!");
						program = m_evolutionCache.Last.Value;
						m_evolutionCache.RemoveLast();
					}
					else if (previousProgramCount < m_generationCache.Count)
					{
						program = m_generationCache.ElementAt(previousProgramCount).Item2.Clone<T>();
						previousProgramCount++;
					}
					else
					{
						bestError *= bestError; // We loosen the search criteria to explore deeper within the tree
					}
					continue;
				}
				error = this.ErrorSum(newProgram, desiredValues);
				//Mutator.GenerationWeights.Normalize();
				m_evolutionCache.AddLast(program);
				while (m_evolutionCache.Count > Options.EvolutionCacheSize)
				{
					m_evolutionCache.RemoveFirst();
				}
				program = newProgram.Clone<T>();
			}
			m_evolutionCache.Clear();
			return program;
		}

		public void RememberSolution(T bestProgram, TOut error)
		{
			m_generationCache.Add((error, bestProgram));
			if (m_generationCache.Count > Options.GenerationCacheSize)
			{
				m_generationCache = m_generationCache.Take(Options.EvolutionCacheSize).ToList();
			}
		}

		private T CreateMutatedGeneration(int genNumber, T baseProgram, IDictionary<TIn, TOut> trainingData, ref float bestError, int popCount)
		{
			var t = new Task[popCount];
			var rnd = new Random();
			var lockObj = new object();
			var mutator = Mutator;
			var errorBound = this.ErrorBound(trainingData);
			T bestProgram = default;
			var minErrorLastGen = bestError;
			var minErrorThisGen = bestError;
			var mutationFactor = minErrorLastGen / errorBound;

			// Check for the easiest win - no error
			if (this.ErrorSum(baseProgram, trainingData) == 0)
			{
				return baseProgram;
			}

			for (var i = 0; i < popCount; ++i)
			{
				t[i] = new Task(() =>
				{
					var program = baseProgram.Clone<T>();  // Create new instance of program to mutate
					mutator.Mutate(program, MathF.Min(1, mutationFactor));
					var error = this.ErrorSum(program, trainingData);
					if (float.IsInfinity(error) || float.IsNaN(error))
					{
						error = int.MaxValue;
					}

					var errorReductionFactor = (int)(Math.Max(0, 1 - (error / minErrorLastGen)) * 10);     // How big is this error compared to the previous generation?
					/*var weightAdustments = program.GetInstructionSet()
						.GroupBy(s => s)
						.Select(s => (s.Key, s.Count() * errorReductionFactor))
						.Where(s => s.Item2 > 0);
					mutator.AdjustWeights(weightAdustments);*/
					lock (lockObj)
					{
						if (error < minErrorThisGen)
						{
							bestProgram = program;
							minErrorThisGen = error;
						}
					}
				});
				t[i].Start();
			}
			Task.WaitAll(t);
			bestError = minErrorThisGen;
			return bestProgram;
		}
	}

}
