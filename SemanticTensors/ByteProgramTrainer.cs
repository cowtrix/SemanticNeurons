using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MachineLearning2
{
	public class ByteProgramTrainer
	{
		const int MaximumGenerationLimit = 100;
		public static float ErrorBound(IDictionary<float, float> desiredValues) =>
			desiredValues.Sum(d => Math.Abs(d.Key - d.Value));
		public static float ErrorSum(ByteProgram prog, IDictionary<float, float> desiredValues) =>
			desiredValues.Sum(d => Math.Abs(prog.Calculate(d.Key) - d.Value));

		public ByteProgram TrainForValues(ByteProgram program, IDictionary<float, float> desiredValues, out int generationCount)
		{
			if (program == null)
			{
				throw new ArgumentNullException(nameof(program));
			}
			generationCount = 0;
			var bestError = ErrorBound(desiredValues) * 100;
			var targetError = ErrorBound(desiredValues) / 100;
			var error = ErrorSum(program, desiredValues); ;
			while (generationCount < MaximumGenerationLimit && error > targetError)
			{
				generationCount++;
				var newProgram = CreateMutatedGeneration(generationCount, program, desiredValues, ref bestError, 1000);
				if (newProgram == null)
				{
					continue;
				}

				error = ErrorSum(newProgram, desiredValues);
				//Console.WriteLine(ProgramReadable(program));
				//Console.WriteLine($"Gen: {generationCount}\tError: {error}");

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
			var minErrorThisGen = bestError;
			for (var i = 0; i < popCount; ++i)
			{
				t[i] = new Task(() =>
				{
					var program = baseProgram.Clone();  // Create new instance of program to mutate
					mutator.Mutate(program);
					var error = ErrorSum(program, desiredValues);

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
