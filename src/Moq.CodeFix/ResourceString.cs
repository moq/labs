using System;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Moq.Properties;

namespace Moq
{
    class ResourceString : LocalizableString
    {
        readonly string name;

        public ResourceString(string name) => this.name = name;

        protected override bool AreEqual(object other)
            => (other as ResourceString)?.name == name;

        protected override int GetHash() => name.GetHashCode();

        protected override string GetText(IFormatProvider formatProvider)
            => Resources.ResourceManager.GetString(name, (formatProvider as CultureInfo ?? CultureInfo.CurrentUICulture)) ?? "";
    }
}
