using PepperDash.Essentials.Core;

namespace CountdownTimerEpi
{
	/// <summary>
	/// Plugin device bridge join map
	/// </summary>
	public class TimerJoinMap : JoinMapBaseAdvanced
	{
		#region Digital

		[JoinName("TimerStart")]
		public JoinDataComplete TimerStart = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Timer countdown start",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("TimerCounting")]
		public JoinDataComplete TimerCountingg = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Timer active feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("TimerCancel")]
		public JoinDataComplete TimerCancel = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 2,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Timer countdown cancel",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("TimerExpired")]
		public JoinDataComplete TimerExpired = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 2,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Timer expired feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("TimerFinish")]
		public JoinDataComplete TimerFinish = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 3,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Timer countdown finish",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("TimerWarning")]
		public JoinDataComplete TimerWarning = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 3,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Timer warning feedback",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

		[JoinName("TimerExtend")]
		public JoinDataComplete TimerExtend = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 4,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Timer extend active countdown",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Digital
			});

        [JoinName("CountUpTimerStart")]
        public JoinDataComplete CountUpTimerStart = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 5,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Timer countup start",
                JoinCapabilities = eJoinCapabilities.FromSIMPL,
                JoinType = eJoinType.Digital
            });

		#endregion


		#region Analog

		[JoinName("TimerCountdownSet")]
		public JoinDataComplete TimerCountdownSet = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Timer countdown set",
				JoinCapabilities = eJoinCapabilities.FromSIMPL,
				JoinType = eJoinType.Analog
			});

		[JoinName("TimerPercentage")]
		public JoinDataComplete TimerPercentage = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Timer percentage complete",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Analog
			});


		#endregion


		#region Serial

		[JoinName("TimerValue")]
		public JoinDataComplete TimerValue = new JoinDataComplete(
			new JoinData
			{
				JoinNumber = 1,
				JoinSpan = 1
			},
			new JoinMetadata
			{
				Description = "Timer value",
				JoinCapabilities = eJoinCapabilities.ToSIMPL,
				JoinType = eJoinType.Serial
			});

        [JoinName("CountUpTimerValue")]
        public JoinDataComplete CountUpTimerValue = new JoinDataComplete(
            new JoinData
            {
                JoinNumber = 5,
                JoinSpan = 1
            },
            new JoinMetadata
            {
                Description = "Count Up Timer value",
                JoinCapabilities = eJoinCapabilities.ToSIMPL,
                JoinType = eJoinType.Serial
            });

		#endregion


		public TimerJoinMap(uint joinStart)
			: base(joinStart, typeof (TimerJoinMap))
		{			
		}
	}
}