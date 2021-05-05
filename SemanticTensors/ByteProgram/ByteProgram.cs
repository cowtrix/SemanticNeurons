using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SemanticNeurons
{
	public class ByteProgram : IEnumerable<byte>, IFunctionObject<float, float>
	{
		public int Length => m_array.Length;
		public byte[] GetBytes() => m_array;
		private readonly byte[] m_array;
		private readonly InstructionSet[] m_instructions;

		public ByteProgram(int programLength)
		{
			m_array = new byte[programLength];
			m_instructions = new InstructionSet[m_array.Length];
			RecalculateInstructions();
		}

		public ByteProgram(IEnumerable<byte> bytes)
		{
			m_array = bytes.ToArray();
			m_instructions = new InstructionSet[m_array.Length];
			RecalculateInstructions();
		}

		public ByteProgram(string compressedString) :
			this(Convert.FromBase64String(compressedString))
		{
		}

		private void RecalculateInstructions()
		{
			for (int i = 0; i < m_array.Length; i++)
			{
				var op = m_array[i];
				if (!Enum.IsDefined(typeof(InstructionSet), op) || op == (byte)InstructionSet.NULL)
				{
					continue;
				}
				m_instructions[i] = (InstructionSet)op;
				if (op >= (byte)InstructionSet.REG_1 && op <= (byte)InstructionSet.REG_4)
				{
					i += sizeof(int);
				}
			}
		}

		public IEnumerable<InstructionSet> GetInstructionSet()
		{
			return m_instructions;
		}

		public float Calculate(float input)
		{
			var reg = new int[4];
			var original = input;
			for (int i = 0; i < m_instructions.Length; i++)
			{
				var op = m_instructions[i];
				int regIndex;
				switch (op)
				{
					case InstructionSet.REG_1:
					case InstructionSet.REG_2:
					case InstructionSet.REG_3:
					case InstructionSet.REG_4:      // registry
						if (i >= m_instructions.Length - sizeof(int))
						{
							// We gotta skip...
							break;
						}
						regIndex = (int)(op - InstructionSet.REG_1);
						reg[regIndex] = BitConverter.ToInt32(m_array, i + 1);
						i += sizeof(int);
						break;

					case InstructionSet.ADD_REG_1:
					case InstructionSet.ADD_REG_2:
					case InstructionSet.ADD_REG_3:
					case InstructionSet.ADD_REG_4:      // Addition
						regIndex = (int)(op - InstructionSet.ADD_REG_1);
						input += reg[regIndex];
						break;

					case InstructionSet.SUB_REG_1:
					case InstructionSet.SUB_REG_2:
					case InstructionSet.SUB_REG_3:
					case InstructionSet.SUB_REG_4:      // Addition
						regIndex = (int)(op - InstructionSet.SUB_REG_1);
						input -= reg[regIndex];
						break;

					case InstructionSet.DIV_REG_1:
					case InstructionSet.DIV_REG_2:
					case InstructionSet.DIV_REG_3:
					case InstructionSet.DIV_REG_4:      // Division
						regIndex = (int)(op - InstructionSet.DIV_REG_1);
						input /= reg[regIndex];
						break;

					case InstructionSet.MUL_REG_1:
					case InstructionSet.MUL_REG_2:
					case InstructionSet.MUL_REG_3:
					case InstructionSet.MUL_REG_4:      // Copy the current value to the register
						regIndex = ((int)op) - (int)InstructionSet.MUL_REG_1;
						input *= reg[regIndex];
						break;

					case InstructionSet.COPY_IN_REG_1:
					case InstructionSet.COPY_IN_REG_2:
					case InstructionSet.COPY_IN_REG_3:
					case InstructionSet.COPY_IN_REG_4:      // Copy the input value to the register
						regIndex = (int)(op - InstructionSet.COPY_IN_REG_1);
						reg[regIndex] = (int)original;
						break;

					case InstructionSet.COPY_VAL_REG_1:
					case InstructionSet.COPY_VAL_REG_2:
					case InstructionSet.COPY_VAL_REG_3:
					case InstructionSet.COPY_VAL_REG_4:      // Copy the current value to the register
						regIndex = (int)(op - InstructionSet.COPY_VAL_REG_1);
						reg[regIndex] = (int)input;
						break;
				}
			}
			return input;
		}

		public ByteProgram Clone() => new ByteProgram(m_array);

		public byte this[int index]
		{
			get => m_array[index];
			set
			{
				m_array[index] = value;
				if(value <= (int)InstructionSet.COPY_VAL_REG_4)
				{
					m_instructions[index] = (InstructionSet)value;
				}
			}
		}

		public IEnumerator<byte> GetEnumerator()
		{
			return ((IEnumerable<byte>)m_array).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return m_array.GetEnumerator();
		}

		public override string ToString()
		{
			var c = new char[Math.Max(4, Length * 2)];
			var s = Convert.ToBase64CharArray(m_array, 0, Length, c, 0);
			return new string(c).TrimEnd('\0');
		}
	}
}
