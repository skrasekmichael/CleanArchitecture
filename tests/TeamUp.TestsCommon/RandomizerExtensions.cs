﻿using Bogus;

namespace TeamUp.TestsCommon;

public static class RandomizerExtensions
{
	public static string Text(this Randomizer rand, int min, int max)
	{
		var len = rand.Int(min, max);
		return rand.String(len, (char)32, (char)126);
	}
}
