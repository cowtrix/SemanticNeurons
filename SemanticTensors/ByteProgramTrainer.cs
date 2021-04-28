using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SemanticTensors
{
	public static class Util
	{
		public static float SafeFloat(this float f)
		{
			if(float.IsNaN(f))
			{
				return float.MaxValue;
			}
			return f;
		}
	}

	public class ByteProgramTrainer
	{
		public int MaximumGenerationLimit { get; set; }
		public int PerGenerationMutationCount { get; set; }
		public static float ErrorBound(IDictionary<float, float> desiredValues) =>
			desiredValues.Sum(d => Math.Abs(d.Key - d.Value));
		public static float ErrorSum(ByteProgram prog, IDictionary<float, float> desiredValues) =>
			desiredValues.Sum(d => Math.Abs(prog.Calculate(d.Key) - d.Value)).SafeFloat();
		public static float ErrorAvg(ByteProgram prog, IDictionary<float, float> desiredValues) =>
			desiredValues.Average(d => Math.Abs(prog.Calculate(d.Key) - d.Value)).SafeFloat();

		const int TRAINING_HISTORY_SIZE = 10;
		private SortedList<float, ByteProgram> m_trainingMemory = new SortedList<float, ByteProgram>();

		public ByteProgramTrainer(int genLimit = 100, int genMutationCount = 64 * 64)
		{
			MaximumGenerationLimit = genLimit;
			PerGenerationMutationCount = genMutationCount;
		}

		private const int EVOLUTION_HISTORY_CACHE_LIMIT = 64 * 64 * 64;
		private LinkedList<ByteProgram> m_evolutionHistory = new LinkedList<ByteProgram>();

		public ByteProgram TrainForValues(ByteProgram program, IDictionary<float, float> desiredValues, out int generationCount)
		{
			if (program == null)
			{
				throw new ArgumentNullException(nameof(program));
			}
			generationCount = 0;
			var bestError = ErrorBound(desiredValues) * 100;
			var targetError = ErrorBound(desiredValues) / 100;
			var error = ErrorSum(program, desiredValues);

			int previousProgramCount = 0;

			while (generationCount < MaximumGenerationLimit && error > targetError)
			{
				generationCount++;
				var newProgram = CreateMutatedGeneration(generationCount, program, desiredValues, ref bestError, PerGenerationMutationCount);
				if (newProgram == null)
				{
					if(m_evolutionHistory.Any())
					{
						//Console.WriteLine($"Not luck - going back!");
						program = m_evolutionHistory.Last.Value;
						m_evolutionHistory.RemoveLast();
					}
					else if (previousProgramCount < m_trainingMemory.Count)
					{
						program = m_trainingMemory.ElementAt(previousProgramCount).Value.Clone();
						previousProgramCount++;
					}
					else
					{
						bestError *= bestError; // We loosen the search criteria to explore deeper within the tree
					}
					continue;
				}
				error = ErrorSum(newProgram, desiredValues);
				ByteProgramMutatorV1.GenerationWeights.Normalize();
				m_evolutionHistory.AddLast(program);
				while(m_evolutionHistory.Count > EVOLUTION_HISTORY_CACHE_LIMIT)
				{
					m_evolutionHistory.RemoveFirst();
				}
				program = newProgram.Clone();
			}
			m_evolutionHistory.Clear();
			return program;
		}

		public void RememberSolution(ByteProgram bestProgram, float error)
		{
			while(m_trainingMemory.ContainsKey(error))
			{
				error += float.Epsilon;
			}
			m_trainingMemory.Add(error, bestProgram);
			if (m_trainingMemory.Count > TRAINING_HISTORY_SIZE)
			{
				m_trainingMemory = new SortedList<float, ByteProgram>(m_trainingMemory.Take(TRAINING_HISTORY_SIZE)
					.ToDictionary(x => x.Key, x => x.Value));
			}
		}

		private ByteProgram CreateMutatedGeneration(int genNumber, ByteProgram baseProgram, IDictionary<float, float> desiredValues, ref float bestError, int popCount)
		{
			var t = new Task[popCount];
			var rnd = new Random();
			var lockObj = new object();
			IByteProgramMutator mutator = new ByteProgramMutatorV1();
			var errorBound = ErrorBound(desiredValues);
			ByteProgram bestProgram = null;
			var minErrorLastGen = bestError;
			var minErrorThisGen = bestError;
			var mutationFactor = minErrorLastGen / errorBound;

			// Check for the easiest win - no error
			if(ErrorSum(baseProgram, desiredValues) == 0)
			{
				return baseProgram;
			}

			for (var i = 0; i < popCount; ++i)
			{
				t[i] = new Task(() =>
				{
					var program = baseProgram.Clone();  // Create new instance of program to mutate
					mutator.Mutate(program, MathF.Min(1, mutationFactor));
					var error = ErrorSum(program, desiredValues);
					if(float.IsInfinity(error) || float.IsNaN(error))
					{
						error = int.MaxValue;
					}

					var errorReductionFactor = (int)(Math.Max(0, 1 - (error / minErrorLastGen)) * 10);     // How big is this error compared to the previous generation?
					var weightAdustments = program.GetInstructionSet()
						.GroupBy(s => s)
						.Select(s => (s.Key, s.Count() * errorReductionFactor))
						.Where(s => s.Item2 > 0);
					mutator.AdjustWeights(weightAdustments);
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
			//Console.WriteLine($"Generation {genNumber}:\t\tError:{bestError}");
			return bestProgram;
		}

		
	}

}
