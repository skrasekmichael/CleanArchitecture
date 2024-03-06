﻿using TeamUp.Common.Abstractions;

namespace TeamUp.EndToEndTests.Mocks;

internal sealed class SkewDateTimeProvider : IDateTimeProvider
{
	public TimeSpan Skew { get; set; } = TimeSpan.Zero;

	public DateTime? ExactTime { get; set; } = null;

	public DateTime UtcNow => ExactTime ?? DateTime.UtcNow + Skew;

	public DateTimeOffset DateTimeOffsetUtcNow => ExactTime ?? DateTimeOffset.UtcNow + Skew;
}
