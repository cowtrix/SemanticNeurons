using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SemanticTensors
{
	public interface IByteProgramMutator
	{
		public void Mutate(ByteProgram pr, float normalizedError);
		public void AdjustWeights(IEnumerable<ValueTuple<InstructionSet, int>> wDelta);
	}

	public class Program
	{
		const int ProgramLength = 64;
		const int MutationCount = 10;

		class printInfo
		{
			public ByteProgram Program;
			public int Generations;
			public int I, J, C;
			internal float ErrorTarget;
			internal float ErrorSum;

			public override string ToString()
			{
				return $"{I}\t{J}\t{C}\t{Generations}\t{ErrorTarget}\t{ErrorSum}\t{Program}";
			}
		}

		static void Main(string[] args)
		{
			var output = new List<printInfo>();
			using Bitmap bitmap = new Bitmap(200, 200);
			using Graphics g = Graphics.FromImage(bitmap);
			var results = File.ReadAllLines("results_637551319087617981.tsv");
			var genHistory = File.ReadAllLines("GenerationHistory_637551319087617981.tsv");
			var genRgx = new Regex(@"([+-]*\d+)x\^2\s*\+\s*([+-]*\d+)x\s*\+\s*([+-]*\d+):\s*Generations: (\d+)");
			var resRgx = new Regex(@"[+-]*\d+\s*[+-]*\d+\s*[+-]*\d+\s*(.+)");
			for (int i = 0; i < genHistory.Length; i++)
			{
				string resultsLine = results[i];
				var genLine = genHistory[i];
				var m = genRgx.Match(genLine);

				var prog = resRgx.Match(resultsLine).Groups[1].Value;
				var progSplit = prog.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
				var bytes = new List<byte>();
				for (int j = 0; j < progSplit.Length; j++)
				{
					string b = progSplit[j];
					
					if (Enum.TryParse(typeof(InstructionSet), b, out var setObj) && setObj is InstructionSet inst)
					{
						bytes.Add((byte)inst);
						if (inst >= InstructionSet.REG_1 && inst <= InstructionSet.REG_4
							&& j < progSplit.Length - 1
							&& int.TryParse(progSplit[j + 1], out var numVal))
						{
							// read int
							var num = BitConverter.GetBytes(numVal);
							bytes.AddRange(num);
							j++;
						}
					}
				}

				var inf = new printInfo
				{
					I = int.Parse(m.Groups[1].Value),
					J = int.Parse(m.Groups[2].Value),
					C = int.Parse(m.Groups[3].Value),
					Generations = int.Parse(m.Groups[4].Value),
				};

				var p = new ByteProgram(bytes);
				var p2 = new ByteProgram(p.ToString());


				var values = new Dictionary<float, float>();
				for (var x = -20; x < 20; x++)
				{
					values.Add(x, inf.I * x * x + inf.J * x + inf.C);
				}
				var bound = ByteProgramTrainer.ErrorBound(values);
				var target = bound / 100f;
				var eSum = ByteProgramTrainer.ErrorSum(p, values);

				inf.Program = p;
				inf.ErrorTarget = target;
				inf.ErrorSum = eSum;

				bitmap.SetPixel(inf.I + 100, inf.J + 100, Color.FromArgb(255, (int)(MathF.Min(1, eSum / target) * 255), (int)((inf.Generations / 100f) * 255), 0));

				output.Add(inf);
			}
			bitmap.Save("output_final.png", ImageFormat.Png);
			File.WriteAllLines("finalOutput.tsv", output.Select(s => s.ToString()));
		}

		static void DoCurvs() { 
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

					if (errorSum < 1)
						trainer.RememberSolution(bestProgram, errorSum);

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
