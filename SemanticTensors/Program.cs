using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace SemanticTensors
{
	public interface IByteProgramMutator
	{
		public void Mutate(ByteProgram pr);
		public void AdjustWeights(params ValueTuple<InstructionSet, int>[] wDelta);
	}

	public class Program
	{
		const int ProgramLength = 64;
		const int MutationCount = 10;

		static void Main(string[] args)
		{
			var size = 200;
			using Bitmap b = new Bitmap(size, size) ;
			using Graphics g = Graphics.FromImage(b);
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

					var bound = ByteProgramTrainer.ErrorBound(values);
					var rawSum = ByteProgramTrainer.ErrorSum(bestProgram, values);
					var errorSum = MathF.Min(1, rawSum / (bound / 100));
					if(float.IsNaN(errorSum))
					{
						errorSum = 0;
					}

					b.SetPixel(x + 100, y + 100, Color.FromArgb((int)(errorSum * 255), (int)((generations / 100f) * 255), 0, 255));

					var tsv = $"{x}\t{y}\t{generations}\t{rawSum}";
					//str.WriteLine(tsv);
					Console.WriteLine(tsv);
				}
			}

			b.Save(@"output.png", ImageFormat.Png);

			var fs = File.AppendText("instructionsProbs.tsv");
			foreach(var w in ByteProgramMutatorV1.GenerationWeights.Weights)
			{
				fs.WriteLine($"{w.Key.ToString()}\t{w.Value}");
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
