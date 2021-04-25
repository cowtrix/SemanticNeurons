using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;

namespace SemanticTensors
{
	public interface IByteProgramMutator
	{
		public void Mutate(ByteProgram pr, float normalizedError);
		public void AdjustWeights(params ValueTuple<InstructionSet, int>[] wDelta);
	}

	public class Program
	{
		const int ProgramLength = 64;
		const int MutationCount = 10;

		static void Main(string[] args)
		{
			var ticks = DateTime.Now.Ticks;
			var logStr = File.AppendText($"GenerationHistory_{ticks}.tsv");

			var size = 200;
			using Bitmap b = new Bitmap(size, size) ;
			using Graphics g = Graphics.FromImage(b);
			var trainer = new ByteProgramTrainer(100);
			var sw = Stopwatch.StartNew();
			var results = new Dictionary<(int, int, int), ByteProgram>();
			var rnd = new Random();

			for(var i = -100; i < 100; i++)
			{
				for (var j = -100; j < 100; j++)
				{
					var values = new Dictionary<float, float>();
					var c = rnd.Next(-100, 100);
					for (var x = -20; x < 20; x++)
					{
						values.Add(x, i * x * x + j * x + c);
					}

					var sw2 = Stopwatch.StartNew();
					var bestProgram = trainer.TrainForValues(new ByteProgram(ProgramLength), values, out var generations);

					var bound = ByteProgramTrainer.ErrorBound(values);
					var rawSum = ByteProgramTrainer.ErrorSum(bestProgram, values);
					var avg = ByteProgramTrainer.ErrorAvg(bestProgram, values);
					var errorSum = MathF.Min(1, rawSum / (bound / 100));

					//Console.WriteLine(bestProgram.PrintInstructionSet());
					/*foreach (var kvp in values)
					{
						var str = $"IN: {kvp.Key}\tEXP: {kvp.Value}\tVAL: {bestProgram.Calculate(kvp.Key)}";
						logStr.WriteLine(str);
						Console.WriteLine(str);
					}*/

					var output = $"{i}x^2\t+ {j}x +\t{c}:\t\tGenerations: {generations}\tError Sum: {rawSum} ({errorSum.ToString("P")})\tError Avg: {avg}\tTime: {sw2.Elapsed}";
					logStr.WriteLine(output);
					Console.WriteLine(output);

					b.SetPixel(i + 100, j + 100, Color.FromArgb((int)(errorSum * 255), (int)((generations / 100f) * 255), 0, 255));
					results.Add((i, j, c), bestProgram);
				}
			}

			Console.WriteLine($"Generation took {sw.Elapsed}");
			File.WriteAllText($"results_{ticks}.tsv", string.Join("\n", results.Select(kvp => $"{kvp.Key.Item1}\t{kvp.Key.Item2}\t{kvp.Key.Item3}\t{kvp.Value.PrintInstructionSet()}")));

			//Console.WriteLine($"Finished after {sw.Elapsed} and {generations} generations. Error sum was {rawSum}.");

			//b.SetPixel((x / 100) + 100, (y / 100) + 100, Color.FromArgb((int)(errorSum * 255), (int)((generations / 100f) * 255), 0, 255));

			/*var tsv = $"{x}\t{y}\t{generations}\t{rawSum}";
			logStr.WriteLine(tsv);
			Console.WriteLine(tsv);

			b.Save(@"output.png", ImageFormat.Png);
			Console.WriteLine("Instruction Set Weights:");
			var fs = File.AppendText("instructionsProbs.tsv");
			foreach(var w in ByteProgramMutatorV1.GenerationWeights.Weights)
			{
				var str = $"{w.Key}\t{w.Value}";
				fs.WriteLine(str);
				Console.WriteLine(str);
			}*/

			
			
			
		}
	}

}
