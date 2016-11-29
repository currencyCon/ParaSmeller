using System;


namespace log4net.Appender
{

    public abstract class AppenderSkeleton 
    {
        #region Protected Instance Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <remarks>
        /// <para>Empty default constructor</para>
        /// </remarks>
        protected AppenderSkeleton()
        {
        }

        #endregion Protected Instance Constructors

        #region Finalizer

        ~AppenderSkeleton()
        {
            // An appender might be closed then garbage collected. 
            // There is no point in closing twice.
            if (!m_closed)
            {
                DoStuff(declaringType, "Finalizing appender named [" +"blub" + "].");
            }
        }

        #endregion Finalizer

        public void DoStuff(Type type, string msg)
        {
            
        }

        private bool m_closed = false;

        public virtual bool Flush(int millisecondsTimeout)
        {
            return true;
        }

        private readonly static Type declaringType = typeof(AppenderSkeleton);

    }
}
