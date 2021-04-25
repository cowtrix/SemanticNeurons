using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SemanticTensors
{
	public class ByteProgram : IEnumerable<byte>
	{
		public int Length => m_array.Length;
		public byte[] GetBytes() => m_array;
		private readonly byte[] m_array;

		public ByteProgram(int programLength)
		{
			m_array = new byte[programLength];
		}

		public ByteProgram(IEnumerable<byte> bytes)
		{
			m_array = bytes.ToArray();
		}

		public IEnumerable<InstructionSet> GetInstructionSet()
		{
			var instructions = m_array;
			for (int i = 0; i < instructions.Length; i++)
			{
				var op = instructions[i];
				if(!Enum.IsDefined(typeof(InstructionSet), op) || op == (byte)InstructionSet.NULL)
				{
					continue;
				}
				yield return (InstructionSet)op;
				if (op >= (byte)InstructionSet.REG_1 && op <= (byte)InstructionSet.REG_4)
				{
					i += sizeof(int);
				}
			}
		}

		public float Calculate(float input)
		{
			var reg = new int[4];
			var original = input;
			var instructions = m_array.Select(x => (InstructionSet)x).ToArray();
			for (int i = 0; i < instructions.Length; i++)
			{
				var op = instructions[i];
				int regIndex;
				switch (op)
				{
					case InstructionSet.REG_1:
					case InstructionSet.REG_2:
					case InstructionSet.REG_3:
					case InstructionSet.REG_4:      // registry
						if (i >= instructions.Length - sizeof(int))
						{
							// We gotta skip...
							break;
						}
						regIndex = ((int)op) - 1;
						reg[regIndex] = BitConverter.ToInt32(m_array, i + 1);
						i += sizeof(int);
						break;

					case InstructionSet.ADD_REG_1:
					case InstructionSet.ADD_REG_2:
					case InstructionSet.ADD_REG_3:
					case InstructionSet.ADD_REG_4:      // Addition
						regIndex = ((int)op) - (int)InstructionSet.ADD_REG_1;
						input += reg[regIndex];
						break;

					case InstructionSet.SUB_REG_1:
					case InstructionSet.SUB_REG_2:
					case InstructionSet.SUB_REG_3:
					case InstructionSet.SUB_REG_4:      // Addition
						regIndex = ((int)op) - (int)InstructionSet.SUB_REG_1;
						input -= reg[regIndex];
						break;

					case InstructionSet.DIV_REG_1:
					case InstructionSet.DIV_REG_2:
					case InstructionSet.DIV_REG_3:
					case InstructionSet.DIV_REG_4:      // Division
						regIndex = ((int)op) - (int)InstructionSet.DIV_REG_1;
						input /= reg[regIndex];
						break;

					case InstructionSet.COPY_IN_REG_1:
					case InstructionSet.COPY_IN_REG_2:
					case InstructionSet.COPY_IN_REG_3:
					case InstructionSet.COPY_IN_REG_4:      // Copy the input value to the register
						regIndex = ((int)op) - (int)InstructionSet.COPY_IN_REG_1;
						reg[regIndex] = (int)original;
						break;

					case InstructionSet.COPY_VAL_REG_1:
					case InstructionSet.COPY_VAL_REG_2:
					case InstructionSet.COPY_VAL_REG_3:
					case InstructionSet.COPY_VAL_REG_4:      // Copy the current value to the register
						regIndex = ((int)op) - (int)InstructionSet.COPY_VAL_REG_1;
						reg[regIndex] = (int)input;
						break;

					case InstructionSet.MUL_REG_1:
					case InstructionSet.MUL_REG_2:
					case InstructionSet.MUL_REG_3:
					case InstructionSet.MUL_REG_4:      // Copy the current value to the register
						regIndex = ((int)op) - (int)InstructionSet.MUL_REG_1;
						input *= reg[regIndex];
						break;
				}
			}
			return input;
		}

		public ByteProgram Clone() => new ByteProgram(m_array);

		public byte this[int index]
		{
			get => m_array[index];
			set => m_array[index] = value;
		}

		public IEnumerator<byte> GetEnumerator()
		{
			return ((IEnumerable<byte>)m_array).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_array.GetEnumerator();
		}
	}
}
