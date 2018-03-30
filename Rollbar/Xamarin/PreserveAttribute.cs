namespace Xamarin.iOS.Foundation
{
    using System;

    /// <summary>
    /// 
    /// This attribute could be used for decorating some classes or their members
    /// in order to prevent removal of unused members (that in fact could be still 
    /// used by the Reflection) by the mtouch's Linker during the optimization phase.
    /// 
    /// </summary>
    /// <seealso cref="System.Attribute" />
    /// <remarks>
    /// When building your application, Visual Studio for Mac or Visual Studio calls a tool called mtouch 
    /// that includes a linker for managed code. It is used to remove from the class libraries the features 
    /// that the application is not using. The goal is to reduce the size of the application, 
    /// which will ship with only the necessary bits.
    /// 
    /// The linker uses static analysis to determine the different code paths that your application is 
    /// susceptible to follow.It's a bit heavy as it has to go through every detail of each assembly, 
    /// to make sure that nothing discoverable is removed. It is not enabled by default on the simulator 
    /// builds to speed up the build time while debugging. However since it produces smaller applications 
    /// it can speed up AOT compilation and uploading to the device, all devices (Release) builds are using 
    /// the linker by default.
    /// 
    /// As the linker is a static tool, it can not mark for inclusion types and methods that are called 
    /// through reflection, or dynamically instantiated.Several options exists to workaround this limitation.
    /// Use of PreserveAttribute is one of them.
    /// 
    /// For more detail regarding the use of this attribute, please, refer to:
    /// https://docs.microsoft.com/en-us/xamarin/ios/deploy-test/linker?tabs=vsmac
    /// </remarks>
    [AttributeUsage(AttributeTargets.All, Inherited = true)]
    public sealed class PreserveAttribute 
        : Attribute
    {

        /// <summary>
        /// Signifies whether all members of the decorated type should be preserved. 
        /// Default value is true.
        /// </summary>
        public bool AllMembers { get; set; } = true;

        /// <summary>
        /// Signifies that the decorated member should be preserved only 
        /// if the containing type was preserved.
        /// </summary>
        public bool Conditional { get; set; } = false;
    }
}
