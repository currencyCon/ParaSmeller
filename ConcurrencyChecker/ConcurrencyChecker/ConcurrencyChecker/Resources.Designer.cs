﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ConcurrencyChecker {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ConcurrencyChecker.Resources", typeof(Resources).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type names should be all uppercase..
        /// </summary>
        internal static string AnalyzerDescription {
            get {
                return ResourceManager.GetString("AnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type name &apos;{0}&apos; contains lowercase letters.
        /// </summary>
        internal static string AnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("AnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The result of this Computation is potentially never awaited.
        /// </summary>
        internal static string AnalyzerMessageFormatFireAndForget {
            get {
                return ResourceManager.GetString("AnalyzerMessageFormatFireAndForget", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The {0} {1} is also used in another synchronized Method . Consider synchronizing also this one..
        /// </summary>
        internal static string AnalyzerMessageFormatHalfSynchronized {
            get {
                return ResourceManager.GetString("AnalyzerMessageFormatHalfSynchronized", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;new Thread&apos; should be replaced with Task.Run.
        /// </summary>
        internal static string AnalyzerMessageFormatSingleLine {
            get {
                return ResourceManager.GetString("AnalyzerMessageFormatSingleLine", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Property is used in a synchronized Member. Consider synchronizing it..
        /// </summary>
        internal static string AnalyzerMessageFormatUnsynchronizedProperty {
            get {
                return ResourceManager.GetString("AnalyzerMessageFormatUnsynchronizedProperty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type name contains lowercase letters.
        /// </summary>
        internal static string AnalyzerTitle {
            get {
                return ResourceManager.GetString("AnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to All Thread.Start should be replaced with Task.Run.
        /// </summary>
        internal static string ETCAnalyzerDescription {
            get {
                return ResourceManager.GetString("ETCAnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;{0}&apos; should be replaced with Task.Run.
        /// </summary>
        internal static string ETCAnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("ETCAnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;new Thread&apos; should be replaced with Task.Run.
        /// </summary>
        internal static string ETCAnalyzerMessageFormatSingleLine {
            get {
                return ResourceManager.GetString("ETCAnalyzerMessageFormatSingleLine", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Thread.Start shouldn&apos;t be used.
        /// </summary>
        internal static string ETCAnalyzerTitle {
            get {
                return ResourceManager.GetString("ETCAnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This Member is used within the class and its Finalizer. Consider Synchronizing the usages..
        /// </summary>
        internal static string FinalizerSynchronizationAnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("FinalizerSynchronizationAnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to In almost all cases Fire and Forget Calls should&apos;nt take place..
        /// </summary>
        internal static string FireAndForgetAnalyzerDescription {
            get {
                return ResourceManager.GetString("FireAndForgetAnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A Computation in a seperate Thread is not awaited.
        /// </summary>
        internal static string FireAndForgetAnalyzerTitle {
            get {
                return ResourceManager.GetString("FireAndForgetAnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Finalizers run asynchronous to the rest of the code. Access to Members in Finalizers should therefore be synchronized. .
        /// </summary>
        internal static string FSAnalyzerDescription {
            get {
                return ResourceManager.GetString("FSAnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unsynchronized Finalizers.
        /// </summary>
        internal static string FSAnalyzerTitle {
            get {
                return ResourceManager.GetString("FSAnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Starvation could happen.
        /// </summary>
        internal static string MWSAnalyzerDescription {
            get {
                return ResourceManager.GetString("MWSAnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Wrong monitor usage.
        /// </summary>
        internal static string MWSAnalyzerTitle {
            get {
                return ResourceManager.GetString("MWSAnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to if should be replaced with while.
        /// </summary>
        internal static string MWSIfAnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("MWSIfAnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pulse should be replaced with PulseAll.
        /// </summary>
        internal static string MWSPulseAnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("MWSPulseAnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No Deadlocks should occur in Code.
        /// </summary>
        internal static string NSMCAnalyzerDescription {
            get {
                return ResourceManager.GetString("NSMCAnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Possible Deadlock with double Locking.
        /// </summary>
        internal static string NSMCAnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("NSMCAnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to NestedSynchronizedMethod Calls.
        /// </summary>
        internal static string NSMCAnalyzerTitle {
            get {
                return ResourceManager.GetString("NSMCAnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Over async desc.
        /// </summary>
        internal static string OAAnalyzerDescription {
            get {
                return ResourceManager.GetString("OAAnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to async shouldn&apos;t be used in private methods.
        /// </summary>
        internal static string OAAnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("OAAnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Async shoudn&apos;t be nested {0} times.
        /// </summary>
        internal static string OAAnalyzerMessageFormatNestedAsync {
            get {
                return ResourceManager.GetString("OAAnalyzerMessageFormatNestedAsync", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Over Async.
        /// </summary>
        internal static string OAAnalyzerTitle {
            get {
                return ResourceManager.GetString("OAAnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Consider using more abstract constructs like lock if not implementing a high performant library..
        /// </summary>
        internal static string PrimitiveSynchronizationAnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("PrimitiveSynchronizationAnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Synchronization Mechanicms used are very primitive. Consider using a higher form of abstraction. .
        /// </summary>
        internal static string PSAnalyzerDescription {
            get {
                return ResourceManager.GetString("PSAnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Primitive Synchronization.
        /// </summary>
        internal static string PSAnalyzerTitle {
            get {
                return ResourceManager.GetString("PSAnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tapirs shouldn&apos;t be alone.
        /// </summary>
        internal static string TapirAnalyzerDescription {
            get {
                return ResourceManager.GetString("TapirAnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tapir Class is lonely.
        /// </summary>
        internal static string TapirAnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("TapirAnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Tapir :).
        /// </summary>
        internal static string TapirAnalyzerTitle {
            get {
                return ResourceManager.GetString("TapirAnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Timeouts can lead to starvation2.
        /// </summary>
        internal static string TRRAnalyzerDescription {
            get {
                return ResourceManager.GetString("TRRAnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Timeouts can lead to starvation.
        /// </summary>
        internal static string TRRAnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("TRRAnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Starvation possible.
        /// </summary>
        internal static string TRRAnalyzerTitle {
            get {
                return ResourceManager.GetString("TRRAnalyzerTitle", resourceCulture);
            }
        }
    }
}
