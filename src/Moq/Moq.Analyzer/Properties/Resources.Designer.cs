﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Moq.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Moq.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Implement mock.
        /// </summary>
        internal static string CustomMockCodeFix_Implement {
            get {
                return ResourceManager.GetString("CustomMockCodeFix_Implement", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Generate {0}.
        /// </summary>
        internal static string GenerateMockCodeFix_TitleFormat {
            get {
                return ResourceManager.GetString("GenerateMockCodeFix_TitleFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invoked method requires a mock to be generated at design-time or compile-time..
        /// </summary>
        internal static string MissingMockAnalyzer_Description {
            get {
                return ResourceManager.GetString("MissingMockAnalyzer_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Expected mock &apos;{0}&apos; was not found in the current compilation..
        /// </summary>
        internal static string MissingMockAnalyzer_Message {
            get {
                return ResourceManager.GetString("MissingMockAnalyzer_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mock not found.
        /// </summary>
        internal static string MissingMockAnalyzer_Title {
            get {
                return ResourceManager.GetString("MissingMockAnalyzer_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Project &apos;{0}&apos; does not reference the required Moq assembly..
        /// </summary>
        internal static string MoqRequired {
            get {
                return ResourceManager.GetString("MoqRequired", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Existing generated mock &apos;{0}&apos; is outdated..
        /// </summary>
        internal static string OutdatedMockAnalyzer_Description {
            get {
                return ResourceManager.GetString("OutdatedMockAnalyzer_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mock  must be updated..
        /// </summary>
        internal static string OutdatedMockAnalyzer_Message {
            get {
                return ResourceManager.GetString("OutdatedMockAnalyzer_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Mock must be updated.
        /// </summary>
        internal static string OutdatedMockAnalyzer_Title {
            get {
                return ResourceManager.GetString("OutdatedMockAnalyzer_Title", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Update {0}.
        /// </summary>
        internal static string UpdateMockCodeFix_TitleFormat {
            get {
                return ResourceManager.GetString("UpdateMockCodeFix_TitleFormat", resourceCulture);
            }
        }
    }
}
