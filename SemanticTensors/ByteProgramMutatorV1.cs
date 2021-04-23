using System;
using System.Linq;

namespace MachineLearning2
{
	public class ByteProgramMutatorV1 : IByteProgramMutator
	{
		private readonly static InstructionSet[] InstructionSetLookup = (InstructionSet[])Enum.GetValues(typeof(InstructionSet));

		public int MutationCount;

		public ByteProgramMutatorV1(int mutationCount)
		{
			MutationCount = mutationCount;
		}

		public void Mutate(ByteProgram program)
		{
			var rnd = new Random();
			// Mutate some n values
			for (var i = 0; i < MutationCount; ++i)
			{
				var randomBit = (byte)rnd.Next(0, InstructionSetLookup.Length - 1);
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
	}

}
