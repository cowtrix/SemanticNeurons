using System;
using System.Collections.Generic;
using System.Linq;

namespace SemanticTensors
{
	public class ByteProgramMutatorV1 : IByteProgramMutator
	{
		public static WeightedSet<InstructionSet> GenerationWeights;
		private readonly static InstructionSet[] InstructionSetLookup;

		static ByteProgramMutatorV1()
		{
			InstructionSetLookup = (InstructionSet[])Enum.GetValues(typeof(InstructionSet));
			GenerationWeights = new WeightedSet<InstructionSet>(InstructionSetLookup);
		}

		public void Mutate(ByteProgram program, float normalizedError)
		{
			var rnd = new Random();
			var MutationCount = normalizedError * program.Length;
			// Mutate some n values
			for (var i = 0; i < MutationCount; ++i)
			{
				var randomBit = (byte)GenerationWeights.GetRandom(rnd);
				//var randomBit = (byte)rnd.Next(0, InstructionSetLookup.Length - 1);
				var randomIndex = rnd.Next(0, program.Length - 1);
				if (randomBit >= (byte)InstructionSet.REG_1 && randomBit <= (byte)InstructionSet.REG_4)
				{
					if (randomIndex >= program.Length - sizeof(int))
					{
						continue;
					}
					// Write a random int into the next 2 bytes
					var rndVal = rnd.Next(int.MinValue, int.MaxValue);
					var rFl = BitConverter.GetBytes(rndVal);
					program[randomIndex + 1] = rFl.ElementAt(0);
					program[randomIndex + 2] = rFl.ElementAt(1);
					program[randomIndex + 3] = rFl.ElementAt(2);
					program[randomIndex + 4] = rFl.ElementAt(3);
				}
				program[randomIndex] = randomBit;
			}
		}

		public void AdjustWeights(IEnumerable<ValueTuple<InstructionSet, int>> wDelta)
		{
			foreach (var adjustment in wDelta)
			{
				GenerationWeights.AddWeight(adjustment.Item1, Math.Max(0, adjustment.Item2));
			}
		}
	}

}
