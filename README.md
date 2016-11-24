# Louw.PublicSuffix
.Net Core Library to parse Public Suffix, i.e. the registrable public suffix. See https://publicsuffix.org/ for details.

Based on original project: https://github.com/tinohager/Nager.PublicSuffix/

Changes:
* Ported to .NET Core Library.
* Fixed library so it passes ALL the comprehensive tests.
* Refactored classes to split functionality into smaller focused classes.
* Made classes immutable. Thus DomainParser can be used as Singleton and is Thread Safe.
* Added CachedTldRuleProvider and FileTldRuleProvider.
* Added functionality to know if Rule was a ICANN or Private domain rule.
(Many of above changes were submitted back to original project)

#####nuget
The package is available on nuget
https://www.nuget.org/packages/Louw.PublicSuffix

```
install-package Louw.PublicSuffix
```


#####Basic Example
```cs
	var domainParser = new DomainParser(new CachedTldRuleProvider());
    var domainInfo = domainParser.Get("sub.test.co.uk");
    Assert.Equal("test.co.uk", domainInfo.RegistrableDomain);
```

#####More Examples

A lot more detailed examples can be found here:
https://github.com/louislouw/Louw.PublicSuffix/blob/master/test/Louw.PublicSuffix.UnitTests/Examples.cs

