namespace SemanticNeurons
{
	public static class Util
	{
		public static float SafeFloat(this float f)
		{
			if(float.IsNaN(f))
			{
				return float.MaxValue;
			}
			return f;
		}
	}

}
