using System;
using System.Linq;
using System.Text;

namespace SemanticTensors
{
	public static class ByteProgramExtensions
	{
		public static string PrintInstructionSet(this ByteProgram program)
		{
			var sb = new StringBuilder();
			for (int i = 0; i < program.Length; i++)
			{
				byte b = program[i];
				if (b == (byte)InstructionSet.NULL)
				{
					continue;
				}
				sb.Append((InstructionSet)b);
				sb.Append(" ");
				if (i < program.Length - sizeof(int)
					&& (b >= (byte)InstructionSet.REG_1 && b <= (byte)InstructionSet.REG_4))
				{
					sb.Append(BitConverter.ToInt32(program.GetBytes(), i + 1));
					sb.Append(" ");
					i += 4;
				}
			}
			return sb.ToString();
		}
	}
}
