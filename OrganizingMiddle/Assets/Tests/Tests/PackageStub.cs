// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

#if false
[assembly:InternalsVisibleTo("workrooms_editor")]
[assembly:InternalsVisibleTo("workrooms_tests")]
[assembly:InternalsVisibleTo("workrooms_tests_editor")]
[assembly:InternalsVisibleTo("workrooms_unittests")]
#endif
[assembly:TogetherPackage("Facebook.SocialVR.Apps.Workrooms.Tests")]

[AttributeUsage(AttributeTargets.Assembly)]
internal class TogetherTestPackageAttribute : Attribute {
  [SuppressMessage("Gendarme.Rules.Performance", "PreferLiteralOverInitOnlyFieldsRule")]
  public const string packageNamespace = "Facebook.SocialVR.Apps.Workrooms.Tests";
  internal TogetherTestPackageAttribute(string packageStubNamespace) {
    this.packageStubNamespace = packageStubNamespace;
  }
  public readonly string packageStubNamespace;
}

namespace Facebook.SocialVR.Apps.Workrooms.Tests {
    public partial class PackageStub {
      static PackageStub() {
        RegisterAttributeTypes();
        RegisterVertsCodeGenFieldSetFactoryTypes();
      }

      // This stub method will be filled in via weaving at assembly compilation time
      private static void RegisterAttributeTypes() {
      }

      // This method is implemented by source code generation (https://fburl.com/code/3xtzsbh2).  The method
      // registers factory types for field sets that use code generation.  For field sets with a registered
      // factory, the driver will use the factory type instead of relying on the fallback implementation
      // which relies on pointer arithmetic and `Unsafe`.
      static partial void RegisterVertsCodeGenFieldSetFactoryTypes();
    }

    


}

