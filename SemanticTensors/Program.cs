using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace MachineLearning2
{
	public interface IByteProgramMutator
	{
		public void Mutate(ByteProgram pr);
	}

	public class Program
	{
		const int ProgramLength = 64;
		const int MutationCount = 10;

		static void Main(string[] args)
		{
			var path = "output.tsv";
			File.Delete(path);
			var str = File.AppendText(path);
			var trainer = new ByteProgramTrainer();
			for (var x = -100; x < 100; ++x)
			{
				for (var y = -100; y < 100; ++y)
				{
					var values = new Dictionary<float, float>
					{
						{ x, y },
					};
					var bestProgram = trainer.TrainForValues(new ByteProgram(ProgramLength), values, out var generations);
					var errorSum = ByteProgramTrainer.ErrorSum(bestProgram, values);
					var tsv = $"{x}\t{y}\t{generations}\t{errorSum}\t{bestProgram.PrintInstructionSet()}";
					str.WriteLine(tsv);
					Console.WriteLine(tsv);
				}
			}
			/*
			Console.WriteLine(bestProgram.PrintInstructionSet());
			foreach (var kvp in values)
			{
				Console.WriteLine($"IN: {kvp.Key}\tEXP: {kvp.Value}\tVAL: {bestProgram.Calculate(kvp.Key)}");
			}*/
		}
	}

}
