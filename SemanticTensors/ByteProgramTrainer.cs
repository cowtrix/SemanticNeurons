using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SemanticTensors
{
	public class ByteProgramTrainer
	{
		const int MaximumGenerationLimit = 100;
		public static float ErrorBound(IDictionary<float, float> desiredValues) =>
			desiredValues.Sum(d => Math.Abs(d.Key - d.Value));
		public static float ErrorSum(ByteProgram prog, IDictionary<float, float> desiredValues) =>
			desiredValues.Sum(d => Math.Abs(prog.Calculate(d.Key) - d.Value));

		private Stack<ByteProgram> m_evolutionHistory = new Stack<ByteProgram>();

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
			while (generationCount < MaximumGenerationLimit && error > targetError)
			{
				generationCount++;
				var newProgram = CreateMutatedGeneration(generationCount, program, desiredValues, ref bestError, 1000);
				if (newProgram == null)
				{
					program = m_evolutionHistory.Pop();
					continue;
				}
				error = ErrorSum(newProgram, desiredValues);
				ByteProgramMutatorV1.GenerationWeights.Normalize();
				m_evolutionHistory.Push(program);
				program = newProgram.Clone();
			}
			return program;
		}

		private ByteProgram CreateMutatedGeneration(int genNumber, ByteProgram baseProgram, IDictionary<float, float> desiredValues, ref float bestError, int popCount)
		{
			var t = new Task[popCount];
			var rnd = new Random();

			var lockObj = new Object();
			IByteProgramMutator mutator = new ByteProgramMutatorV1(10);

			ByteProgram bestProgram = null;
			var minErrorLastGen = bestError;
			var minErrorThisGen = bestError;
			for (var i = 0; i < popCount; ++i)
			{
				t[i] = new Task(() =>
				{
					var program = baseProgram.Clone();  // Create new instance of program to mutate
					mutator.Mutate(program);
					var error = ErrorSum(program, desiredValues);
					if(float.IsInfinity(error) || float.IsNaN(error))
					{
						error = int.MaxValue;
					}

					var errorReductionFactor = (int)((error / minErrorLastGen) * 10f);     // How big is this error compared to the previous generation?
					if(errorReductionFactor > 10)
					{
						errorReductionFactor = 10;
					}
					if(errorReductionFactor < 0)
					{
						errorReductionFactor = 0;
					}
					lock (lockObj)
					{
						var weightAdustments = program.GetInstructionSet().Select(s => (s, errorReductionFactor)).ToArray();
						mutator.AdjustWeights(weightAdustments);
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
