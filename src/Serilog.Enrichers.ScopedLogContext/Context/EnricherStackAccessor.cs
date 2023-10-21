namespace Serilog.Context
{
    internal class EnricherStackAccessor
    {
        private static readonly AsyncLocal<EnricherStack?> _enricherStackCurrent = new AsyncLocal<EnricherStack?>();

        /// <inheritdoc/>
        public EnricherStack? EnricherStack
        {
            get
            {
                return _enricherStackCurrent.Value;
            }
            set
            {
                _enricherStackCurrent.Value = value;
            }
        }

        private sealed class EnricherStackHolder
        {
            public EnricherStack? Enrichers;
        }
    }
}
