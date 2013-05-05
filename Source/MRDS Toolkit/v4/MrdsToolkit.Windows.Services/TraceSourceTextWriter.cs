using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace MrdsToolkit.Windows.Services
{
    /// <summary>
    /// <see cref="TextWriter"/> which redirects all output to a <see cref="TraceSource"/> to use
    /// with standard .NET logging mechanisms.
    /// </summary>
    public class TraceSourceTextWriter : TextWriter
    {
        #region Lifetime

        /// <summary>
        /// Creates an instance with the default format provider and UTF8 encoding.
        /// </summary>
        public TraceSourceTextWriter(TraceSource target, TraceEventType targetEventType)
            : this(target, targetEventType, CultureInfo.CurrentCulture, Encoding.UTF8)
        {
        }

        /// <summary>
        /// Creates an instance with the specified format provider.
        /// </summary>
        public TraceSourceTextWriter(TraceSource target, TraceEventType targetEventType, 
            IFormatProvider formatProvider, Encoding encoding)
            : base(formatProvider)
        {
            _encoding = encoding;
            Target = target;
            TargetEventType = targetEventType;
        }

        #endregion

        #region Public Properties

        /// <returns>
        /// The <see cref="Encoding"/> in which the output is written.
        /// </returns>
        public override Encoding Encoding
        {
            get { return _encoding; }
        }
        private readonly Encoding _encoding;

        /// <summary>
        /// Target <see cref="TraceSource"/> to which all text is written.
        /// </summary>
        public TraceSource Target { get; private set; }

        /// <summary>
        /// Target <see cref="TraceEventType"/> for events logged (messages written).
        /// </summary>
        public TraceEventType TargetEventType { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes text to the trace source.
        /// </summary>
        public override void Write(char[] buffer, int index, int count)
        {
            // Extract message
            var message = new String(buffer, index, count);

            // Log event
            Target.TraceEvent(TargetEventType, 0, message);
        }

        #endregion
    }
}
